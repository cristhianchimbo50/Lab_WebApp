using Lab_Contracts.Auth;
using Lab_APIRest.Services.Auth;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
        {
            try
            {
                var result = await _authService.LoginAsync(dto, ct);

                if (result == null)
                    return Unauthorized(new { Mensaje = "Credenciales inválidas o la cuenta está bloqueada." });

                if (result.EsContraseñaTemporal && string.IsNullOrEmpty(result.AccessToken))
                    return Ok(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en inicio de sesión.");
                return StatusCode(500, new { Mensaje = "Error interno del servidor." });
            }
        }



        [HttpPost("change-password")]
        public async Task<IActionResult> CambiarClave([FromBody] ChangePasswordDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mensaje = "Datos inválidos en la solicitud." });

            try
            {
                bool actualizado = await _authService.CambiarClaveAsync(dto, ct);

                if (!actualizado)
                    return Unauthorized(new { Mensaje = "Correo o contraseña actual incorrectos." });

                return Ok(new { Mensaje = "Contraseña actualizada correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar la contraseña.");
                return StatusCode(500, new { Mensaje = "Error interno al cambiar la contraseña." });
            }
        }


    }
}