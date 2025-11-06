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
        private readonly LabDbContext _context;
        private readonly TokenService _tokenService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AuthService> _logger;
        private readonly PasswordHasher<object> _hasher = new();
        private readonly EmailService _emailService;

        private const int MaxIntentos = 5;
        private static readonly TimeSpan LockoutTiempo = TimeSpan.FromMinutes(15);

        private sealed record UsuarioLoginProjection(
            int id_usuario,
            string correo_usuario,
            string nombre,
            string rol,
            string clave_usuario,
            bool activo,
            bool es_contrasenia_temporal,
            DateTime? fecha_expira_temporal
        );

        private static readonly Func<LabDbContext, string, IAsyncEnumerable<UsuarioLoginProjection>> QryUsuarioPorCorreo =
            EF.CompileAsyncQuery((LabDbContext ctx, string correo) =>
                ctx.usuarios
                    .AsNoTracking()
                    .Where(u => u.correo_usuario == correo)
                    .Select(u => new UsuarioLoginProjection(
                        u.id_usuario,
                        u.correo_usuario,
                        u.nombre,
                        u.rol,
                        u.clave_usuario,
                        u.activo,
                        u.es_contrasenia_temporal,
                        u.fecha_expira_temporal
                    ))
            );

        public AuthService(
            LabDbContext context,
            TokenService tokenService,
            IMemoryCache cache,
            ILogger<AuthService> logger,
            EmailService emailService)
        {
            _context = context;
            _tokenService = tokenService;
            _cache = cache;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<LoginResponseDto?> IniciarSesionAsync(LoginRequestDto solicitud, CancellationToken ct)
        {
            var email = (solicitud.CorreoUsuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(solicitud.Clave))
                return null;

            var emailNorm = email.ToLowerInvariant();

            string cacheKey = $"login_intentos_{emailNorm}";
            if (_cache.TryGetValue<int>(cacheKey, out int intentos) && intentos >= MaxIntentos)
                return null;

            UsuarioLoginProjection? usuarioEntidad = null;
            await foreach (var u in QryUsuarioPorCorreo(_context, email).WithCancellation(ct))
            {
                usuarioEntidad = u;
                break;
            }

            if (usuarioEntidad is null)
            {
                RegistrarIntentoFallido(emailNorm);
                return null;
            }

            var claveOk = _hasher.VerifyHashedPassword(null!, usuarioEntidad.clave_usuario, solicitud.Clave) != PasswordVerificationResult.Failed;
            if (!claveOk)
            {
                RegistrarIntentoFallido(emailNorm);
                return null;
            }

            if (!usuarioEntidad.activo)
                return null;

            _cache.Remove(cacheKey);

            if (usuarioEntidad.es_contrasenia_temporal == true)
            {
                if (usuarioEntidad.fecha_expira_temporal.HasValue && usuarioEntidad.fecha_expira_temporal.Value < DateTime.UtcNow)
                {
                    return new LoginResponseDto
                    {
                        IdUsuario = usuarioEntidad.id_usuario,
                        CorreoUsuario = usuarioEntidad.correo_usuario,
                        Nombre = usuarioEntidad.nombre,
                        Rol = usuarioEntidad.rol,
                        EsContraseniaTemporal = true,
                        AccessToken = null!,
                        ExpiresAtUtc = usuarioEntidad.fecha_expira_temporal.Value,
                        Mensaje = "La contraseña temporal ha expirado. Solicite una nueva."
                    };
                }

                return new LoginResponseDto
                {
                    IdUsuario = usuarioEntidad.id_usuario,
                    CorreoUsuario = usuarioEntidad.correo_usuario,
                    Nombre = usuarioEntidad.nombre,
                    Rol = usuarioEntidad.rol,
                    EsContraseniaTemporal = true,
                    AccessToken = null!,
                    ExpiresAtUtc = usuarioEntidad.fecha_expira_temporal ?? DateTime.UtcNow,
                    Mensaje = "Debe cambiar su contraseña temporal antes de continuar."
                };
            }

            try
            {
                var usuarioActualizar = new Infrastructure.EF.Models.usuario
                {
                    id_usuario = usuarioEntidad.id_usuario,
                    ultimo_acceso = DateTime.UtcNow
                };
                _context.usuarios.Attach(usuarioActualizar);
                _context.Entry(usuarioActualizar).Property(x => x.ultimo_acceso).IsModified = true;
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo registrar la fecha del último acceso del usuario {UsuarioId}", usuarioEntidad.id_usuario);
            }

            int? idPaciente = null;
            if (usuarioEntidad.rol == "paciente")
            {
                var pacienteEntidad = await _context.pacientes.AsNoTracking()
                    .Where(p => p.id_usuario == usuarioEntidad.id_usuario)
                    .Select(p => new { p.id_paciente })
                    .FirstOrDefaultAsync(ct);
                if (pacienteEntidad != null)
                    idPaciente = pacienteEntidad.id_paciente;
            }

            (string token, DateTime expiraUtc) = _tokenService.CreateToken(
                usuarioEntidad.id_usuario,
                usuarioEntidad.correo_usuario,
                usuarioEntidad.nombre,
                usuarioEntidad.rol,
                false,
                idPaciente
            );

            return new LoginResponseDto
            {
                IdUsuario = usuarioEntidad.id_usuario,
                CorreoUsuario = usuarioEntidad.correo_usuario,
                Nombre = usuarioEntidad.nombre,
                Rol = usuarioEntidad.rol,
                EsContraseniaTemporal = false,
                AccessToken = token,
                ExpiresAtUtc = expiraUtc,
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

        public async Task<CambiarContraseniaResponseDto> CambiarContraseniaAsync(CambiarContraseniaDto cambio, CancellationToken ct)
        {
            var correo = cambio.CorreoUsuario.Trim().ToLowerInvariant();
            var usuarioEntidad = await _context.usuarios.FirstOrDefaultAsync(u => u.correo_usuario.ToLower() == correo, ct);

            if (usuarioEntidad == null)
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "Usuario no encontrado." };

            if (usuarioEntidad.es_contrasenia_temporal == true &&
                usuarioEntidad.fecha_expira_temporal.HasValue &&
                usuarioEntidad.fecha_expira_temporal.Value < DateTime.UtcNow)
            {
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "La contraseña temporal ha expirado. Solicite una nueva." };
            }

            var verificacion = _hasher.VerifyHashedPassword(null!, usuarioEntidad.clave_usuario, cambio.ContraseniaActual);
            if (verificacion == PasswordVerificationResult.Failed)
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "La contraseña actual es incorrecta." };

            var nuevaHash = _hasher.HashPassword(null!, cambio.NuevaContrasenia);
            usuarioEntidad.clave_usuario = nuevaHash;
            usuarioEntidad.es_contrasenia_temporal = false;
            usuarioEntidad.fecha_expira_temporal = null;

            await _context.SaveChangesAsync(ct);

            return new CambiarContraseniaResponseDto { Exito = true, Mensaje = "Contraseña actualizada correctamente." };
        }
    }
}
