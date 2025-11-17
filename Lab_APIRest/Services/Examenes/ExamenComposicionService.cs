using Lab_Contracts.Examenes;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Examenes
{
    public class ExamenComposicionService : IExamenComposicionService
    {
        private readonly LabDbContext _context;

        public ExamenComposicionService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<List<ExamenComposicionDto>> ListarComposicionesPorPadreAsync(int idExamenPadre)
        {
            var composiciones = await _context.ExamenComposicion
                .Where(c => c.IdExamenPadre == idExamenPadre && c.Activo)
                .Include(c => c.IdExamenHijoNavigation)
                .Include(c => c.IdExamenPadreNavigation)
                .ToListAsync();

            return composiciones.Select(c => new ExamenComposicionDto
            {
                IdExamenPadre = c.IdExamenPadre,
                IdExamenHijo = c.IdExamenHijo,
                NombreExamenPadre = c.IdExamenPadreNavigation?.NombreExamen,
                NombreExamenHijo = c.IdExamenHijoNavigation?.NombreExamen
            }).ToList();
        }

        public async Task<List<ExamenComposicionDto>> ListarComposicionesPorHijoAsync(int idExamenHijo)
        {
            var composiciones = await _context.ExamenComposicion
                .Where(c => c.IdExamenHijo == idExamenHijo && c.Activo)
                .Include(c => c.IdExamenHijoNavigation)
                .Include(c => c.IdExamenPadreNavigation)
                .ToListAsync();

            return composiciones.Select(c => new ExamenComposicionDto
            {
                IdExamenPadre = c.IdExamenPadre,
                IdExamenHijo = c.IdExamenHijo,
                NombreExamenPadre = c.IdExamenPadreNavigation?.NombreExamen,
                NombreExamenHijo = c.IdExamenHijoNavigation?.NombreExamen
            }).ToList();
        }

        public async Task<bool> GuardarComposicionAsync(ExamenComposicionDto composicionDto)
        {
            var existente = await _context.ExamenComposicion.FirstOrDefaultAsync(c => c.IdExamenPadre == composicionDto.IdExamenPadre && c.IdExamenHijo == composicionDto.IdExamenHijo);
            if (existente != null)
            {
                if (existente.Activo) return false;
                existente.Activo = true;
                existente.FechaFin = null;
                existente.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            var composicion = new ExamenComposicion
            {
                IdExamenPadre = composicionDto.IdExamenPadre,
                IdExamenHijo = composicionDto.IdExamenHijo,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };
            _context.ExamenComposicion.Add(composicion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarComposicionAsync(int idExamenPadre, int idExamenHijo)
        {
            var composicion = await _context.ExamenComposicion.FirstOrDefaultAsync(c => c.IdExamenPadre == idExamenPadre && c.IdExamenHijo == idExamenHijo && c.Activo);
            if (composicion == null) return false;
            composicion.Activo = false;
            composicion.FechaFin = DateTime.UtcNow;
            composicion.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
