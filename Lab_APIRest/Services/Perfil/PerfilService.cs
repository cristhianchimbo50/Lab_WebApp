using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_Contracts.Ajustes.Perfil;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

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

        private static PerfilDto MapPerfil(usuario usuario, paciente? paciente)
        {
            var perfil = new PerfilDto
            {
                IdUsuario = usuario.id_usuario,
                Nombre = usuario.persona_navigation != null ? $"{usuario.persona_navigation.nombres} {usuario.persona_navigation.apellidos}" : string.Empty,
                Correo = usuario.correo ?? string.Empty,
                IdRol = usuario.id_rol,
                NombreRol = usuario.rol_navigation?.nombre ?? string.Empty,
                Activo = usuario.activo == true,
                UltimoAcceso = usuario.ultimo_acceso,
                FechaRegistro = usuario.fecha_creacion
            };

            if (paciente != null)
            {
                perfil.Cedula = paciente.persona_navigation?.cedula;
                perfil.FechaNacimiento = paciente.persona_navigation?.fecha_nacimiento ?? DateOnly.MinValue;
                perfil.Direccion = paciente.persona_navigation?.direccion;
                perfil.Telefono = paciente.persona_navigation?.telefono;
                perfil.FechaRegistro = paciente.fecha_creacion; // prioriza registro paciente
            }
            return perfil;
        }

        public async Task<PerfilResponseDto?> ObtenerDetallePerfilAsync(int idUsuario, CancellationToken ct)
        {
            try
            {
                var usuario = await _context.Usuario
                    .AsNoTracking()
                    .Include(u => u.rol_navigation)
                    .Include(u => u.persona_navigation)
                    .FirstOrDefaultAsync(u => u.id_usuario == idUsuario, ct);

                if (usuario == null)
                    return null;

                paciente? paciente = null;
                if (usuario.rol_navigation.nombre.Equals("paciente", StringComparison.OrdinalIgnoreCase))
                {
                    paciente = await _context.Paciente
                        .AsNoTracking()
                        .Include(p => p.persona_navigation)
                        .FirstOrDefaultAsync(p => p.id_persona == usuario.id_persona && p.activo, ct);
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
