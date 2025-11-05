using Lab_APIRest.Services.Convenios;
using Lab_Contracts.Convenios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Convenios
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,recepcionista")]
    public class ConveniosController : ControllerBase
    {
        private readonly IConvenioService ConvenioService;
        private readonly ILogger<ConveniosController> Logger;

        public ConveniosController(IConvenioService ConvenioService, ILogger<ConveniosController> Logger)
        {
            this.ConvenioService = ConvenioService;
            this.Logger = Logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConvenioDto>>> ObtenerConvenios()
        {
            var Convenios = await ConvenioService.ObtenerConveniosAsync();
            return Ok(Convenios);
        }

        [HttpGet("{IdConvenio:int}")]
        public async Task<ActionResult<ConvenioDetalleDto>> ObtenerDetalle(int IdConvenio)
        {
            var Detalle = await ConvenioService.ObtenerDetalleConvenioAsync(IdConvenio);
            if (Detalle == null)
                return NotFound("No se encontró el convenio solicitado.");
            return Ok(Detalle);
        }

        [HttpGet("ordenes-disponibles/{IdMedico:int}")]
        public async Task<ActionResult<IEnumerable<OrdenDisponibleDto>>> ObtenerOrdenesDisponibles(int IdMedico)
        {
            var Ordenes = await ConvenioService.ObtenerOrdenesDisponiblesAsync(IdMedico);
            return Ok(Ordenes);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost]
        public async Task<ActionResult> RegistrarConvenio([FromBody] ConvenioRegistroDto ConvenioRegistro)
        {
            try
            {
                var Exito = await ConvenioService.RegistrarConvenioAsync(ConvenioRegistro);
                if (!Exito)
                    return BadRequest("No se pudo registrar el convenio.");
                return Ok();
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, "Error al registrar un nuevo convenio.");
                return StatusCode(500, "Ocurrió un error interno al registrar el convenio.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("{IdConvenio:int}/anular")]
        public async Task<ActionResult> AnularConvenio(int IdConvenio)
        {
            try
            {
                var Exito = await ConvenioService.AnularConvenioAsync(IdConvenio);
                if (!Exito)
                    return NotFound("No se encontró el convenio a anular.");
                return Ok();
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, $"Error al anular el convenio con ID {IdConvenio}.");
                return StatusCode(500, "Ocurrió un error interno al anular el convenio.");
            }
        }
    }
}
