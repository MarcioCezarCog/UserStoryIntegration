using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;
using UserStoryIntegration.Application.Interfaces;

namespace UserStoryIntegration.Application.Services.Sessions
{
    /// <summary>
    /// Implementação em memória do gerenciador de sessões de chat
    /// </summary>
    public class InMemoryChatSessionManager : IChatSessionManager
    {
        private readonly ILogger<InMemoryChatSessionManager> _logger;
        private static readonly ConcurrentDictionary<string, ChatHistory> _sessionStore = new();

        /// <summary>
        /// Construtor
        /// </summary>
        public InMemoryChatSessionManager(ILogger<InMemoryChatSessionManager> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public ChatHistory GetOrCreateSession(string sessionId)
        {
            return _sessionStore.GetOrAdd(sessionId, _ => {
                _logger.LogInformation("Criando nova sessão com ID: {SessionId}", sessionId);
                return new ChatHistory();
            });
        }

        /// <inheritdoc/>
        public void SaveSession(string sessionId, ChatHistory chatHistory)
        {
            _sessionStore[sessionId] = chatHistory;
            _logger.LogDebug("Sessão salva: {SessionId} com {MessageCount} mensagens", 
                sessionId, chatHistory.Count);
        }

        /// <inheritdoc/>
        public bool SessionExists(string sessionId)
        {
            return _sessionStore.ContainsKey(sessionId);
        }

        /// <inheritdoc/>
        public bool RemoveSession(string sessionId)
        {
            var result = _sessionStore.TryRemove(sessionId, out _);
            if (result)
            {
                _logger.LogInformation("Sessão removida: {SessionId}", sessionId);
            }
            else
            {
                _logger.LogWarning("Falha ao remover sessão (possivelmente não existente): {SessionId}", sessionId);
            }
            
            return result;
        }
    }
}
