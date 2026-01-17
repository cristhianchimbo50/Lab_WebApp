using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "1,3")]
    public class ExamenComposicionController : ControllerBase
    {
        private readonly IExamenComposicionService _composicionService;
        private readonly ILogger<ExamenComposicionController> _logger;

        public ExamenComposicionController(IExamenComposicionService composicionService, ILogger<ExamenComposicionController> logger)
        {
            _composicionService = composicionService;
            _logger = logger;
        }

        [HttpGet("padre/{id:int}")]
        public async Task<ActionResult<List<ExamenComposicionDto>>> ListarComposicionesPorPadre(int idExamenPadre)
        {
            var lista = await _composicionService.ListarComposicionesPorPadreAsync(idExamenPadre);
            return Ok(lista);
        }

        [HttpGet("hijo/{id:int}")]
        public async Task<ActionResult<List<ExamenComposicionDto>>> ListarComposicionesPorHijo(int idExamenHijo)
        {
            var lista = await _composicionService.ListarComposicionesPorHijoAsync(idExamenHijo);
            return Ok(lista);
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        public async Task<IActionResult> GuardarComposicion([FromBody] ExamenComposicionDto composicionDto)
        {
            try
            {
                var creado = await _composicionService.GuardarComposicionAsync(composicionDto);
                if (!creado) return Conflict("Ya existe esta composición.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la composición de examen.");
                return StatusCode(500, "Error interno al crear la composición.");
            }
        }

        [Authorize(Roles = "1")]
        [HttpDelete]
        public async Task<IActionResult> EliminarComposicion([FromQuery] int idExamenPadre, [FromQuery] int idExamenHijo)
        {
            var eliminado = await _composicionService.EliminarComposicionAsync(idExamenPadre, idExamenHijo);
            if (!eliminado) return NotFound();
            return Ok();
        }
    }
}
