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
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025,
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
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025,
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
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025,
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
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025,
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
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025,
            ytdGrossEarnings: ytdGross);
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
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025,
            ytdEiPremiums: ytdEi);
        Assert.Equal(expected, r.EiPremium);
    }

    // ── Employer QPP ─────────────────────────────────────────────────────────

    [Fact]
    public void EmployerQppMatchesEmployee()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025);
        Assert.Equal(r.QppTier1, r.EmployerQppTier1);
        Assert.Equal(r.QppTier2, r.EmployerQppTier2);
    }

    // ── Employer EI ──────────────────────────────────────────────────────────

    [Fact]
    public void EmployerEiIs140PctOfEmployeeEi()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025);
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
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025,
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
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025,
            ytdGrossEarnings: ytdGross, ytdQppTier1: ytdQpp1,
            ytdQppTier2: ytdQpp2, ytdEiPremiums: ytdEi);
        Assert.Equal(expected, r.QuebecIncomeTax);
    }

    // ── Net pay ───────────────────────────────────────────────────────────────

    [Fact]
    public void NetPay_EqualsGrossMinusTotalDeductions()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025);
        Assert.Equal(r.GrossPay - r.TotalEmployeeDeductions, r.NetPay);
    }

    [Fact]
    public void NetPay_Month1_IsPositive()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2025);
        Assert.True(r.NetPay > 0);
    }
}

// Ground truth: FichierRetenueALasource.xlsx, sheet "Retenues et Charges 2026"
// Incorporated owner (selfEmployed=true): $8,333 gross/month, monthly, Quebec resident, no EI
public class PayrollCalculator2026Tests
{
    private const decimal Gross = 8_333m;
    private const PayFrequency Monthly = PayFrequency.Monthly;

    // ── QPP Tier 1 ──────────────────────────────────────────────────────────
    // Monthly pensionable = 8333 − 3500÷12 = 8041.33; × 6.3% = 506.60
    // Annual max = 4479.30; caps partway through month 9

