using Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ResultEntity> Results => Set<ResultEntity>();

    public DbSet<ValueEntity> Values => Set<ValueEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
