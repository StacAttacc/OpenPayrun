using Microsoft.EntityFrameworkCore;
using OSBooks.Application.Interfaces;
using OSBooks.Domain.Entities;

namespace OSBooks.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<PayRun> PayRuns => Set<PayRun>();
}
