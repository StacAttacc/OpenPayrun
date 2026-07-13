using Microsoft.EntityFrameworkCore;
using OSBooks.Domain.Entities;

namespace OSBooks.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<PayRun> PayRuns { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
