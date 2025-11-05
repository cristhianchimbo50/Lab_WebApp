using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,laboratorista")]
    public class ExamenComposicionController : ControllerBase
    {
        private readonly IExamenComposicionService ComposicionService;
        private readonly ILogger<ExamenComposicionController> Logger;

        public ExamenComposicionController(IExamenComposicionService composicionService, ILogger<ExamenComposicionController> logger)
        {
            ComposicionService = composicionService;
            Logger = logger;
        }

        [HttpGet("padre/{id:int}")]
        public async Task<ActionResult<List<ExamenComposicionDto>>> ObtenerPorPadre(int IdExamenPadre)
        {
            var ListaComposiciones = await ComposicionService.ObtenerPorPadre(IdExamenPadre);
            return Ok(ListaComposiciones);
        }

        [HttpGet("hijo/{id:int}")]
        public async Task<ActionResult<List<ExamenComposicionDto>>> ObtenerPorHijo(int IdExamenHijo)
        {
            var ListaComposiciones = await ComposicionService.ObtenerPorHijo(IdExamenHijo);
            return Ok(ListaComposiciones);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ExamenComposicionDto ComposicionDto)
        {
            try
            {
                var Creado = await ComposicionService.Crear(ComposicionDto);
                if (!Creado) return Conflict("Ya existe esta composición.");
                return Ok();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al crear la composición de examen.");
                return StatusCode(500, "Error interno al crear la composición.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpDelete]
        public async Task<IActionResult> Eliminar([FromQuery] int IdExamenPadre, [FromQuery] int IdExamenHijo)
        {
            var Eliminado = await ComposicionService.Eliminar(IdExamenPadre, IdExamenHijo);
            if (!Eliminado) return NotFound();
            return Ok();
        }
    }
}
