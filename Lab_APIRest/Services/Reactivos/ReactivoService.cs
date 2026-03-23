using Lab_Contracts.Reactivos;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using reactivo = Lab_APIRest.Infrastructure.EF.Models.reactivo;
using movimiento_reactivo = Lab_APIRest.Infrastructure.EF.Models.movimiento_reactivo;
using Microsoft.EntityFrameworkCore;
using Lab_Contracts.Common;
using System;
using System.Linq;

namespace Lab_APIRest.Services.Reactivos
{
    public class ReactivoService : IReactivoService
    {
        private readonly LabDbContext _context;

        public ReactivoService(LabDbContext context)
        {
            _context = context;
        }

        private static ReactivoDto MapReactivo(reactivo entidad) => new()
        {
            IdReactivo = entidad.id_reactivo,
            NombreReactivo = entidad.nombre_reactivo,
            Fabricante = entidad.fabricante ?? string.Empty,
            Unidad = entidad.unidad ?? string.Empty,
            CantidadDisponible = entidad.cantidad_disponible ?? 0m,
            Anulado = !entidad.activo
        };

        public async Task<List<ReactivoDto>> ListarReactivosAsync()
        {
            return await _context.Reactivo
                .Where(r => r.activo)
                .Select(r => MapReactivo(r))
                .ToListAsync();
        }

        public async Task<ReactivoDto?> ObtenerDetalleReactivoAsync(int idReactivo)
        {
            var entidad = await _context.Reactivo.FindAsync(idReactivo);
            return entidad == null ? null : MapReactivo(entidad);
        }

        public async Task<ReactivoDto> GuardarReactivoAsync(ReactivoDto reactivo)
        {
            var entidad = new reactivo
            {
                nombre_reactivo = reactivo.NombreReactivo,
                fabricante = reactivo.Fabricante,
                unidad = reactivo.Unidad,
                cantidad_disponible = reactivo.CantidadDisponible,
                activo = true,
                fecha_creacion = DateTime.UtcNow
            };
            _context.Reactivo.Add(entidad);
            await _context.SaveChangesAsync();
            return MapReactivo(entidad);
        }

        public async Task<bool> GuardarReactivoAsync(int idReactivo, ReactivoDto reactivo)
        {
            var entidad = await _context.Reactivo.FindAsync(idReactivo);
            if (entidad == null) return false;
            entidad.nombre_reactivo = reactivo.NombreReactivo;
            entidad.fabricante = reactivo.Fabricante;
            entidad.unidad = reactivo.Unidad;
            entidad.cantidad_disponible = reactivo.CantidadDisponible;
            entidad.activo = !reactivo.Anulado;
            entidad.fecha_actualizacion = DateTime.UtcNow;
            if (!entidad.activo)
            {
                entidad.fecha_fin = entidad.fecha_fin ?? DateTime.UtcNow;
            }
            else
            {
                entidad.fecha_fin = null;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularReactivoAsync(int idReactivo)
        {
            var entidad = await _context.Reactivo.FindAsync(idReactivo);
            if (entidad == null) return false;
            if (!entidad.activo) return true;
            entidad.activo = false;
            entidad.fecha_fin = DateTime.UtcNow;
            entidad.fecha_actualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegistrarIngresosReactivosAsync(IEnumerable<MovimientoReactivoIngresoDto> ingresos)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var ingreso in ingresos)
                {
                    var entidad = await _context.Reactivo.FirstOrDefaultAsync(r => r.id_reactivo == ingreso.IdReactivo);
                    if (entidad == null || !entidad.activo) continue;

                    var movimiento = new movimiento_reactivo
                    {
                        id_reactivo = ingreso.IdReactivo,
                        tipo_movimiento = "INGRESO",
                        cantidad = ingreso.Cantidad,
                        fecha_movimiento = ingreso.FechaMovimiento,
                        observacion = ingreso.Observacion
                    };
                    await _context.MovimientoReactivo.AddAsync(movimiento);

                    entidad.cantidad_disponible = (entidad.cantidad_disponible ?? 0m) + ingreso.Cantidad;
                    entidad.fecha_actualizacion = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return true;
            }
            catch
            {
                await transaccion.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> RegistrarEgresosReactivosAsync(IEnumerable<MovimientoReactivoEgresoDto> egresos)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var egreso in egresos)
                {
                    var entidad = await _context.Reactivo.FirstOrDefaultAsync(r => r.id_reactivo == egreso.IdReactivo);
                    if (entidad == null || !entidad.activo) continue;

                    var disponible = entidad.cantidad_disponible ?? 0m;
                    if (disponible < egreso.Cantidad)
                        throw new InvalidOperationException($"Stock insuficiente para {entidad.nombre_reactivo}");

                    var movimiento = new movimiento_reactivo
                    {
                        id_reactivo = egreso.IdReactivo,
                        tipo_movimiento = "EGRESO",
                        cantidad = egreso.Cantidad,
                        fecha_movimiento = egreso.FechaMovimiento,
                        observacion = egreso.Observacion,
                        id_detalle_resultado = egreso.IdDetalleResultado
                    };
                    await _context.MovimientoReactivo.AddAsync(movimiento);

                    entidad.cantidad_disponible = disponible - egreso.Cantidad;
                    entidad.fecha_actualizacion = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return true;
            }
            catch
            {
                await transaccion.RollbackAsync();
                return false;
            }
        }

        public async Task<ResultadoPaginadoDto<ReactivoDto>> ListarReactivosPaginadosAsync(ReactivoFiltroDto filtro)
        {
            var query = _context.Reactivo.AsNoTracking().AsQueryable();

            if (!(filtro.IncluirAnulados && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnulados && !filtro.IncluirVigentes)
                    query = query.Where(r => r.activo == false);
                else if (!filtro.IncluirAnulados && filtro.IncluirVigentes)
                    query = query.Where(r => r.activo == true);
            }

            if (!string.IsNullOrWhiteSpace(filtro.CriterioBusqueda) && !string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "nombre":
                        query = query.Where(r => (r.nombre_reactivo ?? "").ToLower().Contains(val));
                        break;
                    case "fabricante":
                        query = query.Where(r => (r.fabricante ?? "").ToLower().Contains(val));
                        break;
                }
            }

            var total = await query.CountAsync();

            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(ReactivoDto.NombreReactivo) => asc ? query.OrderBy(r => r.nombre_reactivo) : query.OrderByDescending(r => r.nombre_reactivo),
                nameof(ReactivoDto.Fabricante) => asc ? query.OrderBy(r => r.fabricante) : query.OrderByDescending(r => r.fabricante),
                nameof(ReactivoDto.Unidad) => asc ? query.OrderBy(r => r.unidad) : query.OrderByDescending(r => r.unidad),
                nameof(ReactivoDto.CantidadDisponible) => asc ? query.OrderBy(r => r.cantidad_disponible) : query.OrderByDescending(r => r.cantidad_disponible),
                _ => asc ? query.OrderBy(r => r.id_reactivo) : query.OrderByDescending(r => r.id_reactivo)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => MapReactivo(r))
                .ToListAsync();

            return new ResultadoPaginadoDto<ReactivoDto>
            {
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }
    }
}
