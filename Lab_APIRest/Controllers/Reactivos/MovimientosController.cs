using Lab_Contracts.Reactivos;
using Lab_APIRest.Services.Reactivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lab_Contracts.Common;

namespace Lab_APIRest.Controllers.Reactivos
{
    /// <summary>
    /// Controlador responsable de gestionar los movimientos de reactivos (ingresos y egresos).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,laboratorista")]
    public class MovimientosController : ControllerBase
    {
        private readonly IMovimientoService _movimientoService;
        private readonly ILogger<MovimientosController> _logger;

        public MovimientosController(IMovimientoService movimientoService, ILogger<MovimientosController> logger)
        {
            _movimientoService = movimientoService;
            _logger = logger;
        }

        [HttpPost("filtrar")]
        public async Task<ActionResult<List<MovimientoReactivoDto>>> ListarMovimientos([FromBody] MovimientoReactivoFiltroDto filtro)
        {
            try
            {
                var movimientos = await _movimientoService.ListarMovimientosAsync(filtro);
                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar los movimientos de reactivos.");
                return StatusCode(500, "Ocurrió un error interno al obtener los movimientos.");
            }
        }

        [HttpPost("buscar")]
        public async Task<ActionResult<ResultadoPaginadoDto<MovimientoReactivoDto>>> ListarMovimientosPaginados([FromBody] MovimientoReactivoFiltroDto filtro)
        {
            try
            {
                var result = await _movimientoService.ListarMovimientosPaginadosAsync(filtro);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar movimientos paginados.");
                return StatusCode(500, "Ocurrió un error interno al obtener los movimientos.");
            }
        }
    }
}
