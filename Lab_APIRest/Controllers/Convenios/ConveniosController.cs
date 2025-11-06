using Lab_APIRest.Services.Convenios;
using Lab_Contracts.Convenios;
using Lab_Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Convenios
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,recepcionista")]
    public class ConveniosController : ControllerBase
    {
        private readonly IConvenioService _convenioService;
        private readonly ILogger<ConveniosController> _logger;

        public ConveniosController(IConvenioService convenioService, ILogger<ConveniosController> logger)
        {
            _convenioService = convenioService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConvenioDto>>> ListarConvenios()
        {
            var lista = await _convenioService.ListarConveniosAsync();
            return Ok(lista);
        }

        [HttpPost("buscar")]
        public async Task<ActionResult<ResultadoPaginadoDto<ConvenioDto>>> ListarConveniosPaginados([FromBody] ConvenioFiltroDto filtro)
        {
            var result = await _convenioService.ListarConveniosPaginadosAsync(filtro);
            return Ok(result);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<ResultadoPaginadoDto<ConvenioDto>>> ListarConveniosPaginados([FromQuery] string? criterio, [FromQuery] string? valor, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _convenioService.ListarConveniosPaginadosAsync(criterio, valor, desde, hasta, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{idConvenio:int}")]
        public async Task<ActionResult<ConvenioDetalleDto>> ObtenerDetalleConvenio(int idConvenio)
        {
            var detalle = await _convenioService.ObtenerDetalleConvenioAsync(idConvenio);
            if (detalle == null) return NotFound("No se encontró el convenio solicitado.");
            return Ok(detalle);
        }

        [HttpGet("ordenes-disponibles/{idMedico:int}")]
        public async Task<ActionResult<IEnumerable<OrdenDisponibleDto>>> ListarOrdenesDisponibles(int idMedico)
        {
            var ordenes = await _convenioService.ListarOrdenesDisponiblesAsync(idMedico);
            return Ok(ordenes);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost]
        public async Task<ActionResult> GuardarConvenio([FromBody] ConvenioRegistroDto convenioRegistro)
        {
            try
            {
                var ok = await _convenioService.GuardarConvenioAsync(convenioRegistro);
                if (!ok) return BadRequest("No se pudo registrar el convenio.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar un nuevo convenio.");
                return StatusCode(500, "Ocurrió un error interno al registrar el convenio.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("{idConvenio:int}/anular")]
        public async Task<ActionResult> AnularConvenio(int idConvenio)
        {
            try
            {
                var ok = await _convenioService.AnularConvenioAsync(idConvenio);
                if (!ok) return NotFound("No se encontró el convenio a anular.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular el convenio con ID {idConvenio}.");
                return StatusCode(500, "Ocurrió un error interno al anular el convenio.");
            }
        }
    }
}
