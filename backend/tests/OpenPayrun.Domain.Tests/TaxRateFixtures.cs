using OpenPayrun.Domain.Entities;
using OpenPayrun.Domain.Enums;

namespace OpenPayrun.Domain.Tests;

internal static class TaxRateFixtures
{
    public static TaxRateSet Rates2025 { get; } = new()
    {
        QppExemption = 3_500m, QppYmpe = 71_300m, QppYampe = 81_200m,
        QppBaseRate = 0.054m, QppAdditionalTier1Rate = 0.01m, QppTier1Rate = 0.064m, QppTier2Rate = 0.04m,
        QppTier1MaxEmployee = 4_339.20m, QppTier2MaxEmployee = 396.00m,
        EiEmployeeRate = 0.0131m, EiEmployerMultiplier = 1.4m,
        EiMaxInsurableEarnings = 65_700m, EiMaxEmployeePremium = 860.67m,
        QpipEmployeeRate = 0.00494m, QpipEmployerRate = 0.00692m,
        QpipMaxInsurableEarnings = 98_000m, QpipMaxEmployeePremium = 484.12m, QpipMaxEmployerPremium = 678.16m,
        FederalBasicPersonalAmount = 16_129m, FederalEmploymentAmount = 1_501m,
        FederalLowestRate = 0.145m, QuebecFederalAbatement = 0.165m,
        QuebecBasicPersonalAmount = 18_571m, QuebecWorkerDeductionMax = 1_420m,
        QuebecWorkerDeductionRate = 0.06m, QuebecLowestRate = 0.14m,
        FssqSmallEmployerRate = 0.0165m, FssqLargeEmployerRate = 0.0426m,
        Brackets =
        [
            new() { BracketType = BracketType.Federal, UpperBound =  57_375m, Rate = 0.145m,  SortOrder = 1 },
            new() { BracketType = BracketType.Federal, UpperBound = 114_750m, Rate = 0.205m,  SortOrder = 2 },
            new() { BracketType = BracketType.Federal, UpperBound = 177_882m, Rate = 0.26m,   SortOrder = 3 },
            new() { BracketType = BracketType.Federal, UpperBound = 253_414m, Rate = 0.29m,   SortOrder = 4 },
            new() { BracketType = BracketType.Federal, UpperBound = null,     Rate = 0.33m,   SortOrder = 5 },
            new() { BracketType = BracketType.Quebec,  UpperBound =  53_255m, Rate = 0.14m,   SortOrder = 1 },
            new() { BracketType = BracketType.Quebec,  UpperBound = 106_495m, Rate = 0.19m,   SortOrder = 2 },
            new() { BracketType = BracketType.Quebec,  UpperBound = 129_590m, Rate = 0.24m,   SortOrder = 3 },
            new() { BracketType = BracketType.Quebec,  UpperBound = null,     Rate = 0.2575m, SortOrder = 4 },
        ]
    };

    public static TaxRateSet Rates2026 { get; } = new()
    {
        QppExemption = 3_500m, QppYmpe = 74_600m, QppYampe = 85_000m,
        QppBaseRate = 0.053m, QppAdditionalTier1Rate = 0.01m, QppTier1Rate = 0.063m, QppTier2Rate = 0.04m,
        QppTier1MaxEmployee = 4_479.30m, QppTier2MaxEmployee = 416.00m,
        EiEmployeeRate = 0.013m, EiEmployerMultiplier = 1.4m,
        EiMaxInsurableEarnings = 68_900m, EiMaxEmployeePremium = 895.70m,
        QpipEmployeeRate = 0.0043m, QpipEmployerRate = 0.00602m,
        QpipMaxInsurableEarnings = 103_000m, QpipMaxEmployeePremium = 442.90m, QpipMaxEmployerPremium = 620.06m,
        FederalBasicPersonalAmount = 16_452m, FederalEmploymentAmount = 1_501m,
        FederalLowestRate = 0.14m, QuebecFederalAbatement = 0.165m,
        QuebecBasicPersonalAmount = 18_952m, QuebecWorkerDeductionMax = 1_450m,
        QuebecWorkerDeductionRate = 0.06m, QuebecLowestRate = 0.14m,
        FssqSmallEmployerRate = 0.0165m, FssqLargeEmployerRate = 0.0426m,
        Brackets =
        [
            new() { BracketType = BracketType.Federal, UpperBound =  58_523m, Rate = 0.14m,   SortOrder = 1 },
            new() { BracketType = BracketType.Federal, UpperBound = 117_045m, Rate = 0.205m,  SortOrder = 2 },
            new() { BracketType = BracketType.Federal, UpperBound = 181_440m, Rate = 0.26m,   SortOrder = 3 },
            new() { BracketType = BracketType.Federal, UpperBound = 258_482m, Rate = 0.29m,   SortOrder = 4 },
            new() { BracketType = BracketType.Federal, UpperBound = null,     Rate = 0.33m,   SortOrder = 5 },
            new() { BracketType = BracketType.Quebec,  UpperBound =  54_345m, Rate = 0.14m,   SortOrder = 1 },
            new() { BracketType = BracketType.Quebec,  UpperBound = 108_680m, Rate = 0.19m,   SortOrder = 2 },
            new() { BracketType = BracketType.Quebec,  UpperBound = 132_245m, Rate = 0.24m,   SortOrder = 3 },
            new() { BracketType = BracketType.Quebec,  UpperBound = null,     Rate = 0.2575m, SortOrder = 4 },
        ]
    };
}