    [Theory]
    [InlineData(0,          0,        506.60)] // Jan
    [InlineData(8_333,      506.60,   506.60)] // Feb
    [InlineData(16_666,   1_013.20,   506.60)] // Mar
    [InlineData(24_999,   1_519.80,   506.60)] // Apr
    [InlineData(33_332,   2_026.40,   506.60)] // May
    [InlineData(41_665,   2_533.00,   506.60)] // Jun
    [InlineData(49_998,   3_039.60,   506.60)] // Jul
    [InlineData(58_331,   3_546.20,   506.60)] // Aug
    [InlineData(66_664,   4_052.80,   426.50)] // Sep — capped (4479.30 − 4052.80)
    [InlineData(74_997,   4_479.30,     0.00)] // Oct — maxed
    [InlineData(83_330,   4_479.30,     0.00)] // Nov
    [InlineData(91_663,   4_479.30,     0.00)] // Dec
    public void QppTier1(decimal ytdGross, decimal ytdQpp1, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            ytdGrossEarnings: ytdGross, ytdQppTier1: ytdQpp1, selfEmployed: true);
        Assert.Equal(expected, r.QppTier1);
    }

    // ── QPP Tier 2 ──────────────────────────────────────────────────────────
    // YMPE = 74 600; YAMPE = 85 000; max = 416.00
    // Earnings enter the band in month 9 when YTD crosses YMPE

    [Theory]
    [InlineData(0,          0,        0.00)]   // Jan–Aug: below YMPE
    [InlineData(8_333,      0,        0.00)]
    [InlineData(16_666,     0,        0.00)]
    [InlineData(24_999,     0,        0.00)]
    [InlineData(33_332,     0,        0.00)]
    [InlineData(41_665,     0,        0.00)]
    [InlineData(49_998,     0,        0.00)]
    [InlineData(58_331,     0,        0.00)]
    [InlineData(66_664,     0,       15.88)]   // Sep — YTD 74997, band = 397, ×4% = 15.88
    [InlineData(74_997,    15.88,   333.32)]   // Oct — band = 8333, ×4% = 333.32
    [InlineData(83_330,   349.20,    66.80)]   // Nov — fills to cap (416.00 − 349.20)
    [InlineData(91_663,   416.00,     0.00)]   // Dec — maxed
    public void QppTier2(decimal ytdGross, decimal ytdQpp2, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            ytdGrossEarnings: ytdGross, ytdQppTier2: ytdQpp2, selfEmployed: true);
        Assert.Equal(expected, r.QppTier2);
    }

    // ── EI = 0 for incorporated owner ────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(8_333)]
    [InlineData(66_664)]
    public void Ei_AlwaysZero_WhenSelfEmployed(decimal ytdGross)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            ytdGrossEarnings: ytdGross, selfEmployed: true);
        Assert.Equal(0m, r.EiPremium);
        Assert.Equal(0m, r.EmployerEi);
    }

    // ── QPIP applies for incorporated owner ──────────────────────────────────
    // $8333×12 = $99 996 < $103 000 max insurable → no cap all year; 35.83/month

    [Theory]
    [InlineData(0,       0,      35.83)] // Jan
    [InlineData(8_333,   35.83,  35.83)] // Feb
    [InlineData(16_666,  71.66,  35.83)] // Mar
    [InlineData(24_999, 107.49,  35.83)] // Apr
    [InlineData(33_332, 143.32,  35.83)] // May
    [InlineData(41_665, 179.15,  35.83)] // Jun
    [InlineData(49_998, 214.98,  35.83)] // Jul
    [InlineData(58_331, 250.81,  35.83)] // Aug
    [InlineData(66_664, 286.64,  35.83)] // Sep
    [InlineData(74_997, 322.47,  35.83)] // Oct
    [InlineData(83_330, 358.30,  35.83)] // Nov
    [InlineData(91_663, 394.13,  35.83)] // Dec — under cap (429.96 < 442.90)
    public void QpipEmployee(decimal ytdGross, decimal ytdQpip, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            ytdGrossEarnings: ytdGross, ytdQpipPremiums: ytdQpip, selfEmployed: true);
        Assert.Equal(expected, r.QpipPremium);
    }

    // ── FSSQ applies for incorporated owner ──────────────────────────────────

    [Fact]
    public void EmployerFssq_AppliesWhenSelfEmployed()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            selfEmployed: true);
        Assert.Equal(137.49m, r.EmployerFssq);
    }

    // ── Employer QPP mirrors employee ────────────────────────────────────────

    [Fact]
    public void EmployerQppMatchesEmployee()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            selfEmployed: true);
        Assert.Equal(r.QppTier1, r.EmployerQppTier1);
        Assert.Equal(r.QppTier2, r.EmployerQppTier2);
    }

    // ── Full month 1 breakdown — Excel verified ──────────────────────────────
    // FichierRetenueALasource.xlsx "Retenues et Charges 2026": gross 8333, net 5760.91

    [Fact]
    public void Month1_FullBreakdown_MatchesExcel()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            selfEmployed: true);

        Assert.Equal(  506.60m, r.QppTier1);
        Assert.Equal(    0.00m, r.QppTier2);
        Assert.Equal(    0.00m, r.EiPremium);
        Assert.Equal(   35.83m, r.QpipPremium);
        Assert.Equal(  932.15m, r.FederalIncomeTax);
        Assert.Equal(1_097.51m, r.QuebecIncomeTax);
        Assert.Equal(2_572.09m, r.TotalEmployeeDeductions);
        Assert.Equal(5_760.91m, r.NetPay);
        Assert.Equal(  506.60m, r.EmployerQppTier1);
        Assert.Equal(    0.00m, r.EmployerEi);
        Assert.Equal(   50.16m, r.EmployerQpip);
        Assert.Equal(  137.49m, r.EmployerFssq);
        Assert.Equal(  694.25m, r.TotalEmployerContributions);
    }

    // ── Net pay invariant ────────────────────────────────────────────────────

    [Fact]
    public void NetPay_EqualsGrossMinusTotalDeductions()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            selfEmployed: true);
        Assert.Equal(r.GrossPay - r.TotalEmployeeDeductions, r.NetPay);
    }

}

