using Microsoft.SemanticKernel.ChatCompletion;

namespace UserStoryIntegration.Application.DTOs.UserStory
{
    /// <summary>
    /// Representa uma requisição para o assistente de user story
    /// </summary>
    public class UserStoryChatRequest
    {
        /// <summary>
        /// Mensagem enviada pelo usuário
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// ID da sessão para manter o histórico da conversa
        /// </summary>
        public string? SessionId { get; set; }
    }

    /// <summary>
    /// Representa uma resposta do assistente de user story
    /// </summary>
    public class UserStoryChatResponse
    {
        /// <summary>
        /// Mensagem de resposta do assistente
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// ID da sessão para futuras interações
        /// </summary>
        public string SessionId { get; set; } = string.Empty;
        
        /// <summary>
        /// User story resultante (se houver)
        /// </summary>
        public UserStoryResult? UserStory { get; set; }
    }

    /// <summary>
    /// Resultado da user story gerada
    /// </summary>
    public class UserStoryResult
    {
        /// <summary>
        /// Texto completo da user story
        /// </summary>
        public string StoryText { get; set; } = string.Empty;
        
        /// <summary>
        /// Lista de critérios de aceitação
        /// </summary>
        public List<string> AcceptanceCriteria { get; set; } = new List<string>();
        
        /// <summary>
        /// Lista de atividades sugeridas
        /// </summary>
        public List<TaskItem>? SuggestedTasks { get; set; }
        
        /// <summary>
        /// Status da review
        /// </summary>
        public bool Reviewed { get; set; }
    }

    /// <summary>
    /// Item de tarefa sugerida
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// Disciplina ou categoria da tarefa
        /// </summary>
        public string Discipline { get; set; } = string.Empty;
        
        /// <summary>
        /// Descrição da tarefa
        /// </summary>
        public string Task { get; set; } = string.Empty;
        
        /// <summary>
        /// Estimativa de esforço em horas
        /// </summary>
        public string EffortHrs { get; set; } = string.Empty;
    }

    /// <summary>
    /// Utilidades para converter entre ChatHistory e o modelo de aplicação
    /// </summary>
    public static class ChatModelExtensions
    {
        /// <summary>
        /// Converte uma mensagem de texto para um ChatHistory
        /// </summary>
        public static ChatHistory ToChatHistory(this string message, string? sessionId = null)
        {
            var history = new ChatHistory();
            history.AddUserMessage(message);
            return history;
        }

        /// <summary>
        /// Extrai uma resposta a partir de um ChatHistory
        /// </summary>
        public static UserStoryChatResponse ToResponse(this ChatHistory chatHistory, string sessionId)
        {
            // Pegar a última mensagem do assistente
            var lastAssistantMessage = chatHistory
                .Where(m => m.Role.ToString().Equals("assistant", StringComparison.OrdinalIgnoreCase))
                .LastOrDefault();

            if (lastAssistantMessage == null)
            {
                return new UserStoryChatResponse
                {
                    Message = "Não foi possível gerar uma resposta.",
                    SessionId = sessionId
                };
            }

            // Analisar se há uma user story bem formada
            var userStoryResult = ExtractUserStory(chatHistory);

            return new UserStoryChatResponse
            {
                Message = lastAssistantMessage.Content ?? string.Empty,
                SessionId = sessionId,
                UserStory = userStoryResult
            };
        }

        /// <summary>
        /// Tenta extrair uma user story formatada do histórico de chat
        /// </summary>
        private static UserStoryResult? ExtractUserStory(ChatHistory chatHistory)
        {
            var messages = chatHistory.ToList();
            
            // Verifica se existe uma mensagem que contém uma user story no formato adequado
            var userStoryMessage = messages
                .Where(m => m.Role.ToString().Equals("assistant", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault(m => 
                    m.Content != null && 
                    m.Content.Contains("Como ") && 
                    m.Content.Contains(", eu quero ") && 
                    m.Content.Contains(" para "));

            if (userStoryMessage == null || string.IsNullOrEmpty(userStoryMessage.Content))
                return null;

            // Verificar se há uma mensagem sobre critérios de aceitação
            var acceptanceCriteriaMessages = messages
                .Where(m => m.Role.ToString().Equals("assistant", StringComparison.OrdinalIgnoreCase) && 
                       m.Content != null &&
                       m.Content.Contains("Critérios de Aceitação:"))
                .ToList();

            // Verificar se há tarefas sugeridas (tabela)
            var taskMessage = messages
                .Where(m => m.Role.ToString().Equals("assistant", StringComparison.OrdinalIgnoreCase) && 
                       m.Content != null &&
                       m.Content.Contains("Discipline") && 
                       m.Content.Contains("Task") && 
                       m.Content.Contains("Effort"))
                .LastOrDefault();

            var result = new UserStoryResult
            {
                StoryText = userStoryMessage.Content ?? string.Empty,
                Reviewed = messages.Any(m => m.Content != null && m.Content.Contains("TRUE"))
            };

            // Extrair critérios de aceitação
            if (acceptanceCriteriaMessages.Any())
            {
                var criteriaText = acceptanceCriteriaMessages.Last().Content ?? string.Empty;
                var startIndex = criteriaText.IndexOf("Critérios de Aceitação:");
                
                if (startIndex >= 0)
                {
                    var criteria = criteriaText.Substring(startIndex)
                        .Split('\n')
                        .Where(line => !string.IsNullOrWhiteSpace(line) && 
                                     !line.Equals("Critérios de Aceitação:", StringComparison.OrdinalIgnoreCase))
                        .Select(line => line.Trim())
                        .ToList();

                    result.AcceptanceCriteria = criteria;
                }
            }

            // Extrair tarefas sugeridas
            if (taskMessage != null && taskMessage.Content != null)
            {
                result.SuggestedTasks = new List<TaskItem>();
                var lines = taskMessage.Content.Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line) && 
                                 !line.Contains("Discipline") && // Excluir cabeçalho
                                 line.Contains("|"))             // Deve ser linha de tabela
                    .ToList();

                foreach (var line in lines)
                {
                    var columns = line.Split('|')
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .ToArray();

                    if (columns.Length >= 3)
                    {
                        result.SuggestedTasks.Add(new TaskItem
                        {
                            Discipline = columns[0],
                            Task = columns[1],
                            EffortHrs = columns[2]
                        });
                    }
                }
            }

            return result;
        }
    }
}
