using Lab_Contracts.Reactivos;
using Lab_APIRest.Services.Reactivos;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Reactivos
{
    /// <summary>
    /// Controlador responsable de la gestión de reactivos: registro, edición, anulación e ingreso/egreso de stock.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReactivosController : ControllerBase
    {
        private readonly IReactivoService _service;
        private readonly ILogger<ReactivosController> _logger;

        /// <summary>
        /// Constructor del controlador de reactivos.
        /// </summary>
        /// <param name="service">Servicio de reactivos inyectado.</param>
        /// <param name="logger">Logger para registrar errores y eventos.</param>
        public ReactivosController(IReactivoService service, ILogger<ReactivosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los reactivos registrados en el sistema.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ReactivoDto>>> ObtenerReactivos()
        {
            var data = await _service.GetReactivosAsync();
            return Ok(data);
        }

        /// <summary>
        /// Obtiene la información de un reactivo específico por su identificador.
        /// </summary>
        /// <param name="id">Identificador del reactivo.</param>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReactivoDto>> ObtenerReactivoPorId(int id)
        {
            var item = await _service.GetReactivoPorIdAsync(id);
            if (item == null)
                return NotFound("Reactivo no encontrado.");

            return Ok(item);
        }

        /// <summary>
        /// Registra un nuevo reactivo.
        /// </summary>
        /// <param name="dto">Datos del reactivo.</param>
        [HttpPost]
        public async Task<ActionResult<ReactivoDto>> RegistrarReactivo([FromBody] ReactivoDto dto)
        {
            try
            {
                var creado = await _service.CrearReactivoAsync(dto);
                return CreatedAtAction(nameof(ObtenerReactivoPorId), new { id = creado.IdReactivo }, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar reactivo.");
                return StatusCode(500, "Error interno al registrar el reactivo.");
            }
        }

        /// <summary>
        /// Edita la información de un reactivo existente.
        /// </summary>
        /// <param name="id">Identificador del reactivo.</param>
        /// <param name="dto">Datos actualizados del reactivo.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarReactivo(int id, [FromBody] ReactivoDto dto)
        {
            if (id != dto.IdReactivo)
                return BadRequest("El identificador no coincide con el reactivo proporcionado.");

            var ok = await _service.EditarReactivoAsync(id, dto);
            if (!ok)
                return NotFound("Reactivo no encontrado.");

            return NoContent();
        }

        /// <summary>
        /// Anula (desactiva) un reactivo existente.
        /// </summary>
        /// <param name="id">Identificador del reactivo.</param>
        [HttpPut("anular/{id:int}")]
        public async Task<IActionResult> AnularReactivo(int id)
        {
            try
            {
                var ok = await _service.AnularReactivoAsync(id);
                if (!ok)
                    return NotFound("Reactivo no encontrado o ya estaba anulado.");

                return Ok(new { mensaje = "Reactivo anulado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular el reactivo con ID {id}.");
                return StatusCode(500, "Error interno al anular el reactivo.");
            }
        }

        /// <summary>
        /// Registra ingresos de reactivos al inventario.
        /// </summary>
        /// <param name="ingresos">Lista de ingresos de reactivos.</param>
        [HttpPost("ingresos")]
        public async Task<ActionResult> RegistrarIngresos([FromBody] IEnumerable<MovimientoReactivoIngresoDto> ingresos)
        {
            if (ingresos == null || !ingresos.Any())
                return BadRequest("No se enviaron datos de ingreso.");

            try
            {
                var ok = await _service.RegistrarIngresosAsync(ingresos);
                if (ok)
                    return Ok(new { mensaje = "Ingresos registrados correctamente." });

                return StatusCode(500, new { mensaje = "Error al registrar los ingresos." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar ingresos de reactivos.");
                return StatusCode(500, "Error interno al registrar ingresos.");
            }
        }

        /// <summary>
        /// Registra egresos (salidas) de reactivos del inventario.
        /// </summary>
        /// <param name="egresos">Lista de egresos de reactivos.</param>
        [HttpPost("egresos")]
        public async Task<ActionResult> RegistrarEgresos([FromBody] IEnumerable<MovimientoReactivoEgresoDto> egresos)
        {
            if (egresos == null || !egresos.Any())
                return BadRequest("No se enviaron datos de egreso.");

            try
            {
                var ok = await _service.RegistrarEgresosAsync(egresos);
                if (ok)
                    return Ok(new { mensaje = "Egresos registrados correctamente." });

                return StatusCode(500, new { mensaje = "Error al registrar los egresos." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar egresos de reactivos.");
                return StatusCode(500, "Error interno al registrar egresos.");
            }
        }
    }
}
