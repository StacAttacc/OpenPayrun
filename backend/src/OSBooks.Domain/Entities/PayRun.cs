namespace OSBooks.Domain.Entities;

public class PayRun
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public decimal GrossPay { get; set; }
    public decimal EmployeeTax { get; set; }
    public decimal EmployerContributions { get; set; }
    public decimal NetPay { get; set; }
}
