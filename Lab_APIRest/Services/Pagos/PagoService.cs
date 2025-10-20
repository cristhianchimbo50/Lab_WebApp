using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;


namespace Lab_APIRest.Services.Pagos
{
    public class PagoService : IPagoService
    {
        private readonly LabDbContext _context;

        public PagoService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<PagoDto?> RegistrarPagoAsync(PagoDto dto)
        {
            Console.WriteLine("PAGO SERVICE, DTO: " + System.Text.Json.JsonSerializer.Serialize(dto));
            var pago = new pago
            {
                id_orden = dto.IdOrden,
                fecha_pago = dto.FechaPago,
                monto_pagado = dto.MontoPagado,
                observacion = dto.Observacion,
                anulado = dto.Anulado
            };

            pago.detalle_pagos = dto.DetallePagos.Select(d => new detalle_pago
            {
                tipo_pago = d.TipoPago,
                monto = d.Monto,
                anulado = d.Anulado
            }).ToList();

            _context.pagos.Add(pago);
            await _context.SaveChangesAsync();

            dto.IdPago = pago.id_pago;
            var detallesGuardados = pago.detalle_pagos.ToList();
            for (int i = 0; i < dto.DetallePagos.Count; i++)
            {
                dto.DetallePagos[i].IdDetallePago = detallesGuardados[i].id_detalle_pago;
                dto.DetallePagos[i].IdPago = pago.id_pago;
            }


            return dto;
        }

        public async Task<List<PagoDto>> ListarPagosPorOrdenAsync(int idOrden)
        {
            var pagos = await _context.pagos
                .Include(p => p.detalle_pagos)
                .Where(p => p.id_orden == idOrden)
                .ToListAsync();

            return pagos.Select(p => new PagoDto
            {
                IdPago = (int)p.id_pago,
                IdOrden = (int)p.id_orden,
                FechaPago = p.fecha_pago,
                MontoPagado = (decimal)p.monto_pagado,
                Observacion = p.observacion,
                Anulado = (bool)p.anulado,
                DetallePagos = p.detalle_pagos.Select(d => new DetallePagoDto
                {
                    IdDetallePago = d.id_detalle_pago,
                    IdPago = (int)d.id_pago,
                    TipoPago = d.tipo_pago,
                    Monto = (decimal)d.monto,
                    Anulado = d.anulado
                }).ToList()
            }).ToList();
        }

        public async Task<PagoDto?> RegistrarCobroCuentaPorCobrarAsync(PagoDto dto)
        {
            var orden = await _context.ordens.FirstOrDefaultAsync(o => o.id_orden == dto.IdOrden);
            if (orden == null) return null;

            var saldo = orden.saldo_pendiente ?? 0;
            var total = (dto.DineroEfectivo) + (dto.Transferencia);

            if (total > saldo)
                total = saldo;

            var pago = new pago
            {
                id_orden = dto.IdOrden,
                fecha_pago = DateTime.Now,
                monto_pagado = total,
                observacion = dto.Observacion,
                anulado = false
            };

            _context.pagos.Add(pago);
            await _context.SaveChangesAsync();

            var detalles = new List<detalle_pago>();

            if ((dto.DineroEfectivo) > 0)
            {
                detalles.Add(new detalle_pago
                {
                    id_pago = pago.id_pago,
                    tipo_pago = "EFECTIVO",
                    monto = dto.DineroEfectivo,
                    anulado = false
                });
            }

            if ((dto.Transferencia) > 0)
            {
                detalles.Add(new detalle_pago
                {
                    id_pago = pago.id_pago,
                    tipo_pago = "TRANSFERENCIA",
                    monto = dto.Transferencia,
                    anulado = false
                });
            }

            if (detalles.Any())
            {
                _context.detalle_pagos.AddRange(detalles);
            }

            orden.total_pagado = (orden.total_pagado ?? 0) + total;
            orden.saldo_pendiente = (orden.total) - (orden.total_pagado ?? 0);
            orden.estado_pago = orden.saldo_pendiente <= 0 ? "PAGADO" : "PENDIENTE";

            await _context.SaveChangesAsync();

            dto.IdPago = pago.id_pago;
            dto.MontoPagado = total;
            dto.FechaPago = pago.fecha_pago;
            dto.DetallePagos = detalles.Select(d => new DetallePagoDto
            {
                IdDetallePago = d.id_detalle_pago,
                IdPago = pago.id_pago,
                TipoPago = d.tipo_pago,
                Monto = (decimal)d.monto,
                Anulado = d.anulado
            }).ToList();

            return dto;
        }

        public async Task<List<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro)
        {
            var query = _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filtro.NumeroOrden))
                query = query.Where(o => o.numero_orden.Contains(filtro.NumeroOrden));

            if (!string.IsNullOrEmpty(filtro.Cedula))
                query = query.Where(o => o.id_pacienteNavigation.cedula_paciente.Contains(filtro.Cedula));

            if (!string.IsNullOrEmpty(filtro.NombrePaciente))
                query = query.Where(o => o.id_pacienteNavigation.nombre_paciente.Contains(filtro.NombrePaciente));

            if (filtro.FechaInicio.HasValue)
                query = query.Where(o => o.fecha_orden >= DateOnly.FromDateTime(filtro.FechaInicio.Value.Date));

            if (filtro.FechaFin.HasValue)
                query = query.Where(o => o.fecha_orden <= DateOnly.FromDateTime(filtro.FechaFin.Value.Date));

            if (filtro.IncluirAnulados.HasValue)
                query = query.Where(o => o.anulado == filtro.IncluirAnulados);
            else
                query = query.Where(o => o.anulado == false || o.anulado == null);

            if (!string.IsNullOrEmpty(filtro.EstadoPago))
                query = query.Where(o => o.estado_pago == filtro.EstadoPago);

            if (string.IsNullOrEmpty(filtro.EstadoPago) || filtro.EstadoPago == "PENDIENTE")
                query = query.Where(o => o.saldo_pendiente > 0);

            var ordenes = await query
                .OrderBy(o => o.id_pacienteNavigation.nombre_paciente)
                .ThenBy(o => o.fecha_orden)
                .ToListAsync();

            return ordenes.Select(o => new OrdenDto
            {
                IdOrden = o.id_orden,
                NumeroOrden = o.numero_orden,
                CedulaPaciente = o.id_pacienteNavigation?.cedula_paciente ?? "",
                NombrePaciente = o.id_pacienteNavigation?.nombre_paciente ?? "",
                Total = o.total,
                TotalPagado = o.total_pagado ?? 0,
                SaldoPendiente = o.saldo_pendiente ?? 0,
                EstadoPago = o.estado_pago,
                FechaOrden = o.fecha_orden,
                Anulado = (bool)o.anulado
            }).ToList();
        }

    }
}
