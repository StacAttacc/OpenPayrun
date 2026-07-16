using Microsoft.EntityFrameworkCore;
using OpenPayrun.Application.Interfaces;
using OpenPayrun.Domain.Entities;

namespace OpenPayrun.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayRun> PayRuns => Set<PayRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .Property(e => e.HourlyRate).HasPrecision(10, 2);

        modelBuilder.Entity<PayRun>(b =>
        {
            b.Property(p => p.GrossPay).HasPrecision(10, 2);
            b.Property(p => p.EmployeeTax).HasPrecision(10, 2);
            b.Property(p => p.EmployerContributions).HasPrecision(10, 2);
            b.Property(p => p.NetPay).HasPrecision(10, 2);
        });
    }
}
