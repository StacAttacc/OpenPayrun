using Microsoft.EntityFrameworkCore;
using OpenPayrun.Domain.Entities;

namespace OpenPayrun.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<TaxRateSet> TaxRateSets { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
