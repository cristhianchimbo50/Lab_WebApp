using Lab_Contracts.Examenes;
using Lab_APIRest.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Examenes
{
    public class ExamenReactivoAsociacionService : IExamenReactivoAsociacionService
    {
        private readonly LabDbContext _context;

        public ExamenReactivoAsociacionService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<List<AsociacionReactivoDto>> ObtenerTodasAsync()
        {
            return await _context.examen_reactivos
                .Include(er => er.id_examenNavigation)
                .Include(er => er.id_reactivoNavigation)
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = er.id_examen_reactivo,
                    IdExamen = (int)er.id_examen,
                    NombreExamen = er.id_examenNavigation.nombre_examen,
                    IdReactivo = (int)er.id_reactivo,
                    NombreReactivo = er.id_reactivoNavigation.nombre_reactivo,
                    CantidadUsada = (decimal)er.cantidad_usada,
                    Unidad = er.unidad
                })
                .ToListAsync();
        }

        public async Task<List<AsociacionReactivoDto>> BuscarPorExamenAsync(string nombreExamen)
        {
            return await _context.examen_reactivos
                .Include(er => er.id_examenNavigation)
                .Include(er => er.id_reactivoNavigation)
                .Where(er => er.id_examenNavigation.nombre_examen.Contains(nombreExamen))
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = er.id_examen_reactivo,
                    IdExamen = (int)er.id_examen,
                    NombreExamen = er.id_examenNavigation.nombre_examen,
                    IdReactivo = (int)er.id_reactivo,
                    NombreReactivo = er.id_reactivoNavigation.nombre_reactivo,
                    CantidadUsada = (decimal)er.cantidad_usada,
                    Unidad = er.unidad
                })
                .ToListAsync();
        }

        public async Task<List<AsociacionReactivoDto>> BuscarPorReactivoAsync(string nombreReactivo)
        {
            return await _context.examen_reactivos
                .Include(er => er.id_examenNavigation)
                .Include(er => er.id_reactivoNavigation)
                .Where(er => er.id_reactivoNavigation.nombre_reactivo.Contains(nombreReactivo))
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = er.id_examen_reactivo,
                    IdExamen = (int)er.id_examen,
                    NombreExamen = er.id_examenNavigation.nombre_examen,
                    IdReactivo = (int)er.id_reactivo,
                    NombreReactivo = er.id_reactivoNavigation.nombre_reactivo,
                    CantidadUsada = (decimal)er.cantidad_usada,
                    Unidad = er.unidad
                })
                .ToListAsync();
        }

        public async Task<AsociacionReactivoDto?> ObtenerPorIdAsync(int idExamenReactivo)
        {
            var er = await _context.examen_reactivos
                .Include(e => e.id_examenNavigation)
                .Include(e => e.id_reactivoNavigation)
                .FirstOrDefaultAsync(e => e.id_examen_reactivo == idExamenReactivo);

            if (er == null) return null;

            return new AsociacionReactivoDto
            {
                IdExamenReactivo = er.id_examen_reactivo,
                IdExamen = (int)er.id_examen,
                NombreExamen = er.id_examenNavigation.nombre_examen,
                IdReactivo = (int)er.id_reactivo,
                NombreReactivo = er.id_reactivoNavigation.nombre_reactivo,
                CantidadUsada = (decimal)er.cantidad_usada,
                Unidad = er.unidad
            };
        }

        public async Task<AsociacionReactivoDto> CrearAsync(AsociacionReactivoDto dto)
        {
            var entity = new Infrastructure.EF.Models.examen_reactivo
            {
                id_examen = dto.IdExamen,
                id_reactivo = dto.IdReactivo,
                cantidad_usada = dto.CantidadUsada,
                unidad = dto.Unidad
            };
            _context.examen_reactivos.Add(entity);
            await _context.SaveChangesAsync();

            dto.IdExamenReactivo = entity.id_examen_reactivo;
            return dto;
        }

        public async Task<bool> EditarAsync(int id, AsociacionReactivoDto dto)
        {
            var entity = await _context.examen_reactivos.FindAsync(id);
            if (entity == null) return false;

            entity.id_examen = dto.IdExamen;
            entity.id_reactivo = dto.IdReactivo;
            entity.cantidad_usada = dto.CantidadUsada;
            entity.unidad = dto.Unidad;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var entity = await _context.examen_reactivos.FindAsync(id);
            if (entity == null) return false;

            _context.examen_reactivos.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AsociacionReactivoDto>> ObtenerPorExamenIdAsync(int idExamen)
        {
            return await _context.examen_reactivos
                .Include(er => er.id_examenNavigation)
                .Include(er => er.id_reactivoNavigation)
                .Where(er => er.id_examen == idExamen)
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = er.id_examen_reactivo,
                    IdExamen = (int)er.id_examen,
                    NombreExamen = er.id_examenNavigation.nombre_examen,
                    IdReactivo = (int)er.id_reactivo,
                    NombreReactivo = er.id_reactivoNavigation.nombre_reactivo,
                    CantidadUsada = (decimal)er.cantidad_usada,
                    Unidad = er.unidad
                })
                .ToListAsync();
        }

        public async Task<bool> GuardarPorExamenAsync(int idExamen, List<AsociacionReactivoDto> asociaciones)
        {
            var actuales = await _context.examen_reactivos
                .Where(er => er.id_examen == idExamen)
                .ToListAsync();

            _context.examen_reactivos.RemoveRange(actuales);

            foreach (var dto in asociaciones)
            {
                var entity = new Infrastructure.EF.Models.examen_reactivo
                {
                    id_examen = idExamen,
                    id_reactivo = dto.IdReactivo,
                    cantidad_usada = dto.CantidadUsada,
                    unidad = dto.Unidad
                };
                _context.examen_reactivos.Add(entity);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
