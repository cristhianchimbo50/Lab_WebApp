using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamenComposicionController : ControllerBase
    {
        private readonly IExamenComposicionService _service;

        public ExamenComposicionController(IExamenComposicionService service)
        {
            _service = service;
        }

        [HttpGet("padre/{id}")]
        public async Task<ActionResult<List<ExamenComposicionDto>>> GetPorPadre(int id)
        {
            var list = await _service.GetComposicionesPorExamenPadreAsync(id);
            return Ok(list);
        }

        [HttpGet("hijo/{id}")]
        public async Task<ActionResult<List<ExamenComposicionDto>>> GetPorHijo(int id)
        {
            var list = await _service.GetComposicionesPorExamenHijoAsync(id);
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ExamenComposicionDto dto)
        {
            var ok = await _service.CrearComposicionAsync(dto);
            if (!ok) return Conflict("Ya existe esta composición.");
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int idExamenPadre, [FromQuery] int idExamenHijo)
        {
            var ok = await _service.EliminarComposicionAsync(idExamenPadre, idExamenHijo);
            if (!ok) return NotFound();
            return Ok();
        }
    }
}
