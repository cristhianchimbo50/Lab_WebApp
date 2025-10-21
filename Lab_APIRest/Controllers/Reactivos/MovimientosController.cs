using Lab_Contracts.Reactivos;
using Lab_APIRest.Services.Reactivos;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Reactivos
{
    /// <summary>
    /// Controlador responsable de gestionar los movimientos de reactivos (ingresos y egresos).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientosController : ControllerBase
    {
        private readonly IMovimientoService _service;
        private readonly ILogger<MovimientosController> _logger;

        /// <summary>
        /// Constructor del controlador de movimientos.
        /// </summary>
        /// <param name="service">Servicio de movimientos de reactivos inyectado.</param>
        /// <param name="logger">Logger para registrar errores y eventos.</param>
        public MovimientosController(IMovimientoService service, ILogger<MovimientosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Filtra los movimientos de reactivos según los criterios proporcionados.
        /// </summary>
        /// <param name="filtro">Criterios de filtrado (rango de fechas, tipo de movimiento, reactivo, etc.).</param>
        /// <returns>Lista de movimientos que cumplen el filtro.</returns>
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
