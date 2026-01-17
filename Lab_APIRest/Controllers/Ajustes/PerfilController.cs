using Lab_Contracts.Ajustes.Perfil;
using Lab_APIRest.Services.Perfil;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lab_APIRest.Controllers.Ajustes
{
    [ApiController]
    [Route("api/ajustes/[controller]")]
    [Authorize(Roles = "1,2,3,4")]
    public class PerfilController : ControllerBase
    {
        private readonly IPerfilService _perfilService;
        private readonly ILogger<PerfilController> _logger;

        public PerfilController(IPerfilService perfilService, ILogger<PerfilController> logger)
        {
            _perfilService = perfilService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDetallePerfil(CancellationToken ct)
        {
            try
            {
                var idUsuarioClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(idUsuarioClaim) || !int.TryParse(idUsuarioClaim, out var idUsuario))
                    return Unauthorized(new { Mensaje = "No se pudo identificar al usuario autenticado." });

                var perfil = await _perfilService.ObtenerDetallePerfilAsync(idUsuario, ct);
                if (perfil == null)
                    return NotFound(new { Mensaje = "No se encontró información del perfil del usuario." });

                return Ok(perfil);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil del usuario autenticado.");
                return StatusCode(500, new { Mensaje = "Ocurrió un error al obtener el perfil." });
            }
        }
    }
}
