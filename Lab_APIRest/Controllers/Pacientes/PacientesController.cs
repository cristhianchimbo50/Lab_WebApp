using Lab_Contracts.Pacientes;
using Lab_APIRest.Services.Pacientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,recepcionista")]
    public class PacientesController : ControllerBase
    {
        private readonly IPacienteService _service;

        public PacientesController(IPacienteService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPacientes()
        {
            var pacientes = await _service.GetPacientesAsync();
            return Ok(pacientes);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPaciente(int id)
        {
            var paciente = await _service.GetPacienteByIdAsync(id);
            if (paciente == null)
                return NotFound(new { mensaje = "No se encontró el paciente solicitado." });

            return Ok(paciente);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarPacientes([FromQuery] string campo, [FromQuery] string valor)
        {
            var pacientes = await _service.BuscarPacientesAsync(campo, valor);
            if (pacientes == null)
                return BadRequest("Campo de búsqueda no soportado. Use: cedula, nombre o correo.");
            return Ok(pacientes);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarPaciente([FromBody] PacienteDto dto)
        {
            var resultado = await _service.RegistrarPacienteAsync(dto);
            if (!resultado.Exito)
                return BadRequest(new { mensaje = resultado.Mensaje });

            return CreatedAtAction(
                nameof(ObtenerPaciente),
                new { id = resultado.Paciente!.IdPaciente },
                new
                {
                    mensaje = "Paciente registrado correctamente. Se ha enviado un correo con sus credenciales.",
                    resultado.Paciente!.IdPaciente,
                    resultado.Paciente!.NombrePaciente,
                    resultado.Paciente!.CorreoElectronicoPaciente,
                    resultado.Paciente!.ContraseniaTemporal
                }
            );
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarPaciente(int id, [FromBody] PacienteDto dto)
        {
            var ok = await _service.EditarPacienteAsync(id, dto);
            if (!ok)
                return NotFound("No se encontró el paciente a editar.");
            return NoContent();
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular/{id:int}")]
        public async Task<IActionResult> AnularPaciente(int id)
        {
            var ok = await _service.AnularPacienteAsync(id);
            if (!ok)
                return NotFound("Paciente no encontrado o ya estaba anulado.");

            return Ok(new { mensaje = "Paciente anulado correctamente." });
        }

        [HttpPost("{id:int}/reenviar-temporal")]
        public async Task<IActionResult> ReenviarTemporal(int id)
        {
            var (exito, mensaje, temp) = await _service.ReenviarCredencialesTemporalesAsync(id);
            if (!exito) return BadRequest(new { mensaje });
            return Ok(new { mensaje, nuevaTemporal = temp });
        }
    }
}
