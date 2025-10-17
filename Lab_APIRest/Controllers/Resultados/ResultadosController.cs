using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Services.PDF;
using Lab_APIRest.Services.Resultados;
using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Controllers.Resultados;

[ApiController]
[Route("api/[controller]")]
public class ResultadosController : ControllerBase
{
    private readonly LabDbContext _context;
    private readonly IResultadoService _resultadoservice;
    private readonly PdfResultadoService _pdfResultadoService;

    public ResultadosController(LabDbContext context, IResultadoService resultadoService, PdfResultadoService pdfResultadoService)
    {
        _context = context;
        _resultadoservice = resultadoService;
        _pdfResultadoService = pdfResultadoService;
    }


    // GET: api/resultados
    [HttpGet]
    public async Task<ActionResult<List<ResultadoListadoDto>>> GetResultados(
        [FromQuery] string? numeroResultado,
        [FromQuery] string? cedula,
        [FromQuery] string? nombre,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] bool? anulado)
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

        var lista = await _resultadoservice.ListarResultadosAsync(filtro);
        return Ok(lista);
    }

    // GET: api/resultados/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ResultadoDetalleDto>> GetDetalleResultado(int id)
    {
        var detalle = await _resultadoservice.ObtenerDetalleResultadoAsync(id);
        if (detalle == null) return NotFound();
        return Ok(detalle);
    }

    [HttpGet("pdf-multiple")]
    public async Task<IActionResult> ObtenerResultadosPdf([FromQuery] List<int> ids)
    {
        if (ids == null || !ids.Any())
            return BadRequest(new { mensaje = "Debe proporcionar al menos un ID de resultado." });

        var resultados = new List<ResultadoCompletoDto>();

        foreach (var id in ids)
        {
            var resultado = await _resultadoservice.ObtenerResultadoCompletoAsync(id);
            if (resultado != null)
                resultados.Add(resultado);
        }

        if (!resultados.Any())
            return NotFound(new { mensaje = "No se encontraron resultados válidos." });

        var pdfBytes = _pdfResultadoService.GenerarResultadosPdf(resultados);

        var nombreArchivo = $"Resultado_{resultados.First().NumeroOrden}.pdf";

        return File(pdfBytes, "application/pdf", nombreArchivo);
    }

    [HttpPut("anular/{id}")]
    public async Task<IActionResult> AnularResultado(int id)
    {
        var ok = await _resultadoservice.AnularResultadoAsync(id);
        if (!ok)
            return NotFound(new { mensaje = "No se encontró el resultado" });

        return Ok(new { mensaje = "Resultado anulado correctamente" });
    }


}
