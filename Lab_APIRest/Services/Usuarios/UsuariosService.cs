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
        private readonly LabDbContext Contexto;
        private readonly EmailService EmailService;
        private readonly PasswordHasher<object> Hasher = new();

        public UsuariosService(LabDbContext Contexto, EmailService EmailService)
        {
            this.Contexto = Contexto;
            this.EmailService = EmailService;
        }

        public async Task<List<UsuarioListadoDto>> ListarUsuariosAsync(UsuarioFiltroDto Filtro, CancellationToken Ct = default)
        {
            var Consulta = Contexto.usuarios
                .Where(UsuarioEntidad => UsuarioEntidad.rol != "paciente");

            if (!string.IsNullOrWhiteSpace(Filtro.Nombre))
                Consulta = Consulta.Where(UsuarioEntidad => UsuarioEntidad.nombre.Contains(Filtro.Nombre));
            if (!string.IsNullOrWhiteSpace(Filtro.Correo))
                Consulta = Consulta.Where(UsuarioEntidad => UsuarioEntidad.correo_usuario.Contains(Filtro.Correo));
            if (!string.IsNullOrWhiteSpace(Filtro.Rol))
                Consulta = Consulta.Where(UsuarioEntidad => UsuarioEntidad.rol == Filtro.Rol);
            if (Filtro.Activo.HasValue)
                Consulta = Consulta.Where(UsuarioEntidad => UsuarioEntidad.activo == Filtro.Activo.Value);

            return await Consulta
                .OrderBy(UsuarioEntidad => UsuarioEntidad.nombre)
                .Select(UsuarioEntidad => new UsuarioListadoDto
                {
                    IdUsuario = UsuarioEntidad.id_usuario,
                    NombreUsuario = UsuarioEntidad.nombre,
                    CorreoUsuario = UsuarioEntidad.correo_usuario,
                    Rol = UsuarioEntidad.rol,
                    Activo = UsuarioEntidad.activo,
                    EsContraseniaTemporal = UsuarioEntidad.es_contrasenia_temporal,
                    FechaCreacion = UsuarioEntidad.fecha_creacion,
                    UltimoAcceso = UsuarioEntidad.ultimo_acceso,
                    FechaExpiraTemporal = UsuarioEntidad.fecha_expira_temporal
                })
                .ToListAsync(Ct);
        }

        public async Task<UsuarioListadoDto?> ObtenerUsuarioPorIdAsync(int IdUsuario, CancellationToken Ct = default)
        {
            var UsuarioEntidad = await Contexto.usuarios.FirstOrDefaultAsync(Usuario => Usuario.id_usuario == IdUsuario && Usuario.rol != "paciente", Ct);
            if (UsuarioEntidad == null) return null;
            return new UsuarioListadoDto
            {
                IdUsuario = UsuarioEntidad.id_usuario,
                NombreUsuario = UsuarioEntidad.nombre,
                CorreoUsuario = UsuarioEntidad.correo_usuario,
                Rol = UsuarioEntidad.rol,
                Activo = UsuarioEntidad.activo,
                EsContraseniaTemporal = UsuarioEntidad.es_contrasenia_temporal,
                FechaCreacion = UsuarioEntidad.fecha_creacion,
                UltimoAcceso = UsuarioEntidad.ultimo_acceso,
                FechaExpiraTemporal = UsuarioEntidad.fecha_expira_temporal
            };
        }

        public async Task<int> CrearUsuarioAsync(UsuarioCrearDto Usuario, CancellationToken Ct = default)
        {
            var NuevaTemporal = GenerarContraseniaTemporal();
            var UsuarioEntidad = new usuario
            {
                nombre = Usuario.NombreUsuario,
                correo_usuario = Usuario.CorreoUsuario,
                rol = Usuario.Rol,
                activo = true,
                es_contrasenia_temporal = true,
                fecha_creacion = DateTime.UtcNow,
                clave_usuario = Hasher.HashPassword(null!, NuevaTemporal),
                fecha_expira_temporal = DateTime.UtcNow.AddDays(3)
            };

            Contexto.usuarios.Add(UsuarioEntidad);
            await Contexto.SaveChangesAsync(Ct);

            await EmailService.EnviarCorreoAsync(
                UsuarioEntidad.correo_usuario,
                UsuarioEntidad.nombre,
                "Credenciales de acceso al la aplicación web",
                $@"<h2>Bienvenido(a) {UsuarioEntidad.nombre}</h2>
                <p>Tu usuario: <b>{UsuarioEntidad.correo_usuario}</b></p>
                <p>Contraseña temporal: <b>{NuevaTemporal}</b></p>
                <p>Debes cambiar tu contraseña en el primer ingreso.</p>"
            );

            return UsuarioEntidad.id_usuario;
        }

        public async Task<bool> EditarUsuarioAsync(UsuarioEditarDto Usuario, CancellationToken Ct = default)
        {
            var UsuarioEntidad = await Contexto.usuarios.FirstOrDefaultAsync(X => X.id_usuario == Usuario.IdUsuario && X.rol != "paciente", Ct);
            if (UsuarioEntidad == null) return false;

            UsuarioEntidad.nombre = Usuario.NombreUsuario;
            UsuarioEntidad.correo_usuario = Usuario.CorreoUsuario;
            UsuarioEntidad.rol = Usuario.Rol;
            UsuarioEntidad.activo = Usuario.Activo;
            UsuarioEntidad.es_contrasenia_temporal = Usuario.EsContraseniaTemporal;

            await Contexto.SaveChangesAsync(Ct);
            return true;
        }

        public async Task<bool> CambiarEstadoAsync(int IdUsuario, bool Activo, string CorreoUsuarioActual, CancellationToken Ct = default)
        {
            var UsuarioEntidad = await Contexto.usuarios.FirstOrDefaultAsync(Usuario => Usuario.id_usuario == IdUsuario && Usuario.rol != "paciente", Ct);
            if (UsuarioEntidad == null) return false;

            if (UsuarioEntidad.correo_usuario.Trim().ToLowerInvariant() == CorreoUsuarioActual.Trim().ToLowerInvariant())
                throw new InvalidOperationException("No puedes deshabilitar tu propio usuario.");


            UsuarioEntidad.activo = Activo;
            await Contexto.SaveChangesAsync(Ct);
            return true;
        }

        public async Task<UsuarioReenviarDto?> ReenviarCredencialesTemporalesAsync(int IdUsuario, CancellationToken Ct = default)
        {
            var UsuarioEntidad = await Contexto.usuarios.FirstOrDefaultAsync(Usuario => Usuario.id_usuario == IdUsuario && Usuario.rol != "paciente", Ct);
            if (UsuarioEntidad == null) return null;
            var NuevaTemporal = GenerarContraseniaTemporal();
            UsuarioEntidad.clave_usuario = Hasher.HashPassword(null!, NuevaTemporal);
            UsuarioEntidad.es_contrasenia_temporal = true;
            UsuarioEntidad.fecha_expira_temporal = DateTime.UtcNow.AddDays(3);
            await Contexto.SaveChangesAsync(Ct);

            await EmailService.EnviarCorreoAsync(
                UsuarioEntidad.correo_usuario,
                UsuarioEntidad.nombre,
                "Nueva contraseña temporal",
                $@"<h2>Contraseña temporal generada</h2>
                <p>Tu usuario: <b>{UsuarioEntidad.correo_usuario}</b></p>
                <p>Nueva contraseña temporal: <b>{NuevaTemporal}</b></p>
                <p>Debes cambiarla en el primer acceso.</p>"
            );

            return new UsuarioReenviarDto
            {
                IdUsuario = UsuarioEntidad.id_usuario,
                NuevaTemporal = NuevaTemporal
            };
        }

        private string GenerarContraseniaTemporal()
        {
            return Guid.NewGuid().ToString("N")[..10];
        }
    }
}
