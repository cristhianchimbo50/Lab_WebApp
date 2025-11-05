using Lab_Contracts.Examenes;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Examenes
{
    public class ExamenComposicionService : IExamenComposicionService
    {
        private readonly LabDbContext Contexto;

        public ExamenComposicionService(LabDbContext contexto)
        {
            Contexto = contexto;
        }

        public async Task<List<ExamenComposicionDto>> ObtenerPorPadre(int IdExamenPadre)
        {
            var Composiciones = await Contexto.Set<examen_composicion>()
                .Where(c => c.id_examen_padre == IdExamenPadre)
                .Include(c => c.id_examen_hijoNavigation)
                .Include(c => c.id_examen_padreNavigation)
                .ToListAsync();

            return Composiciones.Select(c => new ExamenComposicionDto
            {
                IdExamenPadre = c.id_examen_padre,
                IdExamenHijo = c.id_examen_hijo,
                NombreExamenPadre = c.id_examen_padreNavigation?.nombre_examen,
                NombreExamenHijo = c.id_examen_hijoNavigation?.nombre_examen
            }).ToList();
        }

        public async Task<List<ExamenComposicionDto>> ObtenerPorHijo(int IdExamenHijo)
        {
            var Composiciones = await Contexto.Set<examen_composicion>()
                .Where(c => c.id_examen_hijo == IdExamenHijo)
                .Include(c => c.id_examen_hijoNavigation)
                .Include(c => c.id_examen_padreNavigation)
                .ToListAsync();

            return Composiciones.Select(c => new ExamenComposicionDto
            {
                IdExamenPadre = c.id_examen_padre,
                IdExamenHijo = c.id_examen_hijo,
                NombreExamenPadre = c.id_examen_padreNavigation?.nombre_examen,
                NombreExamenHijo = c.id_examen_hijoNavigation?.nombre_examen
            }).ToList();
        }

        public async Task<bool> Crear(ExamenComposicionDto ComposicionDto)
        {
            var Existe = await Contexto.Set<examen_composicion>()
                .AnyAsync(c => c.id_examen_padre == ComposicionDto.IdExamenPadre && c.id_examen_hijo == ComposicionDto.IdExamenHijo);
            if (Existe) return false;

            var Composicion = new examen_composicion
            {
                id_examen_padre = ComposicionDto.IdExamenPadre,
                id_examen_hijo = ComposicionDto.IdExamenHijo
            };

            Contexto.Set<examen_composicion>().Add(Composicion);
            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Eliminar(int IdExamenPadre, int IdExamenHijo)
        {
            var Composicion = await Contexto.Set<examen_composicion>()
                .FirstOrDefaultAsync(c => c.id_examen_padre == IdExamenPadre && c.id_examen_hijo == IdExamenHijo);

            if (Composicion == null) return false;

            Contexto.Set<examen_composicion>().Remove(Composicion);
            await Contexto.SaveChangesAsync();
            return true;
        }
    }
}
