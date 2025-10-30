using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.Services;
using Lab_APIRest.Services.Email;
using Lab_Contracts.Auth;
using Lab_Contracts.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Lab_APIRest.Services.Auth
{
    public class RecuperacionService : IRecuperacionService
    {
        private readonly LabDbContext _db;
        private readonly EmailService _emailService;
        private readonly PasswordHasher<object> _hasher = new();

        public RecuperacionService(LabDbContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task<RespuestaMensajeDto> SolicitarRecuperacionAsync(OlvideContraseniaDto dto, CancellationToken ct)
        {
            var correo = dto.Correo.Trim().ToLowerInvariant();

            var usuario = await _db.usuarios.FirstOrDefaultAsync(u => u.correo_usuario.ToLower() == correo && u.activo, ct);
            if (usuario == null)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El correo no está registrado o el usuario está inactivo." };

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var tokenHash = CalcularHash(token);

            var registro = new recuperacion_contrasenias
            {
                id_usuario = usuario.id_usuario,
                token_hash = tokenHash,
                fecha_expiracion = DateTime.UtcNow.AddMinutes(15),
                usado = false
            };

            _db.recuperacion_contrasenias.Add(registro);
            await _db.SaveChangesAsync(ct);

            var link = $"https://localhost:7283/auth/restablecer?token={Uri.EscapeDataString(token)}"; //Debo cambiar para produccion
            var asunto = "Recuperación de Contraseña - Laboratorio Clínico <strong>'La Inmaculada'</strong>";
            var cuerpoHtml = $@"
                <p>Hola <b>{usuario.nombre}</b>,</p>
                <p>Recibimos una solicitud para restablecer tu contraseña.</p>
                <p>Puedes hacerlo desde el siguiente enlace (válido por 15 minutos):</p>
                <p><a href='{link}' style='color:#0d6efd'>Restablecer contraseña</a></p>
                <p>Si no solicitaste este cambio, ignora este mensaje.</p>
                <br/><p>Saludos,<br/><b>Laboratorio Clínico <strong>'La Inmaculada'</strong></b></p>";

            await _emailService.EnviarCorreoAsync(usuario.correo_usuario, usuario.nombre, asunto, cuerpoHtml);

            return new RespuestaMensajeDto { Exito = true, Mensaje = "Se ha enviado un enlace de recuperación a tu correo electrónico." };
        }

        public async Task<RespuestaMensajeDto> RestablecerContraseniaAsync(RestablecerContraseniaDto dto, CancellationToken ct)
        {
            if (dto.NuevaContrasenia != dto.ConfirmarContrasenia)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Las contraseñas no coinciden." };

            var tokenHash = CalcularHash(dto.Token);

            var registro = await _db.recuperacion_contrasenias
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.token_hash == tokenHash && !r.usado, ct);

            if (registro == null)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace no es válido." };

            if (registro.fecha_expiracion < DateTime.UtcNow)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace ha expirado. Solicita uno nuevo." };

            var usuario = registro.Usuario;
            usuario.clave_usuario = _hasher.HashPassword(null!, dto.NuevaContrasenia);
            usuario.es_contraseña_temporal = false;
            usuario.fecha_expira_temporal = null;

            registro.usado = true;
            registro.usado_en = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            var asunto = "Contraseña actualizada - Laboratorio Clínico <strong>'La Inmaculada'</strong>";
            var cuerpoHtml = $@"
                <p>Hola <b>{usuario.nombre}</b>,</p>
                <p>Tu contraseña fue restablecida correctamente.</p>
                <p>Si no realizaste este cambio, comunícate con soporte de inmediato.</p>
                <br/><p>Saludos,<br/><b>Laboratorio Clínico <strong>'La Inmaculada'</strong></b></p>";

            await _emailService.EnviarCorreoAsync(usuario.correo_usuario, usuario.nombre, asunto, cuerpoHtml);

            return new RespuestaMensajeDto { Exito = true, Mensaje = "Contraseña actualizada correctamente." };
        }

        private static byte[] CalcularHash(string token)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        }
    }
}
