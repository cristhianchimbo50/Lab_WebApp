using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,laboratorista")]
    public class ExamenReactivoAsociacionesController : ControllerBase
    {
        private readonly IExamenReactivoAsociacionService ReactivoAsociacionService;
        private readonly ILogger<ExamenReactivoAsociacionesController> Logger;

        public ExamenReactivoAsociacionesController(
            IExamenReactivoAsociacionService reactivoAsociacionService,
            ILogger<ExamenReactivoAsociacionesController> logger)
        {
            ReactivoAsociacionService = reactivoAsociacionService;
            Logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> ObtenerTodas()
        {
            var ListaAsociaciones = await ReactivoAsociacionService.ObtenerTodas();
            return Ok(ListaAsociaciones);
        }

        [HttpGet("{IdExamenReactivo:int}")]
        public async Task<ActionResult<AsociacionReactivoDto>> ObtenerPorId(int IdExamenReactivo)
        {
            var Asociacion = await ReactivoAsociacionService.ObtenerPorId(IdExamenReactivo);
            if (Asociacion == null) return NotFound();
            return Ok(Asociacion);
        }

        [HttpGet("buscar-examen/{NombreExamen}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> BuscarPorExamen(string NombreExamen)
        {
            var ListaAsociaciones = await ReactivoAsociacionService.BuscarPorExamen(NombreExamen);
            return Ok(ListaAsociaciones);
        }

        [HttpGet("buscar-reactivo/{NombreReactivo}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> BuscarPorReactivo(string NombreReactivo)
        {
            var ListaAsociaciones = await ReactivoAsociacionService.BuscarPorReactivo(NombreReactivo);
            return Ok(ListaAsociaciones);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost]
        public async Task<ActionResult<AsociacionReactivoDto>> Crear([FromBody] AsociacionReactivoDto AsociacionDto)
        {
            try
            {
                var Creado = await ReactivoAsociacionService.Crear(AsociacionDto);
                return CreatedAtAction(nameof(ObtenerPorId), new { IdExamenReactivo = Creado.IdExamenReactivo }, Creado);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al crear asociación examen-reactivo.");
                return StatusCode(500, "Error interno al crear la asociación.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("{IdExamenReactivo:int}")]
        public async Task<IActionResult> Editar(int IdExamenReactivo, [FromBody] AsociacionReactivoDto AsociacionDto)
        {
            var Editado = await ReactivoAsociacionService.Editar(IdExamenReactivo, AsociacionDto);
            if (!Editado) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "administrador")]
        [HttpDelete("{IdExamenReactivo:int}")]
        public async Task<IActionResult> Eliminar(int IdExamenReactivo)
        {
            var Eliminado = await ReactivoAsociacionService.Eliminar(IdExamenReactivo);
            if (!Eliminado) return NotFound();
            return NoContent();
        }

        [Authorize(Roles = "administrador,laboratorista")]
        [HttpGet("asociados/{IdExamen:int}")]
        public async Task<ActionResult<List<AsociacionReactivoDto>>> ObtenerAsociadosPorExamen(int IdExamen)
        {
            var ListaAsociaciones = await ReactivoAsociacionService.ObtenerPorExamenId(IdExamen);
            return Ok(ListaAsociaciones);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost("asociados/{IdExamen:int}")]
        public async Task<IActionResult> GuardarAsociaciones(int IdExamen, [FromBody] List<AsociacionReactivoDto> Asociaciones)
        {
            var Guardado = await ReactivoAsociacionService.GuardarPorExamen(IdExamen, Asociaciones);
            if (Guardado) return Ok();
            return BadRequest("No se pudieron guardar las asociaciones.");
        }
    }
}
