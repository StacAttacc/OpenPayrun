namespace OpenPayrun.Domain.Constants;

public static class TaxRates2025
{
    // QPP — Quebec Pension Plan (replaces CPP for Quebec employees)
    public const decimal QppExemption = 3_500m;
    public const decimal QppYmpe = 71_300m;       // Year's Maximum Pensionable Earnings
    public const decimal QppYampe = 81_200m;      // Year's Additional Maximum Pensionable Earnings (Tier 2 ceiling)
    public const decimal QppBaseRate = 0.054m;    // 5.40% — generates a tax credit
    public const decimal QppAdditionalTier1Rate = 0.01m;  // 1.00% — deductible from income
    public const decimal QppTier1Rate = 0.064m;   // 6.40% combined Tier 1
    public const decimal QppTier2Rate = 0.04m;    // 4.00% — deductible from income
    public const decimal QppTier1MaxEmployee = 4_339.20m;
    public const decimal QppTier2MaxEmployee = 396.00m;

    // EI — Employment Insurance (Quebec reduced rate)
    public const decimal EiEmployeeRate = 0.0131m;
    public const decimal EiEmployerMultiplier = 1.4m;
    public const decimal EiMaxInsurableEarnings = 65_700m;
    public const decimal EiMaxEmployeePremium = 860.67m;

    // QPIP — Quebec Parental Insurance Plan
    public const decimal QpipEmployeeRate = 0.00494m;
    public const decimal QpipEmployerRate = 0.00692m;
    public const decimal QpipMaxInsurableEarnings = 98_000m;
    public const decimal QpipMaxEmployeePremium = 484.12m;
    public const decimal QpipMaxEmployerPremium = 678.16m;

    // Federal income tax brackets (2025 — 14.5% blended first bracket due to mid-year rate change)
    public static readonly (decimal UpperBound, decimal Rate)[] FederalBrackets =
    [
        (57_375m,  0.145m),
        (114_750m, 0.205m),
        (177_882m, 0.26m),
        (253_414m, 0.29m),
        (decimal.MaxValue, 0.33m)
    ];

    public const decimal FederalBasicPersonalAmount = 16_129m;
    public const decimal FederalEmploymentAmount = 1_501m;  // blended 2025 value; pairs with 14.5% blended first-bracket rate
    public const decimal FederalLowestRate = 0.145m;        // 14.5% blended: 15% (Jan–Jun) / 14% (Jul–Dec) mid-year rate change
    public const decimal QuebecFederalAbatement = 0.165m; // Quebec residents get 16.5% abatement on federal tax

    // Quebec provincial income tax brackets
    public static readonly (decimal UpperBound, decimal Rate)[] QuebecBrackets =
    [
        (53_255m,  0.14m),
        (106_495m, 0.19m),
        (129_590m, 0.24m),
        (decimal.MaxValue, 0.2575m)
    ];

    public const decimal QuebecBasicPersonalAmount = 18_571m;
    public const decimal QuebecWorkerDeductionMax = 1_420m;  // Déduction pour travailleur
    public const decimal QuebecWorkerDeductionRate = 0.06m;  // 6% of employment income, capped at $1,420
    public const decimal QuebecLowestRate = 0.14m;

    // FSSQ — Fonds de services de santé (employer-only Quebec payroll tax)
    public const decimal FssqSmallEmployerRate = 0.0165m;  // payroll ≤ $1,000,000
    public const decimal FssqLargeEmployerRate = 0.0426m;  // payroll > $7,000,000
}
