using Lab_Contracts.Auth;
using Lab_APIRest.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Lab_APIRest.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Autentica un usuario y devuelve su token JWT.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
        {
            try
            {
                var result = await _authService.LoginAsync(dto, ct);
                if (result == null)
                    return Unauthorized(new { error = "Credenciales inválidas o cuenta bloqueada." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en inicio de sesión.");
                return StatusCode(500, "Error interno al procesar el inicio de sesión.");
            }
        }
    }
}
