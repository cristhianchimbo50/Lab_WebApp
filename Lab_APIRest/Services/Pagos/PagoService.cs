using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using pago = Lab_APIRest.Infrastructure.EF.Models.pago;
using orden = Lab_APIRest.Infrastructure.EF.Models.orden;
using detalle_pago = Lab_APIRest.Infrastructure.EF.Models.detalle_pago;
using Microsoft.EntityFrameworkCore;
using Lab_Contracts.Common;
using System;
using System.Linq;

namespace Lab_APIRest.Services.Pagos
{
    public class PagoService : IPagoService
    {
        private const int EstadoPagoPendienteId = 1;
        private const int EstadoPagoAbonadoId = 2;
        private const int EstadoPagoPagadoId = 3;

        private const int TipoPagoEfectivoId = 1;
        private const int TipoPagoTransferenciaId = 2;

        private readonly LabDbContext _context;

        public PagoService(LabDbContext context)
        {
            _context = context;
        }

        private static PagoDto MapPago(pago entidadPago)
        {
            var montoRecibido = entidadPago.monto_recibido;
            var montoAplicado = entidadPago.monto_aplicado;
            var montoVuelto = entidadPago.monto_vuelto;

            return new PagoDto
            {
                IdPago = entidadPago.id_pago,
                IdOrden = entidadPago.id_orden ?? 0,
                FechaPago = entidadPago.fecha_pago,
                MontoRecibido = montoRecibido,
                MontoAplicado = montoAplicado,
                MontoVuelto = montoVuelto,
                MontoPagado = montoAplicado,
                Observacion = entidadPago.observacion ?? string.Empty,
                Anulado = !entidadPago.activo,
                DetallePagos = entidadPago.detalle_pago.Select(d => new DetallePagoDto
                {
                    IdDetallePago = d.id_detalle_pago,
                    IdPago = d.id_pago ?? 0,
                    IdTipoPago = d.id_tipo_pago,
                    TipoPago = d.tipo_pago_navigation?.nombre,
                    NumeroComprobante = d.numero_comprobante,
                    Monto = d.monto ?? 0m,
                    Anulado = !d.activo,
                    FechaPago = entidadPago.fecha_pago
                }).ToList()
            };
        }

        public async Task<PagoDto?> GuardarPagoAsync(PagoDto pagoDto)
        {
            var detalles = (pagoDto.DetallePagos ?? new List<DetallePagoDto>())
                .Where(d => d.Monto > 0)
                .Select(d => new detalle_pago
                {
                    id_tipo_pago = d.IdTipoPago ?? MapearTipoPago(d.TipoPago),
                    numero_comprobante = d.NumeroComprobante,
                    monto = d.Monto,
                    activo = !(d.Anulado ?? false),
                    fecha_creacion = DateTime.UtcNow
                })
                .ToList();

            if (!detalles.Any())
                throw new InvalidOperationException("Debe registrar al menos un detalle de pago con monto mayor a cero.");

            if (detalles.Any(d => !d.id_tipo_pago.HasValue))
                throw new InvalidOperationException("Uno o más tipos de pago no son válidos.");

            var montoRecibido = detalles.Sum(d => d.monto ?? 0m);
            var entidadPago = new pago
            {
                id_orden = pagoDto.IdOrden,
                fecha_pago = pagoDto.FechaPago ?? DateTime.UtcNow,
                monto_recibido = montoRecibido,
                monto_aplicado = pagoDto.MontoAplicado == 0 ? montoRecibido : pagoDto.MontoAplicado,
                monto_vuelto = pagoDto.MontoVuelto,
                observacion = pagoDto.Observacion,
                activo = !pagoDto.Anulado
            };

            entidadPago.detalle_pago = detalles;

            _context.Pago.Add(entidadPago);
            await _context.SaveChangesAsync();

            await ActualizarEstadoOrdenAsync(entidadPago.id_orden, entidadPago.monto_aplicado);

            return MapPago(entidadPago);
        }

        public async Task<List<PagoDto>> ListarPagosPorOrdenAsync(int idOrden)
        {
            var pagos = await _context.Pago
                .Include(p => p.detalle_pago)!.ThenInclude(d => d.tipo_pago_navigation)
                .Where(p => p.id_orden == idOrden && p.activo)
                .ToListAsync();

            return pagos.Select(MapPago).ToList();
        }

