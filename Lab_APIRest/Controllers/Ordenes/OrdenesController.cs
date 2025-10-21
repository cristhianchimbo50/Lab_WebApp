using Lab_APIRest.Services.Ordenes;
using Lab_APIRest.Services.Resultados;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenesController : ControllerBase
    {
        private readonly IOrdenService _ordenService;
        private readonly IResultadoService _resultadoService;
        private readonly ILogger<OrdenesController> _logger;

        public OrdenesController(IOrdenService ordenService, IResultadoService resultadoService, ILogger<OrdenesController> logger)
        {
            _ordenService = ordenService;
            _resultadoService = resultadoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrdenDto>>> Listar()
        {
            var ordenes = await _ordenService.ListarOrdenesAsync();
            return Ok(ordenes);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrdenDto>> Obtener(int id)
        {
            var orden = await _ordenService.ObtenerOrdenPorIdAsync(id);
            if (orden == null) return NotFound("Orden no encontrada.");
            return Ok(orden);
        }

        [HttpPost]
        public async Task<ActionResult<OrdenRespuestaDto>> Crear([FromBody] OrdenCompletaDto dto)
        {
            if (dto == null || dto.Orden == null)
                return BadRequest("Datos inválidos.");

            var result = await _ordenService.CrearOrdenAsync(dto);
            if (result == null)
                return StatusCode(500, "No se pudo crear la orden.");

            return CreatedAtAction(nameof(Obtener), new { id = result.IdOrden }, result);
        }

        [HttpPut("anular-completo/{id:int}")]
        public async Task<IActionResult> AnularCompleta(int id)
        {
            var ok = await _ordenService.AnularOrdenCompletaAsync(id);
            if (!ok) return NotFound("Orden no encontrada o ya anulada.");
            return Ok(new { mensaje = "Orden anulada correctamente." });
        }

        [HttpGet("{id:int}/ticket-pdf")]
        public async Task<IActionResult> TicketPdf(int id)
        {
            var pdf = await _ordenService.GenerarTicketOrdenAsync(id);
            if (pdf == null) return NotFound("No se pudo generar el PDF.");
            return File(pdf, "application/pdf", $"orden_{id}_ticket.pdf");
        }

        [HttpPost("ingresar-resultado")]
        public async Task<IActionResult> IngresarResultado([FromBody] ResultadoGuardarDto dto)
        {
            if (dto == null) return BadRequest("Datos inválidos.");
            var ok = await _resultadoService.GuardarResultadosAsync(dto);
            return ok ? Ok(new { mensaje = "Resultados guardados correctamente." }) : BadRequest("Error al guardar resultados.");
        }
    }
}
