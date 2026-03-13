using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_Contracts.Ajustes.Perfil;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lab_APIRest.Services.Perfil
{
    public class PerfilService : IPerfilService
    {
        private readonly LabDbContext _context;
        private readonly ILogger<PerfilService> _logger;

        public PerfilService(LabDbContext context, ILogger<PerfilService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private static PerfilDto MapPerfil(Usuario usuario, Paciente? paciente)
        {
            var perfil = new PerfilDto
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.IdPersonaNavigation != null ? $"{usuario.IdPersonaNavigation.Nombres} {usuario.IdPersonaNavigation.Apellidos}" : string.Empty,
                Correo = usuario.IdPersonaNavigation?.Correo,
                IdRol = usuario.IdRol,
                NombreRol = usuario.IdRolNavigation?.Nombre ?? string.Empty,
                Activo = usuario.Activo == true,
                UltimoAcceso = usuario.UltimoAcceso,
                FechaRegistro = usuario.FechaCreacion
            };

            if (paciente != null)
            {
                perfil.Cedula = paciente.IdPersonaNavigation?.Cedula;
                perfil.FechaNacimiento = paciente.FechaNacPaciente;
                perfil.Direccion = paciente.IdPersonaNavigation?.Direccion;
                perfil.Telefono = paciente.IdPersonaNavigation?.Telefono;
                perfil.FechaRegistro = paciente.FechaCreacion; // prioriza registro paciente
            }
            return perfil;
        }

        public async Task<PerfilResponseDto?> ObtenerDetallePerfilAsync(int idUsuario, CancellationToken ct)
        {
            try
            {
                var usuario = await _context.Usuario
                    .AsNoTracking()
                    .Include(u => u.IdRolNavigation)
                    .Include(u => u.IdPersonaNavigation)
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);

                if (usuario == null)
                    return null;

                Paciente? paciente = null;
                if (usuario.IdRolNavigation.Nombre.Equals("paciente", StringComparison.OrdinalIgnoreCase))
                {
                    paciente = await _context.Paciente
                        .AsNoTracking()
                        .Include(p => p.IdPersonaNavigation)
                        .FirstOrDefaultAsync(p => p.IdPersona == usuario.IdPersona && p.Activo, ct);
                }

                var perfil = MapPerfil(usuario, paciente);
                return new PerfilResponseDto { Perfil = perfil };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener perfil del usuario con ID {IdUsuario}", idUsuario);
                throw;
            }
        }
    }
}