        public async Task<PagoDto?> ProcesarCobroCuentaPorCobrarAsync(PagoDto pagoDto)
        {
            var entidadOrden = await _context.Orden.FirstOrDefaultAsync(o => o.id_orden == pagoDto.IdOrden);
            if (entidadOrden == null) return null;

            var saldo = entidadOrden.saldo_pendiente ?? 0m;
            var totalRecibido = pagoDto.DineroEfectivo + pagoDto.Transferencia;
            var aplicado = Math.Min(totalRecibido, saldo);
            var vuelto = Math.Max(0, totalRecibido - aplicado);

            var entidadPago = new pago
            {
                id_orden = pagoDto.IdOrden,
                fecha_pago = DateTime.UtcNow,
                monto_recibido = totalRecibido,
                monto_aplicado = aplicado,
                monto_vuelto = vuelto,
                observacion = pagoDto.Observacion,
                activo = true
            };
            _context.Pago.Add(entidadPago);
            await _context.SaveChangesAsync();

            var detalles = new List<detalle_pago>();
            if (pagoDto.DineroEfectivo > 0)
            {
                detalles.Add(new detalle_pago
                {
                    id_pago = entidadPago.id_pago,
                    id_tipo_pago = TipoPagoEfectivoId,
                    numero_comprobante = null,
                    monto = pagoDto.DineroEfectivo,
                    activo = true,
                    fecha_creacion = DateTime.UtcNow
                });
            }
            if (pagoDto.Transferencia > 0)
            {
                detalles.Add(new detalle_pago
                {
                    id_pago = entidadPago.id_pago,
                    id_tipo_pago = TipoPagoTransferenciaId,
                    numero_comprobante = null,
                    monto = pagoDto.Transferencia,
                    activo = true,
                    fecha_creacion = DateTime.UtcNow
                });
            }
            if (detalles.Any())
            {
                _context.DetallePago.AddRange(detalles);
                await _context.SaveChangesAsync();
            }

            var pagadoAcumulado = entidadPago.monto_aplicado;
            entidadOrden.saldo_pendiente = Math.Max(0, (entidadOrden.saldo_pendiente ?? entidadOrden.total) - pagadoAcumulado);
            entidadOrden.id_estado_pago = entidadOrden.saldo_pendiente <= 0 ? EstadoPagoPagadoId : (pagadoAcumulado > 0 ? EstadoPagoAbonadoId : EstadoPagoPendienteId);
            await _context.SaveChangesAsync();

            return MapPago(entidadPago);
        }

        private static OrdenDto MapOrden(orden o) => new()
        {
            IdOrden = o.id_orden,
            NumeroOrden = o.numero_orden,
            CedulaPaciente = o.paciente_navigation?.persona_navigation?.cedula ?? string.Empty,
            NombrePaciente = $"{o.paciente_navigation?.persona_navigation?.nombres} {o.paciente_navigation?.persona_navigation?.apellidos}".Trim(),
            FechaOrden = o.fecha_orden,
            Total = o.total,
            TotalPagado = o.total - (o.saldo_pendiente ?? 0m),
            SaldoPendiente = o.saldo_pendiente ?? 0m,
            EstadoPago = o.estado_pago_navigation?.nombre,
            IdEstadoPago = o.id_estado_pago,
            NombreEstadoPago = o.estado_pago_navigation?.nombre,
            Anulado = !o.activo
        };

        public async Task<List<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro)
        {
            var query = _context.Orden.Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(o => o.estado_pago_navigation)
                .Where(o => o.saldo_pendiente > 0);

            if (!string.IsNullOrEmpty(filtro.NumeroOrden))
                query = query.Where(o => o.numero_orden.Contains(filtro.NumeroOrden));
            if (!string.IsNullOrEmpty(filtro.Cedula))
                query = query.Where(o => o.paciente_navigation != null && o.paciente_navigation.persona_navigation!.cedula.Contains(filtro.Cedula));
            if (!string.IsNullOrEmpty(filtro.NombrePaciente))
                query = query.Where(o => o.paciente_navigation != null && (o.paciente_navigation.persona_navigation!.nombres + " " + o.paciente_navigation.persona_navigation!.apellidos).Contains(filtro.NombrePaciente));
            if (filtro.FechaInicio.HasValue)
                query = query.Where(o => o.fecha_orden >= DateOnly.FromDateTime(filtro.FechaInicio.Value.Date));
            if (filtro.FechaFin.HasValue)
                query = query.Where(o => o.fecha_orden <= DateOnly.FromDateTime(filtro.FechaFin.Value.Date));
            if (filtro.IncluirAnulados.HasValue)
                query = query.Where(o => o.activo == !filtro.IncluirAnulados.Value);
            if (!string.IsNullOrEmpty(filtro.EstadoPago))
            {
                var estadoId = MapearEstadoPago(filtro.EstadoPago);
                if (estadoId.HasValue)
                    query = query.Where(o => o.id_estado_pago == estadoId.Value);
            }

            var ordenes = await query
                .OrderBy(o => o.paciente_navigation!.persona_navigation!.nombres)
                .ThenBy(o => o.fecha_orden)
                .ToListAsync();

            return ordenes.Select(MapOrden).ToList();
        }

