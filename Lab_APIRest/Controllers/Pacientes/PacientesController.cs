using Lab_Contracts.Pacientes;
using Lab_APIRest.Services.Pacientes;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers
{
    /// <summary>
    /// Controlador responsable de gestionar la información de los pacientes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PacientesController : ControllerBase
    {
        private readonly IPacienteService _service;
        private readonly ILogger<PacientesController> _logger;

        /// <summary>
        /// Constructor del controlador de pacientes.
        /// </summary>
        /// <param name="service">Servicio de pacientes inyectado.</param>
        /// <param name="logger">Logger para registrar errores y eventos.</param>
        public PacientesController(IPacienteService service, ILogger<PacientesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los pacientes registrados.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PacienteDto>>> ObtenerPacientes()
        {
            var pacientes = await _service.GetPacientesAsync();
            return Ok(pacientes);
        }

        /// <summary>
        /// Obtiene la información de un paciente por su identificador.
        /// </summary>
        /// <param name="id">Identificador del paciente.</param>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PacienteDto>> ObtenerPaciente(int id)
        {
            var paciente = await _service.GetPacienteByIdAsync(id);
            if (paciente == null)
                return NotFound("No se encontró el paciente solicitado.");

            return Ok(paciente);
        }

        /// <summary>
        /// Busca pacientes por cédula, nombre o correo.
        /// </summary>
        /// <param name="campo">Campo de búsqueda (cedula, nombre, correo).</param>
        /// <param name="valor">Valor a buscar.</param>
        [HttpGet("buscar")]
        public async Task<ActionResult<List<PacienteDto>>> BuscarPacientes([FromQuery] string campo, [FromQuery] string valor)
        {
            if (string.IsNullOrWhiteSpace(campo) || string.IsNullOrWhiteSpace(valor))
                return BadRequest("Debe proporcionar un campo y valor para la búsqueda.");

            List<PacienteDto> pacientes = new();

            try
            {
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
                        return BadRequest("Campo de búsqueda no soportado. Use: cedula, nombre o correo.");
                }

                return Ok(pacientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar pacientes.");
                return StatusCode(500, "Error interno al realizar la búsqueda de pacientes.");
            }
        }

        /// <summary>
        /// Registra un nuevo paciente en el sistema.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PacienteDto>> RegistrarPaciente([FromBody] PacienteDto dto)
        {
            if (!ValidarCedula(dto.CedulaPaciente))
                return BadRequest("La cédula ingresada no es válida.");

            try
            {
                int usuarioId = 1; // Usuario por defecto o tomado del contexto de autenticación
                var paciente = await _service.CrearPacienteAsync(dto, usuarioId);
                return CreatedAtAction(nameof(ObtenerPaciente), new { id = paciente.IdPaciente }, paciente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar paciente.");
                return StatusCode(500, "Ocurrió un error interno al registrar el paciente.");
            }
        }

        /// <summary>
        /// Edita la información de un paciente existente.
        /// </summary>
        /// <param name="id">Identificador del paciente.</param>
        /// <param name="dto">Datos actualizados del paciente.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarPaciente(int id, [FromBody] PacienteDto dto)
        {
            if (id != dto.IdPaciente)
                return BadRequest("El identificador del paciente no coincide.");

            if (!ValidarCedula(dto.CedulaPaciente))
                return BadRequest("La cédula ingresada no es válida.");

            var exito = await _service.EditarPacienteAsync(id, dto);
            if (!exito)
                return NotFound("No se encontró el paciente a editar.");

            return NoContent();
        }

        /// <summary>
        /// Anula (desactiva) un paciente del sistema.
        /// </summary>
        /// <param name="id">Identificador del paciente.</param>
        [HttpPut("anular/{id:int}")]
        public async Task<IActionResult> AnularPaciente(int id)
        {
            try
            {
                var exito = await _service.AnularPacienteAsync(id);
                if (!exito)
                    return NotFound("Paciente no encontrado o ya estaba anulado.");

                return Ok(new { mensaje = "Paciente anulado correctamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular paciente con ID {id}.");
                return StatusCode(500, "Error interno al anular el paciente.");
            }
        }

        /// <summary>
        /// Valida si una cédula ecuatoriana es correcta.
        /// </summary>
        /// <param name="cedula">Número de cédula a validar.</param>
        private bool ValidarCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula) || cedula.Length != 10 || !cedula.All(char.IsDigit))
                return false;

            int suma = 0;
            for (int i = 0; i < 9; i++)
            {
                int digito = int.Parse(cedula[i].ToString());
                int coef = (i % 2 == 0) ? 2 : 1;
                int producto = digito * coef;
                suma += (producto >= 10) ? (producto - 9) : producto;
            }

            int ultimoDigito = int.Parse(cedula[9].ToString());
            int digitoCalculado = (suma % 10 == 0) ? 0 : (10 - (suma % 10));

            return ultimoDigito == digitoCalculado;
        }
    }
}
