using Microsoft.SemanticKernel;
using System.IO;
using System.Reflection;
using UserStoryIntegration.Application.Interfaces;
using UserStoryIntegration.Application.Services.Chat;

namespace UserStoryIntegration.Application.Services.Agents.Factory
{
    /// <summary>
    /// Classe base para fábricas de agentes
    /// </summary>
    public abstract class AgentFactoryBase : IAgentFactory
    {
        /// <summary>
        /// Cria um serviço de chat com as instruções especificadas
        /// </summary>
        /// <param name="kernel">Instância do kernel</param>
        /// <returns>Serviço de chat</returns>
        public IUserStoryChatService CreateChatService(Kernel kernel)
        {
            string instructions = GetInstructionsFromResource(GetInstructionsFileName());
            return new UserStoryChatService(kernel, instructions);
        }

        /// <summary>
        /// Obtém o nome do arquivo de instruções
        /// </summary>
        /// <returns>Nome do arquivo</returns>
        protected abstract string GetInstructionsFileName();

        /// <summary>
        /// Obtém as instruções para o agente a partir de um arquivo de recurso
        /// </summary>
        /// <param name="resourceName">Nome do arquivo de recurso</param>
        /// <returns>Conteúdo das instruções</returns>
        protected string GetInstructionsFromResource(string resourceName)
        {
            try
            {
                // Tenta carregar de arquivo físico primeiro para desenvolvimento
                string basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "Agents", "Instructions");
                string filePath = Path.Combine(basePath, resourceName);

                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }

                // Se não encontrar, carrega do recurso embutido
                var assembly = Assembly.GetExecutingAssembly();
                string fullResourceName = $"UserStoryIntegration.Application.Services.Agents.Instructions.{resourceName}";

                using (Stream? stream = assembly.GetManifestResourceStream(fullResourceName))
                {
                    if (stream == null)
                    {
                        throw new FileNotFoundException($"Arquivo de instruções não encontrado: {resourceName}");
                    }

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch
            {
                // Se falhar, retorna as instruções padrão
                // Não precisamos da variável ex, então removemos
                return GetDefaultInstructions();
            }
        }

        /// <summary>
        /// Obtém as instruções padrão em caso de falha no carregamento
        /// </summary>
        /// <returns>Instruções padrão</returns>
        protected abstract string GetDefaultInstructions();
    }
}
