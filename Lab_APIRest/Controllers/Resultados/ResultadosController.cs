using Lab_APIRest.Services.Resultados;
using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<ActionResult<List<ResultadoListadoDto>>> Listar(
            [FromQuery] string? numeroResultado,
            [FromQuery] string? cedula,
            [FromQuery] string? nombre,
            [FromQuery] DateTime? fechaDesde,
            [FromQuery] DateTime? fechaHasta,
            [FromQuery] bool? anulado)
        {
            try
            {
                var filtro = new ResultadoFiltroDto
                {
                    NumeroResultado = numeroResultado,
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

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResultadoDetalleDto>> Obtener(int id)
        {
            try
            {
                var detalle = await _resultadoService.ObtenerDetalleResultadoAsync(id);
                return detalle == null
                    ? NotFound("Resultado no encontrado.")
                    : Ok(detalle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener resultado {id}.");
                return StatusCode(500, "Error interno al obtener el resultado.");
            }
        }

        [HttpGet("pdf-multiple")]
        public async Task<IActionResult> PdfMultiple([FromQuery] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("Debe proporcionar al menos un ID de resultado.");

            try
            {
                var pdfBytes = await _resultadoService.GenerarResultadosPdfAsync(ids);
                if (pdfBytes == null)
                    return NotFound("No se encontraron resultados válidos.");

                return File(pdfBytes, "application/pdf", $"Resultados_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar PDF de resultados.");
                return StatusCode(500, "Error interno al generar PDF.");
            }
        }

        [HttpPut("anular/{id:int}")]
        public async Task<IActionResult> Anular(int id)
        {
            try
            {
                var ok = await _resultadoService.AnularResultadoAsync(id);
                return ok
                    ? Ok(new { mensaje = "Resultado anulado correctamente." })
                    : NotFound("No se encontró el resultado o ya está anulado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular resultado {id}.");
                return StatusCode(500, "Error interno al anular el resultado.");
            }
        }
    }
}
