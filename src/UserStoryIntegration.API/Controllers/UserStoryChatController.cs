using Microsoft.AspNetCore.Mvc;
using UserStoryIntegration.Application.DTOs.UserStory;
using UserStoryIntegration.Application.Interfaces;

namespace UserStoryIntegration.API.Controllers
{
    /// <summary>
    /// Controller para o chat de criação de user stories
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserStoryChatController : ControllerBase
    {
        private readonly IUserStoryService _userStoryService;
        private readonly ILogger<UserStoryChatController> _logger;

        /// <summary>
        /// Construtor
        /// </summary>
        /// <param name="userStoryService">Serviço de user story</param>
        /// <param name="logger">Logger</param>
        public UserStoryChatController(
            IUserStoryService userStoryService,
            ILogger<UserStoryChatController> logger)
        {
            _userStoryService = userStoryService;
            _logger = logger;
        }

        /// <summary>
        /// Envia uma mensagem para o assistente de user story
        /// </summary>
        /// <param name="request">Requisição contendo a mensagem</param>
        /// <returns>Resposta do assistente</returns>
        /// <response code="200">Resposta processada com sucesso</response>
        /// <response code="400">Requisição inválida</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpPost("send")]
        [ProducesResponseType(typeof(UserStoryChatResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserStoryChatResponse>> SendMessage([FromBody] UserStoryChatRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest("A mensagem não pode estar vazia");
                }

                _logger.LogInformation("Recebida requisição para processar mensagem");
                var response = await _userStoryService.ProcessMessageAsync(request);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar mensagem");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { message = "Ocorreu um erro ao processar sua mensagem" });
            }
        }
    }
}
