using Lab_Contracts.Reactivos;
using Lab_APIRest.Services.Reactivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Reactivos
{
    /// <summary>
    /// Controlador responsable de la gestión de reactivos: registro, edición, anulación e ingreso/egreso de stock.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,laboratorista")]
    public class ReactivosController : ControllerBase
    {
        private readonly IReactivoService ReactivoService;
        private readonly ILogger<ReactivosController> Logger;

        public ReactivosController(IReactivoService ReactivoService, ILogger<ReactivosController> Logger)
        {
            this.ReactivoService = ReactivoService;
            this.Logger = Logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ReactivoDto>>> ObtenerReactivos()
        {
            var Reactivos = await ReactivoService.ObtenerReactivosAsync();
            return Ok(Reactivos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReactivoDto>> ObtenerReactivoPorId(int Id)
        {
            var ReactivoEncontrado = await ReactivoService.ObtenerReactivoPorIdAsync(Id);
            if (ReactivoEncontrado == null)
                return NotFound("Reactivo no encontrado.");
            return Ok(ReactivoEncontrado);
        }

        [HttpPost]
        public async Task<ActionResult<ReactivoDto>> RegistrarReactivo([FromBody] ReactivoDto Reactivo)
        {
            try
            {
                var ReactivoCreado = await ReactivoService.CrearReactivoAsync(Reactivo);
                return CreatedAtAction(nameof(ObtenerReactivoPorId), new { id = ReactivoCreado.IdReactivo }, ReactivoCreado);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al registrar reactivo.");
                return StatusCode(500, "Error interno al registrar el reactivo.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarReactivo(int Id, [FromBody] ReactivoDto Reactivo)
        {
            if (Id != Reactivo.IdReactivo)
                return BadRequest("El identificador no coincide con el reactivo proporcionado.");

            var Resultado = await ReactivoService.EditarReactivoAsync(Id, Reactivo);
            if (!Resultado)
                return NotFound("Reactivo no encontrado.");

            return NoContent();
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular/{id:int}")]
        public async Task<IActionResult> AnularReactivo(int Id)
        {
            try
            {
                var Resultado = await ReactivoService.AnularReactivoAsync(Id);
                if (!Resultado)
                    return NotFound("Reactivo no encontrado o ya estaba anulado.");

                return Ok(new { mensaje = "Reactivo anulado correctamente." });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error al anular el reactivo con ID {Id}.");
                return StatusCode(500, "Error interno al anular el reactivo.");
            }
        }

        [HttpPost("ingresos")]
        public async Task<ActionResult> RegistrarIngresos([FromBody] IEnumerable<MovimientoReactivoIngresoDto> Ingresos)
        {
            if (Ingresos == null || !Ingresos.Any())
                return BadRequest("No se enviaron datos de ingreso.");

            try
            {
                var Resultado = await ReactivoService.RegistrarIngresosAsync(Ingresos);
                if (Resultado)
                    return Ok(new { mensaje = "Ingresos registrados correctamente." });

                return StatusCode(500, new { mensaje = "Error al registrar los ingresos." });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al registrar ingresos de reactivos.");
                return StatusCode(500, "Error interno al registrar ingresos.");
            }
        }

        [HttpPost("egresos")]
        public async Task<ActionResult> RegistrarEgresos([FromBody] IEnumerable<MovimientoReactivoEgresoDto> Egresos)
        {
            if (Egresos == null || !Egresos.Any())
                return BadRequest("No se enviaron datos de egreso.");

            try
            {
                var Resultado = await ReactivoService.RegistrarEgresosAsync(Egresos);
                if (Resultado)
                    return Ok(new { mensaje = "Egresos registrados correctamente." });

                return StatusCode(500, new { mensaje = "Error al registrar los egresos." });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al registrar egresos de reactivos.");
                return StatusCode(500, "Error interno al registrar egresos.");
            }
        }
    }
}
