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
            var composiciones = await _context.Set<examen_composicion>()
                .Where(c => c.id_examen_padre == idExamenPadre)
                .Include(c => c.id_examen_hijoNavigation)
                .Include(c => c.id_examen_padreNavigation)
                .ToListAsync();

            return composiciones.Select(c => new ExamenComposicionDto
            {
                IdExamenPadre = c.id_examen_padre,
                IdExamenHijo = c.id_examen_hijo,
                NombreExamenPadre = c.id_examen_padreNavigation?.nombre_examen,
                NombreExamenHijo = c.id_examen_hijoNavigation?.nombre_examen
            }).ToList();
        }

        public async Task<List<ExamenComposicionDto>> ListarComposicionesPorHijoAsync(int idExamenHijo)
        {
            var composiciones = await _context.Set<examen_composicion>()
                .Where(c => c.id_examen_hijo == idExamenHijo)
                .Include(c => c.id_examen_hijoNavigation)
                .Include(c => c.id_examen_padreNavigation)
                .ToListAsync();

            return composiciones.Select(c => new ExamenComposicionDto
            {
                IdExamenPadre = c.id_examen_padre,
                IdExamenHijo = c.id_examen_hijo,
                NombreExamenPadre = c.id_examen_padreNavigation?.nombre_examen,
                NombreExamenHijo = c.id_examen_hijoNavigation?.nombre_examen
            }).ToList();
        }

        public async Task<bool> GuardarComposicionAsync(ExamenComposicionDto composicionDto)
        {
            var existe = await _context.Set<examen_composicion>()
                .AnyAsync(c => c.id_examen_padre == composicionDto.IdExamenPadre && c.id_examen_hijo == composicionDto.IdExamenHijo);
            if (existe) return false;

            var composicion = new examen_composicion
            {
                id_examen_padre = composicionDto.IdExamenPadre,
                id_examen_hijo = composicionDto.IdExamenHijo
            };

            _context.Set<examen_composicion>().Add(composicion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarComposicionAsync(int idExamenPadre, int idExamenHijo)
        {
            var composicion = await _context.Set<examen_composicion>()
                .FirstOrDefaultAsync(c => c.id_examen_padre == idExamenPadre && c.id_examen_hijo == idExamenHijo);

            if (composicion == null) return false;

            _context.Set<examen_composicion>().Remove(composicion);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
