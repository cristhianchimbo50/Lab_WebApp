using Lab_Contracts.Reactivos;
using Lab_APIRest.Services.Reactivos;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Reactivos
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimientosController : ControllerBase
    {
        private readonly IMovimientoService _service;

        public MovimientosController(IMovimientoService service)
        {
            _service = service;
        }

        [HttpPost("filtrar")]
        public async Task<ActionResult<List<MovimientoReactivoDto>>> FiltrarMovimientos([FromBody] MovimientoReactivoFiltroDto filtro)
        {
            var lista = await _service.GetMovimientosAsync(filtro);
            return Ok(lista);
        }
    }
}
