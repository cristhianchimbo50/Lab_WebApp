using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using movimiento_reactivo = Lab_APIRest.Infrastructure.EF.Models.movimiento_reactivo;
using Lab_Contracts.Reactivos;
using Microsoft.EntityFrameworkCore;
using Lab_Contracts.Common;
using System.Linq;

namespace Lab_APIRest.Services.Reactivos
{
    public class MovimientoService : IMovimientoService
    {
        private readonly LabDbContext _context;

        public MovimientoService(LabDbContext context)
        {
            _context = context;
        }

        private static MovimientoReactivoDto MapMovimiento(movimiento_reactivo entidad) => new()
        {
            IdMovimientoReactivo = entidad.id_movimiento_reactivo,
            IdReactivo = entidad.id_reactivo ?? 0,
            NombreReactivo = entidad.reactivo_navigation?.nombre_reactivo ?? string.Empty,
            TipoMovimiento = entidad.tipo_movimiento ?? string.Empty,
            Cantidad = entidad.cantidad ?? 0m,
            FechaMovimiento = entidad.fecha_movimiento,
            Observacion = entidad.observacion ?? string.Empty,
            IdDetalleResultado = entidad.detalle_resultado_navigation != null ? entidad.detalle_resultado_navigation.id_resultado : null,
            NumeroResultado = entidad.detalle_resultado_navigation?.resultado_navigation?.numero_resultado
        };

        public async Task<List<MovimientoReactivoDto>> ListarMovimientosAsync(MovimientoReactivoFiltroDto filtro)
        {
            var consulta = _context.MovimientoReactivo
                .Include(m => m.reactivo_navigation)
                .Include(m => m.detalle_resultado_navigation)!.ThenInclude(dr => dr.resultado_navigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.IdMovimientoReactivo) && int.TryParse(filtro.IdMovimientoReactivo, out int numeroMovimiento))
                consulta = consulta.Where(m => m.id_movimiento_reactivo == numeroMovimiento);

            if (!string.IsNullOrWhiteSpace(filtro.NombreReactivo))
                consulta = consulta.Where(m => m.reactivo_navigation != null && m.reactivo_navigation.nombre_reactivo.Contains(filtro.NombreReactivo));

            if (!string.IsNullOrWhiteSpace(filtro.Observacion))
                consulta = consulta.Where(m => (m.observacion ?? "").Contains(filtro.Observacion));

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

            var movimientos = await consulta
                .OrderByDescending(m => m.fecha_movimiento)
                .Select(m => MapMovimiento(m))
                .ToListAsync();

            return movimientos;
        }

        public async Task<ResultadoPaginadoDto<MovimientoReactivoDto>> ListarMovimientosPaginadosAsync(MovimientoReactivoFiltroDto filtro)
        {
            var consulta = _context.MovimientoReactivo
                .Include(m => m.reactivo_navigation)
                .Include(m => m.detalle_resultado_navigation)!.ThenInclude(d => d.resultado_navigation)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.IdMovimientoReactivo) && int.TryParse(filtro.IdMovimientoReactivo, out int numeroMovimiento))
                consulta = consulta.Where(m => m.id_movimiento_reactivo == numeroMovimiento);
            if (!string.IsNullOrWhiteSpace(filtro.NombreReactivo))
                consulta = consulta.Where(m => m.reactivo_navigation != null && m.reactivo_navigation.nombre_reactivo.Contains(filtro.NombreReactivo));
            if (!string.IsNullOrWhiteSpace(filtro.Observacion))
                consulta = consulta.Where(m => (m.observacion ?? "").Contains(filtro.Observacion));
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
                nameof(MovimientoReactivoDto.NombreReactivo) => asc ? consulta.OrderBy(m => m.reactivo_navigation!.nombre_reactivo) : consulta.OrderByDescending(m => m.reactivo_navigation!.nombre_reactivo),
                nameof(MovimientoReactivoDto.TipoMovimiento) => asc ? consulta.OrderBy(m => m.tipo_movimiento) : consulta.OrderByDescending(m => m.tipo_movimiento),
                nameof(MovimientoReactivoDto.Cantidad) => asc ? consulta.OrderBy(m => m.cantidad) : consulta.OrderByDescending(m => m.cantidad),
                _ => asc ? consulta.OrderBy(m => m.fecha_movimiento) : consulta.OrderByDescending(m => m.fecha_movimiento)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await consulta.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(m => MapMovimiento(m))
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
