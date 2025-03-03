using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace UserStoryIntegration.Application.Steps
{
    public sealed class IdentifyPersonaStep
    {
        private const string APIKey = "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";

        public static async Task<ChatHistory> InvokeAsync(IConfiguration configuration, ChatHistory chatMessages)
        {
            try
            {
                // Criar o kernel com a API atualizada do Semantic Kernel
                var builder = Kernel.CreateBuilder();
                
                // Configurar o cliente OpenAI com endpoint personalizado
                var openAIOptions = new OpenAIClientOptions
                {
                    ApiKey = APIKey,
                    Organization = null,
                    BaseUri = new Uri("https://api.groq.com/openai/v1")
                };
                
                // Adicionar chat completion com a sintaxe correta e opções
                builder.AddOpenAIChatCompletion(
                    modelId: "llama-3.3-70b-versatile", 
                    options: openAIOptions);
                
                Kernel kernel = builder.Build();
                
                var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
                
                // Processar com o agente PO Expert
                string poExpertInstructions = """
                    Você é um assistente de inteligência artificial especializado em ajudar Product Owners (POs) a criar estórias de usuários claras e eficazes.
                    Sua missão é guiar o PO por meio de uma série de perguntas que os ajudarão a definir e detalhar estórias de usuários que atendam aos requisitos do projeto e sejam úteis para a equipe de desenvolvimento.
                    Se identificar que o usuário está falando sobre qualquer outro assunto fora do contexto, não responda a questão, negue de forma respeitosa e o traga para o contexto
                    """;

                var chatHistoryWithSystem = new ChatHistory();
                chatHistoryWithSystem.AddSystemMessage(poExpertInstructions);
                foreach (var message in chatMessages)
                {
                    chatHistoryWithSystem.Add(message);
                }

                ChatMessageContent response = await chatCompletionService.GetChatMessageContentAsync(chatHistoryWithSystem);

                // Reviewer
                string reviewerInstructions = """
                    Regras que você deverá seguir:
                    1 - Você deve responder, se e somente, TRUE ou FALSE
                    2 - Quando uma user story é clara, eficaz e está no formato adequado, responda TRUE. 
                    3 - Em qualquer outro cenário responda FALSE
                    """;

                var reviewChatHistory = new ChatHistory();
                reviewChatHistory.AddSystemMessage(reviewerInstructions);
                foreach (var message in chatMessages)
                {
                    reviewChatHistory.Add(message);
                }
                reviewChatHistory.AddUserMessage("A US está clara, eficaz e no formato adequado? true or false?");

                bool isReviewer = false;
                ChatMessageContent agentReviewerResponse;
                agentReviewerResponse = await chatCompletionService.GetChatMessageContentAsync(reviewChatHistory);
                if (bool.TryParse(agentReviewerResponse.ToString(), out isReviewer) && isReviewer)
                {
                    // Arquiteto
                    string taskOverviewInstructions = """
                        Sua missão é listar as atividades necessárias, com base em uma estória de usuário.
                        Responda em formato de tabela com as colunas: Discipline, Task, Effort Hrs
                        Você deve responder sempre em fromato de tabela (Discipline, Task, Effort Hrs)
                        """;
                    
                    var taskChatHistory = new ChatHistory();
                    taskChatHistory.AddSystemMessage(taskOverviewInstructions);
                    foreach (var message in chatMessages)
                    {
                        taskChatHistory.Add(message);
                    }
                    taskChatHistory.AddUserMessage("Quais atividades sugere para a US em questão?");
                    
                    var resultArchitect = await chatCompletionService.GetChatMessageContentAsync(taskChatHistory);
                    chatMessages.Add(resultArchitect);
                }

                chatMessages.Add(response);

                return chatMessages;
            }
            catch (Exception)
            {
                // Apenas registrar a exceção sem expô-la
                return chatMessages;
            }
        }
    }
}
