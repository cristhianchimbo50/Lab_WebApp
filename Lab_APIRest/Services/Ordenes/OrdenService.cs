using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Services.PDF;
using Lab_APIRest.Infrastructure.Services;
using Lab_Contracts.Common;
using Lab_Contracts.Ordenes;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;

namespace Lab_APIRest.Services.Ordenes
{
    public class OrdenService : IOrdenService
    {
        private readonly LabDbContext _context;
        private readonly PdfTicketService _pdfTicketService;
        private static readonly ConcurrentDictionary<int, bool> _ordenesNotificadas = new();

        public OrdenService(LabDbContext context, PdfTicketService pdfTicketService)
        {
            _context = context;
            _pdfTicketService = pdfTicketService;
        }

        public async Task<List<object>> ListarOrdenesAsync()
        {
            var lista = await _context.ordens
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

            return lista.Cast<object>().ToList();
        }

        public async Task<ResultadoPaginadoDto<OrdenDto>> ListarOrdenesPaginadosAsync(OrdenFiltroDto filtro)
        {
            var query = _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (filtro.FechaDesde.HasValue)
                query = query.Where(o => o.fecha_orden >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue)
                query = query.Where(o => o.fecha_orden <= filtro.FechaHasta.Value);

            if (!(filtro.IncluirAnuladas && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnuladas && !filtro.IncluirVigentes)
                    query = query.Where(o => o.anulado == true);
                else if (!filtro.IncluirAnuladas && filtro.IncluirVigentes)
                    query = query.Where(o => o.anulado == false || o.anulado == null);
            }

            if (!string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "numero": query = query.Where(o => (o.numero_orden ?? "").ToLower().Contains(val)); break;
                    case "cedula": query = query.Where(o => (o.id_pacienteNavigation!.cedula_paciente ?? "").ToLower().Contains(val)); break;
                    case "nombre": query = query.Where(o => (o.id_pacienteNavigation!.nombre_paciente ?? "").ToLower().Contains(val)); break;
                    case "estadoPago": query = query.Where(o => (o.estado_pago ?? "").ToLower().Contains(val)); break;
                }
            }

            if (filtro.IdPaciente.HasValue)
                query = query.Where(o => o.id_paciente == filtro.IdPaciente.Value);

            var totalCount = await query.CountAsync();
            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(OrdenDto.NumeroOrden) => asc ? query.OrderBy(o => o.numero_orden) : query.OrderByDescending(o => o.numero_orden),
                nameof(OrdenDto.CedulaPaciente) => asc ? query.OrderBy(o => o.id_pacienteNavigation!.cedula_paciente) : query.OrderByDescending(o => o.id_pacienteNavigation!.cedula_paciente),
                nameof(OrdenDto.NombrePaciente) => asc ? query.OrderBy(o => o.id_pacienteNavigation!.nombre_paciente) : query.OrderByDescending(o => o.id_pacienteNavigation!.nombre_paciente),
                nameof(OrdenDto.FechaOrden) => asc ? query.OrderBy(o => o.fecha_orden) : query.OrderByDescending(o => o.fecha_orden),
                nameof(OrdenDto.Total) => asc ? query.OrderBy(o => o.total) : query.OrderByDescending(o => o.total),
                nameof(OrdenDto.TotalPagado) => asc ? query.OrderBy(o => o.total_pagado) : query.OrderByDescending(o => o.total_pagado),
                nameof(OrdenDto.SaldoPendiente) => asc ? query.OrderBy(o => o.saldo_pendiente) : query.OrderByDescending(o => o.saldo_pendiente),
                _ => query.OrderByDescending(o => o.id_orden)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrdenDto
                {
                    IdOrden = o.id_orden,
                    NumeroOrden = o.numero_orden!,
                    CedulaPaciente = o.id_pacienteNavigation!.cedula_paciente,
                    NombrePaciente = o.id_pacienteNavigation!.nombre_paciente,
                    FechaOrden = o.fecha_orden,
                    Total = o.total,
                    TotalPagado = o.total_pagado ?? 0m,
                    SaldoPendiente = o.saldo_pendiente ?? 0m,
                    EstadoPago = o.estado_pago!,
                    Anulado = o.anulado ?? false
                })
                .ToListAsync();

