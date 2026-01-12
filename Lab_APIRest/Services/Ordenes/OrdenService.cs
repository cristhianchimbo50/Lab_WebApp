using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Services.PDF;
using Lab_APIRest.Infrastructure.Services;
using Lab_Contracts.Common;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Pacientes;
using Lab_Contracts.Dashboard;
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

        private static OrdenDto MapOrden(Orden entidadOrden) => new()
        {
            IdOrden = entidadOrden.IdOrden,
            NumeroOrden = entidadOrden.NumeroOrden,
            IdPaciente = entidadOrden.IdPaciente,
            CedulaPaciente = entidadOrden.IdPacienteNavigation?.CedulaPaciente,
            NombrePaciente = entidadOrden.IdPacienteNavigation?.NombrePaciente,
            FechaOrden = entidadOrden.FechaOrden,
            Total = entidadOrden.Total,
            TotalPagado = entidadOrden.TotalPagado ?? 0m,
            SaldoPendiente = entidadOrden.SaldoPendiente ?? 0m,
            EstadoPago = entidadOrden.EstadoPago,
            Anulado = !entidadOrden.Activo,
            IdConvenio = entidadOrden.IdConvenio != null,
            IdMedico = entidadOrden.IdMedico,
            Observacion = entidadOrden.Observacion ?? string.Empty,
            Detalles = entidadOrden.DetalleOrden.Select(d => new DetalleOrdenDto
            {
                IdDetalleOrden = 0,
                IdOrden = d.IdOrden,
                Precio = d.Precio ?? 0m
            }).ToList()
        };

        public async Task<List<object>> ListarOrdenesAsync()
        {
            var lista = await _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .Select(entidadOrden => new
                {
                    IdOrden = entidadOrden.IdOrden,
                    NumeroOrden = entidadOrden.NumeroOrden,
                    CedulaPaciente = entidadOrden.IdPacienteNavigation!.CedulaPaciente,
                    NombrePaciente = entidadOrden.IdPacienteNavigation!.NombrePaciente,
                    FechaOrden = entidadOrden.FechaOrden,
                    Total = entidadOrden.Total,
                    TotalPagado = entidadOrden.TotalPagado ?? 0m,
                    SaldoPendiente = entidadOrden.SaldoPendiente ?? 0m,
                    EstadoPago = entidadOrden.EstadoPago,
                    Anulado = !entidadOrden.Activo
                })
                .OrderByDescending(x => x.IdOrden)
                .ToListAsync();

            return lista.Cast<object>().ToList();
        }

        public async Task<ResultadoPaginadoDto<OrdenDto>> ListarOrdenesPaginadosAsync(OrdenFiltroDto filtro)
        {
            var query = _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (filtro.FechaDesde.HasValue)
                query = query.Where(o => o.FechaOrden >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue)
                query = query.Where(o => o.FechaOrden <= filtro.FechaHasta.Value);

            if (!(filtro.IncluirAnuladas && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnuladas && !filtro.IncluirVigentes)
                    query = query.Where(o => o.Activo == false);
                else if (!filtro.IncluirAnuladas && filtro.IncluirVigentes)
                    query = query.Where(o => o.Activo == true);
            }

            if (!string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "numero": query = query.Where(o => (o.NumeroOrden ?? "").ToLower().Contains(val)); break;
                    case "cedula": query = query.Where(o => (o.IdPacienteNavigation!.CedulaPaciente ?? "").ToLower().Contains(val)); break;
                    case "nombre": query = query.Where(o => (o.IdPacienteNavigation!.NombrePaciente ?? "").ToLower().Contains(val)); break;
                    case "estadoPago": query = query.Where(o => (o.EstadoPago ?? "").ToLower().Contains(val)); break;
                }
            }

            if (filtro.IdPaciente.HasValue)
                query = query.Where(o => o.IdPaciente == filtro.IdPaciente.Value);

            var totalCount = await query.CountAsync();
            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(OrdenDto.NumeroOrden) => asc ? query.OrderBy(o => o.NumeroOrden) : query.OrderByDescending(o => o.NumeroOrden),
                nameof(OrdenDto.CedulaPaciente) => asc ? query.OrderBy(o => o.IdPacienteNavigation!.CedulaPaciente) : query.OrderByDescending(o => o.IdPacienteNavigation!.CedulaPaciente),
                nameof(OrdenDto.NombrePaciente) => asc ? query.OrderBy(o => o.IdPacienteNavigation!.NombrePaciente) : query.OrderByDescending(o => o.IdPacienteNavigation!.NombrePaciente),
                nameof(OrdenDto.FechaOrden) => asc ? query.OrderBy(o => o.FechaOrden) : query.OrderByDescending(o => o.FechaOrden),
                nameof(OrdenDto.Total) => asc ? query.OrderBy(o => o.Total) : query.OrderByDescending(o => o.Total),
                nameof(OrdenDto.TotalPagado) => asc ? query.OrderBy(o => o.TotalPagado) : query.OrderByDescending(o => o.TotalPagado),
                nameof(OrdenDto.SaldoPendiente) => asc ? query.OrderBy(o => o.SaldoPendiente) : query.OrderByDescending(o => o.SaldoPendiente),
                _ => query.OrderByDescending(o => o.IdOrden)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(o => o.IdPacienteNavigation)
                .Select(entidadOrden => MapOrden(entidadOrden))
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
            var entidadOrden = await _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .Include(o => o.IdMedicoNavigation)
                .Include(o => o.DetalleOrden).ThenInclude(d => d.IdExamenNavigation)
                .FirstOrDefaultAsync(o => o.IdOrden == idOrden);
            if (entidadOrden == null) return null;

            var resultadosExamen = await _context.DetalleResultado
                .Where(d => d.IdResultadoNavigation.IdOrden == idOrden && d.IdResultadoNavigation.Activo)
                .Select(d => new
                {
                    d.IdExamen,
                    d.IdResultadoNavigation.IdResultado,
                    d.IdResultadoNavigation.NumeroResultado,
                    d.IdResultadoNavigation.FechaResultado,
                    d.IdResultadoNavigation.EstadoResultado
                })
                .ToListAsync();
            var resultadoPorExamen = resultadosExamen
                .GroupBy(x => x.IdExamen)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.FechaResultado).First());

            return new OrdenDetalleDto
            {
                IdOrden = entidadOrden.IdOrden,
                NumeroOrden = entidadOrden.NumeroOrden,
                FechaOrden = entidadOrden.FechaOrden,
                EstadoPago = entidadOrden.EstadoPago,
                IdPaciente = entidadOrden.IdPaciente ?? 0,
                CedulaPaciente = entidadOrden.IdPacienteNavigation?.CedulaPaciente,
                NombrePaciente = entidadOrden.IdPacienteNavigation?.NombrePaciente,
                DireccionPaciente = entidadOrden.IdPacienteNavigation?.DireccionPaciente,
                CorreoPaciente = entidadOrden.IdPacienteNavigation?.CorreoElectronicoPaciente,
                TelefonoPaciente = entidadOrden.IdPacienteNavigation?.TelefonoPaciente,
                IdMedico = entidadOrden.IdMedico,
                NombreMedico = entidadOrden.IdMedicoNavigation?.NombreMedico,
                Anulado = !entidadOrden.Activo,
                Examenes = entidadOrden.DetalleOrden.Select(d =>
                {
                    resultadoPorExamen.TryGetValue(d.IdExamen, out var info);
                    return new ExamenDetalleDto
                    {
                        IdExamen = d.IdExamen,
                        NombreExamen = d.IdExamenNavigation!.NombreExamen ?? string.Empty,
                        NombreEstudio = d.IdExamenNavigation!.Estudio,
                        IdResultado = info?.IdResultado,
                        NumeroResultado = info?.NumeroResultado,
                        EstadoResultado = info?.EstadoResultado ?? "REVISION"
                    };
                }).ToList()
            };
        }

        public async Task<bool> AnularOrdenAsync(int idOrden)
        {
            var entidadOrden = await _context.Orden.FirstOrDefaultAsync(o => o.IdOrden == idOrden);
            if (entidadOrden == null) return false;
            if (!entidadOrden.Activo) return true;
            entidadOrden.Activo = false;
            entidadOrden.FechaFin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OrdenRespuestaDto?> GuardarOrdenAsync(OrdenCompletaDto datosOrden)
        {
            var ultima = await _context.Orden.OrderByDescending(o => o.IdOrden).FirstOrDefaultAsync();
            int siguiente = 1;
            if (ultima != null && !string.IsNullOrEmpty(ultima.NumeroOrden))
            {
                var partes = ultima.NumeroOrden.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimo)) siguiente = ultimo + 1;
            }
            string numeroOrden = $"ORD-{siguiente:D5}";
            var dto = datosOrden.Orden;
            var entidadOrden = new Orden
            {
                IdPaciente = dto.IdPaciente,
                FechaOrden = dto.FechaOrden,
                IdMedico = dto.IdMedico,
                Observacion = dto.Observacion,
                EstadoPago = dto.EstadoPago,
                Activo = true,
                NumeroOrden = numeroOrden,
                Total = dto.Total,
                TotalPagado = dto.TotalPagado,
                SaldoPendiente = dto.SaldoPendiente,
                DetalleOrden = dto.Detalles.Select(d => new DetalleOrden { IdExamen = d.IdExamen, Precio = d.Precio }).ToList()
            };
            _context.Orden.Add(entidadOrden);
            await _context.SaveChangesAsync();
            return new OrdenRespuestaDto { IdOrden = entidadOrden.IdOrden, NumeroOrden = entidadOrden.NumeroOrden };
        }

        public async Task<byte[]?> GenerarOrdenTicketPdfAsync(int idOrden)
        {
            var entidadOrden = await _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .Include(o => o.IdMedicoNavigation)
                .Include(o => o.DetalleOrden).ThenInclude(d => d.IdExamenNavigation)
                .FirstOrDefaultAsync(o => o.IdOrden == idOrden);
            if (entidadOrden == null) return null;
            int edadPaciente = 0;
            if (entidadOrden.IdPacienteNavigation?.FechaNacPaciente is DateOnly fechaNacimiento)
            {
                var hoy = DateTime.Today;
                var fechaNac = fechaNacimiento.ToDateTime(TimeOnly.MinValue);
                edadPaciente = hoy.Year - fechaNac.Year;
                if (fechaNac > hoy.AddYears(-edadPaciente)) edadPaciente--;
            }
            var ticket = new OrdenTicketDto
            {
                NumeroOrden = entidadOrden.NumeroOrden,
                FechaOrden = entidadOrden.FechaOrden.ToDateTime(TimeOnly.MinValue),
                NombrePaciente = entidadOrden.IdPacienteNavigation?.NombrePaciente ?? "(Sin nombre)",
                CedulaPaciente = entidadOrden.IdPacienteNavigation?.CedulaPaciente ?? "(Sin cédula)",
                EdadPaciente = edadPaciente,
                NombreMedico = entidadOrden.IdMedicoNavigation?.NombreMedico ?? "(Sin médico)",
                Total = entidadOrden.Total,
                TotalPagado = entidadOrden.TotalPagado ?? 0m,
                SaldoPendiente = entidadOrden.SaldoPendiente ?? 0m,
                TipoPago = entidadOrden.EstadoPago ?? "Desconocido",
                Examenes = entidadOrden.DetalleOrden.Select(d => new ExamenTicketDto { NombreExamen = d.IdExamenNavigation?.NombreExamen ?? "(Sin examen)", Precio = d.Precio ?? 0m }).ToList()
            };
            return _pdfTicketService.GenerarTicketOrden(ticket);
        }

        public async Task<bool> AnularOrdenCompletaAsync(int idOrden)
        {
            return await AnularOrdenAsync(idOrden);
        }

        public async Task<List<object>> ListarOrdenesPorPacienteAsync(int idPaciente)
        {
            var lista = await _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .Where(o => o.IdPaciente == idPaciente)
                .Select(entidadOrden => new
                {
                    IdOrden = entidadOrden.IdOrden,
                    NumeroOrden = entidadOrden.NumeroOrden,
                    FechaOrden = entidadOrden.FechaOrden,
                    Total = entidadOrden.Total,
                    TotalPagado = entidadOrden.TotalPagado ?? 0m,
                    SaldoPendiente = entidadOrden.SaldoPendiente ?? 0m,
                    EstadoPago = entidadOrden.EstadoPago,
                    Anulado = !entidadOrden.Activo
                })
                .OrderByDescending(x => x.IdOrden)
                .ToListAsync();
            return lista.Cast<object>().ToList();
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenPacienteAsync(int idOrden) => await ObtenerDetalleOrdenAsync(idOrden);

        public async Task<PacienteDashboardDto> ObtenerDashboardPacienteAsync(int idPaciente)
        {
            var ordenesPaciente = _context.Orden
                .Where(o => o.IdPaciente == idPaciente && o.Activo);

            var ordenesActivas = await ordenesPaciente.CountAsync();
            var ultimaOrden = await ordenesPaciente
                .OrderByDescending(o => o.FechaOrden)
                .Select(o => new { o.FechaOrden, o.NumeroOrden })
                .FirstOrDefaultAsync();

            var resultadosDisponibles = await _context.Resultado
                .Include(r => r.IdOrdenNavigation)
                .Where(r => r.Activo && r.IdOrdenNavigation != null && r.IdOrdenNavigation.IdPaciente == idPaciente)
                .CountAsync();

            return new PacienteDashboardDto
            {
                OrdenesActivas = ordenesActivas,
                ResultadosDisponibles = resultadosDisponibles,
                FechaUltimaOrden = ultimaOrden?.FechaOrden,
                NumeroUltimaOrden = ultimaOrden?.NumeroOrden
            };
        }

        public async Task<LaboratoristaHomeDto> ObtenerDashboardLaboratoristaAsync()
        {
            const decimal stockMinimo = 5m;
            var resumen = new LaboratoristaDashboardDto
            {
                OrdenesPendientes = await _context.Orden.CountAsync(o => o.Activo),
                ResultadosPorRegistrar = await _context.Orden
                    .Where(o => o.Activo && !_context.Resultado.Any(r => r.IdOrden == o.IdOrden && r.Activo))
                    .CountAsync(),
                ReactivosStockBajo = await _context.Reactivo
                    .Where(r => r.Activo && (r.CantidadDisponible ?? 0m) < stockMinimo)
                    .CountAsync()
            };

            var ordenesRecientes = await _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .Where(o => o.Activo)
                .OrderByDescending(o => o.FechaOrden)
                .Take(5)
                .Select(o => new LaboratoristaOrdenRecienteDto
                {
                    IdOrden = o.IdOrden,
                    NumeroOrden = o.NumeroOrden ?? string.Empty,
                    NombrePaciente = o.IdPacienteNavigation!.NombrePaciente,
                    EstadoOrden = o.EstadoPago ?? ""
                })
                .ToListAsync();

            return new LaboratoristaHomeDto
            {
                Resumen = resumen,
                OrdenesRecientes = ordenesRecientes
            };
        }

        public async Task VerificarYNotificarResultadosCompletosAsync(int idOrden)
        {
            var orden = await _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .FirstOrDefaultAsync(o => o.IdOrden == idOrden);
            if (orden == null) return;

            var examenesOrden = await _context.DetalleOrden
                .Where(d => d.IdOrden == idOrden)
                .Select(d => d.IdExamen)
                .Distinct()
                .ToListAsync();

            if (!examenesOrden.Any()) return;

            var examenesConResultado = await _context.Resultado
                .Where(r => r.IdOrden == idOrden && r.Activo)
                .SelectMany(r => r.DetalleResultado)
                .Select(dr => dr.IdExamen)
                .Distinct()
                .ToListAsync();

            bool completos = examenesOrden.All(idEx => examenesConResultado.Contains(idEx));
            if (!completos || _ordenesNotificadas.ContainsKey(idOrden)) return;

            var correo = orden.IdPacienteNavigation?.CorreoElectronicoPaciente;
            var nombre = orden.IdPacienteNavigation?.NombrePaciente;
            if (string.IsNullOrWhiteSpace(correo)) return;

            string asunto = "Resultados disponibles - Laboratorio La Inmaculada";
            string cuerpo = $@"<div style='font-family:Arial,sans-serif;color:#333;'><h3>Estimado/a {nombre},</h3><p>Le informamos que todos los resultados de su orden <strong>{orden.NumeroOrden}</strong> están disponibles.</p><p>Puede consultarlos ingresando a su cuenta.</p><p style='margin-top:20px;'>Gracias por confiar en nosotros.<br><strong>Laboratorio Clínico La Inmaculada</strong></p></div>";

            var emailService = new EmailService(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
            await emailService.EnviarCorreoAsync(correo, nombre ?? "Paciente", asunto, cuerpo);
            _ordenesNotificadas.TryAdd(idOrden, true);
        }
    }
}
