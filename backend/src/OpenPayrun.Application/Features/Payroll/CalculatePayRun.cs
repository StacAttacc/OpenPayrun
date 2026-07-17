using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenPayrun.Application.Interfaces;
using OpenPayrun.Domain.Enums;
using OpenPayrun.Domain.Services;

namespace OpenPayrun.Application.Features.Payroll;

public record CalculatePayRunQuery(
    DateOnly PeriodStart,
    decimal GrossPay,
    PayFrequency Frequency,
    bool SelfEmployed = false,
    decimal YtdGrossEarnings = 0,
    decimal YtdQppTier1 = 0
) : IRequest<PayRunResult>;

public record PayRunResult(
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

public class CalculatePayRunHandler(IAppDbContext db) : IRequestHandler<CalculatePayRunQuery, PayRunResult>
{
    public async Task<PayRunResult> Handle(CalculatePayRunQuery req, CancellationToken ct)
    {
        var rates = await db.TaxRateSets
            .Include(r => r.Brackets)
            .Where(r => r.EffectiveFrom <= req.PeriodStart &&
                        (r.EffectiveTo == null || r.EffectiveTo >= req.PeriodStart))
            .FirstOrDefaultAsync(ct)
            ?? throw new NotSupportedException($"No tax rates configured for {req.PeriodStart}.");

        var ytdQppTier1 = req.YtdQppTier1 > 0 ? req.YtdQppTier1 : DeriveYtdQppTier1(req, rates);

        var d = PayrollCalculator.Calculate(
            req.GrossPay, req.Frequency,
            ytdGrossEarnings: req.YtdGrossEarnings,
            ytdQppTier1: ytdQppTier1,
            rates: rates,
            selfEmployed: req.SelfEmployed);

        return new PayRunResult(
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

    private static decimal DeriveYtdQppTier1(CalculatePayRunQuery req, Domain.Entities.TaxRateSet rates)
    {
        if (req.GrossPay <= 0 || req.YtdGrossEarnings <= 0) return 0;
        var priorPeriods = (int)Math.Round(req.YtdGrossEarnings / req.GrossPay);
        var perPeriodExemption = rates.QppExemption / (int)req.Frequency;
        var perPeriodT1 = Math.Round(Math.Max(req.GrossPay - perPeriodExemption, 0) * rates.QppTier1Rate, 2, MidpointRounding.AwayFromZero);
        return Math.Min(priorPeriods * perPeriodT1, rates.QppTier1MaxEmployee);
    }
}
