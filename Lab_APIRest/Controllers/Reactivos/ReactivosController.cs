using Lab_Contracts.Reactivos;
using Lab_APIRest.Services.Reactivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Reactivos
{
    /// <summary>
    /// Controlador responsable de la gestión de reactivos: registro, edición, anulación e ingreso/egreso de stock.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,laboratorista")]
    public class ReactivosController : ControllerBase
    {
        private readonly IReactivoService _service;
        private readonly ILogger<ReactivosController> _logger;

        public ReactivosController(IReactivoService service, ILogger<ReactivosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReactivoDto>>> ObtenerReactivos()
        {
            var data = await _service.GetReactivosAsync();
            return Ok(data);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReactivoDto>> ObtenerReactivoPorId(int id)
        {
            var item = await _service.GetReactivoPorIdAsync(id);
            if (item == null)
                return NotFound("Reactivo no encontrado.");
            return Ok(item);
        }

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

        [Authorize(Roles = "administrador")]
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
