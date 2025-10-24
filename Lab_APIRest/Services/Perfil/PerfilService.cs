using Lab_APIRest.Infrastructure.EF;
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

        public async Task<PerfilResponseDto?> ObtenerPerfilAsync(int idUsuario, CancellationToken ct)
        {
            try
            {
                var usuario = await _context.usuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.id_usuario == idUsuario, ct);

                if (usuario == null)
                    return null;

                var perfil = new PerfilDto
                {
                    IdUsuario = usuario.id_usuario,
                    Nombre = usuario.nombre,
                    Correo = usuario.correo_usuario,
                    Rol = usuario.rol,
                    Estado = usuario.estado,
                    UltimoAcceso = usuario.ultimo_acceso,
                    FechaRegistro = usuario.fecha_creacion
                };

                if (usuario.rol.Equals("Paciente", StringComparison.OrdinalIgnoreCase))
                {
                    var paciente = await _context.pacientes
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.id_usuario == idUsuario && p.anulado == false, ct);

                    if (paciente != null)
                    {
                        perfil.Cedula = paciente.cedula_paciente;
                        perfil.FechaNacimiento = paciente.fecha_nac_paciente;
                        perfil.Direccion = paciente.direccion_paciente;
                        perfil.Telefono = paciente.telefono_paciente;
                        perfil.FechaRegistro = paciente.fecha_registro;
                    }
                }

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
