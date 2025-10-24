using Lab_APIRest.Services.Convenios;
using Lab_Contracts.Convenios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Convenios
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,recepcionista")]
    public class ConveniosController : ControllerBase
    {
        private readonly IConvenioService _service;
        private readonly ILogger<ConveniosController> _logger;

        public ConveniosController(IConvenioService service, ILogger<ConveniosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConvenioDto>>> ObtenerConvenios()
        {
            var convenios = await _service.ObtenerConveniosAsync();
            return Ok(convenios);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ConvenioDetalleDto>> ObtenerDetalle(int id)
        {
            var detalle = await _service.ObtenerDetalleConvenioAsync(id);
            if (detalle == null)
                return NotFound("No se encontró el convenio solicitado.");
            return Ok(detalle);
        }

        [HttpGet("ordenes-disponibles/{idMedico:int}")]
        public async Task<ActionResult<IEnumerable<OrdenDisponibleDto>>> ObtenerOrdenesDisponibles(int idMedico)
        {
            var ordenes = await _service.ObtenerOrdenesDisponiblesAsync(idMedico);
            return Ok(ordenes);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost]
        public async Task<ActionResult> RegistrarConvenio([FromBody] ConvenioRegistroDto dto)
        {
            try
            {
                var exito = await _service.RegistrarConvenioAsync(dto);
                if (!exito)
                    return BadRequest("No se pudo registrar el convenio.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar un nuevo convenio.");
                return StatusCode(500, "Ocurrió un error interno al registrar el convenio.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("{id:int}/anular")]
        public async Task<ActionResult> AnularConvenio(int id)
        {
            try
            {
                var exito = await _service.AnularConvenioAsync(id);
                if (!exito)
                    return NotFound("No se encontró el convenio a anular.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular el convenio con ID {id}.");
                return StatusCode(500, "Ocurrió un error interno al anular el convenio.");
            }
        }
    }
}
