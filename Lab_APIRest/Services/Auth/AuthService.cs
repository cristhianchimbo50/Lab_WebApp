using Lab_APIRest.Infrastructure.EF;
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
            int id_usuario,
            string correo_usuario,
            string nombre,
            string rol,
            string clave_usuario,
            bool activo
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
                        u.activo
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

            var verificacion = _hasher.VerifyHashedPassword(null!, usuarioEntidad.clave_usuario, cambio.ContraseniaActual);
            if (verificacion == PasswordVerificationResult.Failed)
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "La contraseña actual es incorrecta." };

            var nuevaHash = _hasher.HashPassword(null!, cambio.NuevaContrasenia);
            usuarioEntidad.clave_usuario = nuevaHash;

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

            var registros = await _context.tokens_usuarios
                .Include(r => r.Usuario)
                .Where(r => r.tipo_token == "activacion" && !r.usado)
                .ToListAsync(ct);

            var registro = registros.FirstOrDefault(r => r.token_hash.SequenceEqual(tokenHash));

            if (registro == null)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace no es válido o ya fue usado." };

            if (registro.fecha_expiracion < DateTime.UtcNow)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace ha expirado. Solicita uno nuevo." };

            var usuario = registro.Usuario;
            usuario.clave_usuario = _hasher.HashPassword(null!, dto.NuevaContrasenia);
            usuario.activo = true;

            registro.usado = true;
            registro.usado_en = DateTime.UtcNow;

            await _context.SaveChangesAsync(ct);

            var asunto = "Cuenta activada correctamente";
            var cuerpo = $@"
        <p>Hola <b>{usuario.nombre}</b>,</p>
        <p>Tu cuenta ha sido activada exitosamente.</p>
        <p>Ya puedes iniciar sesión con tu correo registrado.</p>";

            await _emailService.EnviarCorreoAsync(usuario.correo_usuario, usuario.nombre, asunto, cuerpo);

            return new RespuestaMensajeDto
            {
                Exito = true,
                Mensaje = "Cuenta activada correctamente. Ya puedes iniciar sesión."
            };
        }
    }
}
