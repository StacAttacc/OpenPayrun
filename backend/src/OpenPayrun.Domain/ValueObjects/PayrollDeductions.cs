namespace OpenPayrun.Domain.ValueObjects;

public record PayrollDeductions
{
    public decimal GrossPay { get; init; }

    // Employee deductions
    public decimal QppTier1 { get; init; }
    public decimal QppTier2 { get; init; }
    public decimal EiPremium { get; init; }
    public decimal QpipPremium { get; init; }
    public decimal FederalIncomeTax { get; init; }
    public decimal QuebecIncomeTax { get; init; }

    // Employer contributions
    public decimal EmployerQppTier1 { get; init; }
    public decimal EmployerQppTier2 { get; init; }
    public decimal EmployerEi { get; init; }
    public decimal EmployerQpip { get; init; }
    public decimal EmployerFssq { get; init; }

    public decimal TotalEmployeeDeductions =>
        QppTier1 + QppTier2 + EiPremium + QpipPremium + FederalIncomeTax + QuebecIncomeTax;

    public decimal NetPay => GrossPay - TotalEmployeeDeductions;

    public decimal TotalEmployerContributions =>
        EmployerQppTier1 + EmployerQppTier2 + EmployerEi + EmployerQpip + EmployerFssq;
}
