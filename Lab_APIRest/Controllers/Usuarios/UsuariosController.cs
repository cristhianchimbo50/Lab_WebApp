using Lab_Contracts.Usuarios;
using Lab_APIRest.Services.Usuarios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Lab_APIRest.Controllers.Usuarios
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "1")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuariosService _usuariosService;

        public UsuariosController(IUsuariosService usuariosService)
        {
            _usuariosService = usuariosService;
        }

        [HttpGet]
        public async Task<ActionResult<List<UsuarioListadoDto>>> ListarUsuarios([FromQuery] UsuarioFiltroDto filtro, CancellationToken ct)
        {
            var resultados = await _usuariosService.ListarUsuariosAsync(filtro, ct);
            return Ok(resultados);
        }

        [HttpGet("{idUsuario}")]
        public async Task<ActionResult<UsuarioListadoDto>> ObtenerDetalleUsuario(int idUsuario, CancellationToken ct)
        {
            var resultado = await _usuariosService.ObtenerDetalleUsuarioAsync(idUsuario, ct);
            if (resultado == null) return NotFound();
            return Ok(resultado);
        }

        [HttpPost]
        public async Task<ActionResult<int>> GuardarUsuario([FromBody] UsuarioCrearDto usuario, CancellationToken ct)
        {
            var idGenerado = await _usuariosService.GuardarUsuarioAsync(usuario, ct);
            return Ok(idGenerado);
        }

        [HttpPut("{idUsuario}")]
        public async Task<ActionResult> GuardarUsuario(int idUsuario, [FromBody] UsuarioEditarDto usuario, CancellationToken ct)
        {
            if (idUsuario != usuario.IdUsuario) return BadRequest();
            var ok = await _usuariosService.GuardarUsuarioAsync(usuario, ct);
            if (!ok) return NotFound();
            return Ok();
        }

        [HttpPut("{idUsuario}/estado")]
        public async Task<IActionResult> CambiarEstadoUsuario(int idUsuario, [FromBody] bool activo, CancellationToken ct)
        {
            var correoActual = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ?? "";
            try
            {
                var exito = await _usuariosService.CambiarEstadoUsuarioAsync(idUsuario, activo, correoActual, ct);
                if (!exito) return NotFound();
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Mensaje = ex.Message });
            }
        }
    }
}
