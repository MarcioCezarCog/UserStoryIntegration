using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;

namespace UserStoryIntegration.Application.Interfaces
{
    /// <summary>
    /// Interface para serviços de chat de user story
    /// </summary>
    public interface IUserStoryChatService
    {
        /// <summary>
        /// Processa um histórico de chat e gera uma resposta
        /// </summary>
        /// <param name="chatHistory">Histórico do chat</param>
        /// <returns>Resposta gerada</returns>
        Task<object> GetCompletionAsync(ChatHistory chatHistory);
    }
}
