using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,laboratorista")]
    public class ExamenReactivoAsociacionesController : ControllerBase
    {
        private readonly IExamenReactivoAsociacionService _reactivoAsociacionService;
        private readonly ILogger<ExamenReactivoAsociacionesController> _logger;

        public ExamenReactivoAsociacionesController(IExamenReactivoAsociacionService reactivoAsociacionService, ILogger<ExamenReactivoAsociacionesController> logger)
        {
            _reactivoAsociacionService = reactivoAsociacionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> ListarAsociaciones()
        {
            var lista = await _reactivoAsociacionService.ListarAsociacionesAsync();
            return Ok(lista);
        }

        [HttpGet("{idExamenReactivo:int}")]
        public async Task<ActionResult<AsociacionReactivoDto>> ObtenerDetalleAsociacion(int idExamenReactivo)
        {
            var asociacion = await _reactivoAsociacionService.ObtenerDetalleAsociacionAsync(idExamenReactivo);
            if (asociacion == null) return NotFound();
            return Ok(asociacion);
        }

        [HttpGet("buscar-examen/{nombreExamen}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> ListarAsociacionesPorExamen(string nombreExamen)
        {
            var lista = await _reactivoAsociacionService.ListarAsociacionesPorExamenAsync(nombreExamen);
            return Ok(lista);
        }

        [HttpGet("buscar-reactivo/{nombreReactivo}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> ListarAsociacionesPorReactivo(string nombreReactivo)
        {
            var lista = await _reactivoAsociacionService.ListarAsociacionesPorReactivoAsync(nombreReactivo);
            return Ok(lista);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost]
        public async Task<ActionResult<AsociacionReactivoDto>> GuardarAsociacion([FromBody] AsociacionReactivoDto asociacionDto)
        {
            try
            {
                var creado = await _reactivoAsociacionService.GuardarAsociacionAsync(asociacionDto);
                return CreatedAtAction(nameof(ObtenerDetalleAsociacion), new { idExamenReactivo = creado.IdExamenReactivo }, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asociación examen-reactivo.");
                return StatusCode(500, "Error interno al crear la asociación.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("{idExamenReactivo:int}")]
        public async Task<IActionResult> GuardarAsociacion(int idExamenReactivo, [FromBody] AsociacionReactivoDto asociacionDto)
        {
            var ok = await _reactivoAsociacionService.GuardarAsociacionAsync(idExamenReactivo, asociacionDto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "administrador")]
        [HttpDelete("{idExamenReactivo:int}")]
        public async Task<IActionResult> AnularAsociacion(int idExamenReactivo)
        {
            var ok = await _reactivoAsociacionService.AnularAsociacionAsync(idExamenReactivo);
            if (!ok) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "administrador,laboratorista")]
        [HttpGet("asociados/{idExamen:int}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> ListarAsociacionesPorExamenId(int idExamen)
        {
            var lista = await _reactivoAsociacionService.ListarAsociacionesPorExamenIdAsync(idExamen);
            return Ok(lista);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost("asociados/{idExamen:int}")]
        public async Task<IActionResult> GuardarAsociacionesPorExamen(int idExamen, [FromBody] List<AsociacionReactivoDto> asociaciones)
        {
            var ok = await _reactivoAsociacionService.GuardarAsociacionesPorExamenAsync(idExamen, asociaciones);
            if (ok) return Ok();
            return BadRequest("No se pudieron guardar las asociaciones.");
        }
    }
}
