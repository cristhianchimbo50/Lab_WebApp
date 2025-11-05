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
        private readonly IPacienteService ServicioPaciente;

        public PacientesController(IPacienteService ServicioPaciente)
        {
            this.ServicioPaciente = ServicioPaciente;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPacientes()
        {
            var ListaPacientes = await ServicioPaciente.ObtenerPacientesAsync();
            return Ok(ListaPacientes);
        }

        [HttpGet("{IdPaciente:int}")]
        public async Task<IActionResult> ObtenerPaciente(int IdPaciente)
        {
            var PacienteEncontrado = await ServicioPaciente.ObtenerPacientePorIdAsync(IdPaciente);
            if (PacienteEncontrado == null)
                return NotFound(new { Mensaje = "No se encontró el paciente solicitado." });

            return Ok(PacienteEncontrado);
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> BuscarPacientes([FromQuery] string CampoBusqueda, [FromQuery] string ValorBusqueda)
        {
            var ResultadoBusqueda = await ServicioPaciente.BuscarPacientesAsync(CampoBusqueda, ValorBusqueda);
            if (ResultadoBusqueda == null)
                return BadRequest("Campo de búsqueda no soportado. Use: cedula, nombre o correo.");
            return Ok(ResultadoBusqueda);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarPaciente([FromBody] PacienteDto DatosPaciente)
        {
            var ResultadoRegistro = await ServicioPaciente.RegistrarPacienteAsync(DatosPaciente);
            if (!ResultadoRegistro.Exito)
                return BadRequest(new { Mensaje = ResultadoRegistro.Mensaje });

            return CreatedAtAction(
                nameof(ObtenerPaciente),
                new { IdPaciente = ResultadoRegistro.Paciente!.IdPaciente },
                new
                {
                    Mensaje = "Paciente registrado correctamente. Se ha enviado un correo con sus credenciales.",
                    ResultadoRegistro.Paciente!.IdPaciente,
                    ResultadoRegistro.Paciente!.NombrePaciente,
                    ResultadoRegistro.Paciente!.CorreoElectronicoPaciente,
                    ResultadoRegistro.Paciente!.ContraseniaTemporal
                }
            );
        }

        [HttpPut("{IdPaciente:int}")]
        public async Task<IActionResult> EditarPaciente(int IdPaciente, [FromBody] PacienteDto DatosPaciente)
        {
            var OkEdicion = await ServicioPaciente.EditarPacienteAsync(IdPaciente, DatosPaciente);
            if (!OkEdicion)
                return NotFound("No se encontró el paciente a editar.");
            return NoContent();
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular/{IdPaciente:int}")]
        public async Task<IActionResult> AnularPaciente(int IdPaciente)
        {
            var OkAnulado = await ServicioPaciente.AnularPacienteAsync(IdPaciente);
            if (!OkAnulado)
                return NotFound("Paciente no encontrado o ya estaba anulado.");

            return Ok(new { Mensaje = "Paciente anulado correctamente." });
        }

        [HttpPost("{IdPaciente:int}/reenviar-temporal")]
        public async Task<IActionResult> ReenviarTemporal(int IdPaciente)
        {
            var (Exito, Mensaje, NuevaTemporal) = await ServicioPaciente.ReenviarCredencialesTemporalesAsync(IdPaciente);
            if (!Exito) return BadRequest(new { Mensaje });
            return Ok(new { Mensaje, NuevaTemporal = NuevaTemporal });
        }
    }
}
