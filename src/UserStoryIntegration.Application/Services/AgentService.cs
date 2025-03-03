using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using UserStoryIntegration.Application.Interfaces;
using UserStoryIntegration.Application.Services.Agents.Factory;

namespace UserStoryIntegration.Application.Services
{
    /// <summary>
    /// Serviço para gerenciamento de agentes
    /// </summary>
    public class AgentService
    {
        private readonly IKernelService _kernelService;
        private readonly ILogger<AgentService> _logger;
        private readonly POExpertAgentFactory _poExpertFactory;
        private readonly ReviewerAgentFactory _reviewerFactory;
        private readonly TaskOverviewAgentFactory _taskOverviewFactory;

        /// <summary>
        /// Construtor
        /// </summary>
        public AgentService(
            IKernelService kernelService,
            ILogger<AgentService> logger,
            POExpertAgentFactory poExpertFactory,
            ReviewerAgentFactory reviewerFactory,
            TaskOverviewAgentFactory taskOverviewFactory)
        {
            _kernelService = kernelService;
            _logger = logger;
            _poExpertFactory = poExpertFactory;
            _reviewerFactory = reviewerFactory;
            _taskOverviewFactory = taskOverviewFactory;
        }

        /// <summary>
        /// Cria um serviço de chat para Product Owner Expert
        /// </summary>
        /// <returns>Serviço de chat</returns>
        public IUserStoryChatService CreatePOExpertChatService()
        {
            Kernel kernel = _kernelService.CreateKernel();
            return _poExpertFactory.CreateChatService(kernel);
        }

        /// <summary>
        /// Cria um serviço de chat para Reviewer
        /// </summary>
        /// <returns>Serviço de chat</returns>
        public IUserStoryChatService CreateReviewerChatService()
        {
            Kernel kernel = _kernelService.CreateKernel();
            return _reviewerFactory.CreateChatService(kernel);
        }

        /// <summary>
        /// Cria um serviço de chat para Task Overview
        /// </summary>
        /// <returns>Serviço de chat</returns>
        public IUserStoryChatService CreateTaskOverviewChatService()
        {
            Kernel kernel = _kernelService.CreateKernel();
            return _taskOverviewFactory.CreateChatService(kernel);
        }

        /// <summary>
        /// Processa uma mensagem com o agente Product Owner
        /// </summary>
        /// <param name="chatHistory">Histórico do chat</param>
        /// <returns>Resposta do agente</returns>
        public async Task<ChatMessageContent> ProcessWithPOExpertAsync(ChatHistory chatHistory)
        {
            try
            {
                var chatService = CreatePOExpertChatService();
                var result = await chatService.GetCompletionAsync(chatHistory);
                return (ChatMessageContent)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem com agente PO Expert");
                throw;
            }
        }

        /// <summary>
        /// Processa uma mensagem com o agente Reviewer
        /// </summary>
        /// <param name="chatHistory">Histórico do chat</param>
        /// <returns>Resposta do agente</returns>
        public async Task<ChatMessageContent> ProcessWithReviewerAsync(ChatHistory chatHistory)
        {
            try
            {
                var chatService = CreateReviewerChatService();
                var result = await chatService.GetCompletionAsync(chatHistory);
                return (ChatMessageContent)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem com agente Reviewer");
                throw;
            }
        }

        /// <summary>
        /// Processa uma mensagem com o agente Task Overview
        /// </summary>
        /// <param name="chatHistory">Histórico do chat</param>
        /// <returns>Resposta do agente</returns>
        public async Task<ChatMessageContent> ProcessWithTaskOverviewAsync(ChatHistory chatHistory)
        {
            try
            {
                var chatService = CreateTaskOverviewChatService();
                var result = await chatService.GetCompletionAsync(chatHistory);
                return (ChatMessageContent)result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem com agente Task Overview");
                throw;
            }
        }
    }
}
