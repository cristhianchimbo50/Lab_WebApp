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
        private readonly IMedicoService ServicioMedico;
        private readonly ILogger<MedicosController> Registro;

        public MedicosController(IMedicoService ServicioMedico, ILogger<MedicosController> Registro)
        {
            this.ServicioMedico = ServicioMedico;
            this.Registro = Registro;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicoDto>>> ObtenerMedicos()
        {
            var ListaMedicos = await ServicioMedico.ObtenerMedicosAsync();
            return Ok(ListaMedicos);
        }

        [HttpGet("{IdMedico:int}")]
        public async Task<ActionResult<MedicoDto>> ObtenerMedico(int IdMedico)
        {
            var MedicoEncontrado = await ServicioMedico.ObtenerMedicoPorIdAsync(IdMedico);
            if (MedicoEncontrado == null)
                return NotFound("No se encontró el médico solicitado.");

            return Ok(MedicoEncontrado);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<List<MedicoDto>>> BuscarMedicos([FromQuery] string CampoBusqueda, [FromQuery] string ValorBusqueda)
        {
            if (string.IsNullOrWhiteSpace(CampoBusqueda) || string.IsNullOrWhiteSpace(ValorBusqueda))
                return BadRequest("Debe proporcionar un campo y valor de búsqueda.");

            try
            {
                List<MedicoDto> ListaMedicos = CampoBusqueda.ToLower() switch
                {
                    "nombre" => await ServicioMedico.ObtenerMedicosPorNombreAsync(ValorBusqueda),
                    "especialidad" => await ServicioMedico.ObtenerMedicosPorEspecialidadAsync(ValorBusqueda),
                    "correo" => await ServicioMedico.ObtenerMedicosPorCorreoAsync(ValorBusqueda),
                    _ => throw new ArgumentException("Campo de búsqueda no soportado. Use: nombre, especialidad o correo.")
                };

                return Ok(ListaMedicos);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                Registro.LogError(ex, "Error al buscar médicos.");
                return StatusCode(500, "Error interno al realizar la búsqueda de médicos.");
            }
        }

        [Authorize(Roles = "administrador,recepcionista")]
        [HttpPost]
        public async Task<ActionResult<MedicoDto>> RegistrarMedico([FromBody] MedicoDto DatosMedico)
        {
            try
            {
                var MedicoCreado = await ServicioMedico.RegistrarMedicoAsync(DatosMedico);
                return CreatedAtAction(nameof(ObtenerMedico), new { IdMedico = MedicoCreado.IdMedico }, MedicoCreado);
            }
            catch (Exception ex)
            {
                Registro.LogError(ex, "Error al registrar médico.");
                return StatusCode(500, "Ocurrió un error interno al registrar el médico.");
            }
        }

        [Authorize(Roles = "administrador,recepcionista")]
        [HttpPut("{IdMedico:int}")]
        public async Task<IActionResult> EditarMedico(int IdMedico, [FromBody] MedicoDto DatosMedico)
        {
            if (IdMedico != DatosMedico.IdMedico)
                return BadRequest("El identificador del médico no coincide.");

            var ExitoOperacion = await ServicioMedico.EditarMedicoAsync(IdMedico, DatosMedico);
            if (!ExitoOperacion) return NotFound("Médico no encontrado.");

            return NoContent();
        }

        [Authorize(Roles = "administrador")]
        [HttpPut("anular/{IdMedico:int}")]
        public async Task<IActionResult> AnularMedico(int IdMedico)
        {
            try
            {
                var ExitoOperacion = await ServicioMedico.AnularMedicoAsync(IdMedico);
                if (!ExitoOperacion) return NotFound("Médico no encontrado.");

                return Ok();
            }
            catch (Exception ex)
            {
                Registro.LogError(ex, $"Error al anular médico con ID {IdMedico}.");
                return StatusCode(500, "Ocurrió un error interno al anular el médico.");
            }
        }
    }
}
