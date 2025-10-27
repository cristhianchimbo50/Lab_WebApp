using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,laboratorista,recepcionista")]
    public class ExamenesController : ControllerBase
    {
        private readonly IExamenService _service;
        private readonly ILogger<ExamenesController> _logger;

        public ExamenesController(IExamenService service, ILogger<ExamenesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamenDto>>> ObtenerExamenes()
        {
            var examenes = await _service.GetExamenesAsync();
            return Ok(examenes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExamenDto>> ObtenerExamen(int id)
        {
            var examen = await _service.GetExamenByIdAsync(id);
            if (examen == null) return NotFound("Examen no encontrado.");
            return Ok(examen);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<ExamenDto>>> BuscarExamenes([FromQuery] string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("Debe proporcionar un nombre válido.");

            var examenes = await _service.BuscarExamenesPorNombreAsync(nombre);
            return Ok(examenes);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost]
        public async Task<ActionResult<ExamenDto>> RegistrarExamen([FromBody] ExamenDto dto)
        {
            try
            {
                var examen = await _service.CrearExamenAsync(dto);
                return CreatedAtAction(nameof(ObtenerExamen), new { id = examen.IdExamen }, examen);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar examen.");
                return StatusCode(500, "Error interno al registrar el examen.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarExamen(int id, [FromBody] ExamenDto dto)
        {
            if (id != dto.IdExamen) return BadRequest("El identificador no coincide.");

            var exito = await _service.EditarExamenAsync(id, dto);
            if (!exito) return NotFound("Examen no encontrado.");
            return NoContent();
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular/{id:int}")]
        public async Task<IActionResult> AnularExamen(int id)
        {
            var exito = await _service.AnularExamenAsync(id);
            if (!exito) return NotFound("Examen no encontrado.");
            return Ok();
        }

        [Authorize(Roles = "administrador,laboratorista")]
        [HttpGet("{id:int}/hijos")]
        public async Task<ActionResult<List<ExamenDto>>> ObtenerHijos(int id)
        {
            var hijos = await _service.ObtenerHijosDeExamenAsync(id);
            return Ok(hijos);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost("{idPadre:int}/hijos/{idHijo:int}")]
        public async Task<IActionResult> AgregarHijo(int idPadre, int idHijo)
        {
            var result = await _service.AgregarExamenHijoAsync(idPadre, idHijo);
            if (!result) return Conflict("La relación ya existe o los datos no son válidos.");
            return Ok();
        }

        [Authorize(Roles = "administrador")]
        [HttpDelete("{idPadre:int}/hijos/{idHijo:int}")]
        public async Task<IActionResult> EliminarHijo(int idPadre, int idHijo)
        {
            var result = await _service.EliminarExamenHijoAsync(idPadre, idHijo);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
