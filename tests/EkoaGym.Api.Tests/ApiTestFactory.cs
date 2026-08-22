using EkoaGym.Application.Auth;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace EkoaGym.Api.Tests;

// Swaps the real SQL Server DbContext for a fresh, isolated InMemory database and supplies
// deterministic Jwt/Progression config so tests don't depend on a developer's local appsettings/.env.
public class ApiTestFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"api-tests-{Guid.NewGuid()}";

    // Shared across every request in this factory's client(s) — lets tests read back reset tokens that
    // would otherwise only be delivered via the real (log-only) IEmailSender implementation.
    public TestEmailSender EmailSender { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKey"] = "integration-test-signing-key-at-least-32-characters-long",
                ["Jwt:Issuer"] = "EkoaGym.Tests",
                ["Jwt:Audience"] = "EkoaGym.Tests",
                ["Jwt:AccessTokenExpiryMinutes"] = "60",
                ["Jwt:RefreshTokenExpiryDays"] = "30",
                ["Progression:WeightIncrementKg"] = "2.5",
                ["ConnectionStrings:DefaultConnection"] = "unused-replaced-by-inmemory-provider"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<EkoaGymDbContext>>();

            // A dedicated internal service provider keeps EF Core from seeing both SqlServer (registered by
            // the app's own Program.cs) and InMemory provider services in the same root container, which
            // otherwise throws "Only a single database provider can be registered in a service provider."
            var inMemoryProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();
            services.AddDbContext<EkoaGymDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.UseInternalServiceProvider(inMemoryProvider);
            });

            services.RemoveAll<IEmailSender>();
            services.AddSingleton<IEmailSender>(EmailSender);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // InMemory has no migrations to run; EnsureCreated() builds the schema and applies HasData seeds.
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<EkoaGymDbContext>().Database.EnsureCreated();

        return host;
    }
}
