using Lab_Contracts.Examenes;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using examen_reactivo = Lab_APIRest.Infrastructure.EF.Models.examen_reactivo;
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
                .Include(er => er.examen_navigation)
                .Include(er => er.reactivo_navigation)
                .Where(er => er.activo && er.examen_navigation.activo && er.reactivo_navigation.activo)
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = BuildIdExamenReactivo(er.id_examen, er.id_reactivo),
                    IdExamen = er.id_examen,
                    NombreExamen = er.examen_navigation.nombre_examen,
                    IdReactivo = er.id_reactivo,
                    NombreReactivo = er.reactivo_navigation.nombre_reactivo,
                    CantidadUsada = er.cantidad_usada,
                    Unidad = er.reactivo_navigation.unidad
                })
                .ToListAsync();
        }

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenAsync(string nombreExamen)
        {
            return await _context.ExamenReactivo
                .Include(er => er.examen_navigation)
                .Include(er => er.reactivo_navigation)
                .Where(er => er.activo && er.examen_navigation.activo && er.examen_navigation.nombre_examen != null && er.examen_navigation.nombre_examen.Contains(nombreExamen))
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = BuildIdExamenReactivo(er.id_examen, er.id_reactivo),
                    IdExamen = er.id_examen,
                    NombreExamen = er.examen_navigation.nombre_examen,
                    IdReactivo = er.id_reactivo,
                    NombreReactivo = er.reactivo_navigation.nombre_reactivo,
                    CantidadUsada = er.cantidad_usada,
                    Unidad = er.reactivo_navigation.unidad
                })
                .ToListAsync();
        }

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorReactivoAsync(string nombreReactivo)
        {
            return await _context.ExamenReactivo
                .Include(er => er.examen_navigation)
                .Include(er => er.reactivo_navigation)
                .Where(er => er.activo && er.reactivo_navigation.activo && er.reactivo_navigation.nombre_reactivo.Contains(nombreReactivo))
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = BuildIdExamenReactivo(er.id_examen, er.id_reactivo),
                    IdExamen = er.id_examen,
                    NombreExamen = er.examen_navigation.nombre_examen,
                    IdReactivo = er.id_reactivo,
                    NombreReactivo = er.reactivo_navigation.nombre_reactivo,
                    CantidadUsada = er.cantidad_usada,
                    Unidad = er.reactivo_navigation.unidad
                })
                .ToListAsync();
        }

        public async Task<AsociacionReactivoDto?> ObtenerDetalleAsociacionAsync(int idExamenReactivo)
        {
            var match = await _context.ExamenReactivo
                .Include(e => e.examen_navigation)
                .Include(e => e.reactivo_navigation)
                .Where(e => e.activo)
                .FirstOrDefaultAsync(e => BuildIdExamenReactivo(e.id_examen, e.id_reactivo) == idExamenReactivo);
            if (match == null) return null;
            return new AsociacionReactivoDto
            {
                IdExamenReactivo = idExamenReactivo,
                IdExamen = match.id_examen,
                NombreExamen = match.examen_navigation.nombre_examen,
                IdReactivo = match.id_reactivo,
                NombreReactivo = match.reactivo_navigation.nombre_reactivo,
                CantidadUsada = match.cantidad_usada,
                Unidad = match.reactivo_navigation.unidad
            };
        }

        public async Task<AsociacionReactivoDto> GuardarAsociacionAsync(AsociacionReactivoDto asociacionDto)
        {
            var existente = await _context.ExamenReactivo.FirstOrDefaultAsync(er => er.id_examen == asociacionDto.IdExamen && er.id_reactivo == asociacionDto.IdReactivo);
            if (existente != null)
            {
                existente.cantidad_usada = asociacionDto.CantidadUsada;
                existente.fecha_actualizacion = DateTime.UtcNow;
                if (!existente.activo)
                {
                    existente.activo = true;
                    existente.fecha_fin = null;
                }
                await _context.SaveChangesAsync();
                asociacionDto.IdExamenReactivo = BuildIdExamenReactivo(existente.id_examen, existente.id_reactivo);
                return asociacionDto;
            }
            var entidad = new examen_reactivo
            {
                id_examen = asociacionDto.IdExamen,
                id_reactivo = asociacionDto.IdReactivo,
                cantidad_usada = asociacionDto.CantidadUsada,
                fecha_creacion = DateTime.UtcNow,
                activo = true
            };
            _context.ExamenReactivo.Add(entidad);
            await _context.SaveChangesAsync();
            asociacionDto.IdExamenReactivo = BuildIdExamenReactivo(entidad.id_examen, entidad.id_reactivo);
            return asociacionDto;
        }

        public async Task<bool> GuardarAsociacionAsync(int idExamenReactivo, AsociacionReactivoDto asociacionDto)
        {
            var entidad = await _context.ExamenReactivo.FirstOrDefaultAsync(er => BuildIdExamenReactivo(er.id_examen, er.id_reactivo) == idExamenReactivo);
            if (entidad == null) return false;
            entidad.cantidad_usada = asociacionDto.CantidadUsada;
            entidad.fecha_actualizacion = DateTime.UtcNow;
            if (!entidad.activo)
            {
                entidad.activo = true;
                entidad.fecha_fin = null;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularAsociacionAsync(int idExamenReactivo)
        {
            var entidad = await _context.ExamenReactivo.FirstOrDefaultAsync(er => BuildIdExamenReactivo(er.id_examen, er.id_reactivo) == idExamenReactivo && er.activo);
            if (entidad == null) return false;
            entidad.activo = false;
            entidad.fecha_fin = DateTime.UtcNow;
            entidad.fecha_actualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenIdAsync(int idExamen)
        {
            return await _context.ExamenReactivo
                .Include(er => er.examen_navigation)
                .Include(er => er.reactivo_navigation)
                .Where(er => er.id_examen == idExamen && er.activo)
                .Select(er => new AsociacionReactivoDto
                {
                    IdExamenReactivo = BuildIdExamenReactivo(er.id_examen, er.id_reactivo),
                    IdExamen = er.id_examen,
                    NombreExamen = er.examen_navigation.nombre_examen,
                    IdReactivo = er.id_reactivo,
                    NombreReactivo = er.reactivo_navigation.nombre_reactivo,
                    CantidadUsada = er.cantidad_usada,
                    Unidad = er.reactivo_navigation.unidad
                })
                .ToListAsync();
        }

        public async Task<bool> GuardarAsociacionesPorExamenAsync(int idExamen, List<AsociacionReactivoDto> asociaciones)
        {
            var actuales = await _context.ExamenReactivo.Where(er => er.id_examen == idExamen).ToListAsync();
            foreach (var act in actuales)
            {
                act.activo = false;
                act.fecha_fin = DateTime.UtcNow;
                act.fecha_actualizacion = DateTime.UtcNow;
            }
            foreach (var asociacionDto in asociaciones)
            {
                var existe = actuales.FirstOrDefault(a => a.id_reactivo == asociacionDto.IdReactivo);
                if (existe != null)
                {
                    existe.activo = true;
                    existe.fecha_fin = null;
                    existe.fecha_actualizacion = DateTime.UtcNow;
                    existe.cantidad_usada = asociacionDto.CantidadUsada;
                }
                else
                {
                    var entidad = new examen_reactivo
                    {
                        id_examen = idExamen,
                        id_reactivo = asociacionDto.IdReactivo,
                        cantidad_usada = asociacionDto.CantidadUsada,
                        fecha_creacion = DateTime.UtcNow,
                        activo = true
                    };
                    _context.ExamenReactivo.Add(entidad);
                }
            }
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
