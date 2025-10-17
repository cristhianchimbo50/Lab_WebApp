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
                }).ToList()
            }).ToList();
        }
    }
}