            return new ResultadoPaginadoDto<OrdenDto>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenAsync(int idOrden)
        {
            var entidad = await _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .Include(o => o.id_medicoNavigation)
                .Include(o => o.detalle_ordens).ThenInclude(d => d.id_examenNavigation)
                .Include(o => o.detalle_ordens).ThenInclude(d => d.id_resultadoNavigation)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);
            if (entidad == null) return null;
            return new OrdenDetalleDto
            {
                IdOrden = entidad.id_orden,
                NumeroOrden = entidad.numero_orden,
                FechaOrden = entidad.fecha_orden,
                EstadoPago = entidad.estado_pago,
                IdPaciente = (int)entidad.id_paciente,
                CedulaPaciente = entidad.id_pacienteNavigation?.cedula_paciente,
                NombrePaciente = entidad.id_pacienteNavigation?.nombre_paciente,
                DireccionPaciente = entidad.id_pacienteNavigation?.direccion_paciente,
                CorreoPaciente = entidad.id_pacienteNavigation?.correo_electronico_paciente,
                TelefonoPaciente = entidad.id_pacienteNavigation?.telefono_paciente,
                IdMedico = entidad.id_medico,
                NombreMedico = entidad.id_medicoNavigation?.nombre_medico,
                Anulado = entidad.anulado ?? false,
                Examenes = entidad.detalle_ordens.Select(d => new ExamenDetalleDto
                {
                    IdExamen = d.id_examen ?? 0,
                    NombreExamen = d.id_examenNavigation!.nombre_examen,
                    NombreEstudio = d.id_examenNavigation!.estudio,
                    IdResultado = d.id_resultado,
                    NumeroResultado = d.id_resultadoNavigation != null ? d.id_resultadoNavigation.numero_resultado : null
                }).ToList()
            };
        }

        public async Task<bool> AnularOrdenAsync(int idOrden)
        {
            var entidad = await _context.ordens
                .Include(o => o.detalle_ordens).ThenInclude(d => d.id_resultadoNavigation).ThenInclude(r => r.detalle_resultados)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);
            if (entidad == null) return false;
            entidad.anulado = true;
            var resultados = entidad.detalle_ordens.Where(d => d.id_resultadoNavigation != null).Select(d => d.id_resultadoNavigation!).Distinct().ToList();
            foreach (var resultado in resultados)
            {
                resultado.anulado = true;
                foreach (var detalle in resultado.detalle_resultados) detalle.anulado = true;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OrdenRespuestaDto?> GuardarOrdenAsync(OrdenCompletaDto datosOrden)
        {
            var ultima = await _context.ordens.OrderByDescending(o => o.id_orden).FirstOrDefaultAsync();
            int siguiente = 1;
            if (ultima != null && !string.IsNullOrEmpty(ultima.numero_orden))
            {
                var partes = ultima.numero_orden.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimo)) siguiente = ultimo + 1;
            }
            string numeroOrden = $"ORD-{siguiente:D5}";
            var dto = datosOrden.Orden;
            var entidad = new orden
            {
                id_paciente = dto.IdPaciente,
                fecha_orden = dto.FechaOrden,
                id_medico = dto.IdMedico,
                observacion = dto.Observacion,
                estado_pago = dto.EstadoPago,
                anulado = false,
                liquidado_convenio = false,
                numero_orden = numeroOrden,
                total = dto.Total,
                total_pagado = dto.TotalPagado,
                saldo_pendiente = dto.SaldoPendiente,
                detalle_ordens = dto.Detalles.Select(d => new detalle_orden { id_examen = d.IdExamen, precio = d.Precio }).ToList()
            };
            _context.ordens.Add(entidad);
            await _context.SaveChangesAsync();
            return new OrdenRespuestaDto { IdOrden = entidad.id_orden, NumeroOrden = entidad.numero_orden };
        }

        public async Task<byte[]?> GenerarOrdenTicketPdfAsync(int idOrden)
        {
            var entidad = await _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .Include(o => o.id_medicoNavigation)
                .Include(o => o.detalle_ordens).ThenInclude(d => d.id_examenNavigation)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);
            if (entidad == null) return null;
            int edadPaciente = 0;
            if (entidad.id_pacienteNavigation?.fecha_nac_paciente is DateOnly fechaNacimiento)
            {
                var hoy = DateTime.Today;
                var fechaNac = fechaNacimiento.ToDateTime(TimeOnly.MinValue);
                edadPaciente = hoy.Year - fechaNac.Year;
                if (fechaNac > hoy.AddYears(-edadPaciente)) edadPaciente--;
            }
            var ticket = new OrdenTicketDto
            {
                NumeroOrden = entidad.numero_orden,
                FechaOrden = entidad.fecha_orden.ToDateTime(TimeOnly.MinValue),
                NombrePaciente = entidad.id_pacienteNavigation?.nombre_paciente ?? "(Sin nombre)",
                CedulaPaciente = entidad.id_pacienteNavigation?.cedula_paciente ?? "(Sin cédula)",
                EdadPaciente = edadPaciente,
                NombreMedico = entidad.id_medicoNavigation?.nombre_medico ?? "(Sin médico)",
                Total = entidad.total,
                TotalPagado = entidad.total_pagado ?? 0,
                SaldoPendiente = entidad.saldo_pendiente ?? 0,
                TipoPago = entidad.estado_pago ?? "Desconocido",
                Examenes = entidad.detalle_ordens.Select(d => new ExamenTicketDto { NombreExamen = d.id_examenNavigation?.nombre_examen ?? "(Sin examen)", Precio = d.precio ?? 0 }).ToList()
            };
            return _pdfTicketService.GenerarTicketOrden(ticket);
        }

        public async Task<bool> AnularOrdenCompletaAsync(int idOrden)
        {
            var entidad = await _context.ordens
                .Include(o => o.detalle_ordens)
                .Include(o => o.resultados).ThenInclude(r => r.detalle_resultados)
                .Include(o => o.pagos).ThenInclude(p => p.detalle_pagos)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);
            if (entidad == null || entidad.anulado == true) return false;
            entidad.anulado = true; entidad.estado_pago = "ANULADO";
            foreach (var detalle in entidad.detalle_ordens) detalle.anulado = true;
            foreach (var resultado in entidad.resultados)
            {
                resultado.anulado = true; foreach (var det in resultado.detalle_resultados) det.anulado = true;
            }
            foreach (var pago in entidad.pagos)
            {
                pago.anulado = true; foreach (var dp in pago.detalle_pagos) dp.anulado = true;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<object>> ListarOrdenesPorPacienteAsync(int idPaciente)
        {
            var lista = await _context.ordens
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
            return lista.Cast<object>().ToList();
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenPacienteAsync(int idOrden) => await ObtenerDetalleOrdenAsync(idOrden);

        public async Task VerificarYNotificarResultadosCompletosAsync(int idOrden)
        {
            var entidad = await _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .Include(o => o.detalle_ordens).ThenInclude(d => d.id_resultadoNavigation)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);
            if (entidad == null) return;
            bool completos = entidad.detalle_ordens.All(d => d.id_resultado != null);
            if (!completos || _ordenesNotificadas.ContainsKey(idOrden)) return;
            var correo = entidad.id_pacienteNavigation?.correo_electronico_paciente;
            var nombre = entidad.id_pacienteNavigation?.nombre_paciente;
            if (string.IsNullOrWhiteSpace(correo)) return;
            string asunto = "Resultados disponibles - Laboratorio La Inmaculada";
            string cuerpo = $@"<div style='font-family:Arial,sans-serif;color:#333;'><h3>Estimado/a {nombre},</h3><p>Le informamos que todos los resultados de su orden <strong>{entidad.numero_orden}</strong> están disponibles.</p><p>Puede consultarlos ingresando a su cuenta.</p><p style='margin-top:20px;'>Gracias por confiar en nosotros.<br><strong>Laboratorio Clínico La Inmaculada</strong></p></div>";
            var emailService = new EmailService(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
            await emailService.EnviarCorreoAsync(correo, nombre ?? "Paciente", asunto, cuerpo);
            _ordenesNotificadas.TryAdd(idOrden, true);
        }
    }
}
