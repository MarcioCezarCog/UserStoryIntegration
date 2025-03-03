using Microsoft.SemanticKernel;
using UserStoryIntegration.Application.Interfaces;

namespace UserStoryIntegration.Application.Services.Agents.Factory
{
    /// <summary>
    /// Interface para fábrica de agentes de chat
    /// </summary>
    public interface IAgentFactory
    {
        /// <summary>
        /// Cria um serviço de chat
        /// </summary>
        /// <param name="kernel">Instância do kernel</param>
        /// <returns>Serviço de chat</returns>
        IUserStoryChatService CreateChatService(Kernel kernel);
    }
}