// Ground truth: formula-derived (no Excel for 2026 regular employee)
// Regular salaried employee (selfEmployed=false): $8,333 gross/month, monthly, Quebec resident
// QPP/QPIP identical to self-employed; EI applies and generates a federal credit reducing federal tax
public class PayrollCalculator2026RegularTests
{
    private const decimal Gross = 8_333m;
    private const PayFrequency Monthly = PayFrequency.Monthly;

    // ── EI ───────────────────────────────────────────────────────────────────
    // Max insurable = $68,900 → insurable cap hit in Sep; premium cap $895.70 hit mid-Sep

    [Theory]
    [InlineData(0,      0.00,  108.33)] // Jan — full
    [InlineData(58_331, 758.31, 108.33)] // Aug — still full (premium cap not yet hit)
    [InlineData(66_664, 866.64,  29.06)] // Sep — partial: premium cap ($895.70) and insurable cap ($68,900) both bite
    [InlineData(74_997, 895.70,   0.00)] // Oct — maxed
    [InlineData(91_663, 895.70,   0.00)] // Dec — maxed
    public void EiPremium(decimal ytdGross, decimal ytdEi, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            ytdGrossEarnings: ytdGross, ytdEiPremiums: ytdEi);
        Assert.Equal(expected, r.EiPremium);
    }

    // ── Employer EI ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0,      0.00,  151.66)] // Jan
    [InlineData(66_664, 866.64,  40.68)] // Sep — partial
    [InlineData(74_997, 895.70,   0.00)] // Oct — maxed
    public void EmployerEi(decimal ytdGross, decimal ytdEi, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            ytdGrossEarnings: ytdGross, ytdEiPremiums: ytdEi);
        Assert.Equal(expected, r.EmployerEi);
    }

    [Fact]
    public void EmployerEiIs140PctOfEmployeeEi()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026);
        Assert.Equal(Math.Round(r.EiPremium * 1.4m, 2, MidpointRounding.AwayFromZero), r.EmployerEi);
    }

    // ── Federal income tax ───────────────────────────────────────────────────
    // Lower than self-employed because EI premiums generate a federal credit (K2 in T4127)
    // Jan–Aug constant: EI annualises to max ($895.70 credit base) regardless of partial cap
    // Sep: EI drops so annualised credit drops; QPP T1 also partially capped
    // Oct+: EI = 0, QPP T1 = 0 → credits collapse further, tax rises

    [Theory]
    [InlineData(0,          0.00,   0.00,   0.00,   0.00,  923.43)] // Jan
    [InlineData(58_331, 3_546.20,   0.00, 758.31, 250.81,  923.43)] // Aug
    [InlineData(66_664, 4_052.80,   0.00, 866.64, 286.64,  928.21)] // Sep — QPP T1 partial + EI partial
    [InlineData(74_997, 4_479.30,  15.88, 895.70, 322.47,  925.57)] // Oct — QPP T1 maxed, T2 active, EI maxed
    [InlineData(83_330, 4_479.30, 349.20, 895.70, 358.30,  971.19)] // Nov — T2 fills cap
    [InlineData(91_663, 4_479.30, 416.00, 895.70, 394.13,  982.63)] // Dec — all caps hit
    public void FederalIncomeTax(decimal ytdGross, decimal ytdQpp1, decimal ytdQpp2, decimal ytdEi, decimal ytdQpip, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            ytdGrossEarnings: ytdGross, ytdQppTier1: ytdQpp1,
            ytdQppTier2: ytdQpp2, ytdEiPremiums: ytdEi, ytdQpipPremiums: ytdQpip);
        Assert.Equal(expected, r.FederalIncomeTax);
    }

    // ── Quebec income tax ────────────────────────────────────────────────────
    // EI generates no Quebec credit (unlike federal), so values match self-employed exactly

    [Theory]
    [InlineData(0,          0.00,   0.00,   0.00,   0.00, 1_097.51)] // Jan
    [InlineData(58_331, 3_546.20,   0.00, 758.31, 250.81, 1_097.51)] // Aug
    [InlineData(66_664, 4_052.80,   0.00, 866.64, 286.64, 1_096.91)] // Sep
    [InlineData(74_997, 4_479.30,  15.88, 895.70, 322.47, 1_049.46)] // Oct
    [InlineData(83_330, 4_479.30, 349.20, 895.70, 358.30, 1_100.10)] // Nov
    [InlineData(91_663, 4_479.30, 416.00, 895.70, 394.13, 1_112.79)] // Dec
    public void QuebecIncomeTax(decimal ytdGross, decimal ytdQpp1, decimal ytdQpp2, decimal ytdEi, decimal ytdQpip, decimal expected)
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            ytdGrossEarnings: ytdGross, ytdQppTier1: ytdQpp1,
            ytdQppTier2: ytdQpp2, ytdEiPremiums: ytdEi, ytdQpipPremiums: ytdQpip);
        Assert.Equal(expected, r.QuebecIncomeTax);
    }

    // ── Full month 1 breakdown ───────────────────────────────────────────────

    [Fact]
    public void Month1_FullBreakdown()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026);

        Assert.Equal(  506.60m, r.QppTier1);
        Assert.Equal(    0.00m, r.QppTier2);
        Assert.Equal(  108.33m, r.EiPremium);
        Assert.Equal(   35.83m, r.QpipPremium);
        Assert.Equal(  923.43m, r.FederalIncomeTax);
        Assert.Equal(1_097.51m, r.QuebecIncomeTax);
        Assert.Equal(2_671.70m, r.TotalEmployeeDeductions);
        Assert.Equal(5_661.30m, r.NetPay);
        Assert.Equal(  506.60m, r.EmployerQppTier1);
        Assert.Equal(  151.66m, r.EmployerEi);
        Assert.Equal(   50.16m, r.EmployerQpip);
        Assert.Equal(  137.49m, r.EmployerFssq);
        Assert.Equal(  845.91m, r.TotalEmployerContributions);
    }

    // ── Full November breakdown (late year: T1 maxed, T2 filling, EI maxed) ──

    [Fact]
    public void November_FullBreakdown()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026,
            ytdGrossEarnings: 83_330m, ytdQppTier1: 4_479.30m, ytdQppTier2: 349.20m,
            ytdEiPremiums: 895.70m, ytdQpipPremiums: 358.30m);

        Assert.Equal(    0.00m, r.QppTier1);
        Assert.Equal(   66.80m, r.QppTier2);
        Assert.Equal(    0.00m, r.EiPremium);
        Assert.Equal(   35.83m, r.QpipPremium);
        Assert.Equal(  971.19m, r.FederalIncomeTax);
        Assert.Equal(1_100.10m, r.QuebecIncomeTax);
        Assert.Equal(2_173.92m, r.TotalEmployeeDeductions);
        Assert.Equal(6_159.08m, r.NetPay);
        Assert.Equal(    0.00m, r.EmployerEi);
        Assert.Equal(   50.16m, r.EmployerQpip);
        Assert.Equal(  137.49m, r.EmployerFssq);
        Assert.Equal(  254.45m, r.TotalEmployerContributions);
    }

    // ── Invariants ───────────────────────────────────────────────────────────

    [Fact]
    public void NetPay_EqualsGrossMinusTotalDeductions()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026);
        Assert.Equal(r.GrossPay - r.TotalEmployeeDeductions, r.NetPay);
    }

    [Fact]
    public void EmployerQppMatchesEmployee()
    {
        var r = PayrollCalculator.Calculate(Gross, Monthly, TaxRateFixtures.Rates2026);
        Assert.Equal(r.QppTier1, r.EmployerQppTier1);
        Assert.Equal(r.QppTier2, r.EmployerQppTier2);
    }
}
