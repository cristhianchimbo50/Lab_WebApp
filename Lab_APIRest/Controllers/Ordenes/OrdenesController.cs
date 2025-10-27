using Lab_APIRest.Services.Ordenes;
using Lab_APIRest.Services.Resultados;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,recepcionista,laboratorista")]
    public class OrdenesController : ControllerBase
    {
        private readonly IOrdenService _ordenService;
        private readonly IResultadoService _resultadoService;

        public OrdenesController(IOrdenService ordenService, IResultadoService resultadoService)
        {
            _ordenService = ordenService;
            _resultadoService = resultadoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOrdenes()
        {
            var data = await _ordenService.GetOrdenesAsync();
            return Ok(data);
        }

        [HttpGet("detalle/{id}")]
        public async Task<IActionResult> ObtenerDetalleOrden(int id)
        {
            var dto = await _ordenService.ObtenerDetalleOrdenOriginalAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [Authorize(Roles = "administrador,recepcionista")]
        [HttpPost]
        public async Task<IActionResult> CrearOrden([FromBody] OrdenCompletaDto dto)
        {
            var result = await _ordenService.CrearOrdenAsync(dto);
            if (result == null) return BadRequest();
            return Ok(result);
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular/{id}")]
        public async Task<IActionResult> AnularOrden(int id)
        {
            var ok = await _ordenService.AnularOrdenAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpGet("{id}/ticket-pdf")]
        public async Task<IActionResult> ObtenerTicketPdf(int id)
        {
            var pdfBytes = await _ordenService.ObtenerTicketPdfAsync(id);
            if (pdfBytes == null)
                return NotFound("Orden no encontrada.");

            return File(pdfBytes, "application/pdf", $"orden_{id}_ticket.pdf");
        }

        [Authorize(Roles = "administrador,laboratorista")]
        [HttpPost("ingresar-resultado")]
        public async Task<IActionResult> IngresarResultado([FromBody] ResultadoGuardarDto dto)
        {
            var ok = await _resultadoService.GuardarResultadosAsync(dto);
            if (ok)
                return Ok(new { mensaje = "Resultados guardados correctamente." });
            else
                return BadRequest(new { mensaje = "No se pudo guardar los resultados." });
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular-completo/{idOrden}")]
        public async Task<IActionResult> AnularOrdenCompleta(int idOrden)
        {
            var exito = await _ordenService.AnularOrdenCompletaAsync(idOrden);
            if (!exito)
                return NotFound("No se pudo anular la orden o ya estaba anulada.");

            return Ok(new { mensaje = "Orden anulada correctamente junto con sus detalles, pagos y resultados." });
        }
    }
}
