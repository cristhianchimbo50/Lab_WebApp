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
                {
                    if (result.ExpiresAtUtc != null && result.ExpiresAtUtc < DateTime.UtcNow)
                        return BadRequest(new
                        {
                            Mensaje = "La contraseña temporal ha expirado. Solicite una nueva en recepción.",
                            Expiracion = result.ExpiresAtUtc
                        });

                    return Ok(new
                    {
                        Mensaje = "Debe cambiar su contraseña temporal antes de continuar.",
                        result.CorreoUsuario,
                        result.Nombre,
                        result.Rol,
                        result.EsContraseñaTemporal,
                        result.ExpiresAtUtc
                    });
                }

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operación inválida en inicio de sesión.");
                return BadRequest(new { Mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en inicio de sesión.");
                return StatusCode(500, new { Mensaje = "Ocurrió un error interno al procesar la solicitud de inicio de sesión." });
            }
        }

        [HttpPost("cambiar-contrasenia")]
        public async Task<IActionResult> CambiarContrasenia([FromBody] CambiarContraseniaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mensaje = "Datos inválidos en la solicitud." });

            try
            {
                var resultado = await _authService.CambiarContraseniaAsync(dto, ct);

                if (!resultado.Exito)
                    return BadRequest(new { Mensaje = resultado.Mensaje });

                return Ok(new { Mensaje = resultado.Mensaje });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al cambiar la contraseña.");
                return BadRequest(new { Mensaje = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Usuario no encontrado al cambiar contraseña.");
                return NotFound(new { Mensaje = "Usuario no encontrado." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar la contraseña.");
                return StatusCode(500, new { Mensaje = "Error interno del servidor." });
            }
        }
    }
}
