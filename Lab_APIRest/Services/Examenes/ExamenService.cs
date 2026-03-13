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
            return await _context.Examen
                .AsNoTracking()
                .Include(e => e.IdEstudioNavigation)
                .Include(e => e.IdGrupoExamenNavigation)
                .Include(e => e.IdTipoMuestraNavigation)
                .Include(e => e.IdTipoExamenNavigation)
                .Include(e => e.IdTecnicaNavigation)
                .Include(e => e.IdTipoRegistroNavigation)
                .Include(e => e.ReferenciaExamen)
                .Select(e => Map(e))
                .ToListAsync();
        }

        public async Task<ExamenDto?> ObtenerDetalleExamenAsync(int idExamen)
        {
            var entidad = await _context.Examen
                .AsNoTracking()
                .Include(e => e.IdEstudioNavigation)
                .Include(e => e.IdGrupoExamenNavigation)
                .Include(e => e.IdTipoMuestraNavigation)
                .Include(e => e.IdTipoExamenNavigation)
                .Include(e => e.IdTecnicaNavigation)
                .Include(e => e.IdTipoRegistroNavigation)
                .Include(e => e.ReferenciaExamen)
                .FirstOrDefaultAsync(x => x.IdExamen == idExamen);
            return entidad == null ? null : Map(entidad);
        }

        public async Task<List<ExamenDto>> ListarExamenesPorNombreAsync(string nombre)
        {
            return await _context.Examen
                .AsNoTracking()
                .Include(e => e.IdEstudioNavigation)
                .Include(e => e.IdGrupoExamenNavigation)
                .Include(e => e.IdTipoMuestraNavigation)
                .Include(e => e.IdTipoExamenNavigation)
                .Include(e => e.IdTecnicaNavigation)
                .Include(e => e.IdTipoRegistroNavigation)
                .Include(e => e.ReferenciaExamen)
                .Where(e => (e.NombreExamen ?? "").Contains(nombre))
                .Select(e => Map(e))
                .ToListAsync();
        }

        public async Task<ExamenDto> GuardarExamenAsync(ExamenDto datosExamen)
        {
            var entidad = new Examen
            {
                NombreExamen = datosExamen.NombreExamen,
                Precio = datosExamen.Precio,
                Activo = true,
                TiempoEntregaMinutos = datosExamen.TiempoEntregaMinutos,
                IdEstudio = datosExamen.IdEstudio,
                IdGrupoExamen = datosExamen.IdGrupoExamen,
                IdTipoMuestra = datosExamen.IdTipoMuestra,
                IdTipoExamen = datosExamen.IdTipoExamen,
                IdTecnica = datosExamen.IdTecnica,
                IdTipoRegistro = datosExamen.IdTipoRegistro,
                TituloExamen = datosExamen.TituloExamen,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Examen.Add(entidad);
            await _context.SaveChangesAsync();

            if (datosExamen.Referencias?.Any() == true)
            {
                foreach (var r in datosExamen.Referencias)
                {
                    _context.ReferenciaExamen.Add(new ReferenciaExamen
                    {
                        IdExamen = entidad.IdExamen,
                        ValorMin = r.ValorMin,
                        ValorMax = r.ValorMax,
                        ValorTexto = r.ValorTexto,
                        Unidad = r.Unidad,
                        Activo = r.Activo,
                        FechaCreacion = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }
            datosExamen.IdExamen = entidad.IdExamen;
            datosExamen.Anulado = false;
            return datosExamen;
        }

        public async Task<bool> GuardarExamenAsync(int idExamen, ExamenDto datosExamen)
        {
            var entidad = await _context.Examen
                .Include(e => e.ReferenciaExamen)
                .FirstOrDefaultAsync(e => e.IdExamen == idExamen);
            if (entidad == null) return false;
            entidad.NombreExamen = datosExamen.NombreExamen;
            entidad.Precio = datosExamen.Precio;
            entidad.TiempoEntregaMinutos = datosExamen.TiempoEntregaMinutos;
            entidad.IdEstudio = datosExamen.IdEstudio;
            entidad.IdGrupoExamen = datosExamen.IdGrupoExamen;
            entidad.IdTipoMuestra = datosExamen.IdTipoMuestra;
            entidad.IdTipoExamen = datosExamen.IdTipoExamen;
            entidad.IdTecnica = datosExamen.IdTecnica;
            entidad.IdTipoRegistro = datosExamen.IdTipoRegistro;
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

            if (entidad.ReferenciaExamen.Any())
            {
                _context.ReferenciaExamen.RemoveRange(entidad.ReferenciaExamen);
                await _context.SaveChangesAsync();
            }
            if (datosExamen.Referencias?.Any() == true)
            {
                foreach (var r in datosExamen.Referencias)
                {
                    _context.ReferenciaExamen.Add(new ReferenciaExamen
                    {
                        IdExamen = entidad.IdExamen,
                        ValorMin = r.ValorMin,
                        ValorMax = r.ValorMax,
                        ValorTexto = r.ValorTexto,
                        Unidad = r.Unidad,
                        Activo = r.Activo,
                        FechaCreacion = DateTime.UtcNow
                    });
                }
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
                .Join(_context.Examen
                        .Include(e => e.IdEstudioNavigation)
                        .Include(e => e.IdGrupoExamenNavigation)
                        .Include(e => e.IdTipoMuestraNavigation)
                        .Include(e => e.IdTipoExamenNavigation)
                        .Include(e => e.IdTecnicaNavigation)
                        .Include(e => e.IdTipoRegistroNavigation)
                        .Include(e => e.ReferenciaExamen),
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
            var query = _context.Examen.AsNoTracking()
                .Include(e => e.IdEstudioNavigation)
                .Include(e => e.IdGrupoExamenNavigation)
                .Include(e => e.IdTipoMuestraNavigation)
                .Include(e => e.IdTipoExamenNavigation)
                .Include(e => e.IdTecnicaNavigation)
                .Include(e => e.IdTipoRegistroNavigation)
                .Include(e => e.ReferenciaExamen)
                .AsQueryable();

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
                    case "estudio": query = query.Where(e => (e.IdEstudioNavigation!.Nombre ?? "").ToLower().Contains(val)); break;
                    case "tipo": query = query.Where(e => (e.IdTipoExamenNavigation!.Nombre ?? "").ToLower().Contains(val)); break;
                    case "tecnica": query = query.Where(e => (e.IdTecnicaNavigation!.Nombre ?? "").ToLower().Contains(val)); break;
                }
            }

            var total = await query.CountAsync();

            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(ExamenDto.NombreExamen) => asc ? query.OrderBy(e => e.NombreExamen) : query.OrderByDescending(e => e.NombreExamen),
                nameof(ExamenDto.Estudio) => asc ? query.OrderBy(e => e.IdEstudioNavigation!.Nombre) : query.OrderByDescending(e => e.IdEstudioNavigation!.Nombre),
                nameof(ExamenDto.TipoExamen) => asc ? query.OrderBy(e => e.IdTipoExamenNavigation!.Nombre) : query.OrderByDescending(e => e.IdTipoExamenNavigation!.Nombre),
                nameof(ExamenDto.Tecnica) => asc ? query.OrderBy(e => e.IdTecnicaNavigation!.Nombre) : query.OrderByDescending(e => e.IdTecnicaNavigation!.Nombre),
                _ => asc ? query.OrderBy(e => e.IdExamen) : query.OrderByDescending(e => e.IdExamen)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(e => Map(e))
                .ToListAsync();

            return new ResultadoPaginadoDto<ExamenDto>
            {
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        private static ExamenDto Map(Examen e)
        {
            var dto = new ExamenDto
            {
                IdExamen = e.IdExamen,
                NombreExamen = e.NombreExamen ?? string.Empty,
                Precio = e.Precio,
                Anulado = !e.Activo,
                TituloExamen = e.TituloExamen,
                TiempoEntregaMinutos = e.TiempoEntregaMinutos,
                IdEstudio = e.IdEstudio,
                NombreEstudio = e.IdEstudioNavigation?.Nombre,
                IdGrupoExamen = e.IdGrupoExamen,
                NombreGrupoExamen = e.IdGrupoExamenNavigation?.Nombre,
                IdTipoMuestra = e.IdTipoMuestra,
                NombreTipoMuestra = e.IdTipoMuestraNavigation?.Nombre,
                IdTipoExamen = e.IdTipoExamen,
                NombreTipoExamen = e.IdTipoExamenNavigation?.Nombre,
                IdTecnica = e.IdTecnica,
                NombreTecnica = e.IdTecnicaNavigation?.Nombre,
                IdTipoRegistro = e.IdTipoRegistro,
                NombreTipoRegistro = e.IdTipoRegistroNavigation?.Nombre,
                // Compatibilidad: nombres legados
                Estudio = e.IdEstudioNavigation?.Nombre,
                TipoMuestra = e.IdTipoMuestraNavigation?.Nombre,
                TiempoEntrega = e.TiempoEntregaMinutos.HasValue ? $"{e.TiempoEntregaMinutos} min" : null,
                TipoExamen = e.IdTipoExamenNavigation?.Nombre,
                Tecnica = e.IdTecnicaNavigation?.Nombre
            };

            if (e.ReferenciaExamen != null)
            {
                dto.Referencias = e.ReferenciaExamen.Select(r => new ReferenciaExamenDto
                {
                    IdReferenciaExamen = r.IdReferenciaExamen,
                    IdExamen = r.IdExamen,
                    ValorMin = r.ValorMin,
                    ValorMax = r.ValorMax,
                    ValorTexto = r.ValorTexto,
                    Unidad = r.Unidad,
                    Activo = r.Activo
                }).ToList();

                var primeraRef = dto.Referencias.FirstOrDefault();
                if (primeraRef != null)
                {
                    dto.ValorReferencia = primeraRef.ValorTexto ??
                        (primeraRef.ValorMin.HasValue || primeraRef.ValorMax.HasValue
                            ? $"{primeraRef.ValorMin}-{primeraRef.ValorMax}"
                            : null);
                    dto.Unidad = primeraRef.Unidad;
                }
            }

            return dto;
        }
    }
}
