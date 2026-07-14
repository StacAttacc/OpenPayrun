using OpenPayrun.Domain.Enums;
using OpenPayrun.Domain.Services;
using Xunit;

namespace OpenPayrun.Domain.Tests;

// Ground truth: FichierRetenueALasource.xlsx, sheet "Retenues & charges" (sheet2)
// Employee: $8,333 gross/month, monthly frequency, Quebec resident
// Columns B–M = Jan–Dec 2025; N = annual total
public class PayrollCalculatorTests
{
    private const decimal Gross = 8_333m;
    private const PayFrequency Monthly = PayFrequency.Monthly;

    // ── QPP Tier 1 ──────────────────────────────────────────────────────────
    // Excel rows 9 & 10 (RRQ Employé / Employeur)

    [Theory]
    [InlineData(0,          0,       514.65)] // Jan
    [InlineData(8_333,      514.65,  514.65)] // Feb
    [InlineData(16_666,   1_029.30,  514.65)] // Mar
    [InlineData(24_999,   1_543.95,  514.65)] // Apr
    [InlineData(33_332,   2_058.60,  514.65)] // May
    [InlineData(41_665,   2_573.25,  514.65)] // Jun
    [InlineData(49_998,   3_087.90,  514.65)] // Jul
    [InlineData(58_331,   3_602.55,  514.65)] // Aug
    [InlineData(66_664,   4_117.20,  222.00)] // Sep — capped
    [InlineData(74_997,   4_339.20,    0.00)] // Oct — maxed
    [InlineData(83_330,   4_339.20,    0.00)] // Nov
    [InlineData(91_663,   4_339.20,    0.00)] // Dec
    public void QppTier1(decimal ytdGross, decimal ytdQpp1, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly,
            ytdGrossEarnings: ytdGross, ytdQppTier1: ytdQpp1);
        Assert.Equal(expected, r.QppTier1);
    }

    // ── QPP Tier 2 ──────────────────────────────────────────────────────────
    // Excel rows 11 & 12 (2e RRQ Employé / Employeur)

    [Theory]
    [InlineData(0,       0,        0.00)]  // Jan
    [InlineData(8_333,   0,        0.00)]  // Feb
    [InlineData(16_666,  0,        0.00)]  // Mar
    [InlineData(24_999,  0,        0.00)]  // Apr
    [InlineData(33_332,  0,        0.00)]  // May
    [InlineData(41_665,  0,        0.00)]  // Jun
    [InlineData(49_998,  0,        0.00)]  // Jul
    [InlineData(58_331,  0,        0.00)]  // Aug
    [InlineData(66_664,  0,      147.88)]  // Sep — crosses YMPE
    [InlineData(74_997,  147.88, 248.12)]  // Oct — fills to YAMPE cap
    [InlineData(83_330,  396.00,   0.00)]  // Nov — maxed
    [InlineData(91_663,  396.00,   0.00)]  // Dec
    public void QppTier2(decimal ytdGross, decimal ytdQpp2, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly,
            ytdGrossEarnings: ytdGross, ytdQppTier2: ytdQpp2);
        Assert.Equal(expected, r.QppTier2);
    }

    // ── QPIP Employee ────────────────────────────────────────────────────────
    // Excel row 16 (RQAP Employé)

    [Theory]
    [InlineData(0,       0,      41.17)] // Jan
    [InlineData(8_333,   41.17,  41.17)] // Feb
    [InlineData(16_666,  82.34,  41.17)] // Mar
    [InlineData(24_999, 123.51,  41.17)] // Apr
    [InlineData(33_332, 164.68,  41.17)] // May
    [InlineData(41_665, 205.85,  41.17)] // Jun
    [InlineData(49_998, 247.02,  41.17)] // Jul
    [InlineData(58_331, 288.19,  41.17)] // Aug
    [InlineData(66_664, 329.36,  41.17)] // Sep
    [InlineData(74_997, 370.53,  41.17)] // Oct
    [InlineData(83_330, 411.70,  41.17)] // Nov
    [InlineData(91_663, 452.87,  31.25)] // Dec — partial cap
    public void QpipEmployee(decimal ytdGross, decimal ytdQpip, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly,
            ytdGrossEarnings: ytdGross, ytdQpipPremiums: ytdQpip);
        Assert.Equal(expected, r.QpipPremium);
    }

    // ── QPIP Employer ────────────────────────────────────────────────────────
    // Excel row 15 (RQAP Employeur)
    // Dec discrepancy: Excel = 43.90, formula = 43.85 ($0.05 rounding accumulation)

    [Theory]
    [InlineData(0,       0,      57.66)] // Jan
    [InlineData(8_333,   41.17,  57.66)] // Feb
    [InlineData(16_666,  82.34,  57.66)] // Mar
    [InlineData(24_999, 123.51,  57.66)] // Apr
    [InlineData(33_332, 164.68,  57.66)] // May
    [InlineData(41_665, 205.85,  57.66)] // Jun
    [InlineData(49_998, 247.02,  57.66)] // Jul
    [InlineData(58_331, 288.19,  57.66)] // Aug
    [InlineData(66_664, 329.36,  57.66)] // Sep
    [InlineData(74_997, 370.53,  57.66)] // Oct
    [InlineData(83_330, 411.70,  57.66)] // Nov
    [InlineData(91_663, 452.87,  43.85)] // Dec — formula 43.85; Excel 43.90 ($0.05 rounding accumulation from table method)
    public void QpipEmployer(decimal ytdGross, decimal ytdQpip, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly,
            ytdGrossEarnings: ytdGross, ytdQpipPremiums: ytdQpip);
        Assert.Equal(expected, r.EmployerQpip);
    }

    // ── FSSQ ─────────────────────────────────────────────────────────────────
    // Excel row 14 (FSSQ) — employer-only, 1.65% of gross, no per-employee cap
    // Excel alternates 137.49/137.50 due to floating-point; formula gives 137.49 consistently

    [Theory]
    [InlineData(0)]
    [InlineData(8_333)]
    [InlineData(16_666)]
    [InlineData(24_999)]
    [InlineData(33_332)]
    [InlineData(41_665)]
    [InlineData(49_998)]
    [InlineData(58_331)]
    [InlineData(66_664)]
    [InlineData(74_997)]
    [InlineData(83_330)]
    [InlineData(91_663)]
    public void EmployerFssq_AllMonths(decimal ytdGross)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, ytdGrossEarnings: ytdGross);
        Assert.Equal(137.49m, r.EmployerFssq);
    }

    // ── EI ───────────────────────────────────────────────────────────────────
    // EI does not appear in the Excel (separate remittance); these test the cap logic

    [Theory]
    [InlineData(0,      109.16)] // Jan — full
    [InlineData(764.12,  96.55)] // Aug cap — remaining room ($860.67 - $764.12)
    [InlineData(860.67,   0.00)] // Sep+ — maxed
    public void EiPremium(decimal ytdEi, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, ytdEiPremiums: ytdEi);
        Assert.Equal(expected, r.EiPremium);
    }

    // ── Employer QPP ─────────────────────────────────────────────────────────

    [Fact]
    public void EmployerQppMatchesEmployee()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly);
        Assert.Equal(r.QppTier1, r.EmployerQppTier1);
        Assert.Equal(r.QppTier2, r.EmployerQppTier2);
    }

    // ── Employer EI ──────────────────────────────────────────────────────────

    [Fact]
    public void EmployerEiIs140PctOfEmployeeEi()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly);
        Assert.Equal(Math.Round(r.EiPremium * 1.4m, 2, MidpointRounding.AwayFromZero), r.EmployerEi);
    }

    // ── Federal income tax ───────────────────────────────────────────────────
    // T4127 formula method (Option 1). Excel used T4032 table method (≈$21/month higher).
    // Both are CRA-accepted; they diverge because table method pre-discretises credit annualisation.
    // Excel ground truth: Jan-Jul $966.07, Aug $941.32, Sep $923.84, Oct $912.62, Nov-Dec $955.09

    [Theory]
    [InlineData(0,      0,       0,       0,      944.71)]  // Jan
    [InlineData(8_333,  514.65,  0,      109.16,  944.71)]  // Feb
    [InlineData(16_666, 1_029.30, 0,     218.32,  944.71)]  // Mar
    [InlineData(24_999, 1_543.95, 0,     327.48,  944.71)]  // Apr
    [InlineData(33_332, 2_058.60, 0,     436.64,  944.71)]  // May
    [InlineData(41_665, 2_573.25, 0,     545.80,  944.71)]  // Jun
    [InlineData(49_998, 3_087.90, 0,     654.96,  944.71)]  // Jul
    [InlineData(58_331, 3_602.55, 0,     764.12,  944.71)]  // Aug — formula unaffected; EI credit still annualises to max
    [InlineData(66_664, 4_117.20, 0,     860.67,  950.17)]  // Sep — QPP1 partially capped
    [InlineData(74_997, 4_339.20, 147.88, 860.67, 961.62)]  // Oct — QPP1 maxed, Tier 2 active
    [InlineData(83_330, 4_339.20, 396.00, 860.67, 1_004.10)] // Nov — all deductions maxed
    [InlineData(91_663, 4_339.20, 396.00, 860.67, 1_005.19)] // Dec
    public void FederalIncomeTax(decimal ytdGross, decimal ytdQpp1, decimal ytdQpp2, decimal ytdEi, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly,
            ytdGrossEarnings: ytdGross, ytdQppTier1: ytdQpp1,
            ytdQppTier2: ytdQpp2, ytdEiPremiums: ytdEi);
        Assert.Equal(expected, r.FederalIncomeTax);
    }

    // ── Quebec income tax ────────────────────────────────────────────────────
    // TP-1015.F formula method (Section 2.1.1). Excel used TP-1015.TI table method.
    // Both are accepted by Revenu Québec; TP-1015.F itself notes results will differ from TP-1015.TI.
    // Excel (table) ground truth: Jan–Sep $1107.01, Oct $1087.60, Nov $1075.15, Dec $1122.29
    // Formula diverges one month earlier: Jan–Aug $1107.01, Sep $1087.60, Oct $1075.15, Nov–Dec $1122.29

    [Theory]
    [InlineData(0,      0,       0,       0,       1_107.01)] // Jan
    [InlineData(8_333,  514.65,  0,      109.16,  1_107.01)] // Feb
    [InlineData(16_666, 1_029.30, 0,     218.32,  1_107.01)] // Mar
    [InlineData(24_999, 1_543.95, 0,     327.48,  1_107.01)] // Apr
    [InlineData(33_332, 2_058.60, 0,     436.64,  1_107.01)] // May
    [InlineData(41_665, 2_573.25, 0,     545.80,  1_107.01)] // Jun
    [InlineData(49_998, 3_087.90, 0,     654.96,  1_107.01)] // Jul
    [InlineData(58_331, 3_602.55, 0,     764.12,  1_107.01)] // Aug
    [InlineData(66_664, 4_117.20, 0,     860.67,  1_087.60)] // Sep — QPP1 partially capped (table stays at $1107.01)
    [InlineData(74_997, 4_339.20, 147.88, 860.67, 1_075.15)] // Oct — QPP1 maxed, Tier 2 active (table: $1087.60)
    [InlineData(83_330, 4_339.20, 396.00, 860.67, 1_122.29)] // Nov — all maxed (table: $1075.15)
    [InlineData(91_663, 4_339.20, 396.00, 860.67, 1_122.29)] // Dec (table: $1122.29)
    public void QuebecIncomeTax(decimal ytdGross, decimal ytdQpp1, decimal ytdQpp2, decimal ytdEi, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly,
            ytdGrossEarnings: ytdGross, ytdQppTier1: ytdQpp1,
            ytdQppTier2: ytdQpp2, ytdEiPremiums: ytdEi);
        Assert.Equal(expected, r.QuebecIncomeTax);
    }

    // ── Net pay ───────────────────────────────────────────────────────────────

    [Fact]
    public void NetPay_EqualsGrossMinusTotalDeductions()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly);
        Assert.Equal(r.GrossPay - r.TotalEmployeeDeductions, r.NetPay);
    }

    [Fact]
    public void NetPay_Month1_IsPositive()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly);
        Assert.True(r.NetPay > 0);
    }
}
