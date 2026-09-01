using MediFlow.Domain.Interfaces;
using MediFlow.Infrastructure.Background;
using MediFlow.Infrastructure.Persistence;
using MediFlow.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediFlow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var useInMemory = configuration.GetValue<bool>("UseInMemoryDatabase", true);

        if (useInMemory)
        {
            services.AddDbContext<MediFlowDbContext>(options =>
                options.UseInMemoryDatabase("MediFlowClaimsDb"));
        }
        else
        {
            services.AddDbContext<MediFlowDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(MediFlowDbContext).Assembly.FullName)));
        }

        // Repositories & Unit of Work
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Background services
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
