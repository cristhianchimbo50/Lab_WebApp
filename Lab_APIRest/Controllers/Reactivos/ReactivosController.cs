using Lab_Contracts.Reactivos;
using Lab_APIRest.Services.Reactivos;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Reactivos
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReactivosController : ControllerBase
    {
        private readonly IReactivoService _service;

        public ReactivosController(IReactivoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReactivoDto>>> GetAll()
        {
            var data = await _service.GetReactivosAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ReactivoDto>> GetById(int id)
        {
            var item = await _service.GetReactivoPorIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<ReactivoDto>> Create(ReactivoDto dto)
        {
            var created = await _service.CrearReactivoAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.IdReactivo }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ReactivoDto dto)
        {
            var ok = await _service.EditarReactivoAsync(id, dto);
            if (!ok) return NotFound();
            return NoContent();
        }

        [HttpPut("anular/{id}")]
        public async Task<IActionResult> Anular(int id)
        {
            var ok = await _service.AnularReactivoAsync(id);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
