using Lab_Contracts.Examenes;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
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

        private static int BuildIdExamenReactivo(int idExamen, int idReactivo) => HashCode.Combine(idExamen, idReactivo);

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesAsync()
        {
            return await _context.ExamenReactivo
                .Include(er => er.IdExamenNavigation)
                .Include(er => er.IdReactivoNavigation)
                .Where(er => er.Activo && er.IdExamenNavigation.Activo && er.IdReactivoNavigation.Activo)
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = BuildIdExamenReactivo(er.IdExamen, er.IdReactivo),
                    IdExamen = er.IdExamen,
                    NombreExamen = er.IdExamenNavigation.NombreExamen,
                    IdReactivo = er.IdReactivo,
                    NombreReactivo = er.IdReactivoNavigation.NombreReactivo,
                    CantidadUsada = er.CantidadUsada,
                    Unidad = er.IdReactivoNavigation.Unidad
                })
                .ToListAsync();
        }

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenAsync(string nombreExamen)
        {
            return await _context.ExamenReactivo
                .Include(er => er.IdExamenNavigation)
                .Include(er => er.IdReactivoNavigation)
                .Where(er => er.Activo && er.IdExamenNavigation.Activo && er.IdExamenNavigation.NombreExamen != null && er.IdExamenNavigation.NombreExamen.Contains(nombreExamen))
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = BuildIdExamenReactivo(er.IdExamen, er.IdReactivo),
                    IdExamen = er.IdExamen,
                    NombreExamen = er.IdExamenNavigation.NombreExamen,
                    IdReactivo = er.IdReactivo,
                    NombreReactivo = er.IdReactivoNavigation.NombreReactivo,
                    CantidadUsada = er.CantidadUsada,
                    Unidad = er.IdReactivoNavigation.Unidad
                })
                .ToListAsync();
        }

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorReactivoAsync(string nombreReactivo)
        {
            return await _context.ExamenReactivo
                .Include(er => er.IdExamenNavigation)
                .Include(er => er.IdReactivoNavigation)
                .Where(er => er.Activo && er.IdReactivoNavigation.Activo && er.IdReactivoNavigation.NombreReactivo.Contains(nombreReactivo))
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = BuildIdExamenReactivo(er.IdExamen, er.IdReactivo),
                    IdExamen = er.IdExamen,
                    NombreExamen = er.IdExamenNavigation.NombreExamen,
                    IdReactivo = er.IdReactivo,
                    NombreReactivo = er.IdReactivoNavigation.NombreReactivo,
                    CantidadUsada = er.CantidadUsada,
                    Unidad = er.IdReactivoNavigation.Unidad
                })
                .ToListAsync();
        }

        public async Task<AsociacionReactivoDto?> ObtenerDetalleAsociacionAsync(int idExamenReactivo)
        {
            var match = await _context.ExamenReactivo
                .Include(e => e.IdExamenNavigation)
                .Include(e => e.IdReactivoNavigation)
                .Where(e => e.Activo)
                .FirstOrDefaultAsync(e => BuildIdExamenReactivo(e.IdExamen, e.IdReactivo) == idExamenReactivo);
            if (match == null) return null;
            return new AsociacionReactivoDto
            {
                IdExamenReactivo = idExamenReactivo,
                IdExamen = match.IdExamen,
                NombreExamen = match.IdExamenNavigation.NombreExamen,
                IdReactivo = match.IdReactivo,
                NombreReactivo = match.IdReactivoNavigation.NombreReactivo,
                CantidadUsada = match.CantidadUsada,
                Unidad = match.IdReactivoNavigation.Unidad
            };
        }

        public async Task<AsociacionReactivoDto> GuardarAsociacionAsync(AsociacionReactivoDto asociacionDto)
        {
            var existente = await _context.ExamenReactivo.FirstOrDefaultAsync(er => er.IdExamen == asociacionDto.IdExamen && er.IdReactivo == asociacionDto.IdReactivo);
            if (existente != null)
            {
                existente.CantidadUsada = asociacionDto.CantidadUsada;
                existente.FechaActualizacion = DateTime.UtcNow;
                if (!existente.Activo)
                {
                    existente.Activo = true;
                    existente.FechaFin = null;
                }
                await _context.SaveChangesAsync();
                asociacionDto.IdExamenReactivo = BuildIdExamenReactivo(existente.IdExamen, existente.IdReactivo);
                return asociacionDto;
            }
            var entidad = new ExamenReactivo
            {
                IdExamen = asociacionDto.IdExamen,
                IdReactivo = asociacionDto.IdReactivo,
                CantidadUsada = asociacionDto.CantidadUsada,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };
            _context.ExamenReactivo.Add(entidad);
            await _context.SaveChangesAsync();
            asociacionDto.IdExamenReactivo = BuildIdExamenReactivo(entidad.IdExamen, entidad.IdReactivo);
            return asociacionDto;
        }

        public async Task<bool> GuardarAsociacionAsync(int idExamenReactivo, AsociacionReactivoDto asociacionDto)
        {
            var entidad = await _context.ExamenReactivo.FirstOrDefaultAsync(er => BuildIdExamenReactivo(er.IdExamen, er.IdReactivo) == idExamenReactivo);
            if (entidad == null) return false;
            entidad.CantidadUsada = asociacionDto.CantidadUsada;
            entidad.FechaActualizacion = DateTime.UtcNow;
            if (!entidad.Activo)
            {
                entidad.Activo = true;
                entidad.FechaFin = null;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularAsociacionAsync(int idExamenReactivo)
        {
            var entidad = await _context.ExamenReactivo.FirstOrDefaultAsync(er => BuildIdExamenReactivo(er.IdExamen, er.IdReactivo) == idExamenReactivo && er.Activo);
            if (entidad == null) return false;
            entidad.Activo = false;
            entidad.FechaFin = DateTime.UtcNow;
            entidad.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenIdAsync(int idExamen)
        {
            return await _context.ExamenReactivo
                .Include(er => er.IdExamenNavigation)
                .Include(er => er.IdReactivoNavigation)
                .Where(er => er.IdExamen == idExamen && er.Activo)
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = BuildIdExamenReactivo(er.IdExamen, er.IdReactivo),
                    IdExamen = er.IdExamen,
                    NombreExamen = er.IdExamenNavigation.NombreExamen,
                    IdReactivo = er.IdReactivo,
                    NombreReactivo = er.IdReactivoNavigation.NombreReactivo,
                    CantidadUsada = er.CantidadUsada,
                    Unidad = er.IdReactivoNavigation.Unidad
                })
                .ToListAsync();
        }

        public async Task<bool> GuardarAsociacionesPorExamenAsync(int idExamen, List<AsociacionReactivoDto> asociaciones)
        {
            var actuales = await _context.ExamenReactivo.Where(er => er.IdExamen == idExamen).ToListAsync();
            foreach (var act in actuales)
            {
                act.Activo = false;
                act.FechaFin = DateTime.UtcNow;
                act.FechaActualizacion = DateTime.UtcNow;
            }
            foreach (var asociacionDto in asociaciones)
            {
                var existe = actuales.FirstOrDefault(a => a.IdReactivo == asociacionDto.IdReactivo);
                if (existe != null)
                {
                    existe.Activo = true;
                    existe.FechaFin = null;
                    existe.FechaActualizacion = DateTime.UtcNow;
                    existe.CantidadUsada = asociacionDto.CantidadUsada;
                }
                else
                {
                    var entidad = new ExamenReactivo
                    {
                        IdExamen = idExamen,
                        IdReactivo = asociacionDto.IdReactivo,
                        CantidadUsada = asociacionDto.CantidadUsada,
                        FechaCreacion = DateTime.UtcNow,
                        Activo = true
                    };
                    _context.ExamenReactivo.Add(entidad);
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
