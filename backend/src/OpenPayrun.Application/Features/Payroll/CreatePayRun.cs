using MediatR;
using OpenPayrun.Application.Interfaces;
using OpenPayrun.Domain.Constants;
using OpenPayrun.Domain.Entities;
using OpenPayrun.Domain.Enums;
using OpenPayrun.Domain.Services;

namespace OpenPayrun.Application.Features.Payroll;

public record CreatePayRunCommand(
    int EmployeeId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal GrossPay,
    PayFrequency Frequency,
    decimal YtdGrossEarnings = 0,
    decimal YtdQppTier1 = 0,
    decimal YtdQppTier2 = 0,
    decimal YtdEiPremiums = 0,
    decimal YtdQpipPremiums = 0
) : IRequest<PayRunResponse>;

public record PayRunResponse(
    int Id,
    int EmployeeId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal GrossPay,
    decimal QppTier1,
    decimal QppTier2,
    decimal EiPremium,
    decimal QpipPremium,
    decimal FederalIncomeTax,
    decimal QuebecIncomeTax,
    decimal TotalEmployeeDeductions,
    decimal NetPay,
    decimal EmployerQppTier1,
    decimal EmployerQppTier2,
    decimal EmployerEi,
    decimal EmployerQpip,
    decimal EmployerFssq,
    decimal TotalEmployerContributions
);

public class CreatePayRunHandler(IAppDbContext db) : IRequestHandler<CreatePayRunCommand, PayRunResponse>
{
    public async Task<PayRunResponse> Handle(CreatePayRunCommand req, CancellationToken ct)
    {
        ITaxRates rates = req.PeriodStart.Year switch
        {
            2025 => new TaxRates2025(),
            _ => throw new NotSupportedException($"No tax rates configured for {req.PeriodStart.Year}.")
        };

        var d = PayrollCalculator.Calculate(
            req.GrossPay, req.Frequency,
            ytdGrossEarnings: req.YtdGrossEarnings,
            ytdQppTier1: req.YtdQppTier1,
            ytdQppTier2: req.YtdQppTier2,
            ytdEiPremiums: req.YtdEiPremiums,
            ytdQpipPremiums: req.YtdQpipPremiums,
            rates: rates);

        var payRun = new PayRun
        {
            EmployeeId = req.EmployeeId,
            PeriodStart = req.PeriodStart,
            PeriodEnd = req.PeriodEnd,
            GrossPay = d.GrossPay,
            EmployeeTax = d.TotalEmployeeDeductions,
            EmployerContributions = d.TotalEmployerContributions,
            NetPay = d.NetPay
        };

        db.PayRuns.Add(payRun);
        await db.SaveChangesAsync(ct);

        return new PayRunResponse(
            payRun.Id,
            payRun.EmployeeId,
            payRun.PeriodStart,
            payRun.PeriodEnd,
            d.GrossPay,
            d.QppTier1,
            d.QppTier2,
            d.EiPremium,
            d.QpipPremium,
            d.FederalIncomeTax,
            d.QuebecIncomeTax,
            d.TotalEmployeeDeductions,
            d.NetPay,
            d.EmployerQppTier1,
            d.EmployerQppTier2,
            d.EmployerEi,
            d.EmployerQpip,
            d.EmployerFssq,
            d.TotalEmployerContributions
        );
    }
}
