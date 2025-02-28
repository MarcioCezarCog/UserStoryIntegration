using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;
using System.Text.Json;

namespace UserStoryIntegration.API.Controllers;

/// <summary>
/// Controller para verificar a saúde da aplicação e seus componentes
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthCheckController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;

    public HealthCheckController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    /// <summary>
    /// Verifica o status de saúde completo da aplicação
    /// </summary>
    /// <returns>Relatório detalhado de saúde</returns>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var report = await _healthCheckService.CheckHealthAsync();
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                data = e.Value.Data,
                exception = e.Value.Exception?.Message
            }),
            totalDuration = report.TotalDuration
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(result)
            : StatusCode((int)HttpStatusCode.ServiceUnavailable, result);
    }
}
