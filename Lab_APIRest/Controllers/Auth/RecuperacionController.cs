using Lab_Contracts.Auth;
using Lab_APIRest.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecuperacionController : ControllerBase
    {
        private readonly IRecuperacionService _recuperacionService;
        private readonly ILogger<RecuperacionController> _logger;

        public RecuperacionController(IRecuperacionService recuperacionService, ILogger<RecuperacionController> logger)
        {
            _recuperacionService = recuperacionService;
            _logger = logger;
        }

        [HttpPost("solicitar")]
        public async Task<ActionResult<RespuestaMensajeDto>> Solicitar([FromBody] OlvideContraseniaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(new RespuestaMensajeDto { Exito = false, Mensaje = "Datos inválidos." });

            try
            {
                var resultado = await _recuperacionService.SolicitarRecuperacionAsync(dto, ct);
                if (!resultado.Exito)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al solicitar recuperación de contraseña para {Correo}", dto.Correo);
                return StatusCode(500, new RespuestaMensajeDto
                {
                    Exito = false,
                    Mensaje = "Ocurrió un error interno. Intente nuevamente más tarde."
                });
            }
        }

        [HttpPost("restablecer")]
        public async Task<ActionResult<RespuestaMensajeDto>> Restablecer([FromBody] RestablecerContraseniaDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(new RespuestaMensajeDto { Exito = false, Mensaje = "Datos inválidos." });

            try
            {
                var resultado = await _recuperacionService.RestablecerContraseniaAsync(dto, ct);
                if (!resultado.Exito)
                    return BadRequest(resultado);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña con token.");
                return StatusCode(500, new RespuestaMensajeDto
                {
                    Exito = false,
                    Mensaje = "Ocurrió un error interno. Intente nuevamente más tarde."
                });
            }
        }
    }
}
