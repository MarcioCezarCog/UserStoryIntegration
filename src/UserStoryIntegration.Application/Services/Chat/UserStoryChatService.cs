using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using UserStoryIntegration.Application.Interfaces;

namespace UserStoryIntegration.Application.Services.Chat
{
    /// <summary>
    /// Implementação de serviço de chat usando o Semantic Kernel
    /// </summary>
    public class UserStoryChatService : IUserStoryChatService
    {
        private readonly Kernel _kernel;
        private readonly string _systemPrompt;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="kernel">Instância do kernel</param>
        /// <param name="systemPrompt">Prompt de sistema</param>
        public UserStoryChatService(Kernel kernel, string systemPrompt)
        {
            _kernel = kernel;
            _systemPrompt = systemPrompt;
        }

        /// <inheritdoc />
        public async Task<object> GetCompletionAsync(ChatHistory chatHistory)
        {
            // Obter o serviço de chat do kernel
            var chatService = _kernel.GetRequiredService<Microsoft.SemanticKernel.ChatCompletion.IChatCompletionService>();

            // Adicionar o system prompt
            var historyWithSystemPrompt = new ChatHistory();
            historyWithSystemPrompt.AddSystemMessage(_systemPrompt);
            
            // Adicionar as mensagens do chat
            foreach (var message in chatHistory)
            {
                historyWithSystemPrompt.Add(message);
            }

            // Obter a resposta
            return await chatService.GetChatMessageContentAsync(historyWithSystemPrompt);
        }
    }
}
