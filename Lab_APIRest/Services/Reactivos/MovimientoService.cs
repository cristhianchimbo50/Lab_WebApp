using Lab_APIRest.Infrastructure.EF;
using Lab_Contracts.Reactivos;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Reactivos
{
    public class MovimientoService : IMovimientoService
    {
        private readonly LabDbContext _context;

        public MovimientoService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<List<MovimientoReactivoDto>> GetMovimientosAsync(MovimientoReactivoFiltroDto filtro)
        {
            var query = _context.movimiento_reactivos
                .Include(m => m.id_reactivoNavigation)
                .Include(m => m.id_detalle_resultadoNavigation)
                    .ThenInclude(d => d.id_resultadoNavigation)
                .AsQueryable();

            // FILTROS
            if (!string.IsNullOrWhiteSpace(filtro.IdMovimientoReactivo))
            {
                if (int.TryParse(filtro.IdMovimientoReactivo, out int numMov))
                    query = query.Where(m => m.id_movimiento_reactivo == numMov);
            }

            if (!string.IsNullOrWhiteSpace(filtro.NombreReactivo))
            {
                query = query.Where(m => m.id_reactivoNavigation.nombre_reactivo
                    .Contains(filtro.NombreReactivo));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Observacion))
            {
                query = query.Where(m => m.observacion.Contains(filtro.Observacion));
            }

            if (filtro.FechaDesde.HasValue)
            {
                query = query.Where(m => m.fecha_movimiento >= filtro.FechaDesde.Value);
            }

            if (filtro.FechaHasta.HasValue)
            {
                query = query.Where(m => m.fecha_movimiento <= filtro.FechaHasta.Value);
            }

            if (!(filtro.IncluirIngresos && filtro.IncluirEgresos))
            {
                if (filtro.IncluirIngresos)
                    query = query.Where(m => m.tipo_movimiento == "INGRESO");
                else if (filtro.IncluirEgresos)
                    query = query.Where(m => m.tipo_movimiento == "EGRESO");
                else
                    query = query.Where(m => false);
            }

            var movimientos = await query
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
    }
}
