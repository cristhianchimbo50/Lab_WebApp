using Lab_Contracts.Pacientes;
using Lab_APIRest.Services.Pacientes;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PacientesController : ControllerBase
    {
        private readonly IPacienteService _service;

        public PacientesController(IPacienteService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PacienteDto>>> GetPacientes()
        {
            var pacientes = await _service.GetPacientesAsync();
            return Ok(pacientes);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PacienteDto>> GetPaciente(int id)
        {
            var paciente = await _service.GetPacienteByIdAsync(id);
            if (paciente == null) return NotFound();
            return Ok(paciente);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<PacienteDto>>> BuscarPacientes([FromQuery] string campo, [FromQuery] string valor)
        {
            if (string.IsNullOrWhiteSpace(campo) || string.IsNullOrWhiteSpace(valor))
                return BadRequest("Debe proporcionar campo y valor.");

            List<PacienteDto> pacientes = new();

            switch (campo.ToLower())
            {
                case "cedula":
                    var porCedula = await _service.GetPacienteByCedulaAsync(valor);
                    if (porCedula != null)
                        pacientes.Add(porCedula);
                    break;

                case "nombre":
                    pacientes = await _service.GetPacientesPorNombreAsync(valor);
                    break;

                case "correo":
                    pacientes = await _service.GetPacientesPorCorreoAsync(valor);
                    break;

                default:
                    return BadRequest("Campo de búsqueda no soportado.");
            }

            return Ok(pacientes);
        }

        [HttpPost]
        public async Task<ActionResult<PacienteDto>> PostPaciente(PacienteDto dto)
        {
            if (!ValidarCedula(dto.CedulaPaciente))
                return BadRequest("La cédula ingresada no es válida.");

            int usuarioId = 1;
            var paciente = await _service.CrearPacienteAsync(dto, usuarioId);
            return CreatedAtAction(nameof(GetPaciente), new { id = paciente.IdPaciente }, paciente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPaciente(int id, PacienteDto dto)
        {
            if (id != dto.IdPaciente) return BadRequest();

            if (!ValidarCedula(dto.CedulaPaciente))
                return BadRequest("La cédula ingresada no es válida.");

            var exito = await _service.EditarPacienteAsync(id, dto);
            if (!exito) return NotFound();
            return NoContent();
        }

        [HttpPut("anular/{id}")]
        public async Task<IActionResult> AnularPaciente(int id)
        {
            var exito = await _service.AnularPacienteAsync(id);
            if (!exito) return NotFound();
            return Ok();
        }

        private bool ValidarCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula) || cedula.Length != 10 || !cedula.All(char.IsDigit))
                return false;

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                int digit = int.Parse(cedula[i].ToString());
                int coef = (i % 2 == 0) ? 2 : 1;
                int product = digit * coef;
                sum += (product >= 10) ? (product - 9) : product;
            }

            int lastDigit = int.Parse(cedula[9].ToString());
            int calculatedDigit = (sum % 10 == 0) ? 0 : (10 - (sum % 10));

            return lastDigit == calculatedDigit;
        }
    }
}
