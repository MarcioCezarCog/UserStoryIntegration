using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UserStoryIntegration.Infrastructure;

/// <summary>
/// Extensões para registrar serviços da camada de infraestrutura
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona serviços da camada de infraestrutura
    /// </summary>
    /// <param name="services">Container de serviços</param>
    /// <param name="configuration">Configuração da aplicação</param>
    /// <returns>Container de serviços</returns>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Será implementado posteriormente
        return services;
    }
}
