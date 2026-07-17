using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenPayrun.Application.Interfaces;
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
    bool SelfEmployed = false,
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
        var rates = await db.TaxRateSets
            .Include(r => r.Brackets)
            .Where(r => r.EffectiveFrom <= req.PeriodStart &&
                        (r.EffectiveTo == null || r.EffectiveTo >= req.PeriodStart))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotSupportedException($"No tax rates configured for {req.PeriodStart}.");

        // Derive ytdQppTier1 from ytdGrossEarnings when caller doesn't supply it (assumes consistent salary)
        var ytdQppTier1 = req.YtdQppTier1 > 0 ? req.YtdQppTier1 : DeriveYtdQppTier1(req, rates);

        var d = PayrollCalculator.Calculate(
            req.GrossPay, req.Frequency,
            ytdGrossEarnings: req.YtdGrossEarnings,
            ytdQppTier1: ytdQppTier1,
            ytdQppTier2: req.YtdQppTier2,
            ytdEiPremiums: req.YtdEiPremiums,
            ytdQpipPremiums: req.YtdQpipPremiums,
            rates: rates,
            selfEmployed: req.SelfEmployed);

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

    private static decimal DeriveYtdQppTier1(CreatePayRunCommand req, Domain.Entities.TaxRateSet rates)
    {
        if (req.GrossPay <= 0 || req.YtdGrossEarnings <= 0) return 0;
        var priorPeriods = (int)Math.Round(req.YtdGrossEarnings / req.GrossPay);
        var perPeriodExemption = rates.QppExemption / (int)req.Frequency;
        // Round per-period amount to match how each period's T1 was actually deducted
        var perPeriodT1 = Math.Round(Math.Max(req.GrossPay - perPeriodExemption, 0) * rates.QppTier1Rate, 2, MidpointRounding.AwayFromZero);
        return Math.Min(priorPeriods * perPeriodT1, rates.QppTier1MaxEmployee);
    }
}
