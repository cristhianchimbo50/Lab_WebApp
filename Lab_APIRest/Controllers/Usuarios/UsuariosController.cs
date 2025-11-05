using Lab_Contracts.Usuarios;
using Lab_APIRest.Services.Usuarios;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Lab_APIRest.Controllers.Usuarios
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService UsuariosService;

        public UsuariosController(IUsuariosService UsuariosService)
        {
            this.UsuariosService = UsuariosService;
        }

        [HttpGet]
        public async Task<ActionResult<List<UsuarioListadoDto>>> ListarUsuarios([FromQuery] UsuarioFiltroDto Filtro, CancellationToken Ct)
        {
            var Resultados = await UsuariosService.ListarUsuariosAsync(Filtro, Ct);
            return Ok(Resultados);
        }

        [HttpGet("{IdUsuario}")]
        public async Task<ActionResult<UsuarioListadoDto>> ObtenerUsuarioPorId(int IdUsuario, CancellationToken Ct)
        {
            var Resultado = await UsuariosService.ObtenerUsuarioPorIdAsync(IdUsuario, Ct);
            if (Resultado == null) return NotFound();
            return Ok(Resultado);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CrearUsuario([FromBody] UsuarioCrearDto Usuario, CancellationToken Ct)
        {
            var IdGenerado = await UsuariosService.CrearUsuarioAsync(Usuario, Ct);
            return Ok(IdGenerado);
        }

        [HttpPut("{IdUsuario}")]
        public async Task<ActionResult> EditarUsuario(int IdUsuario, [FromBody] UsuarioEditarDto Usuario, CancellationToken Ct)
        {
            if (IdUsuario != Usuario.IdUsuario) return BadRequest();
            var OkEditar = await UsuariosService.EditarUsuarioAsync(Usuario, Ct);
            if (!OkEditar) return NotFound();
            return Ok();
        }

        [HttpPut("{IdUsuario}/estado")]
        public async Task<IActionResult> CambiarEstado(int IdUsuario, [FromBody] bool Activo, CancellationToken Ct)
        {
            var CorreoActual = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ?? "";

            try
            {
                var Exito = await UsuariosService.CambiarEstadoAsync(IdUsuario, Activo, CorreoActual, Ct);
                if (!Exito) return NotFound();
                return Ok();
            }
            catch (InvalidOperationException Ex)
            {
                return BadRequest(new { Mensaje = Ex.Message });
            }
        }

        [HttpPut("{IdUsuario}/reenviar")]
        public async Task<ActionResult<UsuarioReenviarDto>> ReenviarCredencialesTemporales(int IdUsuario, CancellationToken Ct)
        {
            var Resultado = await UsuariosService.ReenviarCredencialesTemporalesAsync(IdUsuario, Ct);
            if (Resultado == null) return NotFound();
            return Ok(Resultado);
        }
    }
}
