using Lab_Contracts.Reactivos;
using Lab_APIRest.Services.Reactivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lab_Contracts.Common;
using System.Linq;

namespace Lab_APIRest.Controllers.Reactivos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "1,3")]
    public class ReactivosController : ControllerBase
    {
        private readonly IReactivoService _reactivoService;
        private readonly ILogger<ReactivosController> _logger;

        public ReactivosController(IReactivoService reactivoService, ILogger<ReactivosController> logger)
        {
            _reactivoService = reactivoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReactivoDto>>> ListarReactivos()
        {
            var reactivos = await _reactivoService.ListarReactivosAsync();
            return Ok(reactivos);
        }

        [HttpPost("buscar")]
        public async Task<ActionResult<ResultadoPaginadoDto<ReactivoDto>>> ListarReactivosPaginados([FromBody] ReactivoFiltroDto filtro)
        {
            var result = await _reactivoService.ListarReactivosPaginadosAsync(filtro);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReactivoDto>> ObtenerDetalleReactivo(int id)
        {
            var reactivo = await _reactivoService.ObtenerDetalleReactivoAsync(id);
            if (reactivo == null) return NotFound("Reactivo no encontrado.");
            return Ok(reactivo);
        }

        [HttpPost]
        public async Task<ActionResult<ReactivoDto>> GuardarReactivo([FromBody] ReactivoDto reactivo)
        {
            try
            {
                var creado = await _reactivoService.GuardarReactivoAsync(reactivo);
                return CreatedAtAction(nameof(ObtenerDetalleReactivo), new { id = creado.IdReactivo }, creado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar reactivo.");
                return StatusCode(500, "Error interno al registrar el reactivo.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> GuardarReactivo(int id, [FromBody] ReactivoDto reactivo)
        {
            if (id != reactivo.IdReactivo) return BadRequest("El identificador no coincide con el reactivo proporcionado.");
            var ok = await _reactivoService.GuardarReactivoAsync(id, reactivo);
            if (!ok) return NotFound("Reactivo no encontrado.");
            return NoContent();
        }

        [Authorize(Roles = "1")]
        [HttpPut("anular/{id:int}")]
        public async Task<IActionResult> AnularReactivo(int id)
        {
            try
            {
                var ok = await _reactivoService.AnularReactivoAsync(id);
                if (!ok) return NotFound("Reactivo no encontrado o ya estaba anulado.");
                return Ok(new { mensaje = "Reactivo anulado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular el reactivo con ID {id}.");
                return StatusCode(500, "Error interno al anular el reactivo.");
            }
        }

        [HttpPost("ingresos")]
        public async Task<ActionResult> RegistrarIngresosReactivos([FromBody] IEnumerable<MovimientoReactivoIngresoDto> ingresos)
        {
            if (ingresos == null || !ingresos.Any()) return BadRequest("No se enviaron datos de ingreso.");
            try
            {
                var ok = await _reactivoService.RegistrarIngresosReactivosAsync(ingresos);
                if (ok) return Ok(new { mensaje = "Ingresos registrados correctamente." });
                return StatusCode(500, new { mensaje = "Error al registrar los ingresos." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar ingresos de reactivos.");
                return StatusCode(500, "Error interno al registrar ingresos.");
            }
        }

        [HttpPost("egresos")]
        public async Task<ActionResult> RegistrarEgresosReactivos([FromBody] IEnumerable<MovimientoReactivoEgresoDto> egresos)
        {
            if (egresos == null || !egresos.Any()) return BadRequest("No se enviaron datos de egreso.");
            try
            {
                var ok = await _reactivoService.RegistrarEgresosReactivosAsync(egresos);
                if (ok) return Ok(new { mensaje = "Egresos registrados correctamente." });
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
