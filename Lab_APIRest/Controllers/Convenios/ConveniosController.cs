using Lab_APIRest.Services.Convenios;
using Lab_Contracts.Convenios;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Convenios
{
    /// <summary>
    /// Controlador responsable de gestionar los convenios médicos y sus operaciones relacionadas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ConveniosController : ControllerBase
    {
        private readonly IConvenioService _service;
        private readonly ILogger<ConveniosController> _logger;

        /// <summary>
        /// Constructor del controlador de convenios.
        /// </summary>
        /// <param name="service">Servicio de convenios inyectado.</param>
        /// <param name="logger">Logger para el registro de eventos y errores.</param>
        public ConveniosController(IConvenioService service, ILogger<ConveniosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los convenios registrados en el sistema.
        /// </summary>
        /// <returns>Lista de convenios.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConvenioDto>>> ObtenerConvenios()
        {
            var convenios = await _service.ObtenerConveniosAsync();
            return Ok(convenios);
        }

        /// <summary>
        /// Obtiene el detalle de un convenio específico.
        /// </summary>
        /// <param name="id">Identificador del convenio.</param>
        /// <returns>Detalle del convenio o NotFound si no existe.</returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ConvenioDetalleDto>> ObtenerDetalle(int id)
        {
            var detalle = await _service.ObtenerDetalleConvenioAsync(id);
            if (detalle == null)
                return NotFound("No se encontró el convenio solicitado.");

            return Ok(detalle);
        }

        /// <summary>
        /// Obtiene las órdenes disponibles para un médico que aún no han sido liquidadas en convenios.
        /// </summary>
        /// <param name="idMedico">Identificador del médico.</param>
        /// <returns>Lista de órdenes disponibles.</returns>
        [HttpGet("ordenes-disponibles/{idMedico:int}")]
        public async Task<ActionResult<IEnumerable<OrdenDisponibleDto>>> ObtenerOrdenesDisponibles(int idMedico)
        {
            var ordenes = await _service.ObtenerOrdenesDisponiblesAsync(idMedico);
            return Ok(ordenes);
        }

        /// <summary>
        /// Registra un nuevo convenio con las órdenes asociadas.
        /// </summary>
        /// <param name="dto">Datos del convenio a registrar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPost]
        public async Task<ActionResult> RegistrarConvenio([FromBody] ConvenioRegistroDto dto)
        {
            try
            {
                var exito = await _service.RegistrarConvenioAsync(dto);
                if (!exito)
                    return BadRequest("No se pudo registrar el convenio.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar un nuevo convenio.");
                return StatusCode(500, "Ocurrió un error interno al registrar el convenio.");
            }
        }

        /// <summary>
        /// Anula un convenio y revierte el estado de las órdenes asociadas.
        /// </summary>
        /// <param name="id">Identificador del convenio.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPut("{id:int}/anular")]
        public async Task<ActionResult> AnularConvenio(int id)
        {
            try
            {
                var exito = await _service.AnularConvenioAsync(id);
                if (!exito)
                    return NotFound("No se encontró el convenio a anular.");

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular el convenio con ID {id}.");
                return StatusCode(500, "Ocurrió un error interno al anular el convenio.");
            }
        }
    }
}
