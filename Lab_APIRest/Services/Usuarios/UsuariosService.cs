using Lab_Contracts.Usuarios;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;
using Lab_APIRest.Infrastructure.Services;

namespace Lab_APIRest.Services.Usuarios
{
    public class UsuariosService : IUsuariosService
    {
        private readonly LabDbContext _context;
        private readonly EmailService _emailService;

        public UsuariosService(LabDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private static UsuarioListadoDto MapUsuario(Usuario entidad) => new()
        {
            IdUsuario = entidad.IdUsuario,
            NombreUsuario = entidad.Nombre,
            CorreoUsuario = entidad.CorreoUsuario,
            IdRol = entidad.IdRol,
            NombreRol = entidad.IdRolNavigation?.Nombre ?? string.Empty,
            Activo = entidad.Activo ?? false,
            FechaCreacion = entidad.FechaCreacion,
            UltimoAcceso = entidad.UltimoAcceso
        };

        public async Task<List<UsuarioListadoDto>> ListarUsuariosAsync(UsuarioFiltroDto filtro, CancellationToken ct = default)
        {
            var consulta = _context.Usuario
                .Include(u => u.IdRolNavigation)
                .Where(u => u.IdRolNavigation.Nombre != "paciente");

            if (!string.IsNullOrWhiteSpace(filtro.Nombre)) consulta = consulta.Where(u => u.Nombre.Contains(filtro.Nombre));
            if (!string.IsNullOrWhiteSpace(filtro.Correo)) consulta = consulta.Where(u => u.CorreoUsuario.Contains(filtro.Correo));
            if (filtro.IdRol.HasValue) consulta = consulta.Where(u => u.IdRol == filtro.IdRol.Value);
            if (filtro.Activo.HasValue) consulta = consulta.Where(u => (u.Activo ?? false) == filtro.Activo.Value);

            return await consulta
                .OrderBy(u => u.Nombre)
                .Select(u => MapUsuario(u))
                .ToListAsync(ct);
        }

        public async Task<UsuarioListadoDto?> ObtenerDetalleUsuarioAsync(int idUsuario, CancellationToken ct = default)
        {
            var entidad = await _context.Usuario
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario && u.IdRolNavigation.Nombre != "paciente", ct);
            return entidad == null ? null : MapUsuario(entidad);
        }

        public async Task<int> GuardarUsuarioAsync(UsuarioCrearDto usuario, CancellationToken ct = default)
        {
            var entidad = new Usuario
            {
                Nombre = usuario.NombreUsuario,
                CorreoUsuario = usuario.CorreoUsuario,
                IdRol = usuario.IdRol,
                Activo = false,
                ClaveUsuario = null,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Usuario.Add(entidad);
            await _context.SaveChangesAsync(ct);

            var randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(randomBytes);
            var tokenHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));

            var tokenRegistro = new TokensUsuarios
            {
                IdUsuario = entidad.IdUsuario,
                TokenHash = tokenHash,
                TipoToken = "activacion",
                FechaSolicitud = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddHours(24),
                Usado = false
            };
            _context.TokensUsuarios.Add(tokenRegistro);
            await _context.SaveChangesAsync(ct);

            var tokenUrl = Uri.EscapeDataString(token);
            var dominio = "https://localhost:7283";
            var enlace = $"{dominio}/activar-cuenta?token={tokenUrl}";

            var asunto = "Activación de cuenta - Laboratorio Clínico La Inmaculada";
            var cuerpo = $@"\n        <p>Hola <b>{entidad.Nombre}</b>,</p>\n        <p>Se ha creado una cuenta para ti en el sistema del Laboratorio Clínico La Inmaculada.</p>\n        <p>Para activar tu cuenta y establecer tu contraseña, haz clic en el siguiente enlace:</p>\n        <p><a href='{enlace}' target='_blank'>Activar mi cuenta</a></p>\n        <p>Este enlace estará disponible durante 24 horas.</p>\n        <br/>\n        <p>Si no solicitaste este registro, ignora este mensaje.</p>";

            await _emailService.EnviarCorreoAsync(entidad.CorreoUsuario, entidad.Nombre, asunto, cuerpo);

            return entidad.IdUsuario;
        }

        public async Task<bool> GuardarUsuarioAsync(UsuarioEditarDto usuario, CancellationToken ct = default)
        {
            var entidad = await _context.Usuario.FirstOrDefaultAsync(x => x.IdUsuario == usuario.IdUsuario && x.IdRolNavigation.Nombre != "paciente", ct);
            if (entidad == null) return false;
            entidad.Nombre = usuario.NombreUsuario;
            entidad.CorreoUsuario = usuario.CorreoUsuario;
            entidad.IdRol = usuario.IdRol;
            entidad.Activo = usuario.Activo;
            entidad.FechaActualizacion = DateTime.UtcNow;
            if (entidad.Activo == false)
            {
                entidad.FechaFin = entidad.FechaFin ?? DateTime.UtcNow;
            }
            else
            {
                entidad.FechaFin = null;
            }
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CambiarEstadoUsuarioAsync(int idUsuario, bool activo, string correoUsuarioActual, CancellationToken ct = default)
        {
            var entidad = await _context.Usuario.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario && u.IdRolNavigation.Nombre != "paciente", ct);
            if (entidad == null) return false;
            if (entidad.CorreoUsuario.Trim().ToLowerInvariant() == correoUsuarioActual.Trim().ToLowerInvariant())
                throw new InvalidOperationException("No puedes deshabilitar tu propio usuario.");
            entidad.Activo = activo;
            entidad.FechaActualizacion = DateTime.UtcNow;
            if (entidad.Activo == false)
            {
                entidad.FechaFin = entidad.FechaFin ?? DateTime.UtcNow;
            }
            else
            {
                entidad.FechaFin = null;
            }
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
