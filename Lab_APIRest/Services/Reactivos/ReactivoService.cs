using Lab_Contracts.Reactivos;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;
using Lab_Contracts.Common;

namespace Lab_APIRest.Services.Reactivos
{
    public class ReactivoService : IReactivoService
    {
        private readonly LabDbContext _context;

        public ReactivoService(LabDbContext context)
        {
            _context = context;
        }

        private static ReactivoDto MapReactivo(Reactivo entidad) => new()
        {
            IdReactivo = entidad.IdReactivo,
            NombreReactivo = entidad.NombreReactivo,
            Fabricante = entidad.Fabricante ?? string.Empty,
            Unidad = entidad.Unidad ?? string.Empty,
            CantidadDisponible = entidad.CantidadDisponible ?? 0m,
            Anulado = !entidad.Activo
        };

        public async Task<List<ReactivoDto>> ListarReactivosAsync()
        {
            return await _context.Reactivo
                .Where(r => r.Activo)
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
            var entidad = new Reactivo
            {
                NombreReactivo = reactivo.NombreReactivo,
                Fabricante = reactivo.Fabricante,
                Unidad = reactivo.Unidad,
                CantidadDisponible = reactivo.CantidadDisponible,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Reactivo.Add(entidad);
            await _context.SaveChangesAsync();
            return MapReactivo(entidad);
        }

        public async Task<bool> GuardarReactivoAsync(int idReactivo, ReactivoDto reactivo)
        {
            var entidad = await _context.Reactivo.FindAsync(idReactivo);
            if (entidad == null) return false;
            entidad.NombreReactivo = reactivo.NombreReactivo;
            entidad.Fabricante = reactivo.Fabricante;
            entidad.Unidad = reactivo.Unidad;
            entidad.CantidadDisponible = reactivo.CantidadDisponible;
            entidad.Activo = !reactivo.Anulado;
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

        public async Task<bool> AnularReactivoAsync(int idReactivo)
        {
            var entidad = await _context.Reactivo.FindAsync(idReactivo);
            if (entidad == null) return false;
            if (!entidad.Activo) return true;
            entidad.Activo = false;
            entidad.FechaFin = DateTime.UtcNow;
            entidad.FechaActualizacion = DateTime.UtcNow;
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
                    var entidad = await _context.Reactivo.FirstOrDefaultAsync(r => r.IdReactivo == ingreso.IdReactivo);
                    if (entidad == null || !entidad.Activo) continue;

                    var movimiento = new MovimientoReactivo
                    {
                        IdReactivo = ingreso.IdReactivo,
                        TipoMovimiento = "INGRESO",
                        Cantidad = ingreso.Cantidad,
                        FechaMovimiento = ingreso.FechaMovimiento,
                        Observacion = ingreso.Observacion
                    };
                    await _context.MovimientoReactivo.AddAsync(movimiento);

                    entidad.CantidadDisponible = (entidad.CantidadDisponible ?? 0m) + ingreso.Cantidad;
                    entidad.FechaActualizacion = DateTime.UtcNow;
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
                    var entidad = await _context.Reactivo.FirstOrDefaultAsync(r => r.IdReactivo == egreso.IdReactivo);
                    if (entidad == null || !entidad.Activo) continue;

                    var disponible = entidad.CantidadDisponible ?? 0m;
                    if (disponible < egreso.Cantidad)
                        throw new InvalidOperationException($"Stock insuficiente para {entidad.NombreReactivo}");

                    var movimiento = new MovimientoReactivo
                    {
                        IdReactivo = egreso.IdReactivo,
                        TipoMovimiento = "EGRESO",
                        Cantidad = egreso.Cantidad,
                        FechaMovimiento = egreso.FechaMovimiento,
                        Observacion = egreso.Observacion,
                        IdDetalleResultado = egreso.IdDetalleResultado
                    };
                    await _context.MovimientoReactivo.AddAsync(movimiento);

                    entidad.CantidadDisponible = disponible - egreso.Cantidad;
                    entidad.FechaActualizacion = DateTime.UtcNow;
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
                    query = query.Where(r => r.Activo == false);
                else if (!filtro.IncluirAnulados && filtro.IncluirVigentes)
                    query = query.Where(r => r.Activo == true);
            }

            if (!string.IsNullOrWhiteSpace(filtro.CriterioBusqueda) && !string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "nombre":
                        query = query.Where(r => (r.NombreReactivo ?? "").ToLower().Contains(val));
                        break;
                    case "fabricante":
                        query = query.Where(r => (r.Fabricante ?? "").ToLower().Contains(val));
                        break;
                }
            }

            var total = await query.CountAsync();

            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(ReactivoDto.NombreReactivo) => asc ? query.OrderBy(r => r.NombreReactivo) : query.OrderByDescending(r => r.NombreReactivo),
                nameof(ReactivoDto.Fabricante) => asc ? query.OrderBy(r => r.Fabricante) : query.OrderByDescending(r => r.Fabricante),
                nameof(ReactivoDto.Unidad) => asc ? query.OrderBy(r => r.Unidad) : query.OrderByDescending(r => r.Unidad),
                nameof(ReactivoDto.CantidadDisponible) => asc ? query.OrderBy(r => r.CantidadDisponible) : query.OrderByDescending(r => r.CantidadDisponible),
                _ => asc ? query.OrderBy(r => r.IdReactivo) : query.OrderByDescending(r => r.IdReactivo)
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
