using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Services.PDF;
using Lab_Contracts.Ordenes;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Ordenes
{
    public class OrdenService : IOrdenService
    {
        private readonly LabDbContext _context;
        private readonly PdfTicketService _pdf;

        public OrdenService(LabDbContext context, PdfTicketService pdf)
        {
            _context = context;
            _pdf = pdf;
        }

        public async Task<List<OrdenDto>> ListarOrdenesAsync()
        {
            return await _context.ordens
                .Include(o => o.id_pacienteNavigation)
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
                    Observacion = o.observacion
                })
                .ToListAsync();
        }

        public async Task<OrdenDto?> ObtenerOrdenPorIdAsync(int idOrden)
        {
            var o = await _context.ordens
                .Include(x => x.detalle_ordens)
                    .ThenInclude(d => d.id_examenNavigation)
                .Include(x => x.id_pacienteNavigation)
                .Include(x => x.id_medicoNavigation)
                .FirstOrDefaultAsync(x => x.id_orden == idOrden);

            if (o == null) return null;

            return new OrdenDto
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

            string numeroOrden = $"ORD-{nextOrderNumber:D5}";
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
                numero_orden = numeroOrden,
                total = ordenDto.Total,
                total_pagado = ordenDto.TotalPagado,
                saldo_pendiente = ordenDto.SaldoPendiente,
                detalle_ordens = ordenDto.Detalles.Select(d => new detalle_orden
                {
                    id_examen = d.IdExamen,
                    precio = d.Precio
                }).ToList()
            };

            _context.ordens.Add(entidad);
            await _context.SaveChangesAsync();

            return new OrdenRespuestaDto
            {
                IdOrden = entidad.id_orden,
                NumeroOrden = entidad.numero_orden
            };
        }

        public async Task<bool> AnularOrdenSimpleAsync(int idOrden)
        {
            var orden = await _context.ordens
                .Include(o => o.detalle_ordens)
                    .ThenInclude(d => d.id_resultadoNavigation)
                        .ThenInclude(r => r.detalle_resultados)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);

            if (orden == null || orden.anulado == true)
                return false;

            orden.anulado = true;

            foreach (var d in orden.detalle_ordens)
            {
                if (d.id_resultadoNavigation != null)
                {
                    d.id_resultadoNavigation.anulado = true;
                    foreach (var det in d.id_resultadoNavigation.detalle_resultados)
                        det.anulado = true;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularOrdenCompletaAsync(int idOrden)
        {
            var orden = await _context.ordens
                .Include(o => o.detalle_ordens)
                .Include(o => o.resultados)
                    .ThenInclude(r => r.detalle_resultados)
                .Include(o => o.pagos)
                    .ThenInclude(p => p.detalle_pagos)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);

            if (orden == null || orden.anulado == true)
                return false;

            orden.anulado = true;
            orden.estado_pago = "ANULADO";

            foreach (var d in orden.detalle_ordens)
                d.anulado = true;

            foreach (var r in orden.resultados)
            {
                r.anulado = true;
                foreach (var dr in r.detalle_resultados)
                    dr.anulado = true;
            }

            foreach (var p in orden.pagos)
            {
                p.anulado = true;
                foreach (var dp in p.detalle_pagos)
                    dp.anulado = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<byte[]?> GenerarTicketOrdenAsync(int idOrden)
        {
            var orden = await _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .Include(o => o.id_medicoNavigation)
                .Include(o => o.detalle_ordens)
                    .ThenInclude(d => d.id_examenNavigation)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);

            if (orden == null)
                return null;

            int edadPaciente = 0;

            if (orden.id_pacienteNavigation?.fecha_nac_paciente is DateOnly fechaNacimiento)
            {
                var hoy = DateTime.Today;
                var fechaNac = fechaNacimiento.ToDateTime(TimeOnly.MinValue);
                edadPaciente = hoy.Year - fechaNac.Year;
                if (fechaNac > hoy.AddYears(-edadPaciente))
                    edadPaciente--;
            }

            var ticket = new OrdenTicketDto
            {
                NumeroOrden = orden.numero_orden,
                FechaOrden = orden.fecha_orden.ToDateTime(TimeOnly.MinValue),
                NombrePaciente = orden.id_pacienteNavigation?.nombre_paciente ?? "(Sin nombre)",
                CedulaPaciente = orden.id_pacienteNavigation?.cedula_paciente ?? "(Sin cédula)",
                EdadPaciente = edadPaciente,
                NombreMedico = orden.id_medicoNavigation?.nombre_medico ?? "(Sin médico)",
                Total = orden.total,
                TotalPagado = orden.total_pagado ?? 0,
                SaldoPendiente = orden.saldo_pendiente ?? 0,
                TipoPago = orden.estado_pago ?? "Desconocido",
                Examenes = orden.detalle_ordens.Select(d => new ExamenTicketDto
                {
                    NombreExamen = d.id_examenNavigation?.nombre_examen ?? "(Sin examen)",
                    Precio = d.precio ?? 0
                }).ToList()
            };

            return _pdf.GenerarTicketOrden(ticket);
        }
    }
}
