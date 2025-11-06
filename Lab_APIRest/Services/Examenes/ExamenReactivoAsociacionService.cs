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

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesAsync()
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

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenAsync(string nombreExamen)
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

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorReactivoAsync(string nombreReactivo)
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

        public async Task<AsociacionReactivoDto?> ObtenerDetalleAsociacionAsync(int idExamenReactivo)
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

        public async Task<AsociacionReactivoDto> GuardarAsociacionAsync(AsociacionReactivoDto asociacionDto)
        {
            var entidad = new Infrastructure.EF.Models.examen_reactivo
            {
                id_examen = asociacionDto.IdExamen,
                id_reactivo = asociacionDto.IdReactivo,
                cantidad_usada = asociacionDto.CantidadUsada,
                unidad = asociacionDto.Unidad
            };
            _context.examen_reactivos.Add(entidad);
            await _context.SaveChangesAsync();
            asociacionDto.IdExamenReactivo = entidad.id_examen_reactivo;
            return asociacionDto;
        }

        public async Task<bool> GuardarAsociacionAsync(int idExamenReactivo, AsociacionReactivoDto asociacionDto)
        {
            var entidad = await _context.examen_reactivos.FindAsync(idExamenReactivo);
            if (entidad == null) return false;
            entidad.id_examen = asociacionDto.IdExamen;
            entidad.id_reactivo = asociacionDto.IdReactivo;
            entidad.cantidad_usada = asociacionDto.CantidadUsada;
            entidad.unidad = asociacionDto.Unidad;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularAsociacionAsync(int idExamenReactivo)
        {
            var entidad = await _context.examen_reactivos.FindAsync(idExamenReactivo);
            if (entidad == null) return false;
            _context.examen_reactivos.Remove(entidad);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenIdAsync(int idExamen)
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

        public async Task<bool> GuardarAsociacionesPorExamenAsync(int idExamen, List<AsociacionReactivoDto> asociaciones)
        {
            var actuales = await _context.examen_reactivos.Where(er => er.id_examen == idExamen).ToListAsync();
            _context.examen_reactivos.RemoveRange(actuales);
            foreach (var asociacionDto in asociaciones)
            {
                var entidad = new Infrastructure.EF.Models.examen_reactivo
                {
                    id_examen = idExamen,
                    id_reactivo = asociacionDto.IdReactivo,
                    cantidad_usada = asociacionDto.CantidadUsada,
                    unidad = asociacionDto.Unidad
                };
                _context.examen_reactivos.Add(entidad);
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
