using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,laboratorista,recepcionista")]
    public class ExamenesController : ControllerBase
    {
        private readonly IExamenService _examenService;
        private readonly ILogger<ExamenesController> _logger;

        public ExamenesController(IExamenService examenService, ILogger<ExamenesController> logger)
        {
            _examenService = examenService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamenDto>>> ListarExamenes()
        {
            var lista = await _examenService.ListarExamenesAsync();
            return Ok(lista);
        }

        [HttpGet("{idExamen:int}")]
        public async Task<ActionResult<ExamenDto>> ObtenerDetalleExamen(int idExamen)
        {
            var examen = await _examenService.ObtenerDetalleExamenAsync(idExamen);
            if (examen == null) return NotFound("Examen no encontrado.");
            return Ok(examen);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<ExamenDto>>> ListarExamenes([FromQuery] string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return BadRequest("Debe proporcionar un nombre válido.");
            var lista = await _examenService.ListarExamenesPorNombreAsync(nombre);
            return Ok(lista);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost]
        public async Task<ActionResult<ExamenDto>> GuardarExamen([FromBody] ExamenDto datosExamen)
        {
            try
            {
                var creado = await _examenService.GuardarExamenAsync(datosExamen);
                return CreatedAtAction(nameof(ObtenerDetalleExamen), new { idExamen = creado.IdExamen }, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar examen.");
                return StatusCode(500, "Error interno al registrar el examen.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("{idExamen:int}")]
        public async Task<IActionResult> GuardarExamen(int idExamen, [FromBody] ExamenDto datosExamen)
        {
            if (idExamen != datosExamen.IdExamen) return BadRequest("El identificador no coincide.");
            var ok = await _examenService.GuardarExamenAsync(idExamen, datosExamen);
            if (!ok) return NotFound("Examen no encontrado.");
            return NoContent();
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular/{idExamen:int}")]
        public async Task<IActionResult> AnularExamen(int idExamen)
        {
            var ok = await _examenService.AnularExamenAsync(idExamen);
            if (!ok) return NotFound("Examen no encontrado.");
            return Ok();
        }

        [Authorize(Roles = "administrador,laboratorista")]
        [HttpGet("{idExamen:int}/hijos")]
        public async Task<ActionResult<List<ExamenDto>>> ListarExamenesHijos(int idExamen)
        {
            var hijos = await _examenService.ListarExamenesHijosAsync(idExamen);
            return Ok(hijos);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost("{idPadre:int}/hijos/{idHijo:int}")]
        public async Task<IActionResult> AsignarExamenHijo(int idPadre, int idHijo)
        {
            var ok = await _examenService.AsignarExamenHijoAsync(idPadre, idHijo);
            if (!ok) return Conflict("La relación ya existe o los datos no son válidos.");
            return Ok();
        }

        [Authorize(Roles = "administrador")]
        [HttpDelete("{idPadre:int}/hijos/{idHijo:int}")]
        public async Task<IActionResult> EliminarExamenHijo(int idPadre, int idHijo)
        {
            var ok = await _examenService.EliminarExamenHijoAsync(idPadre, idHijo);
            if (!ok) return NotFound();
            return Ok();
        }
    }
}
