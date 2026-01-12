using Lab_APIRest.Services.Resultados;
using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Lab_Contracts.Common;
using System.Linq;

namespace Lab_APIRest.Controllers.Resultados
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultadosController : ControllerBase
    {
        private readonly IResultadoService _resultadoService;
        private readonly ILogger<ResultadosController> _logger;

        public ResultadosController(IResultadoService resultadoService, ILogger<ResultadosController> logger)
        {
            _resultadoService = resultadoService;
            _logger = logger;
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
        [HttpGet]
        public async Task<ActionResult<List<ResultadoListadoDto>>> ListarResultados([FromQuery] string? numeroResultado, [FromQuery] string? numeroOrden, [FromQuery] string? cedula, [FromQuery] string? nombre, [FromQuery] DateTime? fechaDesde, [FromQuery] DateTime? fechaHasta, [FromQuery] bool? anulado)
        {
            try
            {
                var filtro = new ResultadoFiltroDto
                {
                    NumeroResultado = numeroResultado,
                    NumeroOrden = numeroOrden,
                    Cedula = cedula,
                    Nombre = nombre,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    Anulado = anulado
                };
                var lista = await _resultadoService.ListarResultadosAsync(filtro);
                return Ok(lista);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar resultados.");
                return StatusCode(500, "Error interno al listar resultados.");
            }
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista,paciente")]
        [HttpPost("buscar")]
        public async Task<ActionResult<ResultadoPaginadoDto<ResultadoListadoDto>>> ListarResultadosPaginados([FromBody] ResultadoFiltroDto filtro)
        {
            try
            {
                var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                if (rol == "paciente")
                {
                    var idPacienteClaim = User.FindFirst("IdPaciente")?.Value;
                    if (string.IsNullOrEmpty(idPacienteClaim)) return Forbid();
                    filtro.IdPaciente = int.Parse(idPacienteClaim);
                }
                var result = await _resultadoService.ListarResultadosPaginadosAsync(filtro);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar resultados paginados.");
                return StatusCode(500, "Error interno al buscar resultados.");
            }
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResultadoDetalleDto>> ObtenerDetalleResultado(int id)
        {
            try
            {
                var detalle = await _resultadoService.ObtenerDetalleResultadoAsync(id);
                if (detalle == null) return NotFound("Resultado no encontrado.");
                return Ok(detalle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener resultado {id}.");
                return StatusCode(500, "Error interno al obtener el resultado.");
            }
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista,paciente")]
        [HttpGet("pdf-multiple")]
        public async Task<IActionResult> GenerarResultadosPdf([FromQuery] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest("Debe proporcionar al menos un ID de resultado.");
            try
            {
                var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var idPacienteClaim = User.FindFirst("IdPaciente")?.Value;
                if (rol == "paciente")
                {
                    if (string.IsNullOrEmpty(idPacienteClaim)) return Forbid();
                    var idPaciente = int.Parse(idPacienteClaim);
                    var resultadosPaciente = await _resultadoService.ListarResultadosAsync(new ResultadoFiltroDto());
                    var idsPropios = resultadosPaciente.Where(r => r.IdPaciente == idPaciente).Select(r => r.IdResultado).ToHashSet();
                    if (ids.Any(rid => !idsPropios.Contains(rid))) return Forbid("Intento de acceso a resultados de otro paciente.");
                }
                var pdfBytes = await _resultadoService.GenerarResultadosPdfAsync(ids);
                if (pdfBytes == null) return NotFound("No se encontraron resultados válidos.");
                return File(pdfBytes, "application/pdf", $"Resultados_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de resultados.");
                return StatusCode(500, "Error interno al generar PDF.");
            }
        }

        [Authorize(Roles = "administrador,laboratorista")]
        [HttpPut("anular/{id:int}")]
        public async Task<IActionResult> AnularResultado(int id)
        {
            try
            {
                var ok = await _resultadoService.AnularResultadoAsync(id);
                if (!ok) return NotFound("No se encontró el resultado o ya está anulado.");
                return Ok(new { mensaje = "Resultado anulado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular resultado {id}.");
                return StatusCode(500, "Error interno al anular el resultado.");
            }
        }

        [Authorize(Roles = "administrador,laboratorista")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarResultado(int id, [FromBody] ResultadoActualizarDto dto)
        {
            if (dto == null || id != dto.IdResultado) return BadRequest("Datos de actualización inválidos.");
            try
            {
                var ok = await _resultadoService.ActualizarResultadoAsync(dto);
                if (!ok) return BadRequest(new { mensaje = "No se pudo actualizar el resultado. Verifique estado y datos." });
                return Ok(new { mensaje = "Resultado actualizado y enviado a revisión." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al actualizar resultado {id}.");
                return StatusCode(500, "Error interno al actualizar el resultado.");
            }
        }

        [Authorize(Roles = "paciente")]
        [HttpGet("mis-resultados")]
        public async Task<ActionResult<List<ResultadoListadoDto>>> ListarResultadosPaciente()
        {
            try
            {
                var idPacienteClaim = User.FindFirst("IdPaciente")?.Value;
                if (string.IsNullOrEmpty(idPacienteClaim)) return Forbid();
                var idPaciente = int.Parse(idPacienteClaim);
                var lista = await _resultadoService.ListarResultadosAsync(new ResultadoFiltroDto());
                var propios = lista.Where(r => r.IdPaciente == idPaciente).ToList();
                return Ok(propios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar resultados del paciente.");
                return StatusCode(500, "Error interno al listar resultados del paciente.");
            }
        }

        [Authorize(Roles = "paciente")]
        [HttpGet("mi-detalle/{id:int}")]
        public async Task<ActionResult<ResultadoDetalleDto>> ObtenerDetalleResultadoPaciente(int id)
        {
            try
            {
                var idPacienteClaim = User.FindFirst("IdPaciente")?.Value;
                if (string.IsNullOrEmpty(idPacienteClaim)) return Forbid();
                var idPaciente = int.Parse(idPacienteClaim);
                var detalle = await _resultadoService.ObtenerDetalleResultadoAsync(id);
                if (detalle == null || detalle.IdPaciente != idPaciente) return Forbid("Intento de acceso a resultados de otro paciente.");
                return Ok(detalle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener detalle del resultado {id} para paciente.");
                return StatusCode(500, "Error interno al obtener detalle del resultado.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("{id:int}/revision")]
        public async Task<IActionResult> RevisarResultado(int id, [FromBody] ResultadoRevisionDto revision)
        {
            if (revision == null) return BadRequest("Datos de revisión inválidos.");
            var idRevisorClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idRevisorClaim) || !int.TryParse(idRevisorClaim, out var idRevisor))
                return Forbid();

            try
            {
                var ok = await _resultadoService.RevisarResultadoAsync(id, revision.EstadoResultado, revision.ObservacionRevision, idRevisor);
                if (!ok) return NotFound("No se encontró el resultado o ya fue anulado.");
                return Ok(new { mensaje = "Resultado actualizado correctamente." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al revisar resultado {id}.");
                return StatusCode(500, "Error interno al actualizar el resultado.");
            }
        }
    }
}
