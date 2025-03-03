using Microsoft.SemanticKernel;

namespace UserStoryIntegration.Application.Interfaces
{
    /// <summary>
    /// Interface para o serviço de gerenciamento do Semantic Kernel
    /// </summary>
    public interface IKernelService
    {
        /// <summary>
        /// Cria uma nova instância do Kernel configurada para uso
        /// </summary>
        /// <returns>Instância do Kernel</returns>
        Kernel CreateKernel();
    }
}
