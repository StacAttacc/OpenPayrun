namespace OpenPayrun.Domain.Constants;

public interface ITaxRates
{
    decimal QppExemption { get; }
    decimal QppYmpe { get; }
    decimal QppYampe { get; }
    decimal QppBaseRate { get; }
    decimal QppAdditionalTier1Rate { get; }
    decimal QppTier1Rate { get; }
    decimal QppTier2Rate { get; }
    decimal QppTier1MaxEmployee { get; }
    decimal QppTier2MaxEmployee { get; }

    decimal EiEmployeeRate { get; }
    decimal EiEmployerMultiplier { get; }
    decimal EiMaxInsurableEarnings { get; }
    decimal EiMaxEmployeePremium { get; }

    decimal QpipEmployeeRate { get; }
    decimal QpipEmployerRate { get; }
    decimal QpipMaxInsurableEarnings { get; }
    decimal QpipMaxEmployeePremium { get; }
    decimal QpipMaxEmployerPremium { get; }

    (decimal UpperBound, decimal Rate)[] FederalBrackets { get; }
    decimal FederalBasicPersonalAmount { get; }
    decimal FederalEmploymentAmount { get; }
    decimal FederalLowestRate { get; }
    decimal QuebecFederalAbatement { get; }

    (decimal UpperBound, decimal Rate)[] QuebecBrackets { get; }
    decimal QuebecBasicPersonalAmount { get; }
    decimal QuebecWorkerDeductionMax { get; }
    decimal QuebecWorkerDeductionRate { get; }
    decimal QuebecLowestRate { get; }

    decimal FssqSmallEmployerRate { get; }
    decimal FssqLargeEmployerRate { get; }
}
