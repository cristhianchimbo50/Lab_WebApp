using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_APIRest.Services.Pagos;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _service;

        public PagosController(IPagoService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<PagoDto>> RegistrarPago(PagoDto dto)
        {
            var pago = await _service.RegistrarPagoAsync(dto);
            if (pago == null) return BadRequest();
            return Ok(pago);
        }

        [HttpGet("orden/{idOrden}")]
        public async Task<ActionResult<List<PagoDto>>> ListarPagosPorOrden(int idOrden)
        {
            var pagos = await _service.ListarPagosPorOrdenAsync(idOrden);
            return Ok(pagos);
        }
    }
}
