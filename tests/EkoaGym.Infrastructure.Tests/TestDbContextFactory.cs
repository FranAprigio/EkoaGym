using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Tests;

public static class TestDbContextFactory
{
    public static EkoaGymDbContext Create()
    {
        var options = new DbContextOptionsBuilder<EkoaGymDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new EkoaGymDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
