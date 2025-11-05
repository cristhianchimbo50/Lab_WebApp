using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;


namespace Lab_APIRest.Services.Pagos
{
    public class PagoService : IPagoService
    {
        private readonly LabDbContext Contexto;

        public PagoService(LabDbContext contexto)
        {
            Contexto = contexto;
        }

        public async Task<PagoDto?> RegistrarPago(PagoDto PagoDto)
        {
            var Pago = new pago
            {
                id_orden = PagoDto.IdOrden,
                fecha_pago = PagoDto.FechaPago,
                monto_pagado = PagoDto.MontoPagado,
                observacion = PagoDto.Observacion,
                anulado = PagoDto.Anulado
            };

            Pago.detalle_pagos = PagoDto.DetallePagos.Select(d => new detalle_pago
            {
                tipo_pago = d.TipoPago,
                monto = d.Monto,
                anulado = d.Anulado
            }).ToList();

            Contexto.pagos.Add(Pago);
            await Contexto.SaveChangesAsync();

            PagoDto.IdPago = Pago.id_pago;
            var DetallesGuardados = Pago.detalle_pagos.ToList();
            for (int i = 0; i < PagoDto.DetallePagos.Count; i++)
            {
                PagoDto.DetallePagos[i].IdDetallePago = DetallesGuardados[i].id_detalle_pago;
                PagoDto.DetallePagos[i].IdPago = Pago.id_pago;
            }


            return PagoDto;
        }

        public async Task<List<PagoDto>> ListarPagosPorOrden(int IdOrden)
        {
            var Pagos = await Contexto.pagos
                .Include(p => p.detalle_pagos)
                .Where(p => p.id_orden == IdOrden)
                .ToListAsync();

            return Pagos.Select(p => new PagoDto
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

        public async Task<PagoDto?> RegistrarCobroCuentaPorCobrar(PagoDto PagoDto)
        {
            var Orden = await Contexto.ordens.FirstOrDefaultAsync(o => o.id_orden == PagoDto.IdOrden);
            if (Orden == null) return null;

            var Saldo = Orden.saldo_pendiente ?? 0;
            var Total = (PagoDto.DineroEfectivo) + (PagoDto.Transferencia);

            if (Total > Saldo)
                Total = Saldo;

            var Pago = new pago
            {
                id_orden = PagoDto.IdOrden,
                fecha_pago = DateTime.Now,
                monto_pagado = Total,
                observacion = PagoDto.Observacion,
                anulado = false
            };

            Contexto.pagos.Add(Pago);
            await Contexto.SaveChangesAsync();

            var Detalles = new List<detalle_pago>();

            if ((PagoDto.DineroEfectivo) > 0)
            {
                Detalles.Add(new detalle_pago
                {
                    id_pago = Pago.id_pago,
                    tipo_pago = "EFECTIVO",
                    monto = PagoDto.DineroEfectivo,
                    anulado = false
                });
            }

            if ((PagoDto.Transferencia) > 0)
            {
                Detalles.Add(new detalle_pago
                {
                    id_pago = Pago.id_pago,
                    tipo_pago = "TRANSFERENCIA",
                    monto = PagoDto.Transferencia,
                    anulado = false
                });
            }

            if (Detalles.Any())
            {
                Contexto.detalle_pagos.AddRange(Detalles);
            }

            Orden.total_pagado = (Orden.total_pagado ?? 0) + Total;
            Orden.saldo_pendiente = (Orden.total) - (Orden.total_pagado ?? 0);
            Orden.estado_pago = Orden.saldo_pendiente <= 0 ? "PAGADO" : "PENDIENTE";

            await Contexto.SaveChangesAsync();

            PagoDto.IdPago = Pago.id_pago;
            PagoDto.MontoPagado = Total;
            PagoDto.FechaPago = Pago.fecha_pago;
            PagoDto.DetallePagos = Detalles.Select(d => new DetallePagoDto
            {
                IdDetallePago = d.id_detalle_pago,
                IdPago = Pago.id_pago,
                TipoPago = d.tipo_pago,
                Monto = (decimal)d.monto,
                Anulado = d.anulado
            }).ToList();

            return PagoDto;
        }

        public async Task<List<OrdenDto>> ListarCuentasPorCobrar(PagoFiltroDto Filtro)
        {
            var Query = Contexto.ordens
                .Include(o => o.id_pacienteNavigation)
                .AsQueryable();

            if (!string.IsNullOrEmpty(Filtro.NumeroOrden))
                Query = Query.Where(o => o.numero_orden.Contains(Filtro.NumeroOrden));

            if (!string.IsNullOrEmpty(Filtro.Cedula))
                Query = Query.Where(o => o.id_pacienteNavigation.cedula_paciente.Contains(Filtro.Cedula));

            if (!string.IsNullOrEmpty(Filtro.NombrePaciente))
                Query = Query.Where(o => o.id_pacienteNavigation.nombre_paciente.Contains(Filtro.NombrePaciente));

            if (Filtro.FechaInicio.HasValue)
                Query = Query.Where(o => o.fecha_orden >= DateOnly.FromDateTime(Filtro.FechaInicio.Value.Date));

            if (Filtro.FechaFin.HasValue)
                Query = Query.Where(o => o.fecha_orden <= DateOnly.FromDateTime(Filtro.FechaFin.Value.Date));

            if (Filtro.IncluirAnulados.HasValue)
                Query = Query.Where(o => o.anulado == Filtro.IncluirAnulados);
            else
                Query = Query.Where(o => o.anulado == false || o.anulado == null);

            if (!string.IsNullOrEmpty(Filtro.EstadoPago))
                Query = Query.Where(o => o.estado_pago == Filtro.EstadoPago);

            if (string.IsNullOrEmpty(Filtro.EstadoPago) || Filtro.EstadoPago == "PENDIENTE")
                Query = Query.Where(o => o.saldo_pendiente > 0);

            var Ordenes = await Query
                .OrderBy(o => o.id_pacienteNavigation.nombre_paciente)
                .ThenBy(o => o.fecha_orden)
                .ToListAsync();

            return Ordenes.Select(o => new OrdenDto
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
