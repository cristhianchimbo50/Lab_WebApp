using Lab_APIRest.Infrastructure.EF;
using Lab_Contracts.Reactivos;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Reactivos
{
    public class MovimientoService : IMovimientoService
    {
        private readonly LabDbContext Contexto;

        public MovimientoService(LabDbContext Contexto)
        {
            this.Contexto = Contexto;
        }

        public async Task<List<MovimientoReactivoDto>> ObtenerMovimientosAsync(MovimientoReactivoFiltroDto Filtro)
        {
            var Consulta = Contexto.movimiento_reactivos
                .Include(Movimiento => Movimiento.id_reactivoNavigation)
                .Include(Movimiento => Movimiento.id_detalle_resultadoNavigation)
                    .ThenInclude(DetalleResultado => DetalleResultado.id_resultadoNavigation)
                .AsQueryable();

            // FILTROS
            if (!string.IsNullOrWhiteSpace(Filtro.IdMovimientoReactivo))
            {
                if (int.TryParse(Filtro.IdMovimientoReactivo, out int NumeroMovimiento))
                    Consulta = Consulta.Where(Movimiento => Movimiento.id_movimiento_reactivo == NumeroMovimiento);
            }

            if (!string.IsNullOrWhiteSpace(Filtro.NombreReactivo))
            {
                Consulta = Consulta.Where(Movimiento => Movimiento.id_reactivoNavigation.nombre_reactivo
                    .Contains(Filtro.NombreReactivo));
            }

            if (!string.IsNullOrWhiteSpace(Filtro.Observacion))
            {
                Consulta = Consulta.Where(Movimiento => Movimiento.observacion.Contains(Filtro.Observacion));
            }

            if (Filtro.FechaDesde.HasValue)
            {
                Consulta = Consulta.Where(Movimiento => Movimiento.fecha_movimiento >= Filtro.FechaDesde.Value);
            }

            if (Filtro.FechaHasta.HasValue)
            {
                Consulta = Consulta.Where(Movimiento => Movimiento.fecha_movimiento <= Filtro.FechaHasta.Value);
            }

            if (!(Filtro.IncluirIngresos && Filtro.IncluirEgresos))
            {
                if (Filtro.IncluirIngresos)
                    Consulta = Consulta.Where(Movimiento => Movimiento.tipo_movimiento == "INGRESO");
                else if (Filtro.IncluirEgresos)
                    Consulta = Consulta.Where(Movimiento => Movimiento.tipo_movimiento == "EGRESO");
                else
                    Consulta = Consulta.Where(_ => false);
            }

            var Movimientos = await Consulta
                .OrderByDescending(Movimiento => Movimiento.fecha_movimiento)
                .Select(Movimiento => new MovimientoReactivoDto
                {
                    IdMovimientoReactivo = Movimiento.id_movimiento_reactivo,
                    IdReactivo = (int)Movimiento.id_reactivo,
                    NombreReactivo = Movimiento.id_reactivoNavigation.nombre_reactivo,
                    TipoMovimiento = Movimiento.tipo_movimiento,
                    Cantidad = (decimal)Movimiento.cantidad,
                    FechaMovimiento = Movimiento.fecha_movimiento,
                    Observacion = Movimiento.observacion,
                    IdDetalleResultado = Movimiento.id_detalle_resultado,
                    NumeroResultado = Movimiento.id_detalle_resultadoNavigation != null
                        ? Movimiento.id_detalle_resultadoNavigation.id_resultadoNavigation.numero_resultado
                        : null
                })
                .ToListAsync();

            return Movimientos;
        }
    }
}
