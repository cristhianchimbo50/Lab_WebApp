using Lab_Contracts.Examenes;
using Lab_Contracts.Common;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using examen = Lab_APIRest.Infrastructure.EF.Models.examen;
using referencia_examen = Lab_APIRest.Infrastructure.EF.Models.referencia_examen;
using examen_composicion = Lab_APIRest.Infrastructure.EF.Models.examen_composicion;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Lab_APIRest.Services.Examenes
{
    public class ExamenService : IExamenService
    {
        private readonly LabDbContext _context;
        private readonly ILogger<ExamenService> _logger;

        public ExamenService(LabDbContext context, ILogger<ExamenService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<ExamenDto>> ListarExamenesAsync()
        {
            return await _context.Examen
                .AsNoTracking()
                .Include(e => e.estudio_navigation)
                .Include(e => e.grupo_examen_navigation)
                .Include(e => e.tipo_muestra_navigation)
                .Include(e => e.tipo_examen_navigation)
                .Include(e => e.tecnica_navigation)
                .Include(e => e.tipo_registro_navigation)
                .Include(e => e.referencia_examen)
                .Select(e => Map(e))
                .ToListAsync();
        }

        public async Task<ExamenDto?> ObtenerDetalleExamenAsync(int idExamen)
        {
            var entidad = await _context.Examen
                .AsNoTracking()
                .Include(e => e.estudio_navigation)
                .Include(e => e.grupo_examen_navigation)
                .Include(e => e.tipo_muestra_navigation)
                .Include(e => e.tipo_examen_navigation)
                .Include(e => e.tecnica_navigation)
                .Include(e => e.tipo_registro_navigation)
                .Include(e => e.referencia_examen)
                .FirstOrDefaultAsync(x => x.id_examen == idExamen);
            return entidad == null ? null : Map(entidad);
        }

        public async Task<List<ExamenDto>> ListarExamenesPorNombreAsync(string nombre)
        {
            return await _context.Examen
                .AsNoTracking()
                .Include(e => e.estudio_navigation)
                .Include(e => e.grupo_examen_navigation)
                .Include(e => e.tipo_muestra_navigation)
                .Include(e => e.tipo_examen_navigation)
                .Include(e => e.tecnica_navigation)
                .Include(e => e.tipo_registro_navigation)
                .Include(e => e.referencia_examen)
                .Where(e => (e.nombre_examen ?? "").Contains(nombre))
                .Select(e => Map(e))
                .ToListAsync();
        }

        public async Task<ExamenDto> GuardarExamenAsync(ExamenDto datosExamen)
        {
            try
            {
                var entidad = new examen
                {
                    nombre_examen = datosExamen.NombreExamen,
                    precio = datosExamen.Precio,
                    activo = true,
                    tiempo_entrega_minutos = datosExamen.TiempoEntregaMinutos,
                    id_estudio = datosExamen.IdEstudio,
                    id_grupo_examen = datosExamen.IdGrupoExamen,
                    id_tipo_muestra = datosExamen.IdTipoMuestra,
                    id_tipo_examen = datosExamen.IdTipoExamen,
                    id_tecnica = datosExamen.IdTecnica,
                    id_tipo_registro = datosExamen.IdTipoRegistro,
                    titulo_examen = datosExamen.TituloExamen,
                    fecha_creacion = DateTime.UtcNow
                };
                _context.Examen.Add(entidad);
                await _context.SaveChangesAsync();

                if (datosExamen.Referencias?.Any() == true)
                {
                    foreach (var r in datosExamen.Referencias)
                    {
                        _context.ReferenciaExamen.Add(new referencia_examen
                        {
                            id_examen = entidad.id_examen,
                            valor_min = r.ValorMin,
                            valor_max = r.ValorMax,
                            valor_texto = r.ValorTexto,
                            unidad = r.Unidad,
                            activo = r.Activo,
                            fecha_creacion = DateTime.UtcNow
                        });
                    }
                    await _context.SaveChangesAsync();
                }
                datosExamen.IdExamen = entidad.id_examen;
                datosExamen.Anulado = false;
                return datosExamen;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar el examen {NombreExamen}", datosExamen.NombreExamen);
                throw;
            }
        }

        public async Task<bool> GuardarExamenAsync(int idExamen, ExamenDto datosExamen)
        {
            try
            {
                var entidad = await _context.Examen
                    .Include(e => e.referencia_examen)
                    .FirstOrDefaultAsync(e => e.id_examen == idExamen);
                if (entidad == null) return false;
                entidad.nombre_examen = datosExamen.NombreExamen;
                entidad.precio = datosExamen.Precio;
                entidad.tiempo_entrega_minutos = datosExamen.TiempoEntregaMinutos;
                entidad.id_estudio = datosExamen.IdEstudio;
                entidad.id_grupo_examen = datosExamen.IdGrupoExamen;
                entidad.id_tipo_muestra = datosExamen.IdTipoMuestra;
                entidad.id_tipo_examen = datosExamen.IdTipoExamen;
                entidad.id_tecnica = datosExamen.IdTecnica;
                entidad.id_tipo_registro = datosExamen.IdTipoRegistro;
                entidad.titulo_examen = datosExamen.TituloExamen;
                entidad.activo = !datosExamen.Anulado;
                entidad.fecha_actualizacion = DateTime.UtcNow;
                if (!entidad.activo)
                {
                    entidad.fecha_fin = entidad.fecha_fin ?? DateTime.UtcNow;
                }
                else
                {
                    entidad.fecha_fin = null;
                }

                if (entidad.referencia_examen.Any())
                {
                    _context.ReferenciaExamen.RemoveRange(entidad.referencia_examen);
                    await _context.SaveChangesAsync();
                }
                if (datosExamen.Referencias?.Any() == true)
                {
                    foreach (var r in datosExamen.Referencias)
                    {
                        _context.ReferenciaExamen.Add(new referencia_examen
                        {
                            id_examen = entidad.id_examen,
                            valor_min = r.ValorMin,
                            valor_max = r.ValorMax,
                            valor_texto = r.ValorTexto,
                            unidad = r.Unidad,
                            activo = r.Activo,
                            fecha_creacion = DateTime.UtcNow
                        });
                    }
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el examen {IdExamen}", idExamen);
                throw;
            }
        }

        public async Task<bool> AnularExamenAsync(int idExamen)
        {
            try
            {
                var entidad = await _context.Examen.FindAsync(idExamen);
                if (entidad == null) return false;
                if (!entidad.activo) return true;
                entidad.activo = false;
                entidad.fecha_fin = DateTime.UtcNow;
                entidad.fecha_actualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al anular el examen {IdExamen}", idExamen);
                throw;
            }
        }

        public async Task<List<ExamenDto>> ListarExamenesHijosAsync(int idExamenPadre)
        {
            var hijos = await _context.ExamenComposicion
                .Where(ec => ec.id_examen_padre == idExamenPadre && ec.activo)
                .Join(_context.Examen
                        .Include(e => e.estudio_navigation)
                        .Include(e => e.grupo_examen_navigation)
                        .Include(e => e.tipo_muestra_navigation)
                        .Include(e => e.tipo_examen_navigation)
                        .Include(e => e.tecnica_navigation)
                        .Include(e => e.tipo_registro_navigation)
                        .Include(e => e.referencia_examen),
                    ec => ec.id_examen_hijo,
                    e => e.id_examen,
                    (ec, e) => e)
                .ToListAsync();
            return hijos.Select(Map).ToList();
        }

        public async Task<bool> AsignarExamenHijoAsync(int idExamenPadre, int idExamenHijo)
        {
            var existe = await _context.ExamenComposicion.AnyAsync(x => x.id_examen_padre == idExamenPadre && x.id_examen_hijo == idExamenHijo);
            if (existe)
            {
                var registro = await _context.ExamenComposicion.FirstAsync(x => x.id_examen_padre == idExamenPadre && x.id_examen_hijo == idExamenHijo);
                if (!registro.activo)
                {
                    registro.activo = true;
                    registro.fecha_fin = null;
                    registro.fecha_actualizacion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return false;
            }
            var composicion = new examen_composicion { id_examen_padre = idExamenPadre, id_examen_hijo = idExamenHijo, activo = true, fecha_creacion = DateTime.UtcNow };
            _context.ExamenComposicion.Add(composicion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarExamenHijoAsync(int idExamenPadre, int idExamenHijo)
        {
            var composicion = await _context.ExamenComposicion.FirstOrDefaultAsync(x => x.id_examen_padre == idExamenPadre && x.id_examen_hijo == idExamenHijo && x.activo);
            if (composicion == null) return false;
            composicion.activo = false;
            composicion.fecha_fin = DateTime.UtcNow;
            composicion.fecha_actualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ResultadoPaginadoDto<ExamenDto>> ListarExamenesPaginadosAsync(ExamenFiltroDto filtro)
        {
            var query = _context.Examen.AsNoTracking()
                .Include(e => e.estudio_navigation)
                .Include(e => e.grupo_examen_navigation)
                .Include(e => e.tipo_muestra_navigation)
                .Include(e => e.tipo_examen_navigation)
                .Include(e => e.tecnica_navigation)
                .Include(e => e.tipo_registro_navigation)
                .Include(e => e.referencia_examen)
                .AsQueryable();

            if (!(filtro.IncluirAnulados && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnulados && !filtro.IncluirVigentes)
                    query = query.Where(e => e.activo == false);
                else if (!filtro.IncluirAnulados && filtro.IncluirVigentes)
                    query = query.Where(e => e.activo == true);
            }

            if (!string.IsNullOrWhiteSpace(filtro.CriterioBusqueda) && !string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "nombre": query = query.Where(e => (e.nombre_examen ?? "").ToLower().Contains(val)); break;
                    case "estudio": query = query.Where(e => (e.estudio_navigation!.nombre ?? "").ToLower().Contains(val)); break;
                    case "tipo": query = query.Where(e => (e.tipo_examen_navigation!.nombre ?? "").ToLower().Contains(val)); break;
                    case "tecnica": query = query.Where(e => (e.tecnica_navigation!.nombre ?? "").ToLower().Contains(val)); break;
                }
            }

            var total = await query.CountAsync();

            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(ExamenDto.NombreExamen) => asc ? query.OrderBy(e => e.nombre_examen) : query.OrderByDescending(e => e.nombre_examen),
                nameof(ExamenDto.Estudio) => asc ? query.OrderBy(e => e.estudio_navigation!.nombre) : query.OrderByDescending(e => e.estudio_navigation!.nombre),
                nameof(ExamenDto.TipoExamen) => asc ? query.OrderBy(e => e.tipo_examen_navigation!.nombre) : query.OrderByDescending(e => e.tipo_examen_navigation!.nombre),
                nameof(ExamenDto.Tecnica) => asc ? query.OrderBy(e => e.tecnica_navigation!.nombre) : query.OrderByDescending(e => e.tecnica_navigation!.nombre),
                 _ => asc ? query.OrderBy(e => e.id_examen) : query.OrderByDescending(e => e.id_examen)
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

        public async Task<List<CatalogoExamenDto>> ListarCatalogoAsync(string tipo, bool incluirInactivos = true)
        {
            return NormalizarTipoCatalogo(tipo) switch
            {
                "estudio" => await _context.Estudio.AsNoTracking()
                    .Where(x => incluirInactivos || x.activo)
                    .OrderBy(x => x.nombre)
                    .Select(x => new CatalogoExamenDto { Id = x.id_estudio, Nombre = x.nombre ?? string.Empty, Activo = x.activo })
                    .ToListAsync(),

                "grupo_examen" => await _context.GrupoExamen.AsNoTracking()
                    .Where(x => incluirInactivos || x.activo)
                    .OrderBy(x => x.nombre)
                    .Select(x => new CatalogoExamenDto { Id = x.id_grupo_examen, Nombre = x.nombre ?? string.Empty, Activo = x.activo })
                    .ToListAsync(),

                "tipo_muestra" => await _context.TipoMuestra.AsNoTracking()
                    .Where(x => incluirInactivos || x.activo)
                    .OrderBy(x => x.nombre)
                    .Select(x => new CatalogoExamenDto { Id = x.id_tipo_muestra, Nombre = x.nombre ?? string.Empty, Activo = x.activo })
                    .ToListAsync(),

                "tipo_examen" => await _context.TipoExamen.AsNoTracking()
                    .Where(x => incluirInactivos || x.activo)
                    .OrderBy(x => x.nombre)
                    .Select(x => new CatalogoExamenDto { Id = x.id_tipo_examen, Nombre = x.nombre ?? string.Empty, Activo = x.activo })
                    .ToListAsync(),

                "tecnica" => await _context.Tecnica.AsNoTracking()
                    .Where(x => incluirInactivos || x.activo)
                    .OrderBy(x => x.nombre)
                    .Select(x => new CatalogoExamenDto { Id = x.id_tecnica, Nombre = x.nombre ?? string.Empty, Activo = x.activo })
                    .ToListAsync(),

                "tipo_registro" => await _context.TipoRegistro.AsNoTracking()
                    .Where(x => incluirInactivos || x.activo)
                    .OrderBy(x => x.nombre)
                    .Select(x => new CatalogoExamenDto { Id = x.id_tipo_registro, Nombre = x.nombre ?? string.Empty, Activo = x.activo })
                    .ToListAsync(),

                _ => new List<CatalogoExamenDto>()
            };
        }

        public async Task<CatalogoExamenDto?> GuardarCatalogoAsync(string tipo, CatalogoExamenDto dto)
        {
            var nombre = (dto.Nombre ?? string.Empty).Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(nombre)) return null;

            var ahora = DateTime.UtcNow;

            switch (NormalizarTipoCatalogo(tipo))
            {
                case "estudio":
                    if (dto.Id == 0)
                    {
                        var nuevo = new estudio { nombre = nombre, activo = true, fecha_creacion = ahora };
                        _context.Estudio.Add(nuevo);
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = nuevo.id_estudio, Nombre = nuevo.nombre ?? string.Empty, Activo = nuevo.activo };
                    }
                    else
                    {
                        var existente = await _context.Estudio.FirstOrDefaultAsync(x => x.id_estudio == dto.Id);
                        if (existente == null) return null;
                        existente.nombre = nombre;
                        existente.fecha_actualizacion = ahora;
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = existente.id_estudio, Nombre = existente.nombre ?? string.Empty, Activo = existente.activo };
                    }

                case "grupo_examen":
                    if (dto.Id == 0)
                    {
                        var nuevo = new grupo_examen { nombre = nombre, activo = true, fecha_creacion = ahora };
                        _context.GrupoExamen.Add(nuevo);
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = nuevo.id_grupo_examen, Nombre = nuevo.nombre ?? string.Empty, Activo = nuevo.activo };
                    }
                    else
                    {
                        var existente = await _context.GrupoExamen.FirstOrDefaultAsync(x => x.id_grupo_examen == dto.Id);
                        if (existente == null) return null;
                        existente.nombre = nombre;
                        existente.fecha_actualizacion = ahora;
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = existente.id_grupo_examen, Nombre = existente.nombre ?? string.Empty, Activo = existente.activo };
                    }

                case "tipo_muestra":
                    if (dto.Id == 0)
                    {
                        var nuevo = new tipo_muestra { nombre = nombre, activo = true, fecha_creacion = ahora };
                        _context.TipoMuestra.Add(nuevo);
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = nuevo.id_tipo_muestra, Nombre = nuevo.nombre ?? string.Empty, Activo = nuevo.activo };
                    }
                    else
                    {
                        var existente = await _context.TipoMuestra.FirstOrDefaultAsync(x => x.id_tipo_muestra == dto.Id);
                        if (existente == null) return null;
                        existente.nombre = nombre;
                        existente.fecha_actualizacion = ahora;
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = existente.id_tipo_muestra, Nombre = existente.nombre ?? string.Empty, Activo = existente.activo };
                    }

                case "tipo_examen":
                    if (dto.Id == 0)
                    {
                        var nuevo = new tipo_examen { nombre = nombre, activo = true, fecha_creacion = ahora };
                        _context.TipoExamen.Add(nuevo);
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = nuevo.id_tipo_examen, Nombre = nuevo.nombre ?? string.Empty, Activo = nuevo.activo };
                    }
                    else
                    {
                        var existente = await _context.TipoExamen.FirstOrDefaultAsync(x => x.id_tipo_examen == dto.Id);
                        if (existente == null) return null;
                        existente.nombre = nombre;
                        existente.fecha_actualizacion = ahora;
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = existente.id_tipo_examen, Nombre = existente.nombre ?? string.Empty, Activo = existente.activo };
                    }

                case "tecnica":
                    if (dto.Id == 0)
                    {
                        var nuevo = new tecnica { nombre = nombre, activo = true, fecha_creacion = ahora };
                        _context.Tecnica.Add(nuevo);
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = nuevo.id_tecnica, Nombre = nuevo.nombre ?? string.Empty, Activo = nuevo.activo };
                    }
                    else
                    {
                        var existente = await _context.Tecnica.FirstOrDefaultAsync(x => x.id_tecnica == dto.Id);
                        if (existente == null) return null;
                        existente.nombre = nombre;
                        existente.fecha_actualizacion = ahora;
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = existente.id_tecnica, Nombre = existente.nombre ?? string.Empty, Activo = existente.activo };
                    }

                case "tipo_registro":
                    if (dto.Id == 0)
                    {
                        var nuevo = new tipo_registro { nombre = nombre, activo = true, fecha_creacion = ahora };
                        _context.TipoRegistro.Add(nuevo);
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = nuevo.id_tipo_registro, Nombre = nuevo.nombre ?? string.Empty, Activo = nuevo.activo };
                    }
                    else
                    {
                        var existente = await _context.TipoRegistro.FirstOrDefaultAsync(x => x.id_tipo_registro == dto.Id);
                        if (existente == null) return null;
                        existente.nombre = nombre;
                        existente.fecha_actualizacion = ahora;
                        await _context.SaveChangesAsync();
                        return new CatalogoExamenDto { Id = existente.id_tipo_registro, Nombre = existente.nombre ?? string.Empty, Activo = existente.activo };
                    }
            }

            return null;
        }

        public async Task<bool> CambiarEstadoCatalogoAsync(string tipo, int id, bool activo)
        {
            var ahora = DateTime.UtcNow;

            switch (NormalizarTipoCatalogo(tipo))
            {
                case "estudio":
                    var estudioEntidad = await _context.Estudio.FirstOrDefaultAsync(x => x.id_estudio == id);
                    if (estudioEntidad == null) return false;
                    estudioEntidad.activo = activo;
                    estudioEntidad.fecha_actualizacion = ahora;
                    estudioEntidad.fecha_fin = activo ? null : estudioEntidad.fecha_fin ?? ahora;
                    await _context.SaveChangesAsync();
                    return true;

                case "grupo_examen":
                    var grupoEntidad = await _context.GrupoExamen.FirstOrDefaultAsync(x => x.id_grupo_examen == id);
                    if (grupoEntidad == null) return false;
                    grupoEntidad.activo = activo;
                    grupoEntidad.fecha_actualizacion = ahora;
                    grupoEntidad.fecha_fin = activo ? null : grupoEntidad.fecha_fin ?? ahora;
                    await _context.SaveChangesAsync();
                    return true;

                case "tipo_muestra":
                    var muestraEntidad = await _context.TipoMuestra.FirstOrDefaultAsync(x => x.id_tipo_muestra == id);
                    if (muestraEntidad == null) return false;
                    muestraEntidad.activo = activo;
                    muestraEntidad.fecha_actualizacion = ahora;
                    muestraEntidad.fecha_fin = activo ? null : muestraEntidad.fecha_fin ?? ahora;
                    await _context.SaveChangesAsync();
                    return true;

                case "tipo_examen":
                    var tipoEntidad = await _context.TipoExamen.FirstOrDefaultAsync(x => x.id_tipo_examen == id);
                    if (tipoEntidad == null) return false;
                    tipoEntidad.activo = activo;
                    tipoEntidad.fecha_actualizacion = ahora;
                    tipoEntidad.fecha_fin = activo ? null : tipoEntidad.fecha_fin ?? ahora;
                    await _context.SaveChangesAsync();
                    return true;

                case "tecnica":
                    var tecnicaEntidad = await _context.Tecnica.FirstOrDefaultAsync(x => x.id_tecnica == id);
                    if (tecnicaEntidad == null) return false;
                    tecnicaEntidad.activo = activo;
                    tecnicaEntidad.fecha_actualizacion = ahora;
                    tecnicaEntidad.fecha_fin = activo ? null : tecnicaEntidad.fecha_fin ?? ahora;
                    await _context.SaveChangesAsync();
                    return true;

                case "tipo_registro":
                    var tipoRegistroEntidad = await _context.TipoRegistro.FirstOrDefaultAsync(x => x.id_tipo_registro == id);
                    if (tipoRegistroEntidad == null) return false;
                    tipoRegistroEntidad.activo = activo;
                    tipoRegistroEntidad.fecha_actualizacion = ahora;
                    tipoRegistroEntidad.fecha_fin = activo ? null : tipoRegistroEntidad.fecha_fin ?? ahora;
                    await _context.SaveChangesAsync();
                    return true;

                default:
                    return false;
            }
        }

        private static string NormalizarTipoCatalogo(string tipo)
        {
            var val = (tipo ?? string.Empty).Trim().ToLowerInvariant();
            return val switch
            {
                "estudio" => "estudio",
                "grupoexamen" or "grupo_examen" or "grupo-examen" => "grupo_examen",
                "tipomuestra" or "tipo_muestra" or "tipo-muestra" => "tipo_muestra",
                "tipoexamen" or "tipo_examen" or "tipo-examen" => "tipo_examen",
                "tecnica" or "técnica" => "tecnica",
                "tiporegistro" or "tipo_registro" or "tipo-registro" => "tipo_registro",
                _ => string.Empty
            };
        }

        private static ExamenDto Map(examen e)
        {
            var dto = new ExamenDto
            {
                IdExamen = e.id_examen,
                NombreExamen = e.nombre_examen ?? string.Empty,
                Precio = e.precio,
                Anulado = !e.activo,
                TituloExamen = e.titulo_examen,
                TiempoEntregaMinutos = e.tiempo_entrega_minutos,
                IdEstudio = e.id_estudio,
                NombreEstudio = e.estudio_navigation?.nombre,
                IdGrupoExamen = e.id_grupo_examen,
                NombreGrupoExamen = e.grupo_examen_navigation?.nombre,
                IdTipoMuestra = e.id_tipo_muestra,
                NombreTipoMuestra = e.tipo_muestra_navigation?.nombre,
                IdTipoExamen = e.id_tipo_examen,
                NombreTipoExamen = e.tipo_examen_navigation?.nombre,
                IdTecnica = e.id_tecnica,
                NombreTecnica = e.tecnica_navigation?.nombre,
                IdTipoRegistro = e.id_tipo_registro,
                NombreTipoRegistro = e.tipo_registro_navigation?.nombre,
                // Compatibilidad: nombres legados
                Estudio = e.estudio_navigation?.nombre,
                TipoMuestra = e.tipo_muestra_navigation?.nombre,
                TiempoEntrega = e.tiempo_entrega_minutos.HasValue ? $"{e.tiempo_entrega_minutos} min" : null,
                TipoExamen = e.tipo_examen_navigation?.nombre,
                Tecnica = e.tecnica_navigation?.nombre
            };

            if (e.referencia_examen != null)
            {
                dto.Referencias = e.referencia_examen.Select(r => new ReferenciaExamenDto
                {
                    IdReferenciaExamen = r.id_referencia_examen,
                    IdExamen = r.id_examen,
                    ValorMin = r.valor_min,
                    ValorMax = r.valor_max,
                    ValorTexto = r.valor_texto,
                    Unidad = r.unidad,
                    Activo = r.activo
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
