using Lab_APIRest.Services.Ordenes;
using Lab_APIRest.Services.Resultados;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Resultados;
using Lab_Contracts.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

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

        private string? ObtenerRol()
        {
            string? GetClaim(string type) => User.FindFirst(type)?.Value;
            var role = GetClaim(ClaimTypes.Role)
                ?? GetClaim("IdRol")
                ?? GetClaim("role")
                ?? GetClaim("roles");

            if (string.IsNullOrEmpty(role))
            {
                role = User.Claims.FirstOrDefault(c => c.Type.Equals("idrol", StringComparison.OrdinalIgnoreCase))?.Value
                    ?? User.Claims.FirstOrDefault(c => c.Type.Equals("role", StringComparison.OrdinalIgnoreCase))?.Value
                    ?? User.Claims.FirstOrDefault(c => c.Type.Equals("roles", StringComparison.OrdinalIgnoreCase))?.Value;
            }

            return role;
        }

        private static string NormalizarRol(string? rol)
        {
            if (string.IsNullOrWhiteSpace(rol)) return string.Empty;
            var upper = rol.Trim().ToUpperInvariant();
            return upper switch
            {
                "ADMIN" or "ADMINISTRADOR" => "1",
                "RECEPCIONISTA" => "2",
                "LABORATORISTA" => "3",
                "PACIENTE" => "4",
                _ => upper
            };
        }

        private bool TieneRol(params string[] roles)
        {
            var rol = NormalizarRol(ObtenerRol());
            if (string.IsNullOrEmpty(rol)) return false;
            return roles.Select(NormalizarRol).Any(r => r == rol);
        }

        private bool EsPaciente() => TieneRol("4");

        private int? ObtenerIdPacienteClaim()
        {
            var claim = User.FindFirst("IdPaciente")?.Value
                ?? User.Claims.FirstOrDefault(c => c.Type.Equals("idpaciente", StringComparison.OrdinalIgnoreCase))?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private ActionResult? AsegurarPacienteActual(out int idPaciente)
        {
            idPaciente = ObtenerIdPacienteClaim() ?? -1;
            return idPaciente > 0 ? null : Forbid();
        }

        private ActionResult? AsegurarPacienteActual(int idPacienteRuta, out int idPaciente)
        {
            var validacion = AsegurarPacienteActual(out idPaciente);
            if (validacion != null) return validacion;
            return idPacienteRuta == idPaciente ? null : Forbid();
        }

        [Authorize(Roles = "1,2,3")]
        [HttpGet]
        public async Task<IActionResult> ListarOrdenes()
        {
            var lista = await _ordenService.ListarOrdenesAsync();
            return Ok(lista);
        }

        [Authorize(Roles = "1,2,3,4")]
        [HttpPost("buscar")]
        public async Task<IActionResult> ListarOrdenesPaginados([FromBody] OrdenFiltroDto filtro)
        {
            if (EsPaciente())
            {
                var validacion = AsegurarPacienteActual(out var idPaciente);
                if (validacion != null) return validacion;

                filtro.IdPaciente = idPaciente;
                filtro.IncluirAnuladas = false;
                filtro.IncluirVigentes = true;
            }

            var resultado = await _ordenService.ListarOrdenesPaginadosAsync(filtro);
            return Ok(resultado);
        }

        [Authorize(Roles = "1,2,3,4")]
        [HttpGet("detalle/{idOrden}")]
        public async Task<IActionResult> ObtenerDetalleOrden(int idOrden)
        {
            if (EsPaciente())
            {
                var validacion = AsegurarPacienteActual(out var idPaciente);
                if (validacion != null) return validacion;

                var detallePaciente = await _ordenService.ObtenerDetalleOrdenAsync(idOrden);
                if (detallePaciente == null) return NotFound();
                if (detallePaciente.IdPaciente != idPaciente) return Forbid();
                return Ok(detallePaciente);
            }

            var detalle = await _ordenService.ObtenerDetalleOrdenAsync(idOrden);
            if (detalle == null) return NotFound();
            return Ok(detalle);
        }

        [Authorize(Roles = "1,2")]
        [HttpPost]
        public async Task<IActionResult> GuardarOrden([FromBody] OrdenCompletaDto datosOrden)
        {
            var respuesta = await _ordenService.GuardarOrdenAsync(datosOrden);
            if (respuesta == null) return BadRequest();
            return Ok(respuesta);
        }

        [Authorize(Roles = "1")]
        [HttpPut("anular/{idOrden}")]
        public async Task<IActionResult> AnularOrden(int idOrden)
        {
            var ok = await _ordenService.AnularOrdenAsync(idOrden);
            if (!ok) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "1,2,3,4")]
        [HttpGet("{idOrden}/ticket-pdf")]
        public async Task<IActionResult> GenerarOrdenTicketPdf(int idOrden)
        {
            var pdf = await _ordenService.GenerarOrdenTicketPdfAsync(idOrden);
            if (pdf == null) return NotFound("Orden no encontrada.");
            return File(pdf, "application/pdf", $"orden_{idOrden}_ticket.pdf");
        }

        [Authorize(Roles = "1,3")]
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

        [Authorize(Roles = "1")]
        [HttpPut("anular-completo/{idOrden}")]
        public async Task<IActionResult> AnularOrdenCompleta(int idOrden)
        {
            var ok = await _ordenService.AnularOrdenCompletaAsync(idOrden);
            if (!ok) return NotFound("No se pudo anular la orden o ya estaba anulada.");
            return Ok(new { Mensaje = "Orden anulada correctamente junto con sus detalles, pagos y resultados." });
        }

        [Authorize(Roles = "4")]
        [HttpGet("paciente/{idPaciente}")]
        public async Task<IActionResult> ListarOrdenesPorPaciente(int idPaciente)
        {
            var validacion = AsegurarPacienteActual(idPaciente, out _);
            if (validacion != null) return validacion;
            var lista = await _ordenService.ListarOrdenesPorPacienteAsync(idPaciente);
            return Ok(lista);
        }

        [Authorize(Roles = "4")]
        [HttpGet("paciente/{idPaciente}/detalle/{idOrden}")]
        public async Task<IActionResult> ObtenerDetalleOrdenPaciente(int idPaciente, int idOrden)
        {
            var validacion = AsegurarPacienteActual(idPaciente, out _);
            if (validacion != null) return validacion;
            var detalle = await _ordenService.ObtenerDetalleOrdenAsync(idOrden);
            if (detalle == null) return NotFound();
            var tieneSaldo = detalle.SaldoPendiente > 0;
            return Ok(new { DetalleOrden = detalle, TieneSaldoPendiente = tieneSaldo });
        }

        [Authorize(Roles = "1,2,3")]
        [HttpPost("{idOrden}/verificar-notificacion")]
        public async Task<IActionResult> VerificarYNotificarResultadosCompletos(int idOrden)
        {
            await _ordenService.VerificarYNotificarResultadosCompletosAsync(idOrden);
            return Ok(new { Mensaje = "Verificación de resultados completada." });
        }

        [Authorize(Roles = "4")]
        [HttpGet("paciente/{idPaciente}/resumen")]
        public async Task<IActionResult> ObtenerResumenPaciente(int idPaciente)
        {
            var validacion = AsegurarPacienteActual(idPaciente, out _);
            if (validacion != null) return validacion;
            var resumen = await _ordenService.ObtenerDashboardPacienteAsync(idPaciente);
            return Ok(resumen);
        }

        [Authorize(Roles = "1,3")]
        [HttpGet("laboratorista/resumen")]
        public async Task<IActionResult> ObtenerResumenLaboratorista()
        {
            var data = await _ordenService.ObtenerDashboardLaboratoristaAsync();
            return Ok(data);
        }

        [Authorize(Roles = "1")]
        [HttpGet("administrador/resumen")]
        public async Task<IActionResult> ObtenerResumenAdministrador()
        {
            var data = await _ordenService.ObtenerDashboardAdministradorAsync();
            return Ok(data);
        }

        [Authorize(Roles = "1,2")]
        [HttpGet("recepcionista/resumen")]
        public async Task<IActionResult> ObtenerResumenRecepcionista()
        {
            var data = await _ordenService.ObtenerDashboardRecepcionistaAsync();
            return Ok(data);
        }
    }
}
