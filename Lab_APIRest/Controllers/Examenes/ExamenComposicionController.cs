using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,laboratorista")]
    public class ExamenComposicionController : ControllerBase
    {
        private readonly IExamenComposicionService _service;
        private readonly ILogger<ExamenComposicionController> _logger;

        public ExamenComposicionController(IExamenComposicionService service, ILogger<ExamenComposicionController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("padre/{id:int}")]
        public async Task<ActionResult<List<ExamenComposicionDto>>> ObtenerPorPadre(int id)
        {
            var lista = await _service.GetComposicionesPorExamenPadreAsync(id);
            return Ok(lista);
        }

        [HttpGet("hijo/{id:int}")]
        public async Task<ActionResult<List<ExamenComposicionDto>>> ObtenerPorHijo(int id)
        {
            var lista = await _service.GetComposicionesPorExamenHijoAsync(id);
            return Ok(lista);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ExamenComposicionDto dto)
        {
            try
            {
                var ok = await _service.CrearComposicionAsync(dto);
                if (!ok) return Conflict("Ya existe esta composición.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear la composición de examen.");
                return StatusCode(500, "Error interno al crear la composición.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpDelete]
        public async Task<IActionResult> Eliminar([FromQuery] int idExamenPadre, [FromQuery] int idExamenHijo)
        {
            var ok = await _service.EliminarComposicionAsync(idExamenPadre, idExamenHijo);
            if (!ok) return NotFound();
            return Ok();
        }
    }
}
