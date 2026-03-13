using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.Services;
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
        private readonly EmailService _emailService;
        private readonly PasswordHasher<object> _hasher = new();

        public RecuperacionService(LabDbContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task<RespuestaMensajeDto> SolicitarRecuperacionAsync(OlvideContraseniaDto dto, CancellationToken ct)
        {
            var correo = (dto.Correo ?? string.Empty).Trim().ToLowerInvariant();

            var usuario = await _db.Usuario
                .Include(u => u.IdPersonaNavigation)
                .FirstOrDefaultAsync(u => u.IdPersonaNavigation.Correo.ToLower() == correo && u.Activo == true, ct);
            if (usuario == null)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El correo no está registrado o el usuario está inactivo." };

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
            var tokenHash = CalcularHash(token);

            var registro = new TokensUsuarios
            {
                IdUsuario = usuario.IdUsuario,
                TokenHash = tokenHash,
                FechaExpiracion = DateTime.UtcNow.AddMinutes(15),
                Usado = false,
                TipoToken = "recuperacion"
            };

            _db.TokensUsuarios.Add(registro);
            await _db.SaveChangesAsync(ct);

            var link = $"http://laboratorioinmaculada:9111/auth/restablecer?token={Uri.EscapeDataString(token)}"; // TODO: cambiar dominio en producción
            var asunto = "Recuperación de Contraseña - Laboratorio Clínico 'La Inmaculada'";
            var cuerpoHtml = $@"
                <p>Hola <strong>{usuario.IdPersonaNavigation!.Nombres} {usuario.IdPersonaNavigation!.Apellidos}</strong>,</p>

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

            await _emailService.EnviarCorreoAsync(usuario.IdPersonaNavigation!.Correo, $"{usuario.IdPersonaNavigation!.Nombres} {usuario.IdPersonaNavigation!.Apellidos}", asunto, cuerpoHtml);

            return new RespuestaMensajeDto { Exito = true, Mensaje = "Se ha enviado un enlace de recuperación a tu correo electrónico." };
        }

        public async Task<RespuestaMensajeDto> RestablecerContraseniaAsync(RestablecerContraseniaDto dto, CancellationToken ct)
        {
            if (dto.NuevaContrasenia != dto.ConfirmarContrasenia)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Las contraseñas no coinciden." };

            var tokenHash = CalcularHash(dto.Token);

            var registro = await _db.TokensUsuarios
                .Include(r => r.IdUsuarioNavigation)
                .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && !r.Usado && r.TipoToken == "recuperacion", ct);

            if (registro == null)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace no es válido." };

            if (registro.FechaExpiracion < DateTime.UtcNow)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "El enlace ha expirado. Solicita uno nuevo." };

            var usuario = registro.IdUsuarioNavigation;
            usuario.PasswordHash = _hasher.HashPassword(null!, dto.NuevaContrasenia);

            registro.Usado = true;
            registro.UsadoEn = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            var asunto = "Contraseña actualizada - Laboratorio Clínico La Inmaculada";
            var cuerpoHtml = $@"
                <p>Hola <strong>{usuario.IdPersonaNavigation!.Nombres} {usuario.IdPersonaNavigation!.Apellidos}</strong>,</p>

                <p>Tu contraseña fue restablecida correctamente.</p>

                <p>Si no realizaste este cambio, comunícate con soporte de inmediato.</p>

                <p>
                    Saludos,<br/>
                    <strong>Laboratorio Clínico ""La Inmaculada""</strong>
                </p>";

            await _emailService.EnviarCorreoAsync(usuario.IdPersonaNavigation!.Correo, $"{usuario.IdPersonaNavigation!.Nombres} {usuario.IdPersonaNavigation!.Apellidos}", asunto, cuerpoHtml);

            return new RespuestaMensajeDto { Exito = true, Mensaje = "Contraseña actualizada correctamente." };
        }

        private static byte[] CalcularHash(string token)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        }
    }
}
