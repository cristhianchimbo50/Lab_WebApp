using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamenesController : ControllerBase
    {
        private readonly IExamenService _service;

        public ExamenesController(IExamenService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamenDto>>> GetExamenes()
        {
            var examenes = await _service.GetExamenesAsync();
            return Ok(examenes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ExamenDto>> GetExamen(int id)
        {
            var examen = await _service.GetExamenByIdAsync(id);
            if (examen == null) return NotFound();
            return Ok(examen);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<ExamenDto>>> BuscarExamenes([FromQuery] string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return BadRequest("Debe proporcionar el nombre.");

            var examenes = await _service.BuscarExamenesPorNombreAsync(nombre);
            return Ok(examenes);
        }

        [HttpPost]
        public async Task<ActionResult<ExamenDto>> PostExamen(ExamenDto dto)
        {
            var examen = await _service.CrearExamenAsync(dto);
            return CreatedAtAction(nameof(GetExamen), new { id = examen.IdExamen }, examen);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutExamen(int id, ExamenDto dto)
        {
            if (id != dto.IdExamen) return BadRequest();
            var exito = await _service.EditarExamenAsync(id, dto);
            if (!exito) return NotFound();
            return NoContent();
        }

        [HttpPut("anular/{id}")]
        public async Task<IActionResult> AnularExamen(int id)
        {
            var exito = await _service.AnularExamenAsync(id);
            if (!exito) return NotFound();
            return Ok();
        }

        //Para examen compuesto

        [HttpGet("{id}/hijos")]
        public async Task<ActionResult<List<ExamenDto>>> ObtenerHijos(int id)
        {
            var hijos = await _service.ObtenerHijosDeExamenAsync(id);
            return Ok(hijos);
        }

        [HttpPost("{idPadre}/hijos/{idHijo}")]
        public async Task<IActionResult> AgregarHijo(int idPadre, int idHijo)
        {
            var result = await _service.AgregarExamenHijoAsync(idPadre, idHijo);
            if (!result) return Conflict("La relación ya existe o datos inválidos.");
            return Ok();
        }

        [HttpDelete("{idPadre}/hijos/{idHijo}")]
        public async Task<IActionResult> EliminarHijo(int idPadre, int idHijo)
        {
            var result = await _service.EliminarExamenHijoAsync(idPadre, idHijo);
            if (!result) return NotFound();
            return Ok();
        }

    }
}
