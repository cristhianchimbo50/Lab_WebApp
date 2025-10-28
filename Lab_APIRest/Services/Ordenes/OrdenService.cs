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
        private readonly PdfTicketService _pdfTicketService;

        public OrdenService(LabDbContext context, PdfTicketService pdfTicketService)
        {
            _context = context;
            _pdfTicketService = pdfTicketService;
        }

        public async Task<List<object>> GetOrdenesAsync()
        {
            var data = await _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .Select(o => new
                {
                    IdOrden = o.id_orden,
                    NumeroOrden = o.numero_orden,
                    CedulaPaciente = o.id_pacienteNavigation!.cedula_paciente,
                    NombrePaciente = o.id_pacienteNavigation!.nombre_paciente,
                    FechaOrden = o.fecha_orden,
                    Total = o.total,
                    TotalPagado = o.total_pagado ?? 0,
                    SaldoPendiente = o.saldo_pendiente ?? 0,
                    EstadoPago = o.estado_pago,
                    Anulado = o.anulado ?? false
                })
                .OrderByDescending(x => x.IdOrden)
                .ToListAsync();

            return data.Cast<object>().ToList();
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenOriginalAsync(int id)
        {
            var orden = await _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .Include(o => o.id_medicoNavigation)
                .Include(o => o.detalle_ordens)
                    .ThenInclude(d => d.id_examenNavigation)
                .Include(o => o.detalle_ordens)
                    .ThenInclude(d => d.id_resultadoNavigation)
                .FirstOrDefaultAsync(o => o.id_orden == id);

            if (orden == null)
                return null;

            var dto = new OrdenDetalleDto
            {
                IdOrden = orden.id_orden,
                NumeroOrden = orden.numero_orden,
                FechaOrden = orden.fecha_orden,
                EstadoPago = orden.estado_pago,
                IdPaciente = (int)orden.id_paciente,
                CedulaPaciente = orden.id_pacienteNavigation?.cedula_paciente,
                NombrePaciente = orden.id_pacienteNavigation?.nombre_paciente,
                DireccionPaciente = orden.id_pacienteNavigation?.direccion_paciente,
                CorreoPaciente = orden.id_pacienteNavigation?.correo_electronico_paciente,
                TelefonoPaciente = orden.id_pacienteNavigation?.telefono_paciente,
                IdMedico = orden.id_medico,
                NombreMedico = orden.id_medicoNavigation?.nombre_medico,
                Anulado = orden.anulado ?? false,
                Examenes = orden.detalle_ordens.Select(d => new ExamenDetalleDto
                {
                    IdExamen = d.id_examen ?? 0,
                    NombreExamen = d.id_examenNavigation!.nombre_examen,
                    NombreEstudio = d.id_examenNavigation!.estudio,
                    IdResultado = d.id_resultado,
                    NumeroResultado = d.id_resultadoNavigation != null ? d.id_resultadoNavigation.numero_resultado : null
                }).ToList()
            };

            return dto;
        }

        public async Task<bool> AnularOrdenAsync(int id)
        {
            var orden = await _context.ordens
                .Include(o => o.detalle_ordens)
                    .ThenInclude(d => d.id_resultadoNavigation)
                        .ThenInclude(r => r.detalle_resultados)
                .FirstOrDefaultAsync(o => o.id_orden == id);

            if (orden == null)
                return false;

            orden.anulado = true;

            var resultados = orden.detalle_ordens
                .Where(d => d.id_resultadoNavigation != null)
                .Select(d => d.id_resultadoNavigation!)
                .Distinct()
                .ToList();

            foreach (var r in resultados)
            {
                r.anulado = true;
                foreach (var det in r.detalle_resultados)
                    det.anulado = true;
            }

            await _context.SaveChangesAsync();
            return true;
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

        public async Task<byte[]?> ObtenerTicketPdfAsync(int id)
        {
            var orden = await _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .Include(o => o.id_medicoNavigation)
                .Include(o => o.detalle_ordens!)
                    .ThenInclude(d => d.id_examenNavigation)
                .FirstOrDefaultAsync(o => o.id_orden == id);

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

            var ordenDto = new OrdenTicketDto
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
            return _pdfTicketService.GenerarTicketOrden(ordenDto);
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

        public async Task<List<object>> GetOrdenesPorPacienteAsync(int idPaciente)
        {
            var data = await _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .Where(o => o.id_paciente == idPaciente)
                .Select(o => new
                {
                    IdOrden = o.id_orden,
                    NumeroOrden = o.numero_orden,
                    FechaOrden = o.fecha_orden,
                    Total = o.total,
                    TotalPagado = o.total_pagado ?? 0,
                    SaldoPendiente = o.saldo_pendiente ?? 0,
                    EstadoPago = o.estado_pago,
                    Anulado = o.anulado ?? false
                })
                .OrderByDescending(x => x.IdOrden)
                .ToListAsync();

            return data.Cast<object>().ToList();
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenPacienteAsync(int idOrden)
        {
            return await ObtenerDetalleOrdenOriginalAsync(idOrden);
        }


    }
}
