using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers.Examenes
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,laboratorista,recepcionista")]
    public class ExamenesController : ControllerBase
    {
        private readonly IExamenService ServicioExamen;
        private readonly ILogger<ExamenesController> Registro;

        public ExamenesController(IExamenService ServicioExamen, ILogger<ExamenesController> Registro)
        {
            this.ServicioExamen = ServicioExamen;
            this.Registro = Registro;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamenDto>>> ObtenerExamenes()
        {
            var ListaExamenes = await ServicioExamen.ObtenerExamenesAsync();
            return Ok(ListaExamenes);
        }

        [HttpGet("{IdExamen:int}")]
        public async Task<ActionResult<ExamenDto>> ObtenerExamen(int IdExamen)
        {
            var ExamenEncontrado = await ServicioExamen.ObtenerExamenPorIdAsync(IdExamen);
            if (ExamenEncontrado == null) return NotFound("Examen no encontrado.");
            return Ok(ExamenEncontrado);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<ExamenDto>>> BuscarExamenes([FromQuery] string Nombre)
        {
            if (string.IsNullOrWhiteSpace(Nombre))
                return BadRequest("Debe proporcionar un nombre válido.");

            var ListaEncontrada = await ServicioExamen.BuscarExamenesPorNombreAsync(Nombre);
            return Ok(ListaEncontrada);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost]
        public async Task<ActionResult<ExamenDto>> RegistrarExamen([FromBody] ExamenDto DatosExamen)
        {
            try
            {
                var ExamenCreado = await ServicioExamen.RegistrarExamenAsync(DatosExamen);
                return CreatedAtAction(nameof(ObtenerExamen), new { IdExamen = ExamenCreado.IdExamen }, ExamenCreado);
            }
            catch (Exception ex)
            {
                Registro.LogError(ex, "Error al registrar examen.");
                return StatusCode(500, "Error interno al registrar el examen.");
            }
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("{IdExamen:int}")]
        public async Task<IActionResult> EditarExamen(int IdExamen, [FromBody] ExamenDto DatosExamen)
        {
            if (IdExamen != DatosExamen.IdExamen) return BadRequest("El identificador no coincide.");

            var ExitoEdicion = await ServicioExamen.EditarExamenAsync(IdExamen, DatosExamen);
            if (!ExitoEdicion) return NotFound("Examen no encontrado.");
            return NoContent();
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular/{IdExamen:int}")]
        public async Task<IActionResult> AnularExamen(int IdExamen)
        {
            var ExitoAnular = await ServicioExamen.AnularExamenAsync(IdExamen);
            if (!ExitoAnular) return NotFound("Examen no encontrado.");
            return Ok();
        }

        [Authorize(Roles = "administrador,laboratorista")]
        [HttpGet("{IdExamen:int}/hijos")]
        public async Task<ActionResult<List<ExamenDto>>> ObtenerHijos(int IdExamen)
        {
            var Hijos = await ServicioExamen.ObtenerHijosDeExamenAsync(IdExamen);
            return Ok(Hijos);
        }

        [Authorize(Roles = "administrador")]
        [HttpPost("{IdPadre:int}/hijos/{IdHijo:int}")]
        public async Task<IActionResult> AgregarHijo(int IdPadre, int IdHijo)
        {
            var Resultado = await ServicioExamen.AgregarExamenHijoAsync(IdPadre, IdHijo);
            if (!Resultado) return Conflict("La relación ya existe o los datos no son válidos.");
            return Ok();
        }

        [Authorize(Roles = "administrador")]
        [HttpDelete("{IdPadre:int}/hijos/{IdHijo:int}")]
        public async Task<IActionResult> EliminarHijo(int IdPadre, int IdHijo)
        {
            var Resultado = await ServicioExamen.EliminarExamenHijoAsync(IdPadre, IdHijo);
            if (!Resultado) return NotFound();
            return Ok();
        }
    }
}
