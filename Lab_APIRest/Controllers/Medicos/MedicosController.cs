using Lab_Contracts.Medicos;
using Lab_APIRest.Services.Medicos;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Medicos
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicosController : ControllerBase
    {
        private readonly IMedicoService _service;

        public MedicosController(IMedicoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicoDto>>> GetMedicos()
        {
            var medicos = await _service.GetMedicosAsync();
            return Ok(medicos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MedicoDto>> GetMedico(int id)
        {
            var medico = await _service.GetMedicoByIdAsync(id);
            if (medico == null) return NotFound();
            return Ok(medico);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<MedicoDto>>> BuscarMedicos([FromQuery] string campo, [FromQuery] string valor)
        {
            if (string.IsNullOrWhiteSpace(campo) || string.IsNullOrWhiteSpace(valor))
                return BadRequest("Debe proporcionar campo y valor.");

            List<MedicoDto> medicos = new();

            switch (campo.ToLower())
            {
                case "nombre":
                    medicos = await _service.GetMedicosPorNombreAsync(valor);
                    break;

                case "especialidad":
                    medicos = await _service.GetMedicosPorEspecialidadAsync(valor);
                    break;

                case "correo":
                    medicos = await _service.GetMedicosPorCorreoAsync(valor);
                    break;

                default:
                    return BadRequest("Campo de búsqueda no soportado.");
            }

            return Ok(medicos);
        }

        [HttpPost]
        public async Task<ActionResult<MedicoDto>> PostMedico(MedicoDto dto)
        {
            var medico = await _service.CrearMedicoAsync(dto);
            return CreatedAtAction(nameof(GetMedico), new { id = medico.IdMedico }, medico);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutMedico(int id, MedicoDto dto)
        {
            if (id != dto.IdMedico) return BadRequest();
            var exito = await _service.EditarMedicoAsync(id, dto);
            if (!exito) return NotFound();
            return NoContent();
        }

        [HttpPut("anular/{id}")]
        public async Task<IActionResult> AnularMedico(int id)
        {
            var exito = await _service.AnularMedicoAsync(id);
            if (!exito) return NotFound();
            return Ok();
        }
    }
}
