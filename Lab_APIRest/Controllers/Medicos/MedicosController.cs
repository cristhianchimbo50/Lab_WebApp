using Lab_Contracts.Medicos;
using Lab_APIRest.Services.Medicos;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Medicos
{
    /// <summary>
    /// Controlador responsable de gestionar la información de los médicos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MedicosController : ControllerBase
    {
        private readonly IMedicoService _service;
        private readonly ILogger<MedicosController> _logger;

        /// <summary>
        /// Constructor del controlador de médicos.
        /// </summary>
        /// <param name="service">Servicio de médicos inyectado.</param>
        /// <param name="logger">Logger para registrar errores y eventos.</param>
        public MedicosController(IMedicoService service, ILogger<MedicosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los médicos registrados.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicoDto>>> ObtenerMedicos()
        {
            var medicos = await _service.GetMedicosAsync();
            return Ok(medicos);
        }

        /// <summary>
        /// Obtiene la información de un médico específico por su identificador.
        /// </summary>
        /// <param name="id">Identificador del médico.</param>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MedicoDto>> ObtenerMedico(int id)
        {
            var medico = await _service.GetMedicoByIdAsync(id);
            if (medico == null)
                return NotFound("No se encontró el médico solicitado.");

            return Ok(medico);
        }

        /// <summary>
        /// Busca médicos por nombre, especialidad o correo electrónico.
        /// </summary>
        /// <param name="campo">Campo de búsqueda (nombre, especialidad, correo).</param>
        /// <param name="valor">Valor del campo a buscar.</param>
        [HttpGet("buscar")]
        public async Task<ActionResult<List<MedicoDto>>> BuscarMedicos([FromQuery] string campo, [FromQuery] string valor)
        {
            if (string.IsNullOrWhiteSpace(campo) || string.IsNullOrWhiteSpace(valor))
                return BadRequest("Debe proporcionar un campo y valor de búsqueda.");

            List<MedicoDto> medicos;

            try
            {
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
                        return BadRequest("Campo de búsqueda no soportado. Use: nombre, especialidad o correo.");
                }

                return Ok(medicos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar médicos.");
                return StatusCode(500, "Error interno al realizar la búsqueda de médicos.");
            }
        }

        /// <summary>
        /// Registra un nuevo médico.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<MedicoDto>> RegistrarMedico([FromBody] MedicoDto dto)
        {
            try
            {
                var medico = await _service.CrearMedicoAsync(dto);
                return CreatedAtAction(nameof(ObtenerMedico), new { id = medico.IdMedico }, medico);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar médico.");
                return StatusCode(500, "Ocurrió un error interno al registrar el médico.");
            }
        }

        /// <summary>
        /// Actualiza los datos de un médico existente.
        /// </summary>
        /// <param name="id">Identificador del médico.</param>
        /// <param name="dto">Datos actualizados del médico.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarMedico(int id, [FromBody] MedicoDto dto)
        {
            if (id != dto.IdMedico)
                return BadRequest("El identificador del médico no coincide.");

            var exito = await _service.EditarMedicoAsync(id, dto);
            if (!exito) return NotFound("Médico no encontrado.");

            return NoContent();
        }

        /// <summary>
        /// Anula un médico (lo desactiva sin eliminarlo físicamente).
        /// </summary>
        /// <param name="id">Identificador del médico.</param>
        [HttpPut("anular/{id:int}")]
        public async Task<IActionResult> AnularMedico(int id)
        {
            try
            {
                var exito = await _service.AnularMedicoAsync(id);
                if (!exito) return NotFound("Médico no encontrado.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular médico con ID {id}.");
                return StatusCode(500, "Ocurrió un error interno al anular el médico.");
            }
        }
    }
}
