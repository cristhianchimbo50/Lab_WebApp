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
    public class OrdenesController : ControllerBase
    {
        private readonly IOrdenService _ordenService;
        private readonly IResultadoService _resultadoService;

        public OrdenesController(IOrdenService ordenService, IResultadoService resultadoService)
        {
            _ordenService = ordenService;
            _resultadoService = resultadoService;
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
        [HttpGet]
        public async Task<IActionResult> GetOrdenes()
        {
            var data = await _ordenService.GetOrdenesAsync();
            return Ok(data);
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
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

        [Authorize(Roles = "administrador,recepcionista,laboratorista,paciente")]
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
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    mensaje = "Modelo inválido o datos mal formateados.",
                    errores
                });
            }

            try
            {
                var ok = await _resultadoService.GuardarResultadosAsync(dto);
                if (ok)
                    return Ok(new { mensaje = "Resultados guardados correctamente." });
                else
                    return BadRequest(new { mensaje = "No se pudo guardar los resultados en la base de datos." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error interno en el servidor.",
                    detalle = ex.Message,
                    stack = ex.StackTrace
                });
            }
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

        //Para pacientes

        [Authorize(Roles = "paciente")]
        [HttpGet("paciente/{idPaciente}")]
        public async Task<IActionResult> GetOrdenesPorPaciente(int idPaciente)
        {
            var userId = User.FindFirst("IdPaciente")?.Value;
            if (userId == null || userId != idPaciente.ToString())
                return Forbid();

            var data = await _ordenService.GetOrdenesPorPacienteAsync(idPaciente);
            return Ok(data);
        }

        [Authorize(Roles = "paciente")]
        [HttpGet("paciente/{idPaciente}/detalle/{idOrden}")]
        public async Task<IActionResult> ObtenerDetalleOrdenPaciente(int idPaciente, int idOrden)
        {
            var userId = User.FindFirst("IdPaciente")?.Value;
            if (userId == null || userId != idPaciente.ToString())
                return Forbid();

            var dto = await _ordenService.ObtenerDetalleOrdenOriginalAsync(idOrden);
            if (dto == null)
                return NotFound();

            var tieneSaldoPendiente = dto.SaldoPendiente > 0;
            return Ok(new
            {
                dto,
                TieneSaldoPendiente = tieneSaldoPendiente
            });
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
        [HttpPost("{idOrden}/verificar-notificacion")]
        public async Task<IActionResult> VerificarNotificacion(int idOrden)
        {
            await _ordenService.VerificarYNotificarResultadosCompletosAsync(idOrden);
            return Ok(new { mensaje = "Verificación de resultados completada." });
        }



    }
}
