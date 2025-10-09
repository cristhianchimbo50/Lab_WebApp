using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamenReactivoAsociacionesController : ControllerBase
    {
        private readonly IExamenReactivoAsociacionService _service;

        public ExamenReactivoAsociacionesController(IExamenReactivoAsociacionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> GetAll()
        {
            var data = await _service.ObtenerTodasAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AsociacionReactivoDto>> GetById(int id)
        {
            var item = await _service.ObtenerPorIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("buscar-examen/{nombre}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> BuscarPorExamen(string nombre)
        {
            var data = await _service.BuscarPorExamenAsync(nombre);
            return Ok(data);
        }

        [HttpGet("buscar-reactivo/{nombre}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> BuscarPorReactivo(string nombre)
        {
            var data = await _service.BuscarPorReactivoAsync(nombre);
            return Ok(data);
        }

        [HttpPost]
        public async Task<ActionResult<AsociacionReactivoDto>> Crear(AsociacionReactivoDto dto)
        {
            var creado = await _service.CrearAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = creado.IdExamenReactivo }, creado);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, AsociacionReactivoDto dto)
        {
            var ok = await _service.EditarAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var ok = await _service.EliminarAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpGet("asociados/{idExamen}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> ObtenerAsociadosPorExamen(int idExamen)
        {
            var data = await _service.ObtenerTodasAsync();
            var asociados = data.Where(a => a.IdExamen == idExamen).ToList();
            return Ok(asociados);
        }

        [HttpPost("asociados/{idExamen}")]
        public async Task<IActionResult> GuardarAsociaciones(int idExamen, [FromBody] List<AsociacionReactivoDto> asociaciones)
        {
            var ok = await _service.GuardarPorExamenAsync(idExamen, asociaciones);
            if (ok)
                return Ok();
            return BadRequest();
        }

    }
}
