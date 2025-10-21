using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    /// <summary>
    /// Controlador responsable de gestionar las asociaciones entre exámenes y reactivos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ExamenReactivoAsociacionesController : ControllerBase
    {
        private readonly IExamenReactivoAsociacionService _service;
        private readonly ILogger<ExamenReactivoAsociacionesController> _logger;

        public ExamenReactivoAsociacionesController(IExamenReactivoAsociacionService service, ILogger<ExamenReactivoAsociacionesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>Obtiene todas las asociaciones entre exámenes y reactivos.</summary>
        [HttpGet]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> ObtenerTodas()
        {
            var data = await _service.ObtenerTodasAsync();
            return Ok(data);
        }

        /// <summary>Obtiene una asociación específica por su identificador.</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AsociacionReactivoDto>> ObtenerPorId(int id)
        {
            var item = await _service.ObtenerPorIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        /// <summary>Busca asociaciones filtrando por nombre de examen.</summary>
        [HttpGet("buscar-examen/{nombre}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> BuscarPorExamen(string nombre)
        {
            var data = await _service.BuscarPorExamenAsync(nombre);
            return Ok(data);
        }

        /// <summary>Busca asociaciones filtrando por nombre de reactivo.</summary>
        [HttpGet("buscar-reactivo/{nombre}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> BuscarPorReactivo(string nombre)
        {
            var data = await _service.BuscarPorReactivoAsync(nombre);
            return Ok(data);
        }

        /// <summary>Crea una nueva asociación entre examen y reactivo.</summary>
        [HttpPost]
        public async Task<ActionResult<AsociacionReactivoDto>> Crear([FromBody] AsociacionReactivoDto dto)
        {
            try
            {
                var creado = await _service.CrearAsync(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.IdExamenReactivo }, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear asociación examen-reactivo.");
                return StatusCode(500, "Error interno al crear la asociación.");
            }
        }

        /// <summary>Edita una asociación existente.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, [FromBody] AsociacionReactivoDto dto)
        {
            var ok = await _service.EditarAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>Elimina una asociación por su identificador.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var ok = await _service.EliminarAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>Obtiene las asociaciones de reactivos pertenecientes a un examen.</summary>
        [HttpGet("asociados/{idExamen:int}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> ObtenerAsociadosPorExamen(int idExamen)
        {
            var data = await _service.ObtenerTodasAsync();
            var asociados = data.Where(a => a.IdExamen == idExamen).ToList();
            return Ok(asociados);
        }

        /// <summary>Guarda o actualiza las asociaciones de reactivos para un examen.</summary>
        [HttpPost("asociados/{idExamen:int}")]
        public async Task<IActionResult> GuardarAsociaciones(int idExamen, [FromBody] List<AsociacionReactivoDto> asociaciones)
        {
            var ok = await _service.GuardarPorExamenAsync(idExamen, asociaciones);
            if (ok) return Ok();
            return BadRequest("No se pudieron guardar las asociaciones.");
        }
    }
}
