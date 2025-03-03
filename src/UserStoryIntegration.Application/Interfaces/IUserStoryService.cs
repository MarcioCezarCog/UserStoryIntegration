using UserStoryIntegration.Application.DTOs.UserStory;

namespace UserStoryIntegration.Application.Interfaces
{
    /// <summary>
    /// Interface do serviço de processamento de user stories
    /// </summary>
    public interface IUserStoryService
    {
        /// <summary>
        /// Processa uma mensagem de chat para o assistente de user story
        /// </summary>
        /// <param name="request">Requisição contendo a mensagem</param>
        /// <returns>Resposta do assistente</returns>
        Task<UserStoryChatResponse> ProcessMessageAsync(UserStoryChatRequest request);
    }
}
