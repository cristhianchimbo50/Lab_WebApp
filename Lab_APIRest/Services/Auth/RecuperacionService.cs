using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using tokens_usuarios = Lab_APIRest.Infrastructure.EF.Models.tokens_usuarios;
using Lab_APIRest.Services.Email;
using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Lab_APIRest.Services.Auth
{
    public class RecuperacionService : IRecuperacionService
    {
        private readonly LabDbContext _db;
        private readonly IEmailService _emailService;
        private readonly PasswordHasher<object> _hasher = new();

        public RecuperacionService(LabDbContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task<RespuestaMensajeDto> SolicitarRecuperacionAsync(OlvideContraseniaDto dto, CancellationToken ct)
        {
            var correo = (dto.Correo ?? string.Empty).Trim().ToLowerInvariant();

            var usuario = await _db.Usuario
                .Where(u => u.activo == true && u.correo.ToLower() == correo)
                .Select(u => new
                {
                    Usuario = u,
                    u.id_usuario,
                    u.id_persona,
                    Nombres = u.persona_navigation.nombres,
                    Apellidos = u.persona_navigation.apellidos
                })
                .FirstOrDefaultAsync(ct);

            if (usuario == null)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El correo no está registrado o el usuario está inactivo." };

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var tokenHash = CalcularHash(token);

            var registro = new tokens_usuarios
            {
                id_usuario = usuario.id_usuario,
                token_hash = tokenHash,
                fecha_expiracion = DateTime.UtcNow.AddMinutes(15),
                usado = false,
                tipo_token = "recuperacion"
            };

            _db.TokensUsuarios.Add(registro);
            await _db.SaveChangesAsync(ct);

            var link = $"http://laboratorioinmaculada:9111/auth/restablecer?token={Uri.EscapeDataString(token)}"; // TODO: cambiar dominio en producción
            var asunto = "Recuperación de Contraseña - Laboratorio Clínico 'La Inmaculada'";
            var cuerpoHtml = $@"
                <p>Hola <strong>{usuario.Nombres} {usuario.Apellidos}</strong>,</p>

                <p>Recibimos una solicitud para restablecer tu contraseña.</p>

                <p>Puedes hacerlo desde el siguiente enlace (válido por 15 minutos):</p>

                <p>
                    <a href=""{link}"" style=""color:#0d6efd;"">
                        Restablecer contraseña
                    </a>
                </p>

                <p>Si no solicitaste este cambio, puedes ignorar este mensaje.</p>

                <p>
                    Saludos,<br/>
                    <strong>Laboratorio Clínico ""La Inmaculada""</strong>
                </p>";

            var nombreCompleto = $"{usuario.Nombres} {usuario.Apellidos}".Trim();
            await _emailService.EnviarCorreoAsync(usuario.Usuario.correo, nombreCompleto, asunto, cuerpoHtml);

            return new RespuestaMensajeDto { Exito = true, Mensaje = "Se ha enviado un enlace de recuperación a tu correo electrónico." };
        }

        public async Task<RespuestaMensajeDto> RestablecerContraseniaAsync(RestablecerContraseniaDto dto, CancellationToken ct)
        {
            if (dto.NuevaContrasenia != dto.ConfirmarContrasenia)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Las contraseñas no coinciden." };

            var tokenHash = CalcularHash(dto.Token);

            var registro = await _db.TokensUsuarios
                .Include(r => r.usuario_navigation)
                .FirstOrDefaultAsync(r => r.token_hash == tokenHash && !r.usado && r.tipo_token == "recuperacion", ct);

            if (registro == null)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace no es válido." };

            if (registro.fecha_expiracion < DateTime.UtcNow)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace ha expirado. Solicita uno nuevo." };

            var usuario = registro.usuario_navigation;
            usuario.password_hash = _hasher.HashPassword(null!, dto.NuevaContrasenia);

            registro.usado = true;
            registro.usado_en = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            var persona = await _db.Persona
                .Where(p => p.id_persona == usuario.id_persona)
                .Select(p => new { p.nombres, p.apellidos })
                .FirstOrDefaultAsync(ct);

            var nombreCompleto = $"{persona?.nombres} {persona?.apellidos}".Trim();

            var asunto = "Contraseña actualizada - Laboratorio Clínico La Inmaculada";
            var cuerpoHtml = $@"
                <p>Hola <strong>{nombreCompleto}</strong>,</p>

                <p>Tu contraseña fue restablecida correctamente.</p>

                <p>Si no realizaste este cambio, comunícate con soporte de inmediato.</p>

                <p>
                    Saludos,<br/>
                    <strong>Laboratorio Clínico ""La Inmaculada""</strong>
                </p>";

            await _emailService.EnviarCorreoAsync(usuario.correo, nombreCompleto, asunto, cuerpoHtml);

            return new RespuestaMensajeDto { Exito = true, Mensaje = "Contraseña actualizada correctamente." };
        }

        private static byte[] CalcularHash(string token)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        }
    }
}
