using Lab_APIRest.Services.Convenios;
using Lab_Contracts.Convenios;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Convenios
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConveniosController : ControllerBase
    {
        private readonly IConvenioService _service;

        public ConveniosController(IConvenioService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConvenioDto>>> GetConvenios()
        {
            var convenios = await _service.ObtenerConveniosAsync();
            return Ok(convenios);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ConvenioDetalleDto>> GetConvenioDetalle(int id)
        {
            var detalle = await _service.ObtenerDetalleConvenioAsync(id);
            if (detalle == null) return NotFound();
            return Ok(detalle);
        }

        [HttpGet("ordenes-disponibles/{idMedico}")]
        public async Task<ActionResult<IEnumerable<OrdenDisponibleDto>>> GetOrdenesDisponibles(int idMedico)
        {
            var ordenes = await _service.ObtenerOrdenesDisponiblesAsync(idMedico);
            return Ok(ordenes);
        }

        [HttpPost]
        public async Task<ActionResult> PostConvenio([FromBody] ConvenioRegistroDto dto)
        {
            var result = await _service.RegistrarConvenioAsync(dto);
            if (!result) return BadRequest("No se pudo registrar el convenio.");
            return Ok();
        }

        [HttpPut("{id}/anular")]
        public async Task<ActionResult> AnularConvenio(int id)
        {
            var result = await _service.AnularConvenioAsync(id);
            if (!result) return NotFound();
            return Ok();
        }
    }
}
