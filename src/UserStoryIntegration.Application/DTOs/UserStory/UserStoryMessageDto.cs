namespace UserStoryIntegration.Application.DTOs.UserStory
{
    /// <summary>
    /// Representa uma mensagem no histórico de chat de user story
    /// </summary>
    public class UserStoryMessageDto
    {
        /// <summary>
        /// Papel do remetente (user ou assistant)
        /// </summary>
        public string Role { get; set; } = string.Empty;
        
        /// <summary>
        /// Conteúdo da mensagem
        /// </summary>
        public string Content { get; set; } = string.Empty;
        
        /// <summary>
        /// Data e hora da mensagem
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
