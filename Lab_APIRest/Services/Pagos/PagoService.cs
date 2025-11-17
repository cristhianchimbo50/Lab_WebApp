using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;
using Lab_Contracts.Common;

namespace Lab_APIRest.Services.Pagos
{
    public class PagoService : IPagoService
    {
        private readonly LabDbContext _context;

        public PagoService(LabDbContext context)
        {
            _context = context;
        }

        private static PagoDto MapPago(Pago entidadPago) => new()
        {
            IdPago = entidadPago.IdPago,
            IdOrden = entidadPago.IdOrden ?? 0,
            FechaPago = entidadPago.FechaPago,
            MontoPagado = entidadPago.MontoPagado ?? 0m,
            Observacion = entidadPago.Observacion ?? string.Empty,
            Anulado = !entidadPago.Activo,
            DetallePagos = entidadPago.DetallePago.Select(d => new DetallePagoDto
            {
                IdDetallePago = d.IdDetallePago,
                IdPago = d.IdPago ?? 0,
                TipoPago = d.TipoPago ?? string.Empty,
                Monto = d.Monto ?? 0m,
                Anulado = !d.Activo,
                FechaPago = entidadPago.FechaPago
            }).ToList()
        };

        public async Task<PagoDto?> GuardarPagoAsync(PagoDto pagoDto)
        {
            var entidadPago = new Pago
            {
                IdOrden = pagoDto.IdOrden,
                FechaPago = pagoDto.FechaPago ?? DateTime.UtcNow,
                MontoPagado = pagoDto.MontoPagado,
                Observacion = pagoDto.Observacion,
                Activo = !pagoDto.Anulado
            };

            entidadPago.DetallePago = pagoDto.DetallePagos.Select(d => new DetallePago
            {
                TipoPago = d.TipoPago,
                Monto = d.Monto,
                Activo = !(d.Anulado ?? false),
                FechaCreacion = DateTime.UtcNow
            }).ToList();

            _context.Pago.Add(entidadPago);
            await _context.SaveChangesAsync();

            return MapPago(entidadPago);
        }

        public async Task<List<PagoDto>> ListarPagosPorOrdenAsync(int idOrden)
        {
            var pagos = await _context.Pago
                .Include(p => p.DetallePago)
                .Where(p => p.IdOrden == idOrden && p.Activo)
                .ToListAsync();

            return pagos.Select(MapPago).ToList();
        }

        public async Task<PagoDto?> ProcesarCobroCuentaPorCobrarAsync(PagoDto pagoDto)
        {
            var entidadOrden = await _context.Orden.FirstOrDefaultAsync(o => o.IdOrden == pagoDto.IdOrden);
            if (entidadOrden == null) return null;

            var saldo = entidadOrden.SaldoPendiente ?? 0m;
            var total = pagoDto.DineroEfectivo + pagoDto.Transferencia;
            if (total > saldo) total = saldo;

            var entidadPago = new Pago
            {
                IdOrden = pagoDto.IdOrden,
                FechaPago = DateTime.UtcNow,
                MontoPagado = total,
                Observacion = pagoDto.Observacion,
                Activo = true
            };
            _context.Pago.Add(entidadPago);
            await _context.SaveChangesAsync();

            var detalles = new List<DetallePago>();
            if (pagoDto.DineroEfectivo > 0)
            {
                detalles.Add(new DetallePago
                {
                    IdPago = entidadPago.IdPago,
                    TipoPago = "EFECTIVO",
                    Monto = pagoDto.DineroEfectivo,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                });
            }
            if (pagoDto.Transferencia > 0)
            {
                detalles.Add(new DetallePago
                {
                    IdPago = entidadPago.IdPago,
                    TipoPago = "TRANSFERENCIA",
                    Monto = pagoDto.Transferencia,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                });
            }
            if (detalles.Any())
            {
                _context.DetallePago.AddRange(detalles);
                await _context.SaveChangesAsync();
            }

            entidadOrden.TotalPagado = (entidadOrden.TotalPagado ?? 0m) + total;
            entidadOrden.SaldoPendiente = entidadOrden.Total - (entidadOrden.TotalPagado ?? 0m);
            entidadOrden.EstadoPago = entidadOrden.SaldoPendiente <= 0 ? "PAGADO" : "PENDIENTE";
            await _context.SaveChangesAsync();

            return MapPago(entidadPago);
        }

