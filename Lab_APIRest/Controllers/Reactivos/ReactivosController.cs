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

        [HttpPost("ingresos")]
        public async Task<ActionResult> RegistrarIngresos([FromBody] IEnumerable<MovimientoReactivoIngresoDto> ingresos)
        {
            if (ingresos == null || !ingresos.Any())
                return BadRequest("No se enviaron datos de ingreso.");

            var ok = await _service.RegistrarIngresosAsync(ingresos);
            if (ok)
                return Ok(new { message = "Ingresos registrados correctamente." });
            else
                return StatusCode(500, new { message = "Error al registrar ingresos." });
        }


        [HttpPost("egresos")]
        public async Task<ActionResult> RegistrarEgresos([FromBody] IEnumerable<MovimientoReactivoEgresoDto> egresos)
        {
            if (egresos == null || !egresos.Any())
                return BadRequest("No se enviaron datos de egreso.");

            var ok = await _service.RegistrarEgresosAsync(egresos);
            if (ok)
                return Ok(new { message = "Egresos registrados correctamente." });
            else
                return StatusCode(500, new { message = "Error al registrar egresos." });
        }
    }
}
