namespace OpenPayrun.Domain.Constants;

public sealed class TaxRates2026 : ITaxRates
{
    // QPP — Quebec Pension Plan (source: TP-1015.F 2026-01)
    public decimal QppExemption => 3_500m;
    public decimal QppYmpe => 74_600m;
    public decimal QppYampe => 85_000m;
    public decimal QppBaseRate => 0.053m;
    public decimal QppAdditionalTier1Rate => 0.01m;
    public decimal QppTier1Rate => 0.063m;
    public decimal QppTier2Rate => 0.04m;
    public decimal QppTier1MaxEmployee => 4_479.30m;
    public decimal QppTier2MaxEmployee => 416.00m;

    // EI — Employment Insurance (Quebec reduced rate, source: T4127 Jan 2026 Table 8.7)
    public decimal EiEmployeeRate => 0.013m;
    public decimal EiEmployerMultiplier => 1.4m;
    public decimal EiMaxInsurableEarnings => 68_900m;
    public decimal EiMaxEmployeePremium => 895.70m;

    // QPIP — Quebec Parental Insurance Plan (source: TP-1015.F 2026-01)
    public decimal QpipEmployeeRate => 0.0043m;
    public decimal QpipEmployerRate => 0.00602m;
    public decimal QpipMaxInsurableEarnings => 103_000m;
    public decimal QpipMaxEmployeePremium => 442.90m;
    public decimal QpipMaxEmployerPremium => 620.06m;

    // Federal income tax brackets (source: T4127 Jan 2026, Table 8.1)
    public (decimal UpperBound, decimal Rate)[] FederalBrackets =>
    [
        (58_523m,  0.14m),
        (117_045m, 0.205m),
        (181_440m, 0.26m),
        (258_482m, 0.29m),
        (decimal.MaxValue, 0.33m)
    ];

    public decimal FederalBasicPersonalAmount => 16_452m;
    public decimal FederalEmploymentAmount => 1_501m;
    public decimal FederalLowestRate => 0.14m;  // 14% flat — no longer blended
    public decimal QuebecFederalAbatement => 0.165m;

    // Quebec provincial income tax brackets (source: TP-1015.F 2026-01)
    public (decimal UpperBound, decimal Rate)[] QuebecBrackets =>
    [
        (54_345m,  0.14m),
        (108_680m, 0.19m),
        (132_245m, 0.24m),
        (decimal.MaxValue, 0.2575m)
    ];

    public decimal QuebecBasicPersonalAmount => 18_952m;
    public decimal QuebecWorkerDeductionMax => 1_450m;
    public decimal QuebecWorkerDeductionRate => 0.06m;
    public decimal QuebecLowestRate => 0.14m;

    // FSSQ — Fonds de services de santé (source: TP-1015.F 2026-01)
    public decimal FssqSmallEmployerRate => 0.0165m;
    public decimal FssqLargeEmployerRate => 0.0426m;
}
