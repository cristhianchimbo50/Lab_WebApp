using Lab_Contracts.Medicos;
using Lab_APIRest.Services.Medicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Medicos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,recepcionista")]
    public class MedicosController : ControllerBase
    {
        private readonly IMedicoService _service;
        private readonly ILogger<MedicosController> _logger;

        public MedicosController(IMedicoService service, ILogger<MedicosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicoDto>>> ObtenerMedicos()
        {
            var medicos = await _service.GetMedicosAsync();
            return Ok(medicos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MedicoDto>> ObtenerMedico(int id)
        {
            var medico = await _service.GetMedicoByIdAsync(id);
            if (medico == null)
                return NotFound("No se encontró el médico solicitado.");

            return Ok(medico);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<MedicoDto>>> BuscarMedicos([FromQuery] string campo, [FromQuery] string valor)
        {
            if (string.IsNullOrWhiteSpace(campo) || string.IsNullOrWhiteSpace(valor))
                return BadRequest("Debe proporcionar un campo y valor de búsqueda.");

            try
            {
                List<MedicoDto> medicos = campo.ToLower() switch
                {
                    "nombre" => await _service.GetMedicosPorNombreAsync(valor),
                    "especialidad" => await _service.GetMedicosPorEspecialidadAsync(valor),
                    "correo" => await _service.GetMedicosPorCorreoAsync(valor),
                    _ => throw new ArgumentException("Campo de búsqueda no soportado. Use: nombre, especialidad o correo.")
                };

                return Ok(medicos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar médicos.");
                return StatusCode(500, "Error interno al realizar la búsqueda de médicos.");
            }
        }

        [Authorize(Roles = "administrador,recepcionista")]
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

        [Authorize(Roles = "administrador,recepcionista")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarMedico(int id, [FromBody] MedicoDto dto)
        {
            if (id != dto.IdMedico)
                return BadRequest("El identificador del médico no coincide.");

            var exito = await _service.EditarMedicoAsync(id, dto);
            if (!exito) return NotFound("Médico no encontrado.");

            return NoContent();
        }

        [Authorize(Roles = "administrador")]
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
