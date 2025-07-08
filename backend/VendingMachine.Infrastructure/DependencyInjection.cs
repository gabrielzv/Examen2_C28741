using Microsoft.Extensions.DependencyInjection;
using VendingMachine.Domain.Interfaces;
using VendingMachine.Infrastructure.Repositories;

namespace VendingMachine.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IProductRepository, InMemoryProductRepository>();
        
        return services;
    }
}
