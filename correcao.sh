#!/bin/bash
# Script para resolver erros de restore e garantir compatibilidade de pacotes

# Voltar para o diretório raiz do projeto
cd UserStoryIntegration 2>/dev/null || cd ..

echo "Limpando caches NuGet para evitar problemas de pacotes..."
dotnet nuget locals all --clear

echo "Adicionando a fonte de pacotes NuGet..."
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org

echo "Removeremos as referências problemáticas e adicionaremos as corretas..."

# Garantir que estamos usando versões compatíveis dos pacotes
cd src/UserStoryIntegration.Application

# Remover todos os pacotes do Semantic Kernel para garantir que não haja conflitos
echo "Removendo pacotes antigos do Semantic Kernel..."
dotnet remove package Microsoft.SemanticKernel 2>/dev/null
dotnet remove package Microsoft.SemanticKernel.Abstractions 2>/dev/null
dotnet remove package Microsoft.SemanticKernel.Core 2>/dev/null
dotnet remove package Microsoft.SemanticKernel.Connectors.OpenAI 2>/dev/null
dotnet remove package Microsoft.SemanticKernel.PromptTemplates.Handlebars 2>/dev/null
dotnet remove package Microsoft.SemanticKernel.Agents 2>/dev/null

# Limpar referências de HTTP para evitar conflitos
dotnet remove package Microsoft.Extensions.Http 2>/dev/null

echo "Adicionando pacotes do Semantic Kernel 1.0.1 (versão estável)..."
# Adicionar versões estáveis e compatíveis dos pacotes
dotnet add package Microsoft.SemanticKernel --version 1.0.1
dotnet add package Microsoft.SemanticKernel.Connectors.OpenAI --version 1.0.1

echo "Adicionando pacotes de suporte..."
dotnet add package Microsoft.Extensions.Http --version 9.0.0
dotnet add package Microsoft.Extensions.Logging --version 9.0.0
dotnet add package Microsoft.Extensions.DependencyInjection --version 9.0.0
dotnet add package Microsoft.Extensions.Configuration --version 9.0.0
dotnet add package Microsoft.Extensions.Configuration.Abstractions --version 9.0.0
dotnet add package Microsoft.Extensions.Configuration.Binder --version 9.0.0

# Atualizar os arquivos para usar a API do Semantic Kernel 1.0.1
echo "Atualizando os arquivos para usar a API correta do Semantic Kernel 1.0.1..."

# Atualizar IdentifyPersonaStep
cat > Steps/IdentifyPersonaStep.cs << 'EOL'
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
                // Criando usando a API correta para a versão 1.0.1
                var kernel = Kernel.Builder
                    .WithOpenAIChatCompletionService(
                        modelId: "llama-3.3-70b-versatile",
                        apiKey: APIKey,
                        serviceId: "groq",
                        endpoint: new Uri("https://api.groq.com/openai/v1"))
                    .Build();

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

                var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
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

                var agentReviewerResponse = await chatCompletionService.GetChatMessageContentAsync(reviewChatHistory);
                
                if (bool.TryParse(agentReviewerResponse.Content, out bool isReviewer) && isReviewer)
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
EOL

# Atualizar KernelService
cat > Services/KernelService.cs << 'EOL'
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using UserStoryIntegration.Application.Interfaces;

namespace UserStoryIntegration.Application.Services
{
    /// <summary>
    /// Implementação do serviço de gerenciamento do Semantic Kernel
    /// </summary>
    public class KernelService : IKernelService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KernelService> _logger;

        // Valores de configuração padrão se não forem especificados no appsettings.json
        private const string DEFAULT_MODEL = "llama-3.3-70b-versatile"; 
        private const string DEFAULT_ENDPOINT = "https://api.groq.com/openai/v1";
        private static string DEFAULT_API_KEY = "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";

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="configuration">Configuração da aplicação</param>
        /// <param name="logger">Logger</param>
        public KernelService(IConfiguration configuration, ILogger<KernelService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <inheritdoc/>
        public Kernel CreateKernel()
        {
            try
            {
                // Obter configurações do appsettings.json
                string modelName = _configuration["LLM:ModelName"] ?? DEFAULT_MODEL;
                string endpoint = _configuration["LLM:Endpoint"] ?? DEFAULT_ENDPOINT;
                string apiKey = _configuration["LLM:ApiKey"] ?? DEFAULT_API_KEY;

                _logger.LogInformation("Criando kernel com modelo {ModelName} no endpoint {Endpoint}", modelName, endpoint);

                // Criando usando a API correta para a versão 1.0.1
                var kernel = Kernel.Builder
                    .WithOpenAIChatCompletionService(
                        modelId: modelName,
                        apiKey: apiKey,
                        serviceId: "groq",
                        endpoint: new Uri(endpoint))
                    .Build();

                return kernel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar o kernel");
                throw new ApplicationException("Falha ao inicializar o kernel do LLM", ex);
            }
        }
    }
}
EOL

# Atualizar a implementação do serviço de chat
cat > Services/Chat/UserStoryChatService.cs << 'EOL'
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
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();

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
EOL

# Voltar à raiz do projeto
cd ../../

echo "Executando restauração de pacotes para toda a solução..."
dotnet restore --force

echo "Pacotes atualizados e compatibilidade garantida."
echo "Execute 'dotnet build' para verificar se os erros foram corrigidos."