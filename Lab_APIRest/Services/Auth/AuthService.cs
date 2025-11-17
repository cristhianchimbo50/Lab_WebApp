using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.Services;
using Lab_APIRest.Services.Email;
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
            string CorreoUsuario,
            string Nombre,
            string Rol,
            string? ClaveUsuario,
            bool? Activo
        );

        private static readonly Func<LabDbContext, string, IAsyncEnumerable<UsuarioLoginProjection>> QryUsuarioPorCorreo =
            EF.CompileAsyncQuery((LabDbContext ctx, string correo) =>
                ctx.Usuario
                    .AsNoTracking()
                    .Where(u => u.CorreoUsuario == correo)
                    .Select(u => new UsuarioLoginProjection(
                        u.IdUsuario,
                        u.CorreoUsuario,
                        u.Nombre,
                        u.Rol,
                        u.ClaveUsuario,
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

            if (string.IsNullOrEmpty(usuarioEntidad.ClaveUsuario))
            {
                RegistrarIntentoFallido(emailNorm);
                return null;
            }

            var claveOk = _hasher.VerifyHashedPassword(null!, usuarioEntidad.ClaveUsuario!, solicitud.Clave) != PasswordVerificationResult.Failed;
            if (!claveOk)
            {
                RegistrarIntentoFallido(emailNorm);
                return null;
            }

            // Activo es nullable en el modelo nuevo
            if (usuarioEntidad.Activo != true)
                return null;

            _cache.Remove(cacheKey);

            try
            {
                // Actualizamos solo UltimoAcceso usando la nueva entidad Usuario
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
            if (usuarioEntidad.Rol == "paciente")
            {
                var pacienteEntidad = await _context.Paciente.AsNoTracking()
                    .Where(p => p.IdUsuario == usuarioEntidad.IdUsuario)
                    .Select(p => new { p.IdPaciente })
                    .FirstOrDefaultAsync(ct);
                if (pacienteEntidad != null)
                    idPaciente = pacienteEntidad.IdPaciente;
            }

            (string token, DateTime expiraUtc) = _tokenService.CreateToken(
                usuarioEntidad.IdUsuario,
                usuarioEntidad.CorreoUsuario,
                usuarioEntidad.Nombre,
                usuarioEntidad.Rol,
                false,
                idPaciente
            );

            return new LoginResponseDto
            {
                IdUsuario = usuarioEntidad.IdUsuario,
                CorreoUsuario = usuarioEntidad.CorreoUsuario,
                Nombre = usuarioEntidad.Nombre,
                Rol = usuarioEntidad.Rol,
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
            var usuarioEntidad = await _context.Usuario.FirstOrDefaultAsync(u => u.CorreoUsuario.ToLower() == correo, ct);

            if (usuarioEntidad == null)
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "Usuario no encontrado." };

            if (string.IsNullOrEmpty(usuarioEntidad.ClaveUsuario))
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "Credenciales inválidas." };

            var verificacion = _hasher.VerifyHashedPassword(null!, usuarioEntidad.ClaveUsuario!, cambio.ContraseniaActual);
            if (verificacion == PasswordVerificationResult.Failed)
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "La contraseña actual es incorrecta." };

            var nuevaHash = _hasher.HashPassword(null!, cambio.NuevaContrasenia);
            usuarioEntidad.ClaveUsuario = nuevaHash;

            await _context.SaveChangesAsync(ct);

            return new CambiarContraseniaResponseDto { Exito = true, Mensaje = "Contraseña actualizada correctamente." };
        }

        public async Task<RespuestaMensajeDto> ActivarCuentaAsync(RestablecerContraseniaDto dto, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(dto.Token) ||
                string.IsNullOrWhiteSpace(dto.NuevaContrasenia) ||
                string.IsNullOrWhiteSpace(dto.ConfirmarContrasenia))
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Datos inválidos." };

            if (dto.NuevaContrasenia != dto.ConfirmarContrasenia)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Las contraseñas no coinciden." };

            using var sha = SHA256.Create();
            var tokenHash = sha.ComputeHash(Encoding.UTF8.GetBytes(dto.Token));

            // Ajuste a nuevos nombres de DbSet y propiedades
            var registros = await _context.TokensUsuarios
                .Include(r => r.IdUsuarioNavigation)
                .Where(r => r.TipoToken == "activacion" && !r.Usado)
                .ToListAsync(ct);

            var registro = registros.FirstOrDefault(r => r.TokenHash.SequenceEqual(tokenHash));

            if (registro == null)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace no es válido o ya fue usado." };

            if (registro.FechaExpiracion < DateTime.UtcNow)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace ha expirado. Solicita uno nuevo." };

            var usuario = registro.IdUsuarioNavigation;
            usuario.ClaveUsuario = _hasher.HashPassword(null!, dto.NuevaContrasenia);
            usuario.Activo = true;

            registro.Usado = true;
            registro.UsadoEn = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            var asunto = "Cuenta activada correctamente";
            var cuerpo = $@"\n        <p>Hola <b>{usuario.Nombre}</b>,</p>\n        <p>Tu cuenta ha sido activada exitosamente.</p>\n        <p>Ya puedes iniciar sesión con tu correo registrado.</p>";

            await _emailService.EnviarCorreoAsync(usuario.CorreoUsuario, usuario.Nombre, asunto, cuerpo);

            return new RespuestaMensajeDto
            {
                Exito = true,
                Mensaje = "Cuenta activada correctamente. Ya puedes iniciar sesión."
            };
        }
    }
}
