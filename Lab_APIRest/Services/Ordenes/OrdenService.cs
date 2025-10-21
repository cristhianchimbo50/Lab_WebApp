using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Services.PDF;
using Lab_Contracts.Ordenes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lab_APIRest.Services.Ordenes
{
    public class OrdenService : IOrdenService
    {
        private readonly LabDbContext _context;
        private readonly PdfTicketService _pdf;
        private readonly ILogger<OrdenService> _logger;

        public OrdenService(LabDbContext context, PdfTicketService pdf, ILogger<OrdenService> logger)
        {
            _context = context;
            _pdf = pdf;
            _logger = logger;
        }

        public async Task<List<OrdenDto>> ListarOrdenesAsync()
        {
            return await _context.ordens
                .Include(o => o.id_pacienteNavigation)
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
                    Observacion = o.observacion
                })
                .ToListAsync();
        }

        public async Task<OrdenDto?> ObtenerOrdenPorIdAsync(int idOrden)
        {
            var o = await _context.ordens
                .Include(x => x.detalle_ordens)
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
            using var trx = await _context.Database.BeginTransactionAsync();
            try
            {
                var last = await _context.ordens.OrderByDescending(o => o.id_orden).FirstOrDefaultAsync();
                int next = 1;
                if (last?.numero_orden != null && last.numero_orden.Contains('-'))
                {
                    var p = last.numero_orden.Split('-');
                    if (p.Length == 2 && int.TryParse(p[1], out int n)) next = n + 1;
                }

                var numOrden = $"ORD-{next:D5}";
                var o = dto.Orden;

                var entidad = new orden
                {
                    id_paciente = o.IdPaciente,
                    fecha_orden = o.FechaOrden,
                    id_medico = o.IdMedico,
                    observacion = o.Observacion,
                    estado_pago = o.EstadoPago,
                    numero_orden = numOrden,
                    total = o.Total,
                    total_pagado = o.TotalPagado,
                    saldo_pendiente = o.SaldoPendiente,
                    anulado = false,
                    liquidado_convenio = false,
                    detalle_ordens = o.Detalles.Select(d => new detalle_orden
                    {
                        id_examen = d.IdExamen,
                        precio = d.Precio
                    }).ToList()
                };

                _context.ordens.Add(entidad);
                await _context.SaveChangesAsync();
                await trx.CommitAsync();

                return new OrdenRespuestaDto
                {
                    IdOrden = entidad.id_orden,
                    NumeroOrden = entidad.numero_orden
                };
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                _logger.LogError(ex, "Error creando orden.");
                return null;
            }
        }

        public async Task<bool> AnularOrdenCompletaAsync(int idOrden)
        {
            try
            {
                var o = await _context.ordens
                    .Include(x => x.detalle_ordens)
                    .Include(x => x.resultados).ThenInclude(r => r.detalle_resultados)
                    .Include(x => x.pagos).ThenInclude(p => p.detalle_pagos)
                    .FirstOrDefaultAsync(x => x.id_orden == idOrden);

                if (o == null || o.anulado == true) return false;

                o.anulado = true;
                o.estado_pago = "ANULADO";

                foreach (var d in o.detalle_ordens) d.anulado = true;
                foreach (var r in o.resultados)
                {
                    r.anulado = true;
                    foreach (var dr in r.detalle_resultados) dr.anulado = true;
                }
                foreach (var p in o.pagos)
                {
                    p.anulado = true;
                    foreach (var dp in p.detalle_pagos) dp.anulado = true;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anulando orden.");
                return false;
            }
        }

        public async Task<byte[]?> GenerarTicketOrdenAsync(int idOrden)
        {
            var o = await _context.ordens
                .Include(x => x.id_pacienteNavigation)
                .Include(x => x.id_medicoNavigation)
                .Include(x => x.detalle_ordens).ThenInclude(d => d.id_examenNavigation)
                .FirstOrDefaultAsync(x => x.id_orden == idOrden);

            if (o == null) return null;

            int edad = 0;
            if (o.id_pacienteNavigation?.fecha_nac_paciente is DateOnly fn)
            {
                var h = DateTime.Today;
                var n = fn.ToDateTime(TimeOnly.MinValue);
                edad = h.Year - n.Year;
                if (n > h.AddYears(-edad)) edad--;
            }

            var dto = new OrdenTicketDto
            {
                NumeroOrden = o.numero_orden,
                FechaOrden = o.fecha_orden.ToDateTime(TimeOnly.MinValue),
                NombrePaciente = o.id_pacienteNavigation?.nombre_paciente ?? "",
                CedulaPaciente = o.id_pacienteNavigation?.cedula_paciente ?? "",
                EdadPaciente = edad,
                NombreMedico = o.id_medicoNavigation?.nombre_medico ?? "",
                Total = o.total,
                TotalPagado = o.total_pagado ?? 0,
                SaldoPendiente = o.saldo_pendiente ?? 0,
                TipoPago = o.estado_pago ?? "",
                Examenes = o.detalle_ordens.Select(d => new ExamenTicketDto
                {
                    NombreExamen = d.id_examenNavigation?.nombre_examen ?? "",
                    Precio = d.precio ?? 0
                }).ToList()
            };

            return _pdf.GenerarTicketOrden(dto);
        }
    }
}