        public async Task<ResultadoPaginadoDto<OrdenDto>> ListarCuentasPorCobrarPaginadosAsync(PagoFiltroDto filtro)
        {
            var query = _context.Orden
                .Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(o => o.estado_pago_navigation)
                .AsNoTracking()
                .Where(o => o.saldo_pendiente > 0);

            if (!string.IsNullOrEmpty(filtro.NumeroOrden))
                query = query.Where(o => o.numero_orden.Contains(filtro.NumeroOrden));
            if (!string.IsNullOrEmpty(filtro.Cedula))
                query = query.Where(o => o.paciente_navigation != null && o.paciente_navigation.persona_navigation!.cedula.Contains(filtro.Cedula));
            if (!string.IsNullOrEmpty(filtro.NombrePaciente))
                query = query.Where(o => o.paciente_navigation != null && (o.paciente_navigation.persona_navigation!.nombres + " " + o.paciente_navigation.persona_navigation!.apellidos).Contains(filtro.NombrePaciente));
            if (filtro.FechaInicio.HasValue)
                query = query.Where(o => o.fecha_orden >= DateOnly.FromDateTime(filtro.FechaInicio.Value.Date));
            if (filtro.FechaFin.HasValue)
                query = query.Where(o => o.fecha_orden <= DateOnly.FromDateTime(filtro.FechaFin.Value.Date));
            if (filtro.IncluirAnulados.HasValue)
                query = query.Where(o => o.activo == !filtro.IncluirAnulados.Value);
            if (!string.IsNullOrEmpty(filtro.EstadoPago))
            {
                var estadoId = MapearEstadoPago(filtro.EstadoPago);
                if (estadoId.HasValue)
                    query = query.Where(o => o.id_estado_pago == estadoId.Value);
            }

            var total = await query.CountAsync();

            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(OrdenDto.NumeroOrden) => asc ? query.OrderBy(o => o.numero_orden) : query.OrderByDescending(o => o.numero_orden),
                nameof(OrdenDto.NombrePaciente) => asc ? query.OrderBy(o => o.paciente_navigation!.persona_navigation!.nombres) : query.OrderByDescending(o => o.paciente_navigation!.persona_navigation!.nombres),
                nameof(OrdenDto.FechaOrden) => asc ? query.OrderBy(o => o.fecha_orden) : query.OrderByDescending(o => o.fecha_orden),
                nameof(OrdenDto.Total) => asc ? query.OrderBy(o => o.total) : query.OrderByDescending(o => o.total),
                _ => query.OrderByDescending(o => o.id_orden)
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

        private static int? MapearEstadoPago(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado)) return null;
            return estado.ToUpper() switch
            {
                "PENDIENTE" => EstadoPagoPendienteId,
                "ABONADO" => EstadoPagoAbonadoId,
                "PAGADO" => EstadoPagoPagadoId,
                _ => null
            };
        }

        private static int? MapearTipoPago(string? tipoPago)
        {
            if (string.IsNullOrWhiteSpace(tipoPago)) return null;
            return tipoPago.ToUpper() switch
            {
                "EFECTIVO" => TipoPagoEfectivoId,
                "TRANSFERENCIA" => TipoPagoTransferenciaId,
                _ => null
            };
        }

        private async Task ActualizarEstadoOrdenAsync(int? idOrden, decimal montoAplicado)
        {
            if (!idOrden.HasValue) return;

            var orden = await _context.Orden.FirstOrDefaultAsync(o => o.id_orden == idOrden.Value);
            if (orden == null) return;

            orden.saldo_pendiente = Math.Max(0, (orden.saldo_pendiente ?? orden.total) - montoAplicado);

            if (orden.saldo_pendiente <= 0)
                orden.id_estado_pago = EstadoPagoPagadoId;
            else if (montoAplicado > 0)
                orden.id_estado_pago = EstadoPagoAbonadoId;
            else
                orden.id_estado_pago = EstadoPagoPendienteId;

            await _context.SaveChangesAsync();
        }
    }
}
