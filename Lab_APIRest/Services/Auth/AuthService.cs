using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.Services;
using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

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
            int IdUsuario,
            int IdPersona,
            string Correo,
            string Nombres,
            string Apellidos,
            int IdRol,
            string RolNombre,
            string? PasswordHash,
            bool? Activo
        );

        private static readonly Func<LabDbContext, string, IAsyncEnumerable<UsuarioLoginProjection>> QryUsuarioPorCorreo =
            EF.CompileAsyncQuery((LabDbContext ctx, string correo) =>
                ctx.Usuario
                    .AsNoTracking()
                    .Include(u => u.IdRolNavigation)
                    .Include(u => u.IdPersonaNavigation)
                    .Where(u => u.IdPersonaNavigation.Correo == correo)
                    .Select(u => new UsuarioLoginProjection(
                        u.IdUsuario,
                        u.IdPersona,
                        u.IdPersonaNavigation.Correo,
                        u.IdPersonaNavigation.Nombres,
                        u.IdPersonaNavigation.Apellidos,
                        u.IdRol,
                        u.IdRolNavigation.Nombre,
                        u.PasswordHash,
                        u.Activo
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

            if (string.IsNullOrEmpty(usuarioEntidad.PasswordHash))
            {
                RegistrarIntentoFallido(emailNorm);
                return null;
            }

            var claveOk = _hasher.VerifyHashedPassword(null!, usuarioEntidad.PasswordHash!, solicitud.Clave) != PasswordVerificationResult.Failed;
            if (!claveOk)
            {
                RegistrarIntentoFallido(emailNorm);
                return null;
            }

            if (usuarioEntidad.Activo != true)
                return null;

            _cache.Remove(cacheKey);

            try
            {
                var usuarioActualizar = new Usuario
                {
                    IdUsuario = usuarioEntidad.IdUsuario,
                    UltimoAcceso = DateTime.UtcNow
                };
                _context.Usuario.Attach(usuarioActualizar);
                _context.Entry(usuarioActualizar).Property(x => x.UltimoAcceso).IsModified = true;
                await _context.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo registrar la fecha del último acceso del usuario {UsuarioId}", usuarioEntidad.IdUsuario);
            }

            int? idPaciente = null;
            var esPaciente = string.Equals(usuarioEntidad.RolNombre, "paciente", StringComparison.OrdinalIgnoreCase)
                || usuarioEntidad.IdRol == 4;

            if (esPaciente)
            {
                var pacienteEntidad = await _context.Paciente.AsNoTracking()
                    .Where(p => p.IdPersona == usuarioEntidad.IdPersona)
                    .Select(p => new { p.IdPaciente })
                    .FirstOrDefaultAsync(ct);
                if (pacienteEntidad != null)
                    idPaciente = pacienteEntidad.IdPaciente;
            }

            (string token, DateTime expiraUtc) = _tokenService.CreateToken(
                usuarioEntidad.IdUsuario,
                usuarioEntidad.Correo,
                $"{usuarioEntidad.Nombres} {usuarioEntidad.Apellidos}",
                usuarioEntidad.IdRol,
                usuarioEntidad.RolNombre,
                false,
                idPaciente
            );

            return new LoginResponseDto
            {
                IdUsuario = usuarioEntidad.IdUsuario,
                CorreoUsuario = usuarioEntidad.Correo,
                Nombre = $"{usuarioEntidad.Nombres} {usuarioEntidad.Apellidos}",
                IdRol = usuarioEntidad.IdRol,
                NombreRol = usuarioEntidad.RolNombre,
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
            var usuarioEntidad = await _context.Usuario
                .Include(u => u.IdPersonaNavigation)
                .FirstOrDefaultAsync(u => u.IdPersonaNavigation.Correo.ToLower() == correo, ct);

            if (usuarioEntidad == null)
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "Usuario no encontrado." };

            if (string.IsNullOrEmpty(usuarioEntidad.PasswordHash))
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "Credenciales inválidas." };

            var verificacion = _hasher.VerifyHashedPassword(null!, usuarioEntidad.PasswordHash!, cambio.ContraseniaActual);
            if (verificacion == PasswordVerificationResult.Failed)
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "La contraseña actual es incorrecta." };

            var nuevaHash = _hasher.HashPassword(null!, cambio.NuevaContrasenia);
            usuarioEntidad.PasswordHash = nuevaHash;

            await _context.SaveChangesAsync(ct);

            return new CambiarContraseniaResponseDto { Exito = true, Mensaje = "Contraseña actualizada correctamente." };
        }

        public async Task<RespuestaMensajeDto> ActivarCuentaAsync(RestablecerContraseniaDto dto, CancellationToken ct)
        {
            var token = (dto.Token ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(dto.NuevaContrasenia) ||
                string.IsNullOrWhiteSpace(dto.ConfirmarContrasenia))
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Datos inválidos." };

            if (dto.NuevaContrasenia != dto.ConfirmarContrasenia)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Las contraseñas no coinciden." };

            var tokenHash = CalcularHash(token);

            var registro = await _context.TokensUsuarios
                .Include(r => r.IdUsuarioNavigation)
                .FirstOrDefaultAsync(r => r.TipoToken == "activacion" && !r.Usado && r.TokenHash == tokenHash, ct);

            if (registro == null)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace no es válido o ya fue usado." };

            if (registro.FechaExpiracion < DateTime.UtcNow)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace ha expirado. Solicita uno nuevo." };

            var usuario = registro.IdUsuarioNavigation;
            usuario.PasswordHash = _hasher.HashPassword(null!, dto.NuevaContrasenia);
            usuario.Activo = true;

            registro.Usado = true;
            registro.UsadoEn = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            var persona = await _context.Persona.FirstOrDefaultAsync(p => p.IdPersona == usuario.IdPersona, ct);

            var asunto = "Cuenta activada correctamente";

            var cuerpo = $@"
                <p>Hola <strong>{persona?.Nombres} {persona?.Apellidos}</strong>,</p>

                <p>Tu cuenta ha sido activada exitosamente.</p>

                <p>Ya puedes iniciar sesión con tu correo registrado.</p>";

            var correoDestino = persona?.Correo ?? string.Empty;
            var nombreDestino = $"{persona?.Nombres} {persona?.Apellidos}".Trim();
            await _emailService.EnviarCorreoAsync(correoDestino, nombreDestino, asunto, cuerpo);

            return new RespuestaMensajeDto
            {
                Exito = true,
                Mensaje = "Cuenta activada correctamente. Ya puedes iniciar sesión."
            };
        }

        private static byte[] CalcularHash(string token)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        }
    }
}
