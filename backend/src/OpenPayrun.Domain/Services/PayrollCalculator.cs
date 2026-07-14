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
        decimal federalClaimAmount = 0,
        decimal quebecClaimAmount = 0,
        ITaxRates? rates = null)
    {
        rates ??= new TaxRates2025();
        if (federalClaimAmount == 0) federalClaimAmount = rates.FederalBasicPersonalAmount;
        if (quebecClaimAmount == 0) quebecClaimAmount = rates.QuebecBasicPersonalAmount;

        int periods = (int)frequency;

        var perPeriodExemption = rates.QppExemption / periods;
        var qppTier1Pensionable = Math.Max(grossPay - perPeriodExemption, 0);
        var qppTier1 = CapDeduction(qppTier1Pensionable * rates.QppTier1Rate,
            ytdQppTier1, rates.QppTier1MaxEmployee);

        var qppTier2 = CapDeduction(
            EarningsInBand(ytdGrossEarnings, grossPay, rates.QppYmpe, rates.QppYampe)
                * rates.QppTier2Rate,
            ytdQppTier2, rates.QppTier2MaxEmployee);

        var ei = CapDeduction(
            EarningsInBand(ytdGrossEarnings, grossPay, 0, rates.EiMaxInsurableEarnings)
                * rates.EiEmployeeRate,
            ytdEiPremiums, rates.EiMaxEmployeePremium);

        var qpipBase = EarningsInBand(ytdGrossEarnings, grossPay, 0, rates.QpipMaxInsurableEarnings);
        var qpip = CapDeduction(qpipBase * rates.QpipEmployeeRate,
            ytdQpipPremiums, rates.QpipMaxEmployeePremium);

        var annualGross = grossPay * periods;

        // F5Q per period: QPP enhanced (1%) + Tier 2 — income deduction per T4127 Step 1 / TP-1015.F CSA
        var f5q = qppTier1 * (rates.QppAdditionalTier1Rate / rates.QppTier1Rate) + qppTier2;

        // Federal tax — T4127 formula for Quebec residents
        var federalTaxableIncome = Math.Max(annualGross - f5q * periods, 0);
        var annualQppBaseCredit = Math.Min(
            qppTier1 * (rates.QppBaseRate / rates.QppTier1Rate) * periods,
            rates.QppTier1MaxEmployee * (rates.QppBaseRate / rates.QppTier1Rate));
        var annualEiCredit = Math.Min(ei * periods, rates.EiMaxEmployeePremium);
        var annualQpipCredit = Math.Min(qpipBase * rates.QpipEmployeeRate * periods, rates.QpipMaxEmployeePremium);
        var federalCredits = rates.FederalLowestRate * (federalClaimAmount
            + Math.Min(annualGross, rates.FederalEmploymentAmount)
            + annualQppBaseCredit + annualEiCredit + annualQpipCredit);
        var federalIncomeTax = Math.Max(ApplyBrackets(federalTaxableIncome, rates.FederalBrackets) - federalCredits, 0)
            * (1 - rates.QuebecFederalAbatement) / periods;

        // Quebec tax — TP-1015.F Section 2.1.1 formula
        // H per period: déduction pour travailleur; cap is floor(1 420 ÷ P) — whole-dollar truncation per TP-1015.F
        var hPerPeriod = Math.Min(grossPay * rates.QuebecWorkerDeductionRate,
            Math.Floor(rates.QuebecWorkerDeductionMax / periods));
        var quebecTaxableIncome = Math.Max((grossPay - hPerPeriod - f5q) * periods, 0);
        // Quebec credit is personal amount only — QPP/EI/QPIP are not provincial credits (unlike federal K2Q)
        var quebecCredits = quebecClaimAmount * rates.QuebecLowestRate;
        var quebecIncomeTax = Math.Max(ApplyBrackets(quebecTaxableIncome, rates.QuebecBrackets) - quebecCredits, 0) / periods;

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
            EmployerEi = Round(Round(ei) * rates.EiEmployerMultiplier),
            EmployerQpip = Round(Math.Min(qpipBase * rates.QpipEmployerRate, rates.QpipMaxEmployerPremium)),
            EmployerFssq = Round(grossPay * rates.FssqSmallEmployerRate),
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
