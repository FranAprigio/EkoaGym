using EkoaGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Persistence;

public class EkoaGymDbContext(DbContextOptions<EkoaGymDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(u => u.Email).IsUnique();
        });
    }
}
