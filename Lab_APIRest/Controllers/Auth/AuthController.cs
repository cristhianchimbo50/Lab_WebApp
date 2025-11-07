using Lab_APIRest.Services.Auth;
using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lab_APIRest.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService AuthService;
        private readonly ILogger<AuthController> Logger;

        public AuthController(IAuthService AuthService, ILogger<AuthController> Logger)
        {
            this.AuthService = AuthService;
            this.Logger = Logger;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto Solicitud, CancellationToken Ct)
        {
            try
            {
                var Resultado = await AuthService.IniciarSesionAsync(Solicitud, Ct);

                if (Resultado == null)
                    return Unauthorized(new { Mensaje = "Credenciales inválidas o la cuenta está bloqueada." });

                return Ok(Resultado);
            }
            catch (InvalidOperationException Ex)
            {
                Logger.LogWarning(Ex, "Operación inválida en inicio de sesión.");
                return BadRequest(new { Mensaje = Ex.Message });
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, "Error en inicio de sesión.");
                return StatusCode(500, new { Mensaje = "Ocurrió un error interno al procesar la solicitud de inicio de sesión." });
            }
        }

        [AllowAnonymous]
        [HttpPost("cambiar-contrasenia")]
        public async Task<IActionResult> CambiarContrasenia([FromBody] CambiarContraseniaDto Cambio, CancellationToken Ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(new CambiarContraseniaResponseDto { Exito = false, Mensaje = "Datos inválidos en la solicitud." });

            try
            {
                var Resultado = await AuthService.CambiarContraseniaAsync(Cambio, Ct);

                if (!Resultado.Exito)
                    return BadRequest(Resultado);

                return Ok(Resultado);
            }
            catch (InvalidOperationException Ex)
            {
                Logger.LogWarning(Ex, "Error de validación al cambiar la contraseña.");
                return BadRequest(new CambiarContraseniaResponseDto { Exito = false, Mensaje = Ex.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new CambiarContraseniaResponseDto { Exito = false, Mensaje = "Usuario no encontrado." });
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, "Error al cambiar la contraseña.");
                return StatusCode(500, new CambiarContraseniaResponseDto { Exito = false, Mensaje = "Error interno del servidor." });
            }
        }

        [Authorize]
        [HttpGet("verificar-sesion")]
        public IActionResult VerificarSesion()
        {
            try
            {
                return Ok(new
                {
                    Activa = true,
                    Usuario = User.Identity?.Name,
                    Rol = User.FindFirst(ClaimTypes.Role)?.Value,
                    Mensaje = "Sesión válida."
                });
            }
            catch
            {
                return Unauthorized(new { Activa = false, Mensaje = "Sesión inválida o expirada." });
            }
        }

        [AllowAnonymous]
        [HttpPost("activar-cuenta")]
        public async Task<ActionResult<RespuestaMensajeDto>> ActivarCuenta([FromBody] RestablecerContraseniaDto dto, CancellationToken Ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(new RespuestaMensajeDto { Exito = false, Mensaje = "Datos inválidos." });

            try
            {
                var resultado = await AuthService.ActivarCuentaAsync(dto, Ct);
                return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, "Error al activar cuenta con token.");
                return StatusCode(500, new RespuestaMensajeDto { Exito = false, Mensaje = "Error interno del servidor." });
            }
        }
    }
}
