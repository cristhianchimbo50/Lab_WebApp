using Lab_APIRest.Infrastructure.EF;
using Lab_Contracts.Reactivos;
using Microsoft.EntityFrameworkCore;
using Lab_Contracts.Common;

namespace Lab_APIRest.Services.Reactivos
{
    public class MovimientoService : IMovimientoService
    {
        private readonly LabDbContext _context;

        public MovimientoService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<List<MovimientoReactivoDto>> ListarMovimientosAsync(MovimientoReactivoFiltroDto filtro)
        {
            var consulta = _context.movimiento_reactivos
                .Include(m => m.id_reactivoNavigation)
                .Include(m => m.id_detalle_resultadoNavigation)
                    .ThenInclude(dr => dr.id_resultadoNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.IdMovimientoReactivo))
            {
                if (int.TryParse(filtro.IdMovimientoReactivo, out int numeroMovimiento))
                    consulta = consulta.Where(m => m.id_movimiento_reactivo == numeroMovimiento);
            }

            if (!string.IsNullOrWhiteSpace(filtro.NombreReactivo))
            {
                consulta = consulta.Where(m => m.id_reactivoNavigation.nombre_reactivo
                    .Contains(filtro.NombreReactivo));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Observacion))
            {
                consulta = consulta.Where(m => m.observacion.Contains(filtro.Observacion));
            }

            if (filtro.FechaDesde.HasValue)
            {
                consulta = consulta.Where(m => m.fecha_movimiento >= filtro.FechaDesde.Value);
            }

            if (filtro.FechaHasta.HasValue)
            {
                consulta = consulta.Where(m => m.fecha_movimiento <= filtro.FechaHasta.Value);
            }

            if (!(filtro.IncluirIngresos && filtro.IncluirEgresos))
            {
                if (filtro.IncluirIngresos)
                    consulta = consulta.Where(m => m.tipo_movimiento == "INGRESO");
                else if (filtro.IncluirEgresos)
                    consulta = consulta.Where(m => m.tipo_movimiento == "EGRESO");
                else
                    consulta = consulta.Where(_ => false);
            }

            var movimientos = await consulta
                .OrderByDescending(m => m.fecha_movimiento)
                .Select(m => new MovimientoReactivoDto
                {
                    IdMovimientoReactivo = m.id_movimiento_reactivo,
                    IdReactivo = (int)m.id_reactivo,
                    NombreReactivo = m.id_reactivoNavigation.nombre_reactivo,
                    TipoMovimiento = m.tipo_movimiento,
                    Cantidad = (decimal)m.cantidad,
                    FechaMovimiento = m.fecha_movimiento,
                    Observacion = m.observacion,
                    IdDetalleResultado = m.id_detalle_resultado,
                    NumeroResultado = m.id_detalle_resultadoNavigation != null
                        ? m.id_detalle_resultadoNavigation.id_resultadoNavigation.numero_resultado
                        : null
                })
                .ToListAsync();

            return movimientos;
        }

        public async Task<ResultadoPaginadoDto<MovimientoReactivoDto>> ListarMovimientosPaginadosAsync(MovimientoReactivoFiltroDto filtro)
        {
            var consulta = _context.movimiento_reactivos
                .Include(m => m.id_reactivoNavigation)
                .Include(m => m.id_detalle_resultadoNavigation)!.ThenInclude(d => d.id_resultadoNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.IdMovimientoReactivo))
            {
                if (int.TryParse(filtro.IdMovimientoReactivo, out int numeroMovimiento))
                    consulta = consulta.Where(m => m.id_movimiento_reactivo == numeroMovimiento);
            }
            if (!string.IsNullOrWhiteSpace(filtro.NombreReactivo))
                consulta = consulta.Where(m => m.id_reactivoNavigation.nombre_reactivo.Contains(filtro.NombreReactivo));
            if (!string.IsNullOrWhiteSpace(filtro.Observacion))
                consulta = consulta.Where(m => m.observacion.Contains(filtro.Observacion));
            if (filtro.FechaDesde.HasValue)
                consulta = consulta.Where(m => m.fecha_movimiento >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue)
                consulta = consulta.Where(m => m.fecha_movimiento <= filtro.FechaHasta.Value);
            if (!(filtro.IncluirIngresos && filtro.IncluirEgresos))
            {
                if (filtro.IncluirIngresos) consulta = consulta.Where(m => m.tipo_movimiento == "INGRESO");
                else if (filtro.IncluirEgresos) consulta = consulta.Where(m => m.tipo_movimiento == "EGRESO");
                else consulta = consulta.Where(_ => false);
            }

            var total = await consulta.CountAsync();

            bool asc = filtro.SortAsc;
            consulta = filtro.SortBy switch
            {
                nameof(MovimientoReactivoDto.IdMovimientoReactivo) => asc ? consulta.OrderBy(m => m.id_movimiento_reactivo) : consulta.OrderByDescending(m => m.id_movimiento_reactivo),
                nameof(MovimientoReactivoDto.NombreReactivo) => asc ? consulta.OrderBy(m => m.id_reactivoNavigation.nombre_reactivo) : consulta.OrderByDescending(m => m.id_reactivoNavigation.nombre_reactivo),
                nameof(MovimientoReactivoDto.TipoMovimiento) => asc ? consulta.OrderBy(m => m.tipo_movimiento) : consulta.OrderByDescending(m => m.tipo_movimiento),
                nameof(MovimientoReactivoDto.Cantidad) => asc ? consulta.OrderBy(m => m.cantidad) : consulta.OrderByDescending(m => m.cantidad),
                _ => asc ? consulta.OrderBy(m => m.fecha_movimiento) : consulta.OrderByDescending(m => m.fecha_movimiento)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await consulta.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MovimientoReactivoDto
                {
                    IdMovimientoReactivo = m.id_movimiento_reactivo,
                    IdReactivo = (int)m.id_reactivo,
                    NombreReactivo = m.id_reactivoNavigation.nombre_reactivo,
                    TipoMovimiento = m.tipo_movimiento,
                    Cantidad = (decimal)m.cantidad,
                    FechaMovimiento = m.fecha_movimiento,
                    Observacion = m.observacion,
                    IdDetalleResultado = m.id_detalle_resultado,
                    NumeroResultado = m.id_detalle_resultadoNavigation != null ? m.id_detalle_resultadoNavigation.id_resultadoNavigation.numero_resultado : null
                })
                .ToListAsync();

            return new ResultadoPaginadoDto<MovimientoReactivoDto>
            {
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }
    }
}
