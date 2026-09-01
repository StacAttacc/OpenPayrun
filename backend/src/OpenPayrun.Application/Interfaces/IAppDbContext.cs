using Microsoft.EntityFrameworkCore;
using ScsiTaxCalculator.Domain.Entities;

namespace ScsiTaxCalculator.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<TaxRateSet> TaxRateSets { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
