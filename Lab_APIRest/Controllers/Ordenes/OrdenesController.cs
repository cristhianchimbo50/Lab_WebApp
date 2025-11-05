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
        private readonly IOrdenService ServicioOrden;
        private readonly IResultadoService ServicioResultado;

        public OrdenesController(IOrdenService ServicioOrden, IResultadoService ServicioResultado)
        {
            this.ServicioOrden = ServicioOrden;
            this.ServicioResultado = ServicioResultado;
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
        [HttpGet]
        public async Task<IActionResult> ObtenerOrdenes()
        {
            var ListaOrdenes = await ServicioOrden.ObtenerOrdenesAsync();
            return Ok(ListaOrdenes);
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
        [HttpGet("detalle/{IdOrden}")]
        public async Task<IActionResult> ObtenerDetalleOrden(int IdOrden)
        {
            var DetalleOrden = await ServicioOrden.ObtenerDetalleOrdenOriginalAsync(IdOrden);
            if (DetalleOrden == null) return NotFound();
            return Ok(DetalleOrden);
        }


        [Authorize(Roles = "administrador,recepcionista")]
        [HttpPost]
        public async Task<IActionResult> CrearOrden([FromBody] OrdenCompletaDto DatosOrden)
        {
            var RespuestaCreacion = await ServicioOrden.CrearOrdenAsync(DatosOrden);
            if (RespuestaCreacion == null) return BadRequest();
            return Ok(RespuestaCreacion);
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular/{IdOrden}")]
        public async Task<IActionResult> AnularOrden(int IdOrden)
        {
            var OkAnulado = await ServicioOrden.AnularOrdenAsync(IdOrden);
            if (!OkAnulado) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista,paciente")]
        [HttpGet("{IdOrden}/ticket-pdf")]
        public async Task<IActionResult> ObtenerTicketPdf(int IdOrden)
        {
            var PdfBytes = await ServicioOrden.ObtenerTicketPdfAsync(IdOrden);
            if (PdfBytes == null)
                return NotFound("Orden no encontrada.");

            return File(PdfBytes, "application/pdf", $"orden_{IdOrden}_ticket.pdf");
        }

        [Authorize(Roles = "administrador,laboratorista")]
        [HttpPost("ingresar-resultado")]
        public async Task<IActionResult> IngresarResultado([FromBody] ResultadoGuardarDto DatosResultado)
        {
            if (!ModelState.IsValid)
            {
                var Errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new
                {
                    Mensaje = "Modelo inválido o datos mal formateados.",
                    Errores
                });
            }

            try
            {
                var OkGuardado = await ServicioResultado.GuardarResultadosAsync(DatosResultado);
                if (OkGuardado)
                    return Ok(new { Mensaje = "Resultados guardados correctamente." });
                else
                    return BadRequest(new { Mensaje = "No se pudo guardar los resultados en la base de datos." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Mensaje = "Error interno en el servidor.",
                    Detalle = ex.Message,
                    Stack = ex.StackTrace
                });
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular-completo/{IdOrden}")]
        public async Task<IActionResult> AnularOrdenCompleta(int IdOrden)
        {
            var ExitoAnulado = await ServicioOrden.AnularOrdenCompletaAsync(IdOrden);
            if (!ExitoAnulado)
                return NotFound("No se pudo anular la orden o ya estaba anulada.");

            return Ok(new { Mensaje = "Orden anulada correctamente junto con sus detalles, pagos y resultados." });
        }

        //Para pacientes

        [Authorize(Roles = "paciente")]
        [HttpGet("paciente/{IdPaciente}")]
        public async Task<IActionResult> ObtenerOrdenesPorPaciente(int IdPaciente)
        {
            var UserId = User.FindFirst("IdPaciente")?.Value;
            if (UserId == null || UserId != IdPaciente.ToString())
                return Forbid();

            var ListaOrdenes = await ServicioOrden.ObtenerOrdenesPorPacienteAsync(IdPaciente);
            return Ok(ListaOrdenes);
        }

        [Authorize(Roles = "paciente")]
        [HttpGet("paciente/{IdPaciente}/detalle/{IdOrden}")]
        public async Task<IActionResult> ObtenerDetalleOrdenPaciente(int IdPaciente, int IdOrden)
        {
            var UserId = User.FindFirst("IdPaciente")?.Value;
            if (UserId == null || UserId != IdPaciente.ToString())
                return Forbid();

            var DetalleOrden = await ServicioOrden.ObtenerDetalleOrdenOriginalAsync(IdOrden);
            if (DetalleOrden == null)
                return NotFound();

            var TieneSaldoPendiente = DetalleOrden.SaldoPendiente > 0;
            return Ok(new
            {
                DetalleOrden,
                TieneSaldoPendiente = TieneSaldoPendiente
            });
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
        [HttpPost("{IdOrden}/verificar-notificacion")]
        public async Task<IActionResult> VerificarNotificacion(int IdOrden)
        {
            await ServicioOrden.VerificarYNotificarResultadosCompletosAsync(IdOrden);
            return Ok(new { Mensaje = "Verificación de resultados completada." });
        }



    }
}
