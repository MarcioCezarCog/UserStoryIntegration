using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;
using UserStoryIntegration.Application.DTOs.UserStory;
using UserStoryIntegration.Application.Interfaces;

namespace UserStoryIntegration.Application.Services
{
    /// <summary>
    /// Implementação do serviço de processamento de user stories
    /// </summary>
    public class UserStoryService : IUserStoryService
    {
        private readonly ILogger<UserStoryService> _logger;
        private readonly IConfiguration _configuration;
        private readonly AgentService _agentService;

        /// <summary>
        /// Armazenamento em memória de sessões de chat (em produção, isso seria um banco de dados)
        /// </summary>
        private static readonly ConcurrentDictionary<string, ChatHistory> _sessionStore = new();

        /// <summary>
        /// Construtor
        /// </summary>
        public UserStoryService(
            ILogger<UserStoryService> logger,
            IConfiguration configuration,
            AgentService agentService)
        {
            _logger = logger;
            _configuration = configuration;
            _agentService = agentService;
        }

        /// <inheritdoc/>
        public async Task<UserStoryChatResponse> ProcessMessageAsync(UserStoryChatRequest request)
        {
            try
            {
                _logger.LogInformation("Processando mensagem: {Message}", request.Message);

                // Obter ou criar sessão
                string sessionId = request.SessionId ?? Guid.NewGuid().ToString();
                var chatHistory = GetOrCreateSession(sessionId);

                // Adicionar mensagem do usuário
                chatHistory.AddUserMessage(request.Message);

                // Processar com o agente PO Expert
                var poResponse = await _agentService.ProcessWithPOExpertAsync(chatHistory);
                chatHistory.Add(poResponse);

                // Verificar se a user story está completa
                bool isComplete = await CheckIfStoryIsComplete(chatHistory);

                // Se estiver completa, gerar tarefas sugeridas
                if (isComplete)
                {
                    await GenerateSuggestedTasks(chatHistory);
                }

                // Atualizar a sessão
                _sessionStore[sessionId] = chatHistory;

                // Converter para resposta
                var response = chatHistory.ToResponse(sessionId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem");
                
                return new UserStoryChatResponse
                {
                    Message = "Ocorreu um erro ao processar sua mensagem. Por favor, tente novamente.",
                    SessionId = request.SessionId ?? Guid.NewGuid().ToString()
                };
            }
        }

        /// <summary>
        /// Obtém ou cria uma sessão de chat
        /// </summary>
        private ChatHistory GetOrCreateSession(string sessionId)
        {
            if (_sessionStore.TryGetValue(sessionId, out var existingSession))
            {
                return existingSession;
            }

            var newSession = new ChatHistory();
            _sessionStore[sessionId] = newSession;
            return newSession;
        }

        /// <summary>
        /// Verifica se a user story está completa e bem formada
        /// </summary>
        private async Task<bool> CheckIfStoryIsComplete(ChatHistory chatHistory)
        {
            try
            {
                // Clonar o histórico para não modificar o original
                var reviewChatHistory = new ChatHistory([.. chatHistory]);
                
                // Adicionar a pergunta específica para o revisor
                reviewChatHistory.AddUserMessage("A US está clara, eficaz e no formato adequado? true or false?");
                
                // Processar com o agente revisor
                var reviewerResponse = await _agentService.ProcessWithReviewerAsync(reviewChatHistory);
                
                // Analisar a resposta (TRUE ou FALSE)
                bool isComplete = false;
                if (bool.TryParse(reviewerResponse.Content, out bool result))
                {
                    isComplete = result;
                }
                
                _logger.LogInformation("User story completa: {IsComplete}", isComplete);
                
                return isComplete;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se a user story está completa");
                return false;
            }
        }

        /// <summary>
        /// Gera tarefas sugeridas para a user story
        /// </summary>
        private async Task GenerateSuggestedTasks(ChatHistory chatHistory)
        {
            try
            {
                // Clonar o histórico para não modificar o original
                var taskChatHistory = new ChatHistory([.. chatHistory]);
                
                // Adicionar a pergunta específica para o gerador de tarefas
                taskChatHistory.AddUserMessage("Quais atividades sugere para a US em questão?");
                
                // Processar com o agente de tarefas
                var taskResponse = await _agentService.ProcessWithTaskOverviewAsync(taskChatHistory);
                
                // Adicionar a resposta ao histórico original
                chatHistory.Add(taskResponse);
                
                _logger.LogInformation("Tarefas sugeridas geradas com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar tarefas sugeridas");
            }
        }
    }
}
