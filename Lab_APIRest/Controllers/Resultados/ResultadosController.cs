using Lab_APIRest.Services.Resultados;
using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lab_APIRest.Controllers.Resultados
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResultadosController : ControllerBase
    {
        private readonly IResultadoService ResultadoService;
        private readonly ILogger<ResultadosController> Logger;

        public ResultadosController(IResultadoService ResultadoService, ILogger<ResultadosController> Logger)
        {
            this.ResultadoService = ResultadoService;
            this.Logger = Logger;
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
        [HttpGet]
        public async Task<ActionResult<List<ResultadoListadoDto>>> Listar(
            [FromQuery] string? NumeroResultado,
            [FromQuery] string? NumeroOrden,
            [FromQuery] string? Cedula,
            [FromQuery] string? Nombre,
            [FromQuery] DateTime? FechaDesde,
            [FromQuery] DateTime? FechaHasta,
            [FromQuery] bool? Anulado)
        {
            try
            {
                var Filtro = new ResultadoFiltroDto
                {
                    NumeroResultado = NumeroResultado,
                    NumeroOrden = NumeroOrden,
                    Cedula = Cedula,
                    Nombre = Nombre,
                    FechaDesde = FechaDesde,
                    FechaHasta = FechaHasta,
                    Anulado = Anulado
                };

                var Lista = await ResultadoService.ListarResultadosAsync(Filtro);
                return Ok(Lista);
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, "Error al listar resultados.");
                return StatusCode(500, "Error interno al listar resultados.");
            }
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResultadoDetalleDto>> Obtener(int Id)
        {
            try
            {
                var Detalle = await ResultadoService.ObtenerDetalleResultadoAsync(Id);
                return Detalle == null
                    ? NotFound("Resultado no encontrado.")
                    : Ok(Detalle);
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, $"Error al obtener resultado {Id}.");
                return StatusCode(500, "Error interno al obtener el resultado.");
            }
        }

        [Authorize(Roles = "administrador,recepcionista,laboratorista,paciente")]
        [HttpGet("pdf-multiple")]
        public async Task<IActionResult> PdfMultiple([FromQuery] List<int> Ids)
        {
            if (Ids == null || !Ids.Any())
                return BadRequest("Debe proporcionar al menos un ID de resultado.");

            try
            {
                var Rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var IdPacienteClaim = User.FindFirst("IdPaciente")?.Value;

                if (Rol == "paciente")
                {
                    if (string.IsNullOrEmpty(IdPacienteClaim))
                        return Forbid();

                    var IdPaciente = int.Parse(IdPacienteClaim);

                    var ResultadosPaciente = await ResultadoService.ListarResultadosAsync(new ResultadoFiltroDto
                    {
                        Cedula = null,
                        Nombre = null
                    });

                    var IdsPropios = ResultadosPaciente
                        .Where(Resultado => Resultado.IdPaciente == IdPaciente)
                        .Select(Resultado => Resultado.IdResultado)
                        .ToHashSet();

                    if (Ids.Any(IdResultado => !IdsPropios.Contains(IdResultado)))
                        return Forbid("Intento de acceso a resultados de otro paciente.");
                }

                var PdfBytes = await ResultadoService.GenerarResultadosPdfAsync(Ids);
                if (PdfBytes == null)
                    return NotFound("No se encontraron resultados válidos.");

                return File(PdfBytes, "application/pdf", $"Resultados_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, "Error al generar PDF de resultados.");
                return StatusCode(500, "Error interno al generar PDF.");
            }
        }

        [Authorize(Roles = "administrador,laboratorista")]
        [HttpPut("anular/{id:int}")]
        public async Task<IActionResult> Anular(int Id)
        {
            try
            {
                var OkAnular = await ResultadoService.AnularResultadoAsync(Id);
                return OkAnular
                    ? Ok(new { mensaje = "Resultado anulado correctamente." })
                    : NotFound("No se encontró el resultado o ya está anulado.");
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, $"Error al anular resultado {Id}.");
                return StatusCode(500, "Error interno al anular el resultado.");
            }
        }

        [Authorize(Roles = "paciente")]
        [HttpGet("mis-resultados")]
        public async Task<ActionResult<List<ResultadoListadoDto>>> MisResultados()
        {
            try
            {
                var IdPacienteClaim = User.FindFirst("IdPaciente")?.Value;
                if (string.IsNullOrEmpty(IdPacienteClaim))
                    return Forbid();

                var IdPaciente = int.Parse(IdPacienteClaim);

                var Lista = await ResultadoService.ListarResultadosAsync(new ResultadoFiltroDto());
                var Propios = Lista.Where(Resultado => Resultado.IdPaciente == IdPaciente).ToList();

                return Ok(Propios);
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, "Error al listar resultados del paciente.");
                return StatusCode(500, "Error interno al listar resultados del paciente.");
            }
        }

        [Authorize(Roles = "paciente")]
        [HttpGet("mi-detalle/{id:int}")]
        public async Task<ActionResult<ResultadoDetalleDto>> ObtenerDetallePaciente(int Id)
        {
            try
            {
                var IdPacienteClaim = User.FindFirst("IdPaciente")?.Value;
                if (string.IsNullOrEmpty(IdPacienteClaim))
                    return Forbid();

                var IdPaciente = int.Parse(IdPacienteClaim);
                var Detalle = await ResultadoService.ObtenerDetalleResultadoAsync(Id);

                if (Detalle == null || Detalle.IdPaciente != IdPaciente)
                    return Forbid("Intento de acceso a resultados de otro paciente.");

                return Ok(Detalle);
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, $"Error al obtener detalle del resultado {Id} para paciente.");
                return StatusCode(500, "Error interno al obtener detalle del resultado.");
            }
        }
    }
}
