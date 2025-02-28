namespace UserStoryIntegration.API.Extensions;

/// <summary>
/// Extensões para o pipeline HTTP da API
/// </summary>
public static class ApiExtensions
{
    /// <summary>
    /// Configura middleware para logging de requisições
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        // Será implementado posteriormente
        return app;
    }

    /// <summary>
    /// Configura middleware para tratamento de exceções da API
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseApiExceptionHandling(this IApplicationBuilder app)
    {
        // Será implementado posteriormente
        return app;
    }
}
