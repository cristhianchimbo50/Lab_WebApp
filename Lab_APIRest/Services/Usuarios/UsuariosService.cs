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
        private readonly LabDbContext _db;
        private readonly EmailService _emailService;
        private readonly PasswordHasher<object> _hasher = new();

        public UsuariosService(LabDbContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task<List<UsuarioListadoDto>> GetUsuariosAsync(UsuarioFiltroDto filtro, CancellationToken ct = default)
        {
            var query = _db.usuarios
                .Where(x => x.rol != "paciente");

            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                query = query.Where(x => x.nombre.Contains(filtro.Nombre));
            if (!string.IsNullOrWhiteSpace(filtro.Correo))
                query = query.Where(x => x.correo_usuario.Contains(filtro.Correo));
            if (!string.IsNullOrWhiteSpace(filtro.Rol))
                query = query.Where(x => x.rol == filtro.Rol);
            if (filtro.Activo.HasValue)
                query = query.Where(x => x.activo == filtro.Activo.Value);

            return await query
                .OrderBy(x => x.nombre)
                .Select(x => new UsuarioListadoDto
                {
                    IdUsuario = x.id_usuario,
                    NombreUsuario = x.nombre,
                    CorreoUsuario = x.correo_usuario,
                    Rol = x.rol,
                    Activo = x.activo,
                    EsContraseniaTemporal = x.es_contraseña_temporal,
                    FechaCreacion = x.fecha_creacion,
                    UltimoAcceso = x.ultimo_acceso,
                    FechaExpiraTemporal = x.fecha_expira_temporal
                })
                .ToListAsync(ct);
        }

        public async Task<UsuarioListadoDto?> GetUsuarioPorIdAsync(int idUsuario, CancellationToken ct = default)
        {
            var x = await _db.usuarios.FirstOrDefaultAsync(u => u.id_usuario == idUsuario && u.rol != "paciente", ct);
            if (x == null) return null;
            return new UsuarioListadoDto
            {
                IdUsuario = x.id_usuario,
                NombreUsuario = x.nombre,
                CorreoUsuario = x.correo_usuario,
                Rol = x.rol,
                Activo = x.activo,
                EsContraseniaTemporal = x.es_contraseña_temporal,
                FechaCreacion = x.fecha_creacion,
                UltimoAcceso = x.ultimo_acceso,
                FechaExpiraTemporal = x.fecha_expira_temporal
            };
        }

        public async Task<int> CrearUsuarioAsync(UsuarioCrearDto dto, CancellationToken ct = default)
        {
            var nuevaTemporal = GenerarContraseñaTemporal();
            var usuario = new usuario
            {
                nombre = dto.NombreUsuario,
                correo_usuario = dto.CorreoUsuario,
                rol = dto.Rol,
                activo = true,
                es_contraseña_temporal = true,
                fecha_creacion = DateTime.UtcNow,
                clave_usuario = _hasher.HashPassword(null!, nuevaTemporal),
                fecha_expira_temporal = DateTime.UtcNow.AddDays(3)
            };

            _db.usuarios.Add(usuario);
            await _db.SaveChangesAsync(ct);

            await _emailService.EnviarCorreoAsync(
                usuario.correo_usuario,
                usuario.nombre,
                "Credenciales de acceso al sistema",
                $@"<h2>Bienvenido(a) {usuario.nombre}</h2>
                <p>Tu usuario: <b>{usuario.correo_usuario}</b></p>
                <p>Contraseña temporal: <b>{nuevaTemporal}</b></p>
                <p>Debes cambiar tu contraseña en el primer ingreso.</p>"
            );

            return usuario.id_usuario;
        }

        public async Task<bool> EditarUsuarioAsync(UsuarioEditarDto dto, CancellationToken ct = default)
        {
            var usuario = await _db.usuarios.FirstOrDefaultAsync(x => x.id_usuario == dto.IdUsuario && x.rol != "paciente", ct);
            if (usuario == null) return false;

            usuario.nombre = dto.NombreUsuario;
            usuario.correo_usuario = dto.CorreoUsuario;
            usuario.rol = dto.Rol;
            usuario.activo = dto.Activo;
            usuario.es_contraseña_temporal = dto.EsContraseniaTemporal;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> CambiarEstadoAsync(int idUsuario, bool activo, string correoUsuarioActual, CancellationToken ct = default)
        {
            var usuario = await _db.usuarios.FirstOrDefaultAsync(x => x.id_usuario == idUsuario && x.rol != "paciente", ct);
            if (usuario == null) return false;

            if (usuario.correo_usuario.Trim().ToLowerInvariant() == correoUsuarioActual.Trim().ToLowerInvariant())
                throw new InvalidOperationException("No puedes deshabilitar tu propio usuario.");


            usuario.activo = activo;
            await _db.SaveChangesAsync(ct);
            return true;
        }


        public async Task<UsuarioReenviarDto?> ReenviarCredencialesTemporalesAsync(int idUsuario, CancellationToken ct = default)
        {
            var usuario = await _db.usuarios.FirstOrDefaultAsync(u => u.id_usuario == idUsuario && u.rol != "paciente", ct);
            if (usuario == null) return null;
            var nuevaTemp = GenerarContraseñaTemporal();
            usuario.clave_usuario = _hasher.HashPassword(null!, nuevaTemp);
            usuario.es_contraseña_temporal = true;
            usuario.fecha_expira_temporal = DateTime.UtcNow.AddDays(3);
            await _db.SaveChangesAsync(ct);

            await _emailService.EnviarCorreoAsync(
                usuario.correo_usuario,
                usuario.nombre,
                "Nueva contraseña temporal",
                $@"<h2>Contraseña temporal generada</h2>
                <p>Tu usuario: <b>{usuario.correo_usuario}</b></p>
                <p>Nueva contraseña temporal: <b>{nuevaTemp}</b></p>
                <p>Debes cambiarla en el primer acceso.</p>"
            );

            return new UsuarioReenviarDto
            {
                IdUsuario = usuario.id_usuario,
                NuevaTemporal = nuevaTemp
            };
        }

        private string GenerarContraseñaTemporal()
        {
            return Guid.NewGuid().ToString("N")[..10];
        }
    }
}
