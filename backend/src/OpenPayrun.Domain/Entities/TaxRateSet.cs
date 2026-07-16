using System.ComponentModel.DataAnnotations.Schema;
using OpenPayrun.Domain.Constants;
using OpenPayrun.Domain.Enums;

namespace OpenPayrun.Domain.Entities;

public class TaxRateSet : ITaxRates
{
    public int Id { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }

    // QPP
    public decimal QppExemption { get; set; }
    public decimal QppYmpe { get; set; }
    public decimal QppYampe { get; set; }
    public decimal QppBaseRate { get; set; }
    public decimal QppAdditionalTier1Rate { get; set; }
    public decimal QppTier1Rate { get; set; }
    public decimal QppTier2Rate { get; set; }
    public decimal QppTier1MaxEmployee { get; set; }
    public decimal QppTier2MaxEmployee { get; set; }

    // EI
    public decimal EiEmployeeRate { get; set; }
    public decimal EiEmployerMultiplier { get; set; }
    public decimal EiMaxInsurableEarnings { get; set; }
    public decimal EiMaxEmployeePremium { get; set; }

    // QPIP
    public decimal QpipEmployeeRate { get; set; }
    public decimal QpipEmployerRate { get; set; }
    public decimal QpipMaxInsurableEarnings { get; set; }
    public decimal QpipMaxEmployeePremium { get; set; }
    public decimal QpipMaxEmployerPremium { get; set; }

    // Federal
    public decimal FederalBasicPersonalAmount { get; set; }
    public decimal FederalEmploymentAmount { get; set; }
    public decimal FederalLowestRate { get; set; }
    public decimal QuebecFederalAbatement { get; set; }

    // Quebec
    public decimal QuebecBasicPersonalAmount { get; set; }
    public decimal QuebecWorkerDeductionMax { get; set; }
    public decimal QuebecWorkerDeductionRate { get; set; }
    public decimal QuebecLowestRate { get; set; }

    // FSSQ
    public decimal FssqSmallEmployerRate { get; set; }
    public decimal FssqLargeEmployerRate { get; set; }

    public List<TaxBracket> Brackets { get; set; } = [];

    [NotMapped]
    public (decimal UpperBound, decimal Rate)[] FederalBrackets =>
        Brackets.Where(b => b.BracketType == BracketType.Federal)
                .OrderBy(b => b.SortOrder)
                .Select(b => (b.UpperBound ?? decimal.MaxValue, b.Rate))
                .ToArray();

    [NotMapped]
    public (decimal UpperBound, decimal Rate)[] QuebecBrackets =>
        Brackets.Where(b => b.BracketType == BracketType.Quebec)
                .OrderBy(b => b.SortOrder)
                .Select(b => (b.UpperBound ?? decimal.MaxValue, b.Rate))
                .ToArray();
}
