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
                Nombre = usuario.Nombre,
                Correo = usuario.CorreoUsuario,
                Rol = usuario.Rol,
                Activo = usuario.Activo == true,
                UltimoAcceso = usuario.UltimoAcceso,
                FechaRegistro = usuario.FechaCreacion
            };

            if (paciente != null)
            {
                perfil.Cedula = paciente.CedulaPaciente;
                perfil.FechaNacimiento = paciente.FechaNacPaciente;
                perfil.Direccion = paciente.DireccionPaciente;
                perfil.Telefono = paciente.TelefonoPaciente;
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
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);

                if (usuario == null)
                    return null;

                Paciente? paciente = null;
                if (usuario.Rol.Equals("paciente", StringComparison.OrdinalIgnoreCase))
                {
                    paciente = await _context.Paciente
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.IdUsuario == idUsuario && p.Activo, ct);
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
