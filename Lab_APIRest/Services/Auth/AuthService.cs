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
        private readonly LabDbContext Contexto;
        private readonly TokenService TokenService;
        private readonly IMemoryCache Cache;
        private readonly ILogger<AuthService> Logger;
        private readonly PasswordHasher<object> Hasher = new();
        private readonly EmailService EmailService;

        private const int MaxIntentos = 5;
        private static readonly TimeSpan LockoutTiempo = TimeSpan.FromMinutes(15);

        public AuthService(
            LabDbContext Contexto,
            TokenService TokenService,
            IMemoryCache Cache,
            ILogger<AuthService> Logger,
            EmailService EmailService)
        {
            this.Contexto = Contexto;
            this.TokenService = TokenService;
            this.Cache = Cache;
            this.Logger = Logger;
            this.EmailService = EmailService;
        }

        public async Task<LoginResponseDto?> IniciarSesionAsync(LoginRequestDto Solicitud, CancellationToken Ct)
        {
            var Email = (Solicitud.CorreoUsuario ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Solicitud.Clave))
                return null;

            // Normalizar a minúsculas solo en memoria; no aplicar ToLower a la columna para preservar uso de índices.
            var emailNorm = Email.ToLowerInvariant();

            string CacheKey = $"login_intentos_{emailNorm}";
            if (Cache.TryGetValue<int>(CacheKey, out int Intentos) && Intentos >= MaxIntentos)
                return null;

            // Proyección ligera para evitar materializar columnas no usadas y acelerar consulta
            var UsuarioEntidad = await Contexto.usuarios
                .AsNoTracking()
                .Where(U => U.correo_usuario == Email || U.correo_usuario == emailNorm)
                .Select(u => new
                {
                    u.id_usuario,
                    u.correo_usuario,
                    u.nombre,
                    u.rol,
                    u.clave_usuario,
                    u.activo,
                    u.es_contrasenia_temporal,
                    u.fecha_expira_temporal
                })
                .FirstOrDefaultAsync(Ct);

            if (UsuarioEntidad is null)
            {
                RegistrarIntentoFallido(emailNorm);
                return null;
            }

            var ClaveOk = Hasher.VerifyHashedPassword(null!, UsuarioEntidad.clave_usuario, Solicitud.Clave) != PasswordVerificationResult.Failed;
            if (!ClaveOk)
            {
                RegistrarIntentoFallido(emailNorm);
                return null;
            }

            if (!UsuarioEntidad.activo)
                return null;

            Cache.Remove(CacheKey);

            if (UsuarioEntidad.es_contrasenia_temporal == true)
            {
                if (UsuarioEntidad.fecha_expira_temporal.HasValue && UsuarioEntidad.fecha_expira_temporal.Value < DateTime.UtcNow)
                {
                    return new LoginResponseDto
                    {
                        IdUsuario = UsuarioEntidad.id_usuario,
                        CorreoUsuario = UsuarioEntidad.correo_usuario,
                        Nombre = UsuarioEntidad.nombre,
                        Rol = UsuarioEntidad.rol,
                        EsContraseniaTemporal = true,
                        AccessToken = null!,
                        ExpiresAtUtc = UsuarioEntidad.fecha_expira_temporal.Value,
                        Mensaje = "La contraseña temporal ha expirado. Solicite una nueva."
                    };
                }

                return new LoginResponseDto
                {
                    IdUsuario = UsuarioEntidad.id_usuario,
                    CorreoUsuario = UsuarioEntidad.correo_usuario,
                    Nombre = UsuarioEntidad.nombre,
                    Rol = UsuarioEntidad.rol,
                    EsContraseniaTemporal = true,
                    AccessToken = null!,
                    ExpiresAtUtc = UsuarioEntidad.fecha_expira_temporal ?? DateTime.UtcNow,
                    Mensaje = "Debe cambiar su contraseña temporal antes de continuar."
                };
            }

            try
            {
                var UsuarioActualizar = new Infrastructure.EF.Models.usuario
                {
                    id_usuario = UsuarioEntidad.id_usuario,
                    ultimo_acceso = DateTime.UtcNow
                };
                Contexto.usuarios.Attach(UsuarioActualizar);
                Contexto.Entry(UsuarioActualizar).Property(x => x.ultimo_acceso).IsModified = true;
                await Contexto.SaveChangesAsync(Ct);
            }
            catch (Exception Ex)
            {
                Logger.LogWarning(Ex, "No se pudo registrar la fecha del último acceso del usuario {UsuarioId}", UsuarioEntidad.id_usuario);
            }

            int? IdPaciente = null;
            if (UsuarioEntidad.rol == "paciente")
            {
                var PacienteEntidad = await Contexto.pacientes.AsNoTracking()
                    .Where(P => P.id_usuario == UsuarioEntidad.id_usuario)
                    .Select(p => new { p.id_paciente })
                    .FirstOrDefaultAsync(Ct);
                if (PacienteEntidad != null)
                    IdPaciente = PacienteEntidad.id_paciente;
            }

            (string Token, DateTime ExpiraUtc) = TokenService.CreateToken(
                UsuarioEntidad.id_usuario,
                UsuarioEntidad.correo_usuario,
                UsuarioEntidad.nombre,
                UsuarioEntidad.rol,
                false,
                IdPaciente
            );

            return new LoginResponseDto
            {
                IdUsuario = UsuarioEntidad.id_usuario,
                CorreoUsuario = UsuarioEntidad.correo_usuario,
                Nombre = UsuarioEntidad.nombre,
                Rol = UsuarioEntidad.rol,
                EsContraseniaTemporal = false,
                AccessToken = Token,
                ExpiresAtUtc = ExpiraUtc,
                Mensaje = "Inicio de sesión exitoso. La sesión expirará en 1 hora."
            };
        }

        private void RegistrarIntentoFallido(string Email)
        {
            string CacheKey = $"login_intentos_{Email}";
            int Intentos = Cache.TryGetValue(CacheKey, out int actuales) ? actuales : 0;
            Intentos++;
            Cache.Set(CacheKey, Intentos, LockoutTiempo);
        }

        public async Task<CambiarContraseniaResponseDto> CambiarContraseniaAsync(CambiarContraseniaDto Cambio, CancellationToken Ct)
        {
            var Correo = Cambio.CorreoUsuario.Trim().ToLowerInvariant();
            var UsuarioEntidad = await Contexto.usuarios.FirstOrDefaultAsync(U => U.correo_usuario.ToLower() == Correo, Ct);

            if (UsuarioEntidad == null)
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "Usuario no encontrado." };

            if (UsuarioEntidad.es_contrasenia_temporal == true &&
                UsuarioEntidad.fecha_expira_temporal.HasValue &&
                UsuarioEntidad.fecha_expira_temporal.Value < DateTime.UtcNow)
            {
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "La contraseña temporal ha expirado. Solicite una nueva." };
            }

            var Verificacion = Hasher.VerifyHashedPassword(null!, UsuarioEntidad.clave_usuario, Cambio.ContraseniaActual);
            if (Verificacion == PasswordVerificationResult.Failed)
                return new CambiarContraseniaResponseDto { Exito = false, Mensaje = "La contraseña actual es incorrecta." };

            var NuevaHash = Hasher.HashPassword(null!, Cambio.NuevaContrasenia);
            UsuarioEntidad.clave_usuario = NuevaHash;
            UsuarioEntidad.es_contrasenia_temporal = false;
            UsuarioEntidad.fecha_expira_temporal = null;

            await Contexto.SaveChangesAsync(Ct);

            return new CambiarContraseniaResponseDto { Exito = true, Mensaje = "Contraseña actualizada correctamente." };
        }
    }
}
