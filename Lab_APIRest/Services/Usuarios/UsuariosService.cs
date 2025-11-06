using Lab_Contracts.Usuarios;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Lab_APIRest.Infrastructure.Services;

namespace Lab_APIRest.Services.Usuarios
{
    public class UsuariosService : IUsuariosService
    {
        private readonly LabDbContext _context;
        private readonly EmailService _emailService;
        private readonly PasswordHasher<object> _hasher = new();

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
                    EsContraseniaTemporal = u.es_contrasenia_temporal,
                    FechaCreacion = u.fecha_creacion,
                    UltimoAcceso = u.ultimo_acceso,
                    FechaExpiraTemporal = u.fecha_expira_temporal
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
                EsContraseniaTemporal = entidad.es_contrasenia_temporal,
                FechaCreacion = entidad.fecha_creacion,
                UltimoAcceso = entidad.ultimo_acceso,
                FechaExpiraTemporal = entidad.fecha_expira_temporal
            };
        }

        public async Task<int> GuardarUsuarioAsync(UsuarioCrearDto usuario, CancellationToken ct = default)
        {
            var temporal = GenerarContraseniaTemporal();
            var entidad = new usuario
            {
                nombre = usuario.NombreUsuario,
                correo_usuario = usuario.CorreoUsuario,
                rol = usuario.Rol,
                activo = true,
                es_contrasenia_temporal = true,
                fecha_creacion = DateTime.UtcNow,
                clave_usuario = _hasher.HashPassword(null!, temporal),
                fecha_expira_temporal = DateTime.UtcNow.AddDays(3)
            };
            _context.usuarios.Add(entidad);
            await _context.SaveChangesAsync(ct);

            await _emailService.EnviarCorreoAsync(
                entidad.correo_usuario,
                entidad.nombre,
                "Credenciales de acceso al la aplicación web",
                $"<h2>Bienvenido(a) {entidad.nombre}</h2>\n<p>Tu usuario: <b>{entidad.correo_usuario}</b></p>\n<p>Contraseña temporal: <b>{temporal}</b></p>\n<p>Debes cambiar tu contraseña en el primer ingreso.</p>"
            );

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
            entidad.es_contrasenia_temporal = usuario.EsContraseniaTemporal;
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

        public async Task<UsuarioReenviarDto?> ReenviarCredencialesTemporalesUsuarioAsync(int idUsuario, CancellationToken ct = default)
        {
            var entidad = await _context.usuarios.FirstOrDefaultAsync(u => u.id_usuario == idUsuario && u.rol != "paciente", ct);
            if (entidad == null) return null;
            var nuevaTemporal = GenerarContraseniaTemporal();
            entidad.clave_usuario = _hasher.HashPassword(null!, nuevaTemporal);
            entidad.es_contrasenia_temporal = true;
            entidad.fecha_expira_temporal = DateTime.UtcNow.AddDays(3);
            await _context.SaveChangesAsync(ct);

            await _emailService.EnviarCorreoAsync(
                entidad.correo_usuario,
                entidad.nombre,
                "Nueva contraseña temporal",
                $"<h2>Contraseña temporal generada</h2>\n<p>Tu usuario: <b>{entidad.correo_usuario}</b></p>\n<p>Nueva contraseña temporal: <b>{nuevaTemporal}</b></p>\n<p>Debes cambiarla en el primer acceso.</p>"
            );

            return new UsuarioReenviarDto
            {
                IdUsuario = entidad.id_usuario,
                NuevaTemporal = nuevaTemporal
            };
        }

        private string GenerarContraseniaTemporal() => Guid.NewGuid().ToString("N")[..10];
    }
}
