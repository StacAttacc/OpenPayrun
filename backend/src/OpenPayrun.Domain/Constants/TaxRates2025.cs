namespace OpenPayrun.Domain.Constants;

public sealed class TaxRates2025 : ITaxRates
{
    // QPP — Quebec Pension Plan (replaces CPP for Quebec employees)
    public decimal QppExemption => 3_500m;
    public decimal QppYmpe => 71_300m;
    public decimal QppYampe => 81_200m;
    public decimal QppBaseRate => 0.054m;
    public decimal QppAdditionalTier1Rate => 0.01m;
    public decimal QppTier1Rate => 0.064m;
    public decimal QppTier2Rate => 0.04m;
    public decimal QppTier1MaxEmployee => 4_339.20m;
    public decimal QppTier2MaxEmployee => 396.00m;

    // EI — Employment Insurance (Quebec reduced rate)
    public decimal EiEmployeeRate => 0.0131m;
    public decimal EiEmployerMultiplier => 1.4m;
    public decimal EiMaxInsurableEarnings => 65_700m;
    public decimal EiMaxEmployeePremium => 860.67m;

    // QPIP — Quebec Parental Insurance Plan
    public decimal QpipEmployeeRate => 0.00494m;
    public decimal QpipEmployerRate => 0.00692m;
    public decimal QpipMaxInsurableEarnings => 98_000m;
    public decimal QpipMaxEmployeePremium => 484.12m;
    public decimal QpipMaxEmployerPremium => 678.16m;

    // Federal income tax brackets (2025 — 14.5% blended first bracket due to mid-year rate change)
    public (decimal UpperBound, decimal Rate)[] FederalBrackets =>
    [
        (57_375m,  0.145m),
        (114_750m, 0.205m),
        (177_882m, 0.26m),
        (253_414m, 0.29m),
        (decimal.MaxValue, 0.33m)
    ];

    public decimal FederalBasicPersonalAmount => 16_129m;
    public decimal FederalEmploymentAmount => 1_501m;  // blended 2025 value; pairs with 14.5% blended first-bracket rate
    public decimal FederalLowestRate => 0.145m;        // 14.5% blended: 15% (Jan–Jun) / 14% (Jul–Dec) mid-year rate change
    public decimal QuebecFederalAbatement => 0.165m;

    // Quebec provincial income tax brackets
    public (decimal UpperBound, decimal Rate)[] QuebecBrackets =>
    [
        (53_255m,  0.14m),
        (106_495m, 0.19m),
        (129_590m, 0.24m),
        (decimal.MaxValue, 0.2575m)
    ];

    public decimal QuebecBasicPersonalAmount => 18_571m;
    public decimal QuebecWorkerDeductionMax => 1_420m;
    public decimal QuebecWorkerDeductionRate => 0.06m;
    public decimal QuebecLowestRate => 0.14m;

    // FSSQ — Fonds de services de santé (employer-only Quebec payroll tax)
    public decimal FssqSmallEmployerRate => 0.0165m;
    public decimal FssqLargeEmployerRate => 0.0426m;
}
