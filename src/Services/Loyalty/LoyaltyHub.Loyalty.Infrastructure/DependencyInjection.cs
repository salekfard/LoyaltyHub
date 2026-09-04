using LoyaltyHub.Loyalty.Application.Interfaces;
using LoyaltyHub.Loyalty.Infrastructure.Persistence;
using LoyaltyHub.Loyalty.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltyHub.Loyalty.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("LoyaltyDb")
            ?? throw new InvalidOperationException("Connection string 'LoyaltyDb' is not configured.");

        services.AddDbContext<LoyaltyDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IScoreCalculationRepository, ScoreCalculationRepository>();
        services.AddSingleton<LoyaltyDbInitializer>();

        return services;
    }
}
