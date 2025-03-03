using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
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

                // Configurar o cliente OpenAI com endpoint personalizado
                var openAIOptions = new OpenAIClientOptions
                {
                    ApiKey = apiKey,
                    Organization = null,
                    BaseUri = new Uri(endpoint)
                };
                
                // Criar o kernel com a API atualizada do Semantic Kernel
                var builder = Kernel.CreateBuilder();
                
                // Adicionar chat completion com a sintaxe correta e opções
                builder.AddOpenAIChatCompletion(
                    modelId: modelName, 
                    options: openAIOptions);
                
                return builder.Build();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar o kernel");
                throw new ApplicationException("Falha ao inicializar o kernel do LLM", ex);
            }
        }
    }
}
