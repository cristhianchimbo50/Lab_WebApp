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

        public async Task<List<UsuarioListadoDto>> ListarUsuariosAsync(UsuarioFiltroDto filtro, CancellationToken ct = default)
        {
            var consulta = _context.usuarios.Where(u => u.rol != "paciente");
            if (!string.IsNullOrWhiteSpace(filtro.Nombre)) consulta = consulta.Where(u => u.nombre.Contains(filtro.Nombre));
            if (!string.IsNullOrWhiteSpace(filtro.Correo)) consulta = consulta.Where(u => u.correo_usuario.Contains(filtro.Correo));
            if (!string.IsNullOrWhiteSpace(filtro.Rol)) consulta = consulta.Where(u => u.rol == filtro.Rol);
            if (filtro.Activo.HasValue) consulta = consulta.Where(u => u.activo == filtro.Activo.Value);

            return await consulta
                .OrderBy(u => u.nombre)
                .Select(u => new UsuarioListadoDto
                {
                    IdUsuario = u.id_usuario,
                    NombreUsuario = u.nombre,
                    CorreoUsuario = u.correo_usuario,
                    Rol = u.rol,
                    Activo = u.activo,
                    FechaCreacion = u.fecha_creacion,
                    UltimoAcceso = u.ultimo_acceso
                })
                .ToListAsync(ct);
        }

        public async Task<UsuarioListadoDto?> ObtenerDetalleUsuarioAsync(int idUsuario, CancellationToken ct = default)
        {
            var entidad = await _context.usuarios.FirstOrDefaultAsync(u => u.id_usuario == idUsuario && u.rol != "paciente", ct);
            if (entidad == null) return null;
            return new UsuarioListadoDto
            {
                IdUsuario = entidad.id_usuario,
                NombreUsuario = entidad.nombre,
                CorreoUsuario = entidad.correo_usuario,
                Rol = entidad.rol,
                Activo = entidad.activo,
                FechaCreacion = entidad.fecha_creacion,
                UltimoAcceso = entidad.ultimo_acceso
            };
        }

        public async Task<int> GuardarUsuarioAsync(UsuarioCrearDto usuario, CancellationToken ct = default)
        {
            var entidad = new usuario
            {
                nombre = usuario.NombreUsuario,
                correo_usuario = usuario.CorreoUsuario,
                rol = usuario.Rol,
                activo = false,
                clave_usuario = null,
                fecha_creacion = DateTime.UtcNow
            };

            _context.usuarios.Add(entidad);
            await _context.SaveChangesAsync(ct);

            var randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(randomBytes);
            var tokenHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));

            var tokenRegistro = new tokens_usuarios
            {
                id_usuario = entidad.id_usuario,
                token_hash = tokenHash,
                tipo_token = "activacion",
                fecha_solicitud = DateTime.UtcNow,
                fecha_expiracion = DateTime.UtcNow.AddHours(24),
                usado = false
            };
            _context.tokens_usuarios.Add(tokenRegistro);
            await _context.SaveChangesAsync(ct);

            var tokenUrl = Uri.EscapeDataString(token);
            var dominio = "https://localhost:7283";//CAMBIAR PRODUCCION
            var enlace = $"{dominio}/activar-cuenta?token={tokenUrl}";

            var asunto = "Activación de cuenta - Laboratorio Clínico La Inmaculada";
            var cuerpo = $@"
        <p>Hola <b>{entidad.nombre}</b>,</p>
        <p>Se ha creado una cuenta para ti en el sistema del Laboratorio Clínico La Inmaculada.</p>
        <p>Para activar tu cuenta y establecer tu contraseña, haz clic en el siguiente enlace:</p>
        <p><a href='{enlace}' target='_blank'>Activar mi cuenta</a></p>
        <p>Este enlace estará disponible durante 24 horas.</p>
        <br/>
        <p>Si no solicitaste este registro, ignora este mensaje.</p>";

            await _emailService.EnviarCorreoAsync(entidad.correo_usuario, entidad.nombre, asunto, cuerpo);

            return entidad.id_usuario;
        }

        public async Task<bool> GuardarUsuarioAsync(UsuarioEditarDto usuario, CancellationToken ct = default)
        {
            var entidad = await _context.usuarios.FirstOrDefaultAsync(x => x.id_usuario == usuario.IdUsuario && x.rol != "paciente", ct);
            if (entidad == null) return false;
            entidad.nombre = usuario.NombreUsuario;
            entidad.correo_usuario = usuario.CorreoUsuario;
            entidad.rol = usuario.Rol;
            entidad.activo = usuario.Activo;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CambiarEstadoUsuarioAsync(int idUsuario, bool activo, string correoUsuarioActual, CancellationToken ct = default)
        {
            var entidad = await _context.usuarios.FirstOrDefaultAsync(u => u.id_usuario == idUsuario && u.rol != "paciente", ct);
            if (entidad == null) return false;
            if (entidad.correo_usuario.Trim().ToLowerInvariant() == correoUsuarioActual.Trim().ToLowerInvariant())
                throw new InvalidOperationException("No puedes deshabilitar tu propio usuario.");
            entidad.activo = activo;
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}
