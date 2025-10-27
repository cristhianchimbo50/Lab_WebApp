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
        private readonly IUsuariosService _service;

        public UsuariosController(IUsuariosService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<UsuarioListadoDto>>> GetUsuarios([FromQuery] UsuarioFiltroDto filtro, CancellationToken ct)
        {
            var result = await _service.GetUsuariosAsync(filtro, ct);
            return Ok(result);
        }

        [HttpGet("{idUsuario}")]
        public async Task<ActionResult<UsuarioListadoDto>> GetUsuarioPorId(int idUsuario, CancellationToken ct)
        {
            var result = await _service.GetUsuarioPorIdAsync(idUsuario, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CrearUsuario([FromBody] UsuarioCrearDto dto, CancellationToken ct)
        {
            var id = await _service.CrearUsuarioAsync(dto, ct);
            return Ok(id);
        }

        [HttpPut("{idUsuario}")]
        public async Task<ActionResult> EditarUsuario(int idUsuario, [FromBody] UsuarioEditarDto dto, CancellationToken ct)
        {
            if (idUsuario != dto.IdUsuario) return BadRequest();
            var ok = await _service.EditarUsuarioAsync(dto, ct);
            if (!ok) return NotFound();
            return Ok();
        }

        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] bool activo, CancellationToken ct)
        {


            var correoActual = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ?? "";

            try
            {
                var exito = await _service.CambiarEstadoAsync(id, activo, correoActual, ct);
                if (!exito) return NotFound();
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Mensaje = ex.Message });
            }
        }




        [HttpPut("{idUsuario}/reenviar")]
        public async Task<ActionResult<UsuarioReenviarDto>> ReenviarCredencialesTemporales(int idUsuario, CancellationToken ct)
        {
            var result = await _service.ReenviarCredencialesTemporalesAsync(idUsuario, ct);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
