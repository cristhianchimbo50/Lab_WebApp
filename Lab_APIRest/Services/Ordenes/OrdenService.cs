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
using Microsoft.Extensions.Logging;

namespace Lab_APIRest.Services.Ordenes
{
    public class OrdenService : IOrdenService
    {
        private readonly LabDbContext _context;
        private readonly PdfTicketService _pdfTicketService;
        private readonly ILogger<OrdenService> _logger;
        private static readonly ConcurrentDictionary<int, bool> _ordenesNotificadas = new();

        public OrdenService(LabDbContext context, PdfTicketService pdfTicketService, ILogger<OrdenService> logger)
        {
            _context = context;
            _pdfTicketService = pdfTicketService;
            _logger = logger;
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
            EstadoOrden = entidadOrden.EstadoOrden,
            ResultadosHabilitados = entidadOrden.ResultadosHabilitados,
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
                .ToListAsync();

            var dtoItems = items.Select(MapOrden).ToList();

            return new ResultadoPaginadoDto<OrdenDto>
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = dtoItems
            };
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenAsync(int idOrden)
        {
            var entidadOrden = await _context.Orden
                .Include(o => o.IdPacienteNavigation)!.ThenInclude(p => p.IdGeneroNavigation)
                .Include(o => o.IdMedicoNavigation)
                .Include(o => o.DetalleOrden).ThenInclude(d => d.IdExamenNavigation)
                .FirstOrDefaultAsync(o => o.IdOrden == idOrden);
            if (entidadOrden == null) return null;

            var examenesOrden = entidadOrden.DetalleOrden.Select(d => d.IdExamen).Distinct().ToList();

            var composiciones = await _context.ExamenComposicion
                .Where(ec => ec.Activo && examenesOrden.Contains(ec.IdExamenPadre))
                .ToListAsync();

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
                EstadoOrden = entidadOrden.EstadoOrden,
                ResultadosHabilitados = entidadOrden.ResultadosHabilitados,
                IdPaciente = entidadOrden.IdPaciente ?? 0,
                CedulaPaciente = entidadOrden.IdPacienteNavigation?.CedulaPaciente,
                NombrePaciente = entidadOrden.IdPacienteNavigation?.NombrePaciente,
                DireccionPaciente = entidadOrden.IdPacienteNavigation?.DireccionPaciente,
                CorreoPaciente = entidadOrden.IdPacienteNavigation?.CorreoElectronicoPaciente,
                TelefonoPaciente = entidadOrden.IdPacienteNavigation?.TelefonoPaciente,
                GeneroPaciente = entidadOrden.IdPacienteNavigation?.IdGeneroNavigation?.Nombre,
                IdMedico = entidadOrden.IdMedico,
                NombreMedico = entidadOrden.IdMedicoNavigation?.NombreMedico,
                Anulado = !entidadOrden.Activo,
                Examenes = entidadOrden.DetalleOrden.Select(d =>
                {
                    resultadoPorExamen.TryGetValue(d.IdExamen, out var info);
                    if (info == null)
                    {
                        var hijosIds = composiciones
                            .Where(c => c.IdExamenPadre == d.IdExamen)
                            .Select(c => c.IdExamenHijo)
                            .ToList();
                        var infoHijo = resultadosExamen
                            .Where(r => hijosIds.Contains(r.IdExamen))
                            .OrderByDescending(r => r.FechaResultado)
                            .FirstOrDefault();
                        info = infoHijo;
                    }

                    return new ExamenDetalleDto
                    {
                        IdExamen = d.IdExamen,
                        NombreExamen = d.IdExamenNavigation!.NombreExamen ?? string.Empty,
                        NombreEstudio = d.IdExamenNavigation!.Estudio,
                        IdResultado = info?.IdResultado,
                        NumeroResultado = info?.NumeroResultado,
                        EstadoResultado = info?.EstadoResultado ?? "PENDIENTE"
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
                EstadoOrden = "EN_PROCESO",
                ResultadosHabilitados = false,
                Activo = true,
                NumeroOrden = numeroOrden,
                Total = dto.Total,
                TotalPagado = dto.TotalPagado,
                SaldoPendiente = dto.SaldoPendiente,
                DetalleOrden = dto.Detalles.Select(d => new DetalleOrden { IdExamen = d.IdExamen, Precio = d.Precio }).ToList()
            };
            _context.Orden.Add(entidadOrden);
            await _context.SaveChangesAsync();

            try
            {
                var paciente = await _context.Paciente.FirstOrDefaultAsync(p => p.IdPaciente == entidadOrden.IdPaciente);
                if (paciente != null && !string.IsNullOrWhiteSpace(paciente.CorreoElectronicoPaciente))
                {
                    string asunto = "Orden registrada - Laboratorio La Inmaculada";
                    string cuerpo = $"<div style='font-family:Arial,sans-serif;color:#333;'><h3>Estimado/a {paciente.NombrePaciente},</h3><p>Su orden <strong>{entidadOrden.NumeroOrden}</strong> ha sido registrada.</p><p>Fecha de orden: {entidadOrden.FechaOrden:dd/MM/yyyy}</p><p>Gracias por confiar en nosotros.</p><p style='margin-top:20px;'><strong>Laboratorio Clínico La Inmaculada</strong></p></div>";
                    var emailService = new EmailService(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
                    await emailService.EnviarCorreoAsync(paciente.CorreoElectronicoPaciente, paciente.NombrePaciente ?? "Paciente", asunto, cuerpo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo de orden registrada para la orden {IdOrden}", entidadOrden.IdOrden);
            }

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
                .Include(o => o.IdMedicoNavigation)
                .Where(o => o.Activo)
                .OrderByDescending(o => o.FechaOrden)
                .Take(5)
                .Select(o => new LaboratoristaOrdenRecienteDto
                {
                    IdOrden = o.IdOrden,
                    NumeroOrden = o.NumeroOrden ?? string.Empty,
                    NombrePaciente = o.IdPacienteNavigation!.NombrePaciente,
                    Medico = o.IdMedicoNavigation != null ? o.IdMedicoNavigation.NombreMedico : string.Empty,
                    EstadoPago = o.EstadoPago ?? string.Empty,
                    ListoParaEntrega = o.ResultadosHabilitados == true
                })
                .ToListAsync();

            var alertas = new List<string>();
            if (resumen.ReactivosStockBajo > 0)
                alertas.Add($"{resumen.ReactivosStockBajo} reactivo(s) con stock bajo");
            if (resumen.ResultadosPorRegistrar > 0)
                alertas.Add($"{resumen.ResultadosPorRegistrar} orden(es) con resultados pendientes por registrar");
            if (resumen.OrdenesPendientes > 0 && !alertas.Any())
                alertas.Add($"{resumen.OrdenesPendientes} orden(es) en curso");

            return new LaboratoristaHomeDto
            {
                Resumen = resumen,
                OrdenesRecientes = ordenesRecientes,
                Alertas = new LaboratoristaAlertasDto { Mensajes = alertas }
            };
        }

        private static string NormalizarEstado(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado)) return string.Empty;
            var upper = estado.ToUpperInvariant();
            var normalized = upper
                .Replace("Á", "A").Replace("É", "E").Replace("Í", "I").Replace("Ó", "O").Replace("Ú", "U")
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty);
            return normalized;
        }

        public async Task<AdminHomeDto> ObtenerDashboardAdministradorAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var resultados = await _context.Resultado
                .AsNoTracking()
                .Where(r => r.Activo)
                .Include(r => r.IdOrdenNavigation)!.ThenInclude(o => o.IdPacienteNavigation)
                .Include(r => r.DetalleResultado)!.ThenInclude(d => d.IdExamenNavigation)
                .OrderByDescending(r => r.FechaResultado)
                .ToListAsync();

            var resultadosNorm = resultados
                .Select(r => new
                {
                    Resultado = r,
                    EstadoNorm = NormalizarEstado(r.EstadoResultado)
                })
                .ToList();

            var resumen = new AdminDashboardDto
            {
                OrdenesHoy = await _context.Orden.CountAsync(o => o.Activo && o.FechaOrden == hoy),
                ResultadosPendientesAprobacion = resultadosNorm.Count(x => x.EstadoNorm == "REVISION"),
                ResultadosCorreccion = resultadosNorm.Count(x => x.EstadoNorm == "CORRECCION"),
                ReactivosCriticos = await _context.Reactivo.CountAsync(r => r.Activo && (r.CantidadDisponible ?? 0m) < 3m),
                UsuariosTotales = await _context.Usuario.CountAsync(u => u.Activo == true && u.IdRolNavigation.Nombre != "paciente")
            };

            var pendientes = resultadosNorm
                .Where(x => string.IsNullOrEmpty(x.EstadoNorm)
                    || x.EstadoNorm == "PENDIENTE"
                    || x.EstadoNorm == "REVISION"
                    || x.EstadoNorm == "CORRECCION")
                .Take(5)
                .Select(x => new AdminResultadoPendienteDto
                {
                    IdResultado = x.Resultado.IdResultado,
                    IdOrden = x.Resultado.IdOrden,
                    NumeroOrden = x.Resultado.IdOrdenNavigation!.NumeroOrden ?? string.Empty,
                    Paciente = x.Resultado.IdOrdenNavigation!.IdPacienteNavigation!.NombrePaciente ?? string.Empty,
                    TipoExamen = x.Resultado.DetalleResultado
                        .Select(d => d.IdExamenNavigation!.TituloExamen ?? d.IdExamenNavigation!.NombreExamen)
                        .FirstOrDefault() ?? string.Empty,
                    Estado = x.Resultado.EstadoResultado ?? "PENDIENTE"
                })
                .ToList();

            var alertas = new List<string>();
            if (resumen.ReactivosCriticos > 0)
                alertas.Add($"{resumen.ReactivosCriticos} reactivo(s) con stock crítico");
            if (resumen.ResultadosPendientesAprobacion > 0)
                alertas.Add($"{resumen.ResultadosPendientesAprobacion} resultado(s) in revisión");
            if (resumen.ResultadosCorreccion > 0)
                alertas.Add($"{resumen.ResultadosCorreccion} resultado(s) en corrección");
            if (!pendientes.Any())
                alertas.Add("No hay resultados pendientes de aprobación en este momento.");

            return new AdminHomeDto
            {
                Resumen = resumen,
                Pendientes = pendientes,
                Alertas = new AdminAlertasDto { Mensajes = alertas }
            };
        }

        public async Task VerificarYNotificarResultadosCompletosAsync(int idOrden)
        {
            var orden = await _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .Include(o => o.DetalleOrden)
                .FirstOrDefaultAsync(o => o.IdOrden == idOrden);
            if (orden == null) return;

            var examenesOrden = orden.DetalleOrden.Select(d => d.IdExamen).Distinct().ToList();
            if (!examenesOrden.Any()) return;

            var examenesAprobados = await _context.Resultado
                .Where(r => r.IdOrden == idOrden && r.Activo && r.EstadoResultado == "APROBADO")
                .SelectMany(r => r.DetalleResultado)
                .Select(dr => dr.IdExamen)
                .Distinct()
                .ToListAsync();

            bool todosAprobados = examenesOrden.All(examenesAprobados.Contains);
            orden.EstadoOrden = todosAprobados ? "FINALIZADO" : "EN_PROCESO";

            bool habilitar = todosAprobados && string.Equals(orden.EstadoPago, "PAGADO", StringComparison.OrdinalIgnoreCase);
            bool debeNotificar = habilitar && !orden.ResultadosHabilitados && !_ordenesNotificadas.ContainsKey(idOrden);
            orden.ResultadosHabilitados = habilitar;

            await _context.SaveChangesAsync();

            if (debeNotificar)
            {
                var correo = orden.IdPacienteNavigation?.CorreoElectronicoPaciente;
                var nombre = orden.IdPacienteNavigation?.NombrePaciente;
                if (!string.IsNullOrWhiteSpace(correo))
                {
                    string asunto = "Resultados habilitados - Laboratorio La Inmaculada";
                    string cuerpo = $@"<div style='font-family:Arial,sans-serif;color:#333;'><h3>Estimado/a {nombre},</h3><p>Su orden <strong>{orden.NumeroOrden}</strong> ha sido finalizada y los resultados ya están habilitados.</p><p>Puede consultarlos ingresando a su cuenta.</p><p style='margin-top:20px;'>Gracias por confiar en nosotros.<br><strong>Laboratorio Clínico La Inmaculada</strong></p></div>";

                    var emailService = new EmailService(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
                    await emailService.EnviarCorreoAsync(correo, nombre ?? "Paciente", asunto, cuerpo);
                    _ordenesNotificadas.TryAdd(idOrden, true);
                }
            }
        }

        public async Task<RecepcionistaHomeDto> ObtenerDashboardRecepcionistaAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var resumen = new RecepcionistaDashboardDto
            {
                OrdenesRegistradas = await _context.Orden.CountAsync(o => o.Activo && o.FechaOrden == hoy),
                CuentasPorCobrar = await _context.Orden.CountAsync(o => o.Activo && (o.SaldoPendiente ?? 0m) > 0m),
                ResultadosDisponibles = await _context.Orden.CountAsync(o => o.Activo && o.ResultadosHabilitados == true)
            };

            var ordenesRecientes = await _context.Orden
                .AsNoTracking()
                .Include(o => o.IdPacienteNavigation)
                .Include(o => o.IdMedicoNavigation)
                .Where(o => o.Activo)
                .OrderByDescending(o => o.FechaOrden)
                .Take(5)
                .Select(o => new RecepcionistaOrdenRecienteDto
                {
                    IdOrden = o.IdOrden,
                    NumeroOrden = o.NumeroOrden ?? string.Empty,
                    Paciente = o.IdPacienteNavigation != null ? o.IdPacienteNavigation.NombrePaciente : string.Empty,
                    Medico = o.IdMedicoNavigation != null ? o.IdMedicoNavigation.NombreMedico : string.Empty,
                    EstadoPago = o.EstadoPago ?? "-",
                    ListoParaEntrega = o.ResultadosHabilitados == true
                })
                .ToListAsync();

            var alertas = new List<string>();
            if (resumen.CuentasPorCobrar > 0)
                alertas.Add($"{resumen.CuentasPorCobrar} cuenta(s) por cobrar pendientes.");
            if (resumen.ResultadosDisponibles > 0)
                alertas.Add($"{resumen.ResultadosDisponibles} resultado(s) listos para entrega.");
            if (!ordenesRecientes.Any())
                alertas.Add("No hay órdenes recientes registradas.");

            return new RecepcionistaHomeDto
            {
                Resumen = resumen,
                OrdenesRecientes = ordenesRecientes,
                Alertas = new RecepcionistaAlertasDto { Mensajes = alertas }
            };
        }
    }
}
