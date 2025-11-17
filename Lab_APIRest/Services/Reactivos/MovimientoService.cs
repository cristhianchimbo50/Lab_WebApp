using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
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

        private static MovimientoReactivoDto MapMovimiento(MovimientoReactivo entidad) => new()
        {
            IdMovimientoReactivo = entidad.IdMovimientoReactivo,
            IdReactivo = entidad.IdReactivo ?? 0,
            NombreReactivo = entidad.IdReactivoNavigation?.NombreReactivo ?? string.Empty,
            TipoMovimiento = entidad.TipoMovimiento ?? string.Empty,
            Cantidad = entidad.Cantidad ?? 0m,
            FechaMovimiento = entidad.FechaMovimiento,
            Observacion = entidad.Observacion ?? string.Empty,
            IdDetalleResultado = entidad.DetalleResultado != null ? entidad.DetalleResultado.IdResultado : null,
            NumeroResultado = entidad.DetalleResultado?.IdResultadoNavigation?.NumeroResultado
        };

        public async Task<List<MovimientoReactivoDto>> ListarMovimientosAsync(MovimientoReactivoFiltroDto filtro)
        {
            var consulta = _context.MovimientoReactivo
                .Include(m => m.IdReactivoNavigation)
                .Include(m => m.DetalleResultado)!.ThenInclude(dr => dr.IdResultadoNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.IdMovimientoReactivo) && int.TryParse(filtro.IdMovimientoReactivo, out int numeroMovimiento))
                consulta = consulta.Where(m => m.IdMovimientoReactivo == numeroMovimiento);

            if (!string.IsNullOrWhiteSpace(filtro.NombreReactivo))
                consulta = consulta.Where(m => m.IdReactivoNavigation != null && m.IdReactivoNavigation.NombreReactivo.Contains(filtro.NombreReactivo));

            if (!string.IsNullOrWhiteSpace(filtro.Observacion))
                consulta = consulta.Where(m => (m.Observacion ?? "").Contains(filtro.Observacion));

            if (filtro.FechaDesde.HasValue)
                consulta = consulta.Where(m => m.FechaMovimiento >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue)
                consulta = consulta.Where(m => m.FechaMovimiento <= filtro.FechaHasta.Value);

            if (!(filtro.IncluirIngresos && filtro.IncluirEgresos))
            {
                if (filtro.IncluirIngresos) consulta = consulta.Where(m => m.TipoMovimiento == "INGRESO");
                else if (filtro.IncluirEgresos) consulta = consulta.Where(m => m.TipoMovimiento == "EGRESO");
                else consulta = consulta.Where(_ => false);
            }

            var movimientos = await consulta
                .OrderByDescending(m => m.FechaMovimiento)
                .Select(m => MapMovimiento(m))
                .ToListAsync();

            return movimientos;
        }

        public async Task<ResultadoPaginadoDto<MovimientoReactivoDto>> ListarMovimientosPaginadosAsync(MovimientoReactivoFiltroDto filtro)
        {
            var consulta = _context.MovimientoReactivo
                .Include(m => m.IdReactivoNavigation)
                .Include(m => m.DetalleResultado)!.ThenInclude(d => d.IdResultadoNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.IdMovimientoReactivo) && int.TryParse(filtro.IdMovimientoReactivo, out int numeroMovimiento))
                consulta = consulta.Where(m => m.IdMovimientoReactivo == numeroMovimiento);
            if (!string.IsNullOrWhiteSpace(filtro.NombreReactivo))
                consulta = consulta.Where(m => m.IdReactivoNavigation != null && m.IdReactivoNavigation.NombreReactivo.Contains(filtro.NombreReactivo));
            if (!string.IsNullOrWhiteSpace(filtro.Observacion))
                consulta = consulta.Where(m => (m.Observacion ?? "").Contains(filtro.Observacion));
            if (filtro.FechaDesde.HasValue)
                consulta = consulta.Where(m => m.FechaMovimiento >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue)
                consulta = consulta.Where(m => m.FechaMovimiento <= filtro.FechaHasta.Value);
            if (!(filtro.IncluirIngresos && filtro.IncluirEgresos))
            {
                if (filtro.IncluirIngresos) consulta = consulta.Where(m => m.TipoMovimiento == "INGRESO");
                else if (filtro.IncluirEgresos) consulta = consulta.Where(m => m.TipoMovimiento == "EGRESO");
                else consulta = consulta.Where(_ => false);
            }

            var total = await consulta.CountAsync();

            bool asc = filtro.SortAsc;
            consulta = filtro.SortBy switch
            {
                nameof(MovimientoReactivoDto.IdMovimientoReactivo) => asc ? consulta.OrderBy(m => m.IdMovimientoReactivo) : consulta.OrderByDescending(m => m.IdMovimientoReactivo),
                nameof(MovimientoReactivoDto.NombreReactivo) => asc ? consulta.OrderBy(m => m.IdReactivoNavigation!.NombreReactivo) : consulta.OrderByDescending(m => m.IdReactivoNavigation!.NombreReactivo),
                nameof(MovimientoReactivoDto.TipoMovimiento) => asc ? consulta.OrderBy(m => m.TipoMovimiento) : consulta.OrderByDescending(m => m.TipoMovimiento),
                nameof(MovimientoReactivoDto.Cantidad) => asc ? consulta.OrderBy(m => m.Cantidad) : consulta.OrderByDescending(m => m.Cantidad),
                _ => asc ? consulta.OrderBy(m => m.FechaMovimiento) : consulta.OrderByDescending(m => m.FechaMovimiento)
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
