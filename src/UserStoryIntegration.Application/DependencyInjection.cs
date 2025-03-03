using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using UserStoryIntegration.Application.Interfaces;
using UserStoryIntegration.Application.Services;
using UserStoryIntegration.Application.Services.Agents.Factory;

namespace UserStoryIntegration.Application
{
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
            // Registrar fábricas de agentes
            services.AddSingleton<POExpertAgentFactory>();
            services.AddSingleton<ReviewerAgentFactory>();
            services.AddSingleton<TaskOverviewAgentFactory>();
            
            // Registrar serviços
            services.AddSingleton<IKernelService, KernelService>();
            services.AddSingleton<AgentService>();
            services.AddScoped<IUserStoryService, UserStoryService>();
            
            return services;
        }
    }
}
