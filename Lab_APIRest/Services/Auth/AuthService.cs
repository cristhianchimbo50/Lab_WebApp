using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.Services;
using Lab_APIRest.Services.Email;
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
        private readonly EmailService _emailService;

        private const int MaxIntentos = 5;
        private static readonly TimeSpan LockoutTiempo = TimeSpan.FromMinutes(15);

        public AuthService(
            LabDbContext db,
            TokenService tokenService,
            IMemoryCache cache,
            ILogger<AuthService> logger,
            EmailService emailService)
        {
            _db = db;
            _tokenService = tokenService;
            _cache = cache;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct)
        {
            var email = (dto.CorreoUsuario ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(dto.Clave))
                return null;

            string cacheKey = $"login_intentos_{email}";
            if (_cache.TryGetValue<int>(cacheKey, out int intentos) && intentos >= MaxIntentos)
                return null;

            var usuario = await _db.usuarios.AsNoTracking()
                .FirstOrDefaultAsync(u => u.correo_usuario.ToLower() == email, ct);

            if (usuario is null)
            {
                RegistrarIntentoFallido(email);
                return null;
            }

            var claveOk = _hasher.VerifyHashedPassword(null!, usuario.clave_usuario, dto.Clave) != PasswordVerificationResult.Failed;
            if (!claveOk)
            {
                RegistrarIntentoFallido(email);
                return null;
            }

            if (!usuario.activo)
                return null;

            _cache.Remove(cacheKey);

            if (usuario.es_contrasenia_temporal == true)
            {
                if (usuario.fecha_expira_temporal.HasValue && usuario.fecha_expira_temporal.Value < DateTime.UtcNow)
                {
                    return new LoginResponseDto
                    {
                        IdUsuario = usuario.id_usuario,
                        CorreoUsuario = usuario.correo_usuario,
                        Nombre = usuario.nombre,
                        Rol = usuario.rol,
                        EsContraseniaTemporal = true,
                        AccessToken = null,
                        ExpiresAtUtc = usuario.fecha_expira_temporal.Value,
                        Mensaje = "La contraseña temporal ha expirado. Solicite una nueva."
                    };
                }

                return new LoginResponseDto
                {
                    IdUsuario = usuario.id_usuario,
                    CorreoUsuario = usuario.correo_usuario,
                    Nombre = usuario.nombre,
                    Rol = usuario.rol,
                    EsContraseniaTemporal = true,
                    AccessToken = null,
                    ExpiresAtUtc = (DateTime)usuario.fecha_expira_temporal,
                    Mensaje = "Debe cambiar su contraseña temporal antes de continuar."
                };
            }

            try
            {
                var usuarioActualizar = new Infrastructure.EF.Models.usuario
                {
                    id_usuario = usuario.id_usuario,
                    ultimo_acceso = DateTime.UtcNow
                };
                _db.usuarios.Attach(usuarioActualizar);
                _db.Entry(usuarioActualizar).Property(x => x.ultimo_acceso).IsModified = true;
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo registrar la fecha del último acceso del usuario {UsuarioId}", usuario.id_usuario);
            }

            int? idPaciente = null;
            if (usuario.rol == "paciente")
            {
                var paciente = await _db.pacientes.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.id_usuario == usuario.id_usuario, ct);
                if (paciente != null)
                    idPaciente = paciente.id_paciente;
            }

            (string token, DateTime exp) = _tokenService.CreateToken(
                usuario.id_usuario,
                usuario.correo_usuario,
                usuario.nombre,
                usuario.rol,
                false,
                idPaciente
            );

            return new LoginResponseDto
            {
                IdUsuario = usuario.id_usuario,
                CorreoUsuario = usuario.correo_usuario,
                Nombre = usuario.nombre,
                Rol = usuario.rol,
                EsContraseniaTemporal = false,
                AccessToken = token,
                ExpiresAtUtc = exp,
                Mensaje = "Inicio de sesión exitoso. La sesión expirará en 1 hora."
            };
        }

        private void RegistrarIntentoFallido(string email)
        {
            string cacheKey = $"login_intentos_{email}";
            int intentos = _cache.TryGetValue(cacheKey, out int actuales) ? actuales : 0;
            intentos++;
            _cache.Set(cacheKey, intentos, LockoutTiempo);
        }

        public async Task<CambiarContraseniaResponseDto> CambiarContraseniaAsync(CambiarContraseniaDto dto, CancellationToken ct)
        {
            var correo = dto.CorreoUsuario.Trim().ToLowerInvariant();
            var usuario = await _db.usuarios.FirstOrDefaultAsync(u => u.correo_usuario.ToLower() == correo, ct);

            if (usuario == null)
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "Usuario no encontrado." };

            if (usuario.es_contrasenia_temporal == true &&
                usuario.fecha_expira_temporal.HasValue &&
                usuario.fecha_expira_temporal.Value < DateTime.UtcNow)
            {
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "La contraseña temporal ha expirado. Solicite una nueva." };
            }

            var verificacion = _hasher.VerifyHashedPassword(null!, usuario.clave_usuario, dto.ContraseniaActual);
            if (verificacion == PasswordVerificationResult.Failed)
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "La contraseña actual es incorrecta." };

            var nuevaHash = _hasher.HashPassword(null!, dto.NuevaContrasenia);
            usuario.clave_usuario = nuevaHash;
            usuario.es_contrasenia_temporal = false;
            usuario.fecha_expira_temporal = null;

            await _db.SaveChangesAsync(ct);

            return new CambiarContraseniaResponseDto { Exito = true, Mensaje = "Contraseña actualizada correctamente." };
        }
    }
}
