using Lab_APIRest.Infrastructure.EF.Models;
using Lab_Contracts.Ordenes;
using Lab_APIRest.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Lab_APIRest.Services.Ordenes
{
    public class OrdenService : IOrdenService
    {
        private readonly LabDbContext _context;

        public OrdenService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrdenDto>> ListarOrdenesAsync()
        {
            var ordenes = await _context.ordens
                .Include(o => o.detalle_ordens)
                .OrderByDescending(o => o.id_orden)
                .Select(o => new OrdenDto
                {
                    IdOrden = o.id_orden,
                    NumeroOrden = o.numero_orden,
                    IdPaciente = o.id_paciente ?? 0,
                    FechaOrden = o.fecha_orden,
                    Total = o.total,
                    SaldoPendiente = o.saldo_pendiente ?? 0,
                    TotalPagado = o.total_pagado ?? 0,
                    EstadoPago = o.estado_pago,
                    Anulado = o.anulado ?? false,
                    LiquidadoConvenio = o.liquidado_convenio ?? false,
                    IdMedico = o.id_medico,
                    Observacion = o.observacion,
                    Detalles = o.detalle_ordens.Select(d => new DetalleOrdenDto
                    {
                        IdDetalleOrden = d.id_detalle_orden,
                        IdOrden = d.id_orden ?? 0,
                        IdExamen = d.id_examen ?? 0,
                        Precio = d.precio ?? 0,
                        IdResultado = d.id_resultado
                    }).ToList()
                })
                .ToListAsync();

            return ordenes;
        }

        public async Task<OrdenDto?> ObtenerOrdenPorIdAsync(int idOrden)
        {
            var o = await _context.ordens
                .Include(x => x.detalle_ordens)
                .FirstOrDefaultAsync(x => x.id_orden == idOrden);

            if (o == null) return null;

            return new OrdenDto
            {
                IdOrden = o.id_orden,
                NumeroOrden = o.numero_orden,
                IdPaciente = (int)o.id_paciente,
                FechaOrden = o.fecha_orden,
                Total = o.total,
                SaldoPendiente = (decimal)o.saldo_pendiente,
                TotalPagado = (decimal)o.total_pagado,
                EstadoPago = o.estado_pago,
                Anulado = (bool)o.anulado,
                LiquidadoConvenio = (bool)o.liquidado_convenio,
                IdMedico = o.id_medico,
                Observacion = o.observacion,
                Detalles = o.detalle_ordens.Select(d => new DetalleOrdenDto
                {
                    IdDetalleOrden = d.id_detalle_orden,
                    IdOrden = (int)d.id_orden,
                    IdExamen = (int)d.id_examen,
                    Precio = (decimal)d.precio,
                    IdResultado = d.id_resultado
                }).ToList()
            };
        }

        public async Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto dto)
        {
            var lastOrder = await _context.ordens.OrderByDescending(o => o.id_orden).FirstOrDefaultAsync();
            int nextOrderNumber = 1;
            if (lastOrder != null && !string.IsNullOrEmpty(lastOrder.numero_orden))
            {
                var parts = lastOrder.numero_orden.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out int lastNumber))
                    nextOrderNumber = lastNumber + 1;
            }
            string numeroOrden = $"ORD-{nextOrderNumber.ToString("D5")}";

            var ordenDto = dto.Orden;

            var entidad = new orden
            {
                id_paciente = ordenDto.IdPaciente,
                fecha_orden = ordenDto.FechaOrden,
                id_medico = ordenDto.IdMedico,
                observacion = ordenDto.Observacion,
                estado_pago = ordenDto.EstadoPago,
                anulado = false,
                liquidado_convenio = false,
                numero_orden = numeroOrden
            };

            entidad.detalle_ordens = ordenDto.Detalles.Select(d => new detalle_orden
            {
                id_examen = d.IdExamen,
                precio = d.Precio
            }).ToList();

            entidad.total = ordenDto.Total;
            entidad.total_pagado = ordenDto.TotalPagado;
            entidad.saldo_pendiente = ordenDto.SaldoPendiente;
            entidad.estado_pago = ordenDto.EstadoPago;

            _context.ordens.Add(entidad);
            await _context.SaveChangesAsync();

            return new OrdenRespuestaDto
            {
                IdOrden = entidad.id_orden,
                NumeroOrden = entidad.numero_orden
            };
        }
    }
}
