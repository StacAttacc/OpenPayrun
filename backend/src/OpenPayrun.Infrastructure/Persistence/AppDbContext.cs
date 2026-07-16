using Microsoft.EntityFrameworkCore;
using OpenPayrun.Application.Interfaces;
using OpenPayrun.Domain.Entities;

namespace OpenPayrun.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayRun> PayRuns => Set<PayRun>();
    public DbSet<TaxRateSet> TaxRateSets => Set<TaxRateSet>();

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

        modelBuilder.Entity<TaxRateSet>(b =>
        {
            b.HasIndex(r => r.EffectiveFrom);
            foreach (var name in new[]
            {
                nameof(TaxRateSet.QppExemption), nameof(TaxRateSet.QppYmpe), nameof(TaxRateSet.QppYampe),
                nameof(TaxRateSet.QppTier1MaxEmployee), nameof(TaxRateSet.QppTier2MaxEmployee),
                nameof(TaxRateSet.EiMaxInsurableEarnings), nameof(TaxRateSet.EiMaxEmployeePremium),
                nameof(TaxRateSet.QpipMaxInsurableEarnings), nameof(TaxRateSet.QpipMaxEmployeePremium),
                nameof(TaxRateSet.QpipMaxEmployerPremium), nameof(TaxRateSet.FederalBasicPersonalAmount),
                nameof(TaxRateSet.FederalEmploymentAmount), nameof(TaxRateSet.QuebecBasicPersonalAmount),
                nameof(TaxRateSet.QuebecWorkerDeductionMax),
            })
                b.Property(name).HasPrecision(12, 2);

            foreach (var name in new[]
            {
                nameof(TaxRateSet.QppBaseRate), nameof(TaxRateSet.QppAdditionalTier1Rate),
                nameof(TaxRateSet.QppTier1Rate), nameof(TaxRateSet.QppTier2Rate),
                nameof(TaxRateSet.EiEmployeeRate), nameof(TaxRateSet.EiEmployerMultiplier),
                nameof(TaxRateSet.QpipEmployeeRate), nameof(TaxRateSet.QpipEmployerRate),
                nameof(TaxRateSet.FederalLowestRate), nameof(TaxRateSet.QuebecFederalAbatement),
                nameof(TaxRateSet.QuebecWorkerDeductionRate), nameof(TaxRateSet.QuebecLowestRate),
                nameof(TaxRateSet.FssqSmallEmployerRate), nameof(TaxRateSet.FssqLargeEmployerRate),
            })
                b.Property(name).HasPrecision(10, 6);

            b.HasMany(r => r.Brackets).WithOne(br => br.TaxRateSet).HasForeignKey(br => br.TaxRateSetId);
        });

        modelBuilder.Entity<TaxBracket>(b =>
        {
            b.Property(br => br.UpperBound).HasPrecision(20, 4);
            b.Property(br => br.Rate).HasPrecision(10, 6);
        });
    }
}
