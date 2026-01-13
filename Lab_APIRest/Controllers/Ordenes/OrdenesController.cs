using Lab_APIRest.Services.Ordenes;
using Lab_APIRest.Services.Resultados;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Resultados;
using Lab_Contracts.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
        public async Task<IActionResult> ListarOrdenes()
        {
            var lista = await _ordenService.ListarOrdenesAsync();
            return Ok(lista);
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista,paciente")]
        [HttpPost("buscar")]
        public async Task<IActionResult> ListarOrdenesPaginados([FromBody] OrdenFiltroDto filtro)
        {
            if (User.IsInRole("paciente"))
            {
                var idPacienteClaim = User.FindFirst("IdPaciente")?.Value;
                if (!int.TryParse(idPacienteClaim, out var idPaciente))
                    return Forbid();

                filtro.IdPaciente = idPaciente;
                filtro.IncluirAnuladas = false;
                filtro.IncluirVigentes = true;
            }

            var resultado = await _ordenService.ListarOrdenesPaginadosAsync(filtro);
            return Ok(resultado);
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
        [HttpGet("detalle/{idOrden}")]
        public async Task<IActionResult> ObtenerDetalleOrden(int idOrden)
        {
            var detalle = await _ordenService.ObtenerDetalleOrdenAsync(idOrden);
            if (detalle == null) return NotFound();
            return Ok(detalle);
        }

        [Authorize(Roles = "administrador,recepcionista")]
        [HttpPost]
        public async Task<IActionResult> GuardarOrden([FromBody] OrdenCompletaDto datosOrden)
        {
            var respuesta = await _ordenService.GuardarOrdenAsync(datosOrden);
            if (respuesta == null) return BadRequest();
            return Ok(respuesta);
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular/{idOrden}")]
        public async Task<IActionResult> AnularOrden(int idOrden)
        {
            var ok = await _ordenService.AnularOrdenAsync(idOrden);
            if (!ok) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista,paciente")]
        [HttpGet("{idOrden}/ticket-pdf")]
        public async Task<IActionResult> GenerarOrdenTicketPdf(int idOrden)
        {
            var pdf = await _ordenService.GenerarOrdenTicketPdfAsync(idOrden);
            if (pdf == null) return NotFound("Orden no encontrada.");
            return File(pdf, "application/pdf", $"orden_{idOrden}_ticket.pdf");
        }

        [Authorize(Roles = "administrador,laboratorista")]
        [HttpPost("ingresar-resultado")]
        public async Task<IActionResult> GuardarResultadosOrden([FromBody] ResultadoGuardarDto datosResultado)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { Mensaje = "Modelo inválido o datos mal formateados.", Errores = errores });
            }
            try
            {
                var ok = await _resultadoService.GuardarResultadosAsync(datosResultado);
                if (ok) return Ok(new { Mensaje = "Resultados guardados correctamente." });
                return BadRequest(new { Mensaje = "No se pudo guardar los resultados en la base de datos." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Mensaje = "Error interno en el servidor.", Detalle = ex.Message, Stack = ex.StackTrace });
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular-completo/{idOrden}")]
        public async Task<IActionResult> AnularOrdenCompleta(int idOrden)
        {
            var ok = await _ordenService.AnularOrdenCompletaAsync(idOrden);
            if (!ok) return NotFound("No se pudo anular la orden o ya estaba anulada.");
            return Ok(new { Mensaje = "Orden anulada correctamente junto con sus detalles, pagos y resultados." });
        }

        [Authorize(Roles = "paciente")]
        [HttpGet("paciente/{idPaciente}")]
        public async Task<IActionResult> ListarOrdenesPorPaciente(int idPaciente)
        {
            var userId = User.FindFirst("IdPaciente")?.Value;
            if (userId == null || userId != idPaciente.ToString()) return Forbid();
            var lista = await _ordenService.ListarOrdenesPorPacienteAsync(idPaciente);
            return Ok(lista);
        }

        [Authorize(Roles = "paciente")]
        [HttpGet("paciente/{idPaciente}/detalle/{idOrden}")]
        public async Task<IActionResult> ObtenerDetalleOrdenPaciente(int idPaciente, int idOrden)
        {
            var userId = User.FindFirst("IdPaciente")?.Value;
            if (userId == null || userId != idPaciente.ToString()) return Forbid();
            var detalle = await _ordenService.ObtenerDetalleOrdenAsync(idOrden);
            if (detalle == null) return NotFound();
            var tieneSaldo = detalle.SaldoPendiente > 0;
            return Ok(new { DetalleOrden = detalle, TieneSaldoPendiente = tieneSaldo });
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
        [HttpPost("{idOrden}/verificar-notificacion")]
        public async Task<IActionResult> VerificarYNotificarResultadosCompletos(int idOrden)
        {
            await _ordenService.VerificarYNotificarResultadosCompletosAsync(idOrden);
            return Ok(new { Mensaje = "Verificación de resultados completada." });
        }

        [Authorize(Roles = "paciente")]
        [HttpGet("paciente/{idPaciente}/resumen")]
        public async Task<IActionResult> ObtenerResumenPaciente(int idPaciente)
        {
            var userId = User.FindFirst("IdPaciente")?.Value;
            if (userId == null || userId != idPaciente.ToString()) return Forbid();
            var resumen = await _ordenService.ObtenerDashboardPacienteAsync(idPaciente);
            return Ok(resumen);
        }

        [Authorize(Roles = "laboratorista,administrador")]
        [HttpGet("laboratorista/resumen")]
        public async Task<IActionResult> ObtenerResumenLaboratorista()
        {
            var data = await _ordenService.ObtenerDashboardLaboratoristaAsync();
            return Ok(data);
        }

        [Authorize(Roles = "administrador")]
        [HttpGet("administrador/resumen")]
        public async Task<IActionResult> ObtenerResumenAdministrador()
        {
            var data = await _ordenService.ObtenerDashboardAdministradorAsync();
            return Ok(data);
        }

        [Authorize(Roles = "recepcionista,administrador")]
        [HttpGet("recepcionista/resumen")]
        public async Task<IActionResult> ObtenerResumenRecepcionista()
        {
            var data = await _ordenService.ObtenerDashboardRecepcionistaAsync();
            return Ok(data);
        }
    }
}
