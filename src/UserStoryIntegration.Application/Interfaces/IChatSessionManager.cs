using Microsoft.SemanticKernel.ChatCompletion;

namespace UserStoryIntegration.Application.Interfaces
{
    /// <summary>
    /// Interface para gerenciamento de sessões de chat
    /// </summary>
    public interface IChatSessionManager
    {
        /// <summary>
        /// Obtém ou cria uma sessão de chat
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <returns>Histórico de chat da sessão</returns>
        ChatHistory GetOrCreateSession(string sessionId);
        
        /// <summary>
        /// Salva uma sessão de chat
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <param name="chatHistory">Histórico de chat a ser salvo</param>
        void SaveSession(string sessionId, ChatHistory chatHistory);
        
        /// <summary>
        /// Verifica se uma sessão existe
        /// </summary>
        /// <param name="sessionId">ID da sessão</param>
        /// <returns>True se a sessão existe</returns>
        bool SessionExists(string sessionId);
        
        /// <summary>
        /// Remove uma sessão
        /// </summary>
        /// <param name="sessionId">ID da sessão a ser removida</param>
        /// <returns>True se a operação foi bem-sucedida</returns>
        bool RemoveSession(string sessionId);
    }
}
