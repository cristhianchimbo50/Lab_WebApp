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
        private readonly IMovimientoService MovimientoService;
        private readonly ILogger<MovimientosController> Logger;

        public MovimientosController(IMovimientoService MovimientoService, ILogger<MovimientosController> Logger)
        {
            this.MovimientoService = MovimientoService;
            this.Logger = Logger;
        }

        [HttpPost("filtrar")]
        public async Task<ActionResult<List<MovimientoReactivoDto>>> FiltrarMovimientos([FromBody] MovimientoReactivoFiltroDto Filtro)
        {
            try
            {
                var Movimientos = await MovimientoService.ObtenerMovimientosAsync(Filtro);
                return Ok(Movimientos);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al filtrar los movimientos de reactivos.");
                return StatusCode(500, "Ocurrió un error interno al obtener los movimientos.");
            }
        }
    }
}
