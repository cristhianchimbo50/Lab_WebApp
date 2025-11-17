using Lab_Contracts.Examenes;
using Lab_Contracts.Common;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Lab_APIRest.Services.Examenes
{
    public class ExamenService : IExamenService
    {
        private readonly LabDbContext _context;

        public ExamenService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<List<ExamenDto>> ListarExamenesAsync()
        {
            return await _context.Examen.AsNoTracking()
                .Select(e => Map(e))
                .ToListAsync();
        }

        public async Task<ExamenDto?> ObtenerDetalleExamenAsync(int idExamen)
        {
            var entidad = await _context.Examen.AsNoTracking().FirstOrDefaultAsync(x => x.IdExamen == idExamen);
            return entidad == null ? null : Map(entidad);
        }

        public async Task<List<ExamenDto>> ListarExamenesPorNombreAsync(string nombre)
        {
            return await _context.Examen.AsNoTracking()
                .Where(e => (e.NombreExamen ?? "").Contains(nombre))
                .Select(e => Map(e))
                .ToListAsync();
        }

        public async Task<ExamenDto> GuardarExamenAsync(ExamenDto datosExamen)
        {
            var entidad = new Examen
            {
                NombreExamen = datosExamen.NombreExamen,
                ValorReferencia = datosExamen.ValorReferencia,
                Unidad = datosExamen.Unidad,
                Precio = datosExamen.Precio,
                Activo = true,
                Estudio = datosExamen.Estudio,
                TipoMuestra = datosExamen.TipoMuestra,
                TiempoEntrega = datosExamen.TiempoEntrega,
                TipoExamen = datosExamen.TipoExamen,
                Tecnica = datosExamen.Tecnica,
                TituloExamen = datosExamen.TituloExamen,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Examen.Add(entidad);
            await _context.SaveChangesAsync();
            datosExamen.IdExamen = entidad.IdExamen;
            datosExamen.Anulado = false;
            return datosExamen;
        }

        public async Task<bool> GuardarExamenAsync(int idExamen, ExamenDto datosExamen)
        {
            var entidad = await _context.Examen.FindAsync(idExamen);
            if (entidad == null) return false;
            entidad.NombreExamen = datosExamen.NombreExamen;
            entidad.ValorReferencia = datosExamen.ValorReferencia;
            entidad.Unidad = datosExamen.Unidad;
            entidad.Precio = datosExamen.Precio;
            entidad.Estudio = datosExamen.Estudio;
            entidad.TipoMuestra = datosExamen.TipoMuestra;
            entidad.TiempoEntrega = datosExamen.TiempoEntrega;
            entidad.TipoExamen = datosExamen.TipoExamen;
            entidad.Tecnica = datosExamen.Tecnica;
            entidad.TituloExamen = datosExamen.TituloExamen;
            entidad.Activo = !datosExamen.Anulado;
            entidad.FechaActualizacion = DateTime.UtcNow;
            if (!entidad.Activo)
            {
                entidad.FechaFin = entidad.FechaFin ?? DateTime.UtcNow;
            }
            else
            {
                entidad.FechaFin = null;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularExamenAsync(int idExamen)
        {
            var entidad = await _context.Examen.FindAsync(idExamen);
            if (entidad == null) return false;
            if (!entidad.Activo) return true;
            entidad.Activo = false;
            entidad.FechaFin = DateTime.UtcNow;
            entidad.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ExamenDto>> ListarExamenesHijosAsync(int idExamenPadre)
        {
            var hijos = await _context.ExamenComposicion
                .Where(ec => ec.IdExamenPadre == idExamenPadre && ec.Activo)
                .Join(_context.Examen,
                    ec => ec.IdExamenHijo,
                    e => e.IdExamen,
                    (ec, e) => e)
                .ToListAsync();
            return hijos.Select(Map).ToList();
        }

        public async Task<bool> AsignarExamenHijoAsync(int idExamenPadre, int idExamenHijo)
        {
            var existe = await _context.ExamenComposicion.AnyAsync(x => x.IdExamenPadre == idExamenPadre && x.IdExamenHijo == idExamenHijo);
            if (existe)
            {
                var registro = await _context.ExamenComposicion.FirstAsync(x => x.IdExamenPadre == idExamenPadre && x.IdExamenHijo == idExamenHijo);
                if (!registro.Activo)
                {
                    registro.Activo = true;
                    registro.FechaFin = null;
                    registro.FechaActualizacion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return false;
            }
            var composicion = new ExamenComposicion { IdExamenPadre = idExamenPadre, IdExamenHijo = idExamenHijo, Activo = true, FechaCreacion = DateTime.UtcNow };
            _context.ExamenComposicion.Add(composicion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarExamenHijoAsync(int idExamenPadre, int idExamenHijo)
        {
            var composicion = await _context.ExamenComposicion.FirstOrDefaultAsync(x => x.IdExamenPadre == idExamenPadre && x.IdExamenHijo == idExamenHijo && x.Activo);
            if (composicion == null) return false;
            composicion.Activo = false;
            composicion.FechaFin = DateTime.UtcNow;
            composicion.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ResultadoPaginadoDto<ExamenDto>> ListarExamenesPaginadosAsync(ExamenFiltroDto filtro)
        {
            var query = _context.Examen.AsNoTracking().AsQueryable();

            if (!(filtro.IncluirAnulados && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnulados && !filtro.IncluirVigentes)
                    query = query.Where(e => e.Activo == false);
                else if (!filtro.IncluirAnulados && filtro.IncluirVigentes)
                    query = query.Where(e => e.Activo == true);
            }

            if (!string.IsNullOrWhiteSpace(filtro.CriterioBusqueda) && !string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "nombre": query = query.Where(e => (e.NombreExamen ?? "").ToLower().Contains(val)); break;
                    case "estudio": query = query.Where(e => (e.Estudio ?? "").ToLower().Contains(val)); break;
                    case "tipo": query = query.Where(e => (e.TipoExamen ?? "").ToLower().Contains(val)); break;
                    case "tecnica": query = query.Where(e => (e.Tecnica ?? "").ToLower().Contains(val)); break;
                }
            }

            var total = await query.CountAsync();

            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(ExamenDto.NombreExamen) => asc ? query.OrderBy(e => e.NombreExamen) : query.OrderByDescending(e => e.NombreExamen),
                nameof(ExamenDto.Estudio) => asc ? query.OrderBy(e => e.Estudio) : query.OrderByDescending(e => e.Estudio),
                nameof(ExamenDto.TipoExamen) => asc ? query.OrderBy(e => e.TipoExamen) : query.OrderByDescending(e => e.TipoExamen),
                nameof(ExamenDto.Tecnica) => asc ? query.OrderBy(e => e.Tecnica) : query.OrderByDescending(e => e.Tecnica),
                _ => asc ? query.OrderBy(e => e.IdExamen) : query.OrderByDescending(e => e.IdExamen)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(e => new ExamenDto
                {
                    IdExamen = e.IdExamen,
                    NombreExamen = e.NombreExamen ?? string.Empty,
                    ValorReferencia = e.ValorReferencia,
                    Unidad = e.Unidad,
                    Precio = e.Precio,
                    Anulado = !e.Activo,
                    Estudio = e.Estudio,
                    TipoMuestra = e.TipoMuestra,
                    TiempoEntrega = e.TiempoEntrega,
                    TipoExamen = e.TipoExamen,
                    Tecnica = e.Tecnica,
                    TituloExamen = e.TituloExamen
                }).ToListAsync();

            return new ResultadoPaginadoDto<ExamenDto>
            {
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        private static ExamenDto Map(Examen e) => new()
        {
            IdExamen = e.IdExamen,
            NombreExamen = e.NombreExamen ?? string.Empty,
            ValorReferencia = e.ValorReferencia,
            Unidad = e.Unidad,
            Precio = e.Precio,
            Anulado = !e.Activo,
            Estudio = e.Estudio,
            TipoMuestra = e.TipoMuestra,
            TiempoEntrega = e.TiempoEntrega,
            TipoExamen = e.TipoExamen,
            Tecnica = e.Tecnica,
            TituloExamen = e.TituloExamen
        };
    }
}
