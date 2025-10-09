using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.Services;
using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly LabDbContext _db;
    private readonly TokenService _tokens;
    private readonly IMemoryCache _cache;
    private readonly PasswordHasher<object> _hasher = new();

    const int MaxIntentos = 5;
    static readonly TimeSpan Lockout = TimeSpan.FromMinutes(15);

    public AuthController(LabDbContext db, TokenService tokens, IMemoryCache cache)
    {
        _db = db; _tokens = tokens; _cache = cache;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var email = (dto.CorreoUsuario ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(dto.Clave))
            return Unauthorized(new { error = "Datos vacíos" });

        var usuario = await _db.usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.correo_usuario.ToLower() == email, ct);

        if (usuario is null)
            return Unauthorized(new { error = "Usuario no encontrado" });

        var claveOk = _hasher.VerifyHashedPassword(null!, usuario.clave_usuario, dto.Clave) != PasswordVerificationResult.Failed;
        if (!claveOk)
            return Unauthorized(new { error = "Clave incorrecta" });

        if (!string.Equals(usuario.estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { error = "Usuario no activo" });

        (string token, DateTime exp) = _tokens.CreateToken(
            usuario.id_usuario,
            usuario.correo_usuario,
            usuario.nombre,
            usuario.rol,
            usuario.es_contraseña_temporal ?? false);

        var resp = new LoginResponseDto
        {
            IdUsuario = usuario.id_usuario,
            CorreoUsuario = usuario.correo_usuario,
            Nombre = usuario.nombre,
            Rol = usuario.rol,
            EsContraseñaTemporal = usuario.es_contraseña_temporal ?? false,
            AccessToken = token,
            ExpiresAtUtc = exp
        };
        return Ok(resp);
    }
}
