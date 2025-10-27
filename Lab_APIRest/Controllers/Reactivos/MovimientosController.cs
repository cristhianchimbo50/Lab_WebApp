using Lab_Contracts.Reactivos;
using Lab_APIRest.Services.Reactivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IMovimientoService _service;
        private readonly ILogger<MovimientosController> _logger;

        public MovimientosController(IMovimientoService service, ILogger<MovimientosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("filtrar")]
        public async Task<ActionResult<List<MovimientoReactivoDto>>> FiltrarMovimientos([FromBody] MovimientoReactivoFiltroDto filtro)
        {
            try
            {
                var lista = await _service.GetMovimientosAsync(filtro);
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al filtrar los movimientos de reactivos.");
                return StatusCode(500, "Ocurrió un error interno al obtener los movimientos.");
            }
        }
    }
}
