using Microsoft.Extensions.DependencyInjection;

namespace UserStoryIntegration.Application;

/// <summary>
/// Extensões para registrar serviços da camada de aplicação
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona serviços da camada de aplicação
    /// </summary>
    /// <param name="services">Container de serviços</param>
    /// <returns>Container de serviços</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Será implementado posteriormente
        return services;
    }
}
