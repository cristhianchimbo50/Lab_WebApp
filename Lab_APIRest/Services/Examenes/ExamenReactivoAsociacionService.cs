using Lab_Contracts.Examenes;
using Lab_APIRest.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Examenes
{
    public class ExamenReactivoAsociacionService : IExamenReactivoAsociacionService
    {
        private readonly LabDbContext Contexto;

        public ExamenReactivoAsociacionService(LabDbContext contexto)
        {
            Contexto = contexto;
        }

        public async Task<List<AsociacionReactivoDto>> ObtenerTodas()
        {
            return await Contexto.examen_reactivos
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

        public async Task<List<AsociacionReactivoDto>> BuscarPorExamen(string NombreExamen)
        {
            return await Contexto.examen_reactivos
                .Include(er => er.id_examenNavigation)
                .Include(er => er.id_reactivoNavigation)
                .Where(er => er.id_examenNavigation.nombre_examen.Contains(NombreExamen))
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

        public async Task<List<AsociacionReactivoDto>> BuscarPorReactivo(string NombreReactivo)
        {
            return await Contexto.examen_reactivos
                .Include(er => er.id_examenNavigation)
                .Include(er => er.id_reactivoNavigation)
                .Where(er => er.id_reactivoNavigation.nombre_reactivo.Contains(NombreReactivo))
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

        public async Task<AsociacionReactivoDto?> ObtenerPorId(int IdExamenReactivo)
        {
            var er = await Contexto.examen_reactivos
                .Include(e => e.id_examenNavigation)
                .Include(e => e.id_reactivoNavigation)
                .FirstOrDefaultAsync(e => e.id_examen_reactivo == IdExamenReactivo);

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

        public async Task<AsociacionReactivoDto> Crear(AsociacionReactivoDto AsociacionDto)
        {
            var entidad = new Infrastructure.EF.Models.examen_reactivo
            {
                id_examen = AsociacionDto.IdExamen,
                id_reactivo = AsociacionDto.IdReactivo,
                cantidad_usada = AsociacionDto.CantidadUsada,
                unidad = AsociacionDto.Unidad
            };
            Contexto.examen_reactivos.Add(entidad);
            await Contexto.SaveChangesAsync();

            AsociacionDto.IdExamenReactivo = entidad.id_examen_reactivo;
            return AsociacionDto;
        }

        public async Task<bool> Editar(int IdExamenReactivo, AsociacionReactivoDto AsociacionDto)
        {
            var entidad = await Contexto.examen_reactivos.FindAsync(IdExamenReactivo);
            if (entidad == null) return false;

            entidad.id_examen = AsociacionDto.IdExamen;
            entidad.id_reactivo = AsociacionDto.IdReactivo;
            entidad.cantidad_usada = AsociacionDto.CantidadUsada;
            entidad.unidad = AsociacionDto.Unidad;

            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Eliminar(int IdExamenReactivo)
        {
            var entidad = await Contexto.examen_reactivos.FindAsync(IdExamenReactivo);
            if (entidad == null) return false;

            Contexto.examen_reactivos.Remove(entidad);
            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<List<AsociacionReactivoDto>> ObtenerPorExamenId(int IdExamen)
        {
            return await Contexto.examen_reactivos
                .Include(er => er.id_examenNavigation)
                .Include(er => er.id_reactivoNavigation)
                .Where(er => er.id_examen == IdExamen)
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

        public async Task<bool> GuardarPorExamen(int IdExamen, List<AsociacionReactivoDto> Asociaciones)
        {
            var actuales = await Contexto.examen_reactivos
                .Where(er => er.id_examen == IdExamen)
                .ToListAsync();

            Contexto.examen_reactivos.RemoveRange(actuales);

            foreach (var AsociacionDto in Asociaciones)
            {
                var entidad = new Infrastructure.EF.Models.examen_reactivo
                {
                    id_examen = IdExamen,
                    id_reactivo = AsociacionDto.IdReactivo,
                    cantidad_usada = AsociacionDto.CantidadUsada,
                    unidad = AsociacionDto.Unidad
                };
                Contexto.examen_reactivos.Add(entidad);
            }

            await Contexto.SaveChangesAsync();
            return true;
        }
    }
}
