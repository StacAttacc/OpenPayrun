using OpenPayrun.Domain.Constants;
using OpenPayrun.Domain.Enums;
using OpenPayrun.Domain.ValueObjects;

namespace OpenPayrun.Domain.Services;

public static class PayrollCalculator
{
    public static PayrollDeductions Calculate(
        decimal grossPay,
        PayFrequency frequency,
        decimal ytdGrossEarnings = 0,
        decimal ytdQppTier1 = 0,
        decimal ytdQppTier2 = 0,
        decimal ytdEiPremiums = 0,
        decimal ytdQpipPremiums = 0,
        decimal federalClaimAmount = TaxRates2025.FederalBasicPersonalAmount,
        decimal quebecClaimAmount = TaxRates2025.QuebecBasicPersonalAmount,
        decimal fssqRate = TaxRates2025.FssqSmallEmployerRate)
    {
        int periods = (int)frequency;

        var perPeriodExemption = TaxRates2025.QppExemption / periods;
        var qppTier1Pensionable = Math.Max(grossPay - perPeriodExemption, 0);
        var qppTier1 = CapDeduction(qppTier1Pensionable * TaxRates2025.QppTier1Rate,
            ytdQppTier1, TaxRates2025.QppTier1MaxEmployee);

        var qppTier2 = CapDeduction(
            EarningsInBand(ytdGrossEarnings, grossPay, TaxRates2025.QppYmpe, TaxRates2025.QppYampe)
                * TaxRates2025.QppTier2Rate,
            ytdQppTier2, TaxRates2025.QppTier2MaxEmployee);

        var ei = CapDeduction(
            EarningsInBand(ytdGrossEarnings, grossPay, 0, TaxRates2025.EiMaxInsurableEarnings)
                * TaxRates2025.EiEmployeeRate,
            ytdEiPremiums, TaxRates2025.EiMaxEmployeePremium);

        var qpipBase = EarningsInBand(ytdGrossEarnings, grossPay, 0, TaxRates2025.QpipMaxInsurableEarnings);
        var qpip = CapDeduction(qpipBase * TaxRates2025.QpipEmployeeRate,
            ytdQpipPremiums, TaxRates2025.QpipMaxEmployeePremium);

        var annualGross = grossPay * periods;

        // F5Q per period: QPP enhanced (1%) + Tier 2 — income deduction per T4127 Step 1 / TP-1015.F CSA
        var f5q = qppTier1 * (TaxRates2025.QppAdditionalTier1Rate / TaxRates2025.QppTier1Rate) + qppTier2;

        // Federal tax — T4127 formula for Quebec residents
        var federalTaxableIncome = Math.Max(annualGross - f5q * periods, 0);
        var annualQppBaseCredit = Math.Min(
            qppTier1 * (TaxRates2025.QppBaseRate / TaxRates2025.QppTier1Rate) * periods,
            TaxRates2025.QppTier1MaxEmployee * (TaxRates2025.QppBaseRate / TaxRates2025.QppTier1Rate));
        var annualEiCredit = Math.Min(ei * periods, TaxRates2025.EiMaxEmployeePremium);
        var annualQpipCredit = Math.Min(qpipBase * TaxRates2025.QpipEmployeeRate * periods, TaxRates2025.QpipMaxEmployeePremium);
        var federalCredits = TaxRates2025.FederalLowestRate * (federalClaimAmount
            + Math.Min(annualGross, TaxRates2025.FederalEmploymentAmount)
            + annualQppBaseCredit + annualEiCredit + annualQpipCredit);
        var federalIncomeTax = Math.Max(ApplyBrackets(federalTaxableIncome, TaxRates2025.FederalBrackets) - federalCredits, 0)
            * (1 - TaxRates2025.QuebecFederalAbatement) / periods;

        // Quebec tax — TP-1015.F Section 2.1.1 formula
        // H per period: déduction pour travailleur; cap is floor(1 420 ÷ P) — whole-dollar truncation per TP-1015.F
        var hPerPeriod = Math.Min(grossPay * TaxRates2025.QuebecWorkerDeductionRate,
            Math.Floor(TaxRates2025.QuebecWorkerDeductionMax / periods));
        var quebecTaxableIncome = Math.Max((grossPay - hPerPeriod - f5q) * periods, 0);
        // Quebec credit is personal amount only — QPP/EI/QPIP are not provincial credits (unlike federal K2Q)
        var quebecCredits = quebecClaimAmount * TaxRates2025.QuebecLowestRate;
        var quebecIncomeTax = Math.Max(ApplyBrackets(quebecTaxableIncome, TaxRates2025.QuebecBrackets) - quebecCredits, 0) / periods;

        return new PayrollDeductions
        {
            GrossPay = grossPay,
            QppTier1 = Round(qppTier1),
            QppTier2 = Round(qppTier2),
            EiPremium = Round(ei),
            QpipPremium = Round(qpip),
            FederalIncomeTax = Round(federalIncomeTax),
            QuebecIncomeTax = Round(quebecIncomeTax),
            EmployerQppTier1 = Round(qppTier1),
            EmployerQppTier2 = Round(qppTier2),
            EmployerEi = Round(Round(ei) * TaxRates2025.EiEmployerMultiplier),
            EmployerQpip = Round(Math.Min(qpipBase * TaxRates2025.QpipEmployerRate, TaxRates2025.QpipMaxEmployerPremium)),
            EmployerFssq = Round(grossPay * fssqRate),
        };
    }

    private static decimal EarningsInBand(decimal ytd, decimal grossPay, decimal low, decimal high) =>
        Math.Max(Math.Min(ytd + grossPay, high) - Math.Max(ytd, low), 0);

    private static decimal CapDeduction(decimal amount, decimal ytdAmount, decimal annualMax) =>
        Math.Max(Math.Min(amount, annualMax - ytdAmount), 0);

    private static decimal ApplyBrackets(decimal income, (decimal UpperBound, decimal Rate)[] brackets)
    {
        decimal tax = 0;
        decimal lower = 0;
        foreach (var (upper, rate) in brackets)
        {
            if (income <= lower) break;
            tax += (Math.Min(income, upper) - lower) * rate;
            lower = upper;
        }
        return tax;
    }

    private static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
