using Lab_Contracts.Examenes;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using examen_composicion = Lab_APIRest.Infrastructure.EF.Models.examen_composicion;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lab_APIRest.Services.Examenes
{
    public class ExamenComposicionService : IExamenComposicionService
    {
        private readonly LabDbContext _context;
        private readonly ILogger<ExamenComposicionService> _logger;

        public ExamenComposicionService(LabDbContext context, ILogger<ExamenComposicionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ExamenComposicionDto>> ListarComposicionesPorPadreAsync(int idExamenPadre)
        {
            var composiciones = await _context.ExamenComposicion
                .Where(c => c.id_examen_padre == idExamenPadre && c.activo)
                .Include(c => c.examen_hijo_navigation)
                .Include(c => c.examen_padre_navigation)
                .ToListAsync();

            return composiciones.Select(c => new ExamenComposicionDto
            {
                IdExamenPadre = c.id_examen_padre,
                IdExamenHijo = c.id_examen_hijo,
                NombreExamenPadre = c.examen_padre_navigation?.nombre_examen,
                NombreExamenHijo = c.examen_hijo_navigation?.nombre_examen
            }).ToList();
        }

        public async Task<List<ExamenComposicionDto>> ListarComposicionesPorHijoAsync(int idExamenHijo)
        {
            var composiciones = await _context.ExamenComposicion
                .Where(c => c.id_examen_hijo == idExamenHijo && c.activo)
                .Include(c => c.examen_hijo_navigation)
                .Include(c => c.examen_padre_navigation)
                .ToListAsync();

            return composiciones.Select(c => new ExamenComposicionDto
            {
                IdExamenPadre = c.id_examen_padre,
                IdExamenHijo = c.id_examen_hijo,
                NombreExamenPadre = c.examen_padre_navigation?.nombre_examen,
                NombreExamenHijo = c.examen_hijo_navigation?.nombre_examen
            }).ToList();
        }

        public async Task<bool> GuardarComposicionAsync(ExamenComposicionDto composicionDto)
        {
            try
            {
                var existente = await _context.ExamenComposicion.FirstOrDefaultAsync(c => c.id_examen_padre == composicionDto.IdExamenPadre && c.id_examen_hijo == composicionDto.IdExamenHijo);
                if (existente != null)
                {
                    if (existente.activo) return false;
                    existente.activo = true;
                    existente.fecha_fin = null;
                    existente.fecha_actualizacion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    return true;
                }
                var composicion = new examen_composicion
                {
                    id_examen_padre = composicionDto.IdExamenPadre,
                    id_examen_hijo = composicionDto.IdExamenHijo,
                    fecha_creacion = DateTime.UtcNow,
                    activo = true
                };
                _context.ExamenComposicion.Add(composicion);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la composición de examen. Padre: {Padre}, Hijo: {Hijo}", composicionDto.IdExamenPadre, composicionDto.IdExamenHijo);
                return false;
            }
        }

        public async Task<bool> EliminarComposicionAsync(int idExamenPadre, int idExamenHijo)
        {
            try
            {
                var composicion = await _context.ExamenComposicion.FirstOrDefaultAsync(c => c.id_examen_padre == idExamenPadre && c.id_examen_hijo == idExamenHijo && c.activo);
                if (composicion == null) return false;
                composicion.activo = false;
                composicion.fecha_fin = DateTime.UtcNow;
                composicion.fecha_actualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la composición de examen. Padre: {Padre}, Hijo: {Hijo}", idExamenPadre, idExamenHijo);
                return false;
            }
        }
    }
}
