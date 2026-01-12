using Lab_Contracts.Medicos;
using Lab_APIRest.Services.Medicos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lab_Contracts.Common;
using System.Linq;

namespace Lab_APIRest.Controllers.Medicos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,recepcionista")]
    public class MedicosController : ControllerBase
    {
        private readonly IMedicoService _medicoService;
        private readonly ILogger<MedicosController> _logger;

        public MedicosController(IMedicoService medicoService, ILogger<MedicosController> logger)
        {
            _medicoService = medicoService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicoDto>>> ListarMedicos()
        {
            var lista = await _medicoService.ListarMedicosAsync();
            return Ok(lista);
        }

        [HttpPost("buscar")]
        public async Task<ActionResult<ResultadoPaginadoDto<MedicoDto>>> ListarMedicosPaginados([FromBody] MedicoFiltroDto filtro)
        {
            var result = await _medicoService.ListarMedicosPaginadosAsync(filtro);
            return Ok(result);
        }

        [HttpGet("{idMedico:int}")]
        public async Task<ActionResult<MedicoDto>> ObtenerDetalleMedico(int idMedico)
        {
            var medico = await _medicoService.ObtenerDetalleMedicoAsync(idMedico);
            if (medico == null) return NotFound("No se encontró el médico solicitado.");
            return Ok(medico);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<MedicoDto>>> ListarMedicos([FromQuery] string campo, [FromQuery] string valor)
        {
            if (string.IsNullOrWhiteSpace(campo) || string.IsNullOrWhiteSpace(valor)) return BadRequest("Debe proporcionar un campo y valor de búsqueda.");
            try
            {
                List<MedicoDto> lista = campo.ToLower() switch
                {
                    "nombre" => await _medicoService.ListarMedicosPorNombreAsync(valor),
                    "especialidad" => await _medicoService.ListarMedicosPorEspecialidadAsync(valor),
                    "correo" => await _medicoService.ListarMedicosPorCorreoAsync(valor),
                    _ => throw new ArgumentException("Campo de búsqueda no soportado. Use: nombre, especialidad o correo.")
                };
                return Ok(lista);
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
        public async Task<ActionResult<MedicoDto>> GuardarMedico([FromBody] MedicoDto medicoDto)
        {
            try
            {
                medicoDto.Correo = medicoDto.Correo?.Trim() ?? string.Empty;
                var creado = await _medicoService.GuardarMedicoAsync(medicoDto);
                return CreatedAtAction(nameof(ObtenerDetalleMedico), new { idMedico = creado.IdMedico }, creado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar médico.");
                return StatusCode(500, "Ocurrió un error interno al registrar el médico.");
            }
        }

        [Authorize(Roles = "administrador,recepcionista")]
        [HttpPut("{idMedico:int}")]
        public async Task<IActionResult> GuardarMedico(int idMedico, [FromBody] MedicoDto medicoDto)
        {
            if (idMedico != medicoDto.IdMedico) return BadRequest("El identificador del médico no coincide.");
            try
            {
                medicoDto.Correo = medicoDto.Correo?.Trim() ?? string.Empty;
                var ok = await _medicoService.GuardarMedicoAsync(idMedico, medicoDto);
                if (!ok) return NotFound("Médico no encontrado.");
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar médico con ID {MedicoId}.", idMedico);
                return StatusCode(500, "Ocurrió un error interno al actualizar el médico.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular/{idMedico:int}")]
        public async Task<IActionResult> AnularMedico(int idMedico)
        {
            try
            {
                var ok = await _medicoService.AnularMedicoAsync(idMedico);
                if (!ok) return NotFound("Médico no encontrado.");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular médico con ID {idMedico}.");
                return StatusCode(500, "Ocurrió un error interno al anular el médico.");
            }
        }
    }
}
