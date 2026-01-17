using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lab_Contracts.Common;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "1,3,2")]
    public class ExamenesController : ControllerBase
    {
        private readonly IExamenService _examenService;

        public ExamenesController(IExamenService examenService)
        {
            _examenService = examenService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ExamenDto>>> ListarExamenes()
        {
            var result = await _examenService.ListarExamenesAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ExamenDto?>> ObtenerDetalle(int id)
        {
            var result = await _examenService.ObtenerDetalleExamenAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<ExamenDto>>> BuscarPorNombre([FromQuery] string nombre)
        {
            var result = await _examenService.ListarExamenesPorNombreAsync(nombre ?? string.Empty);
            return Ok(result);
        }

        [HttpPost("buscar")]
        public async Task<ActionResult<ResultadoPaginadoDto<ExamenDto>>> ListarExamenesPaginados([FromBody] ExamenFiltroDto filtro)
        {
            var result = await _examenService.ListarExamenesPaginadosAsync(filtro);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "1")]
        public async Task<ActionResult<ExamenDto>> Crear([FromBody] ExamenDto examen)
        {
            var creado = await _examenService.GuardarExamenAsync(examen);
            return CreatedAtAction(nameof(ObtenerDetalle), new { id = creado.IdExamen }, creado);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ExamenDto examen)
        {
            var ok = await _examenService.GuardarExamenAsync(id, examen);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPut("anular/{id:int}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> Anular(int id)
        {
            var ok = await _examenService.AnularExamenAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpGet("{idPadre:int}/hijos")]
        public async Task<ActionResult<List<ExamenDto>>> ListarHijos(int idPadre)
        {
            var result = await _examenService.ListarExamenesHijosAsync(idPadre);
            return Ok(result);
        }

        [HttpPost("{idPadre:int}/hijos/{idHijo:int}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> AsignarHijo(int idPadre, int idHijo)
        {
            await _examenService.AsignarExamenHijoAsync(idPadre, idHijo);
            return NoContent();
        }

        [HttpDelete("{idPadre:int}/hijos/{idHijo:int}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> EliminarHijo(int idPadre, int idHijo)
        {
            var ok = await _examenService.EliminarExamenHijoAsync(idPadre, idHijo);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
