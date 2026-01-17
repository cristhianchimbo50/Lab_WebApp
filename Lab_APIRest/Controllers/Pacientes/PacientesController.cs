using Lab_Contracts.Pacientes;
using Lab_APIRest.Services.Pacientes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lab_Contracts.Common;
using System.Linq;

namespace Lab_APIRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "1,2")]
    public class PacientesController : ControllerBase
    {
        private readonly IPacienteService _pacienteService;

        public PacientesController(IPacienteService pacienteService)
        {
            _pacienteService = pacienteService;
        }

        [HttpGet]
        public async Task<IActionResult> ListarPacientes()
        {
            var lista = await _pacienteService.ListarPacientesAsync();
            return Ok(lista);
        }

        [HttpPost("buscar")]
        public async Task<ActionResult<ResultadoPaginadoDto<PacienteDto>>> ListarPacientesPaginados([FromBody] PacienteFiltroDto filtro)
        {
            var result = await _pacienteService.ListarPacientesPaginadosAsync(filtro);
            return Ok(result);
        }

        [HttpGet("{idPaciente:int}")]
        public async Task<IActionResult> ObtenerDetallePaciente(int idPaciente)
        {
            var paciente = await _pacienteService.ObtenerDetallePacienteAsync(idPaciente);
            if (paciente == null)
                return NotFound(new { Mensaje = "No se encontró el paciente solicitado." });

            return Ok(paciente);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> ListarPacientes([FromQuery] string criterio, [FromQuery] string valor)
        {
            var resultado = await _pacienteService.ListarPacientesAsync(criterio, valor);
            if (resultado == null)
                return BadRequest("Campo de búsqueda no soportado. Use: cedula, nombre o correo.");
            return Ok(resultado);
        }

        [HttpGet("generos")]
        public async Task<ActionResult<List<GeneroDto>>> ListarGeneros()
        {
            var generos = await _pacienteService.ListarGenerosAsync();
            return Ok(generos);
        }

        [HttpPost]
        public async Task<IActionResult> GuardarPaciente([FromBody] PacienteDto dto)
        {
            var resultado = await _pacienteService.GuardarPacienteAsync(dto);
            if (!resultado.Exito)
                return BadRequest(new { Mensaje = resultado.Mensaje });

            return CreatedAtAction(
                nameof(ObtenerDetallePaciente),
                new { idPaciente = resultado.Paciente!.IdPaciente },
                new
                {
                    Mensaje = "Paciente registrado correctamente. Se ha enviado un correo con el enlace de activación.",
                    resultado.Paciente!.IdPaciente,
                    resultado.Paciente!.NombrePaciente,
                    resultado.Paciente!.CorreoElectronicoPaciente
                }
            );
        }

        [HttpPut("{idPaciente:int}")]
        public async Task<IActionResult> GuardarPaciente(int idPaciente, [FromBody] PacienteDto dto)
        {
            var ok = await _pacienteService.GuardarPacienteAsync(idPaciente, dto);
            if (!ok)
                return NotFound("No se encontró el paciente a editar.");
            return NoContent();
        }

        [Authorize(Roles = "1")]
        [HttpPut("anular/{idPaciente:int}")]
        public async Task<IActionResult> AnularPaciente(int idPaciente)
        {
            var ok = await _pacienteService.AnularPacienteAsync(idPaciente);
            if (!ok)
                return NotFound("Paciente no encontrado o ya estaba anulado.");

            return Ok(new { Mensaje = "Paciente anulado correctamente." });
        }
    }
}
