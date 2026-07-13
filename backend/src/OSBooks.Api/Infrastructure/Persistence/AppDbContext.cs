using Microsoft.EntityFrameworkCore;
using OSBooks.Api.Features.Employees;
using OSBooks.Api.Features.Payroll;

namespace OSBooks.Api.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayRun> PayRuns => Set<PayRun>();
}
