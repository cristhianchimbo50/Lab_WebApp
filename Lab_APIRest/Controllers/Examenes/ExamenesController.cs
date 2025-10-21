using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    /// <summary>
    /// Controlador responsable de gestionar los exámenes y sus relaciones.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExamenesController : ControllerBase
    {
        private readonly IExamenService _service;
        private readonly ILogger<ExamenesController> _logger;

        public ExamenesController(IExamenService service, ILogger<ExamenesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>Obtiene todos los exámenes registrados.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamenDto>>> ObtenerExamenes()
        {
            var examenes = await _service.GetExamenesAsync();
            return Ok(examenes);
        }

        /// <summary>Obtiene un examen específico por su identificador.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExamenDto>> ObtenerExamen(int id)
        {
            var examen = await _service.GetExamenByIdAsync(id);
            if (examen == null) return NotFound("Examen no encontrado.");
            return Ok(examen);
        }

        /// <summary>Busca exámenes por nombre parcial o completo.</summary>
        [HttpGet("buscar")]
        public async Task<ActionResult<List<ExamenDto>>> BuscarExamenes([FromQuery] string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("Debe proporcionar un nombre válido.");

            var examenes = await _service.BuscarExamenesPorNombreAsync(nombre);
            return Ok(examenes);
        }

        /// <summary>Registra un nuevo examen.</summary>
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

        /// <summary>Edita la información de un examen existente.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarExamen(int id, [FromBody] ExamenDto dto)
        {
            if (id != dto.IdExamen) return BadRequest("El identificador no coincide.");

            var exito = await _service.EditarExamenAsync(id, dto);
            if (!exito) return NotFound("Examen no encontrado.");
            return NoContent();
        }

        /// <summary>Anula un examen (desactiva su disponibilidad).</summary>
        [HttpPut("anular/{id:int}")]
        public async Task<IActionResult> AnularExamen(int id)
        {
            var exito = await _service.AnularExamenAsync(id);
            if (!exito) return NotFound("Examen no encontrado.");
            return Ok();
        }

        /// <summary>Obtiene los exámenes hijos asociados a un examen padre.</summary>
        [HttpGet("{id:int}/hijos")]
        public async Task<ActionResult<List<ExamenDto>>> ObtenerHijos(int id)
        {
            var hijos = await _service.ObtenerHijosDeExamenAsync(id);
            return Ok(hijos);
        }

        /// <summary>Agrega un examen hijo a un examen padre.</summary>
        [HttpPost("{idPadre:int}/hijos/{idHijo:int}")]
        public async Task<IActionResult> AgregarHijo(int idPadre, int idHijo)
        {
            var result = await _service.AgregarExamenHijoAsync(idPadre, idHijo);
            if (!result) return Conflict("La relación ya existe o los datos no son válidos.");
            return Ok();
        }

        /// <summary>Elimina la relación entre un examen padre e hijo.</summary>
        [HttpDelete("{idPadre:int}/hijos/{idHijo:int}")]
        public async Task<IActionResult> EliminarHijo(int idPadre, int idHijo)
        {
            var result = await _service.EliminarExamenHijoAsync(idPadre, idHijo);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
