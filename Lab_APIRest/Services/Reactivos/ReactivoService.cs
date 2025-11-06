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

        public async Task<List<ReactivoDto>> ListarReactivosAsync()
        {
            return await _context.reactivos
                .Where(r => !(r.anulado ?? false))
                .Select(r => new ReactivoDto
                {
                    IdReactivo = r.id_reactivo,
                    NombreReactivo = r.nombre_reactivo,
                    Fabricante = r.fabricante,
                    Unidad = r.unidad,
                    Anulado = r.anulado ?? false,
                    CantidadDisponible = (decimal)r.cantidad_disponible
                })
                .ToListAsync();
        }

        public async Task<ReactivoDto?> ObtenerDetalleReactivoAsync(int idReactivo)
        {
            var entidad = await _context.reactivos.FindAsync(idReactivo);
            if (entidad == null) return null;
            return new ReactivoDto
            {
                IdReactivo = entidad.id_reactivo,
                NombreReactivo = entidad.nombre_reactivo,
                Fabricante = entidad.fabricante,
                Unidad = entidad.unidad,
                Anulado = entidad.anulado ?? false,
                CantidadDisponible = (decimal)entidad.cantidad_disponible
            };
        }

        public async Task<ReactivoDto> GuardarReactivoAsync(ReactivoDto reactivo)
        {
            var entidad = new reactivo
            {
                nombre_reactivo = reactivo.NombreReactivo,
                fabricante = reactivo.Fabricante,
                unidad = reactivo.Unidad,
                anulado = false,
                cantidad_disponible = reactivo.CantidadDisponible
            };
            _context.reactivos.Add(entidad);
            await _context.SaveChangesAsync();
            reactivo.IdReactivo = entidad.id_reactivo;
            return reactivo;
        }

        public async Task<bool> GuardarReactivoAsync(int idReactivo, ReactivoDto reactivo)
        {
            var entidad = await _context.reactivos.FindAsync(idReactivo);
            if (entidad == null) return false;

            entidad.nombre_reactivo = reactivo.NombreReactivo;
            entidad.fabricante = reactivo.Fabricante;
            entidad.unidad = reactivo.Unidad;
            entidad.cantidad_disponible = reactivo.CantidadDisponible;
            entidad.anulado = reactivo.Anulado;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularReactivoAsync(int idReactivo)
        {
            var entidad = await _context.reactivos.FindAsync(idReactivo);
            if (entidad == null) return false;

            entidad.anulado = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegistrarIngresosReactivosAsync(IEnumerable<MovimientoReactivoIngresoDto> ingresos)
        {
            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var ingreso in ingresos)
                {
                    var entidad = await _context.reactivos.FirstOrDefaultAsync(r => r.id_reactivo == ingreso.IdReactivo);
                    if (entidad == null) continue;

                    var movimiento = new movimiento_reactivo
                    {
                        id_reactivo = ingreso.IdReactivo,
                        tipo_movimiento = "INGRESO",
                        cantidad = ingreso.Cantidad,
                        fecha_movimiento = ingreso.FechaMovimiento,
                        observacion = ingreso.Observacion
                    };
                    await _context.movimiento_reactivos.AddAsync(movimiento);

                    entidad.cantidad_disponible += ingreso.Cantidad;
                    _context.reactivos.Update(entidad);
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
            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var egreso in egresos)
                {
                    var entidad = await _context.reactivos.FirstOrDefaultAsync(r => r.id_reactivo == egreso.IdReactivo);
                    if (entidad == null) continue;

                    if (entidad.cantidad_disponible < egreso.Cantidad)
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
                    await _context.movimiento_reactivos.AddAsync(movimiento);
                    entidad.cantidad_disponible -= egreso.Cantidad;
                    _context.reactivos.Update(entidad);
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
            var query = _context.reactivos.AsNoTracking().AsQueryable();

            if (!(filtro.IncluirAnulados && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnulados && !filtro.IncluirVigentes)
                    query = query.Where(r => r.anulado == true);
                else if (!filtro.IncluirAnulados && filtro.IncluirVigentes)
                    query = query.Where(r => r.anulado == false || r.anulado == null);
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
                .Select(r => new ReactivoDto
                {
                    IdReactivo = r.id_reactivo,
                    NombreReactivo = r.nombre_reactivo,
                    Fabricante = r.fabricante,
                    Unidad = r.unidad,
                    Anulado = r.anulado ?? false,
                    CantidadDisponible = (decimal)r.cantidad_disponible
                })
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