        private static OrdenDto MapOrden(Orden o) => new()
        {
            IdOrden = o.IdOrden,
            NumeroOrden = o.NumeroOrden,
            CedulaPaciente = o.IdPacienteNavigation?.CedulaPaciente ?? string.Empty,
            NombrePaciente = o.IdPacienteNavigation?.NombrePaciente ?? string.Empty,
            FechaOrden = o.FechaOrden,
            Total = o.Total,
            TotalPagado = o.TotalPagado ?? 0m,
            SaldoPendiente = o.SaldoPendiente ?? 0m,
            EstadoPago = o.EstadoPago,
            Anulado = !o.Activo
        };

        public async Task<List<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro)
        {
            var query = _context.Orden.Include(o => o.IdPacienteNavigation).Where(o => o.SaldoPendiente > 0);

            if (!string.IsNullOrEmpty(filtro.NumeroOrden))
                query = query.Where(o => o.NumeroOrden.Contains(filtro.NumeroOrden));
            if (!string.IsNullOrEmpty(filtro.Cedula))
                query = query.Where(o => o.IdPacienteNavigation != null && o.IdPacienteNavigation.CedulaPaciente.Contains(filtro.Cedula));
            if (!string.IsNullOrEmpty(filtro.NombrePaciente))
                query = query.Where(o => o.IdPacienteNavigation != null && o.IdPacienteNavigation.NombrePaciente.Contains(filtro.NombrePaciente));
            if (filtro.FechaInicio.HasValue)
                query = query.Where(o => o.FechaOrden >= DateOnly.FromDateTime(filtro.FechaInicio.Value.Date));
            if (filtro.FechaFin.HasValue)
                query = query.Where(o => o.FechaOrden <= DateOnly.FromDateTime(filtro.FechaFin.Value.Date));
            if (filtro.IncluirAnulados.HasValue)
                query = query.Where(o => o.Activo == !filtro.IncluirAnulados.Value);
            if (!string.IsNullOrEmpty(filtro.EstadoPago))
                query = query.Where(o => o.EstadoPago == filtro.EstadoPago);

            var ordenes = await query
                .OrderBy(o => o.IdPacienteNavigation!.NombrePaciente)
                .ThenBy(o => o.FechaOrden)
                .ToListAsync();

            return ordenes.Select(MapOrden).ToList();
        }

        public async Task<ResultadoPaginadoDto<OrdenDto>> ListarCuentasPorCobrarPaginadosAsync(PagoFiltroDto filtro)
        {
            var query = _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .AsNoTracking()
                .Where(o => o.SaldoPendiente > 0);

            if (!string.IsNullOrEmpty(filtro.NumeroOrden))
                query = query.Where(o => o.NumeroOrden.Contains(filtro.NumeroOrden));
            if (!string.IsNullOrEmpty(filtro.Cedula))
                query = query.Where(o => o.IdPacienteNavigation != null && o.IdPacienteNavigation.CedulaPaciente.Contains(filtro.Cedula));
            if (!string.IsNullOrEmpty(filtro.NombrePaciente))
                query = query.Where(o => o.IdPacienteNavigation != null && o.IdPacienteNavigation.NombrePaciente.Contains(filtro.NombrePaciente));
            if (filtro.FechaInicio.HasValue)
                query = query.Where(o => o.FechaOrden >= DateOnly.FromDateTime(filtro.FechaInicio.Value.Date));
            if (filtro.FechaFin.HasValue)
                query = query.Where(o => o.FechaOrden <= DateOnly.FromDateTime(filtro.FechaFin.Value.Date));
            if (filtro.IncluirAnulados.HasValue)
                query = query.Where(o => o.Activo == !filtro.IncluirAnulados.Value);
            if (!string.IsNullOrEmpty(filtro.EstadoPago))
                query = query.Where(o => o.EstadoPago == filtro.EstadoPago);

            var total = await query.CountAsync();

            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(OrdenDto.NumeroOrden) => asc ? query.OrderBy(o => o.NumeroOrden) : query.OrderByDescending(o => o.NumeroOrden),
                nameof(OrdenDto.NombrePaciente) => asc ? query.OrderBy(o => o.IdPacienteNavigation!.NombrePaciente) : query.OrderByDescending(o => o.IdPacienteNavigation!.NombrePaciente),
                nameof(OrdenDto.FechaOrden) => asc ? query.OrderBy(o => o.FechaOrden) : query.OrderByDescending(o => o.FechaOrden),
                nameof(OrdenDto.Total) => asc ? query.OrderBy(o => o.Total) : query.OrderByDescending(o => o.Total),
                _ => query.OrderByDescending(o => o.IdOrden)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => MapOrden(o))
                .ToListAsync();

            return new ResultadoPaginadoDto<OrdenDto>
            {
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }
    }
}
