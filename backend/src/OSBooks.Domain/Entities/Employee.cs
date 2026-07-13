namespace OSBooks.Domain.Entities;

public class Employee
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public decimal HourlyRate { get; set; }
    public DateTime StartDate { get; set; }
}
