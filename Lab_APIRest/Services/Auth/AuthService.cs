using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.Services;
using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Lab_APIRest.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly LabDbContext _db;
        private readonly TokenService _tokenService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthService> _logger;
        private readonly PasswordHasher<object> _hasher = new();

        private const int MaxIntentos = 5;
        private static readonly TimeSpan LockoutTiempo = TimeSpan.FromMinutes(15);

        public AuthService(LabDbContext db, TokenService tokenService, IMemoryCache cache, ILogger<AuthService> logger)
        {
            _db = db;
            _tokenService = tokenService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct)
        {
            var email = (dto.CorreoUsuario ?? "").Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(dto.Clave))
                return null;

            string cacheKey = $"login_intentos_{email}";
            if (_cache.TryGetValue<int>(cacheKey, out int intentos) && intentos >= MaxIntentos)
            {
                _logger.LogWarning("Cuenta bloqueada temporalmente para {Email}", email);
                return null;
            }

            var usuario = await _db.usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.correo_usuario.ToLower() == email, ct);

            if (usuario is null)
            {
                RegistrarIntentoFallido(email);
                _logger.LogWarning("Intento de login fallido - usuario no encontrado: {Email}", email);
                return null;
            }

            var claveOk = _hasher.VerifyHashedPassword(null!, usuario.clave_usuario, dto.Clave) != PasswordVerificationResult.Failed;
            if (!claveOk)
            {
                RegistrarIntentoFallido(email);
                _logger.LogWarning("Intento de login fallido - contraseña incorrecta: {Email}", email);
                return null;
            }

            if (!string.Equals(usuario.estado, "ACTIVO", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Usuario inactivo: {Email}", email);
                return null;
            }

            // Limpia intentos fallidos
            _cache.Remove(cacheKey);

            (string token, DateTime exp) = _tokenService.CreateToken(
                usuario.id_usuario,
                usuario.correo_usuario,
                usuario.nombre,
                usuario.rol,
                usuario.es_contraseña_temporal ?? false
            );

            _logger.LogInformation("Inicio de sesión exitoso: {Email}", usuario.correo_usuario);

            return new LoginResponseDto
            {
                IdUsuario = usuario.id_usuario,
                CorreoUsuario = usuario.correo_usuario,
                Nombre = usuario.nombre,
                Rol = usuario.rol,
                EsContraseñaTemporal = usuario.es_contraseña_temporal ?? false,
                AccessToken = token,
                ExpiresAtUtc = exp
            };
        }

        private void RegistrarIntentoFallido(string email)
        {
            string cacheKey = $"login_intentos_{email}";
            int intentos = _cache.TryGetValue(cacheKey, out int actuales) ? actuales : 0;
            intentos++;
            _cache.Set(cacheKey, intentos, LockoutTiempo);

            if (intentos >= MaxIntentos)
                _logger.LogWarning("Usuario {Email} bloqueado temporalmente por intentos fallidos.", email);
        }
    }
}
