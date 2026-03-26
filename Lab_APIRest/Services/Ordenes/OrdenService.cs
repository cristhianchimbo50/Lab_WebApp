using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using orden = Lab_APIRest.Infrastructure.EF.Models.orden;
using detalle_orden = Lab_APIRest.Infrastructure.EF.Models.detalle_orden;
using Lab_APIRest.Services.PDF;
using Lab_APIRest.Services.Email;
using Lab_Contracts.Common;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_Contracts.Pacientes;
using Lab_Contracts.Dashboard;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using pago = Lab_APIRest.Infrastructure.EF.Models.pago;
using detalle_pago = Lab_APIRest.Infrastructure.EF.Models.detalle_pago;

namespace Lab_APIRest.Services.Ordenes
{
    public class OrdenService : IOrdenService
    {
        private readonly LabDbContext _context;
        private readonly PdfTicketService _pdfTicketService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrdenService> _logger;
        private static readonly ConcurrentDictionary<int, bool> _ordenesNotificadas = new();

        private const int EstadoOrdenEnProcesoId = EstadoIds.Orden.EnProceso;
        private const int EstadoOrdenFinalizadaId = EstadoIds.Orden.Finalizada;
        private const int EstadoOrdenAnuladaId = EstadoIds.Orden.Anulada;

        private const int EstadoResultadoAprobadoId = EstadoIds.Resultado.Aprobado;
        private const int EstadoResultadoPendienteId = EstadoIds.Resultado.Pendiente;
        private const int EstadoResultadoRevisionId = EstadoIds.Resultado.EnRevision;
        private const int EstadoResultadoCorreccionId = EstadoIds.Resultado.Correccion;

        private const int EstadoPagoPendienteId = EstadoIds.Pago.Pendiente;
        private const int EstadoPagoAbonadoId = EstadoIds.Pago.Abonado;
        private const int EstadoPagoPagadoId = EstadoIds.Pago.Pagado;

        public OrdenService(
            LabDbContext context,
            PdfTicketService pdfTicketService,
            IEmailService emailService,
            ILogger<OrdenService> logger)
        {
            _context = context;
            _pdfTicketService = pdfTicketService;
            _emailService = emailService;
            _logger = logger;
        }

        private static bool EsEstadoAprobado(int? idEstadoResultado)
            => idEstadoResultado == EstadoResultadoAprobadoId;

        private static HashSet<int> CalcularExamenesRequeridos(List<int> examenesOrden, List<examen_composicion> composicionesActivas)
        {
            var padresConHijos = composicionesActivas
                .Select(c => c.id_examen_padre)
                .Distinct()
                .ToHashSet();

            var hijosRequeridos = composicionesActivas
                .Select(c => c.id_examen_hijo)
                .Distinct();

            return examenesOrden
                .Where(idExamen => !padresConHijos.Contains(idExamen))
                .Concat(hijosRequeridos)
                .Distinct()
                .ToHashSet();
        }

        private static bool EsOrdenFinalizada(orden entidadOrden)
            => entidadOrden.id_estado_orden == EstadoOrdenFinalizadaId;

        private static string ObtenerNombreEstadoOrden(int idEstadoOrden)
            => idEstadoOrden switch
            {
                EstadoOrdenEnProcesoId => "EN PROCESO",
                EstadoOrdenFinalizadaId => "FINALIZADA",
                EstadoOrdenAnuladaId => "ANULADA",
                _ => string.Empty
            };

        private static OrdenDto MapOrden(orden entidadOrden) => new()
        {
            IdOrden = entidadOrden.id_orden,
            NumeroOrden = entidadOrden.numero_orden,
            IdPaciente = entidadOrden.id_paciente,
            CedulaPaciente = entidadOrden.paciente_navigation?.persona_navigation?.cedula,
            NombrePaciente = $"{entidadOrden.paciente_navigation?.persona_navigation?.nombres} {entidadOrden.paciente_navigation?.persona_navigation?.apellidos}",
            FechaOrden = entidadOrden.fecha_orden,
            Total = entidadOrden.total,
            SaldoPendiente = entidadOrden.saldo_pendiente ?? 0m,
            TotalPagado = entidadOrden.total - (entidadOrden.saldo_pendiente ?? 0m),
            IdEstadoPago = entidadOrden.id_estado_pago,
            EstadoPago = entidadOrden.estado_pago_navigation?.nombre,
            NombreEstadoPago = entidadOrden.estado_pago_navigation?.nombre,
            IdEstadoOrden = entidadOrden.id_estado_orden,
            EstadoOrden = ObtenerNombreEstadoOrden(entidadOrden.id_estado_orden),
            ResultadosHabilitados = EsOrdenFinalizada(entidadOrden),
            Anulado = !entidadOrden.activo,
            IdConvenio = entidadOrden.id_convenio != null,
            IdMedico = entidadOrden.id_medico,
            Observacion = entidadOrden.observacion ?? string.Empty,
            Detalles = entidadOrden.detalle_orden.Select(d => new DetalleOrdenDto
            {
                IdDetalleOrden = 0,
                IdOrden = d.id_orden,
                Precio = d.precio ?? 0m
            }).ToList()
        };

        public async Task<List<object>> ListarOrdenesAsync()
        {
            var lista = await _context.Orden
                .Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(o => o.estado_orden_navigation)
                .Include(o => o.estado_pago_navigation)
                .Select(entidadOrden => new
                {
                    IdOrden = entidadOrden.id_orden,
                    NumeroOrden = entidadOrden.numero_orden,
                    CedulaPaciente = entidadOrden.paciente_navigation!.persona_navigation!.cedula,
                    NombrePaciente = $"{entidadOrden.paciente_navigation!.persona_navigation!.nombres} {entidadOrden.paciente_navigation!.persona_navigation!.apellidos}",
                    FechaOrden = entidadOrden.fecha_orden,
                    Total = entidadOrden.total,
                    TotalPagado = entidadOrden.total - (entidadOrden.saldo_pendiente ?? 0m),
                    SaldoPendiente = entidadOrden.saldo_pendiente ?? 0m,
                    EstadoPago = entidadOrden.estado_pago_navigation != null ? entidadOrden.estado_pago_navigation.nombre : null,
                    IdEstadoPago = entidadOrden.id_estado_pago,
                    Anulado = !entidadOrden.activo
                })
                .OrderByDescending(x => x.IdOrden)
                .ToListAsync();

            return lista.Cast<object>().ToList();
        }

        public async Task<ResultadoPaginadoDto<OrdenDto>> ListarOrdenesPaginadosAsync(OrdenFiltroDto filtro)
        {
            var query = _context.Orden
                .Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(o => o.estado_orden_navigation)
                .Include(o => o.estado_pago_navigation)
                .AsNoTracking()
                .AsQueryable();

            if (filtro.FechaDesde.HasValue)
                query = query.Where(o => o.fecha_orden >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue)
                query = query.Where(o => o.fecha_orden <= filtro.FechaHasta.Value);

            if (!(filtro.IncluirAnuladas && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnuladas && !filtro.IncluirVigentes)
                    query = query.Where(o => o.activo == false);
                else if (!filtro.IncluirAnuladas && filtro.IncluirVigentes)
                    query = query.Where(o => o.activo == true);
            }

            if (!string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "numero": query = query.Where(o => (o.numero_orden ?? "").ToLower().Contains(val)); break;
                    case "cedula": query = query.Where(o => (o.paciente_navigation!.persona_navigation!.cedula ?? "").ToLower().Contains(val)); break;
                    case "nombre": query = query.Where(o => ((o.paciente_navigation!.persona_navigation!.nombres + " " + o.paciente_navigation!.persona_navigation!.apellidos) ?? "").ToLower().Contains(val)); break;
                    case "estadoPago":
                        int? estadoId = val.ToUpper() switch
                        {
                            "PENDIENTE" => EstadoPagoPendienteId,
                            "ABONADO" => EstadoPagoAbonadoId,
                            "PAGADO" => EstadoPagoPagadoId,
                            _ => null
                        };
                        if (estadoId.HasValue) query = query.Where(o => o.id_estado_pago == estadoId.Value);
                        break;
                }
            }

            if (filtro.IdPaciente.HasValue)
                query = query.Where(o => o.id_paciente == filtro.IdPaciente.Value);

            var totalCount = await query.CountAsync();
            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(OrdenDto.NumeroOrden) => asc ? query.OrderBy(o => o.numero_orden) : query.OrderByDescending(o => o.numero_orden),
                nameof(OrdenDto.CedulaPaciente) => asc ? query.OrderBy(o => o.paciente_navigation!.persona_navigation!.cedula) : query.OrderByDescending(o => o.paciente_navigation!.persona_navigation!.cedula),
                nameof(OrdenDto.NombrePaciente) => asc ? query.OrderBy(o => o.paciente_navigation!.persona_navigation!.nombres) : query.OrderByDescending(o => o.paciente_navigation!.persona_navigation!.nombres),
                nameof(OrdenDto.FechaOrden) => asc ? query.OrderBy(o => o.fecha_orden) : query.OrderByDescending(o => o.fecha_orden),
                nameof(OrdenDto.Total) => asc ? query.OrderBy(o => o.total) : query.OrderByDescending(o => o.total),
                nameof(OrdenDto.TotalPagado) => asc ? query.OrderBy(o => o.total - (o.saldo_pendiente ?? 0m)) : query.OrderByDescending(o => o.total - (o.saldo_pendiente ?? 0m)),
                nameof(OrdenDto.SaldoPendiente) => asc ? query.OrderBy(o => o.saldo_pendiente) : query.OrderByDescending(o => o.saldo_pendiente),
                _ => query.OrderByDescending(o => o.id_orden)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await query.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(o => o.paciente_navigation)
                .Include(o => o.estado_orden_navigation)
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
                .Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)!.ThenInclude(per => per.genero_navigation)
                .Include(o => o.medico_navigation)
                .Include(o => o.estado_orden_navigation)
                .Include(o => o.estado_pago_navigation)
                .Include(o => o.detalle_orden)!.ThenInclude(d => d.examen_navigation)!.ThenInclude(e => e.estudio_navigation)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);
            if (entidadOrden == null) return null;

            var examenesOrden = entidadOrden.detalle_orden.Select(d => d.id_examen).Distinct().ToList();

            var composiciones = await _context.ExamenComposicion
                .Where(ec => ec.activo && examenesOrden.Contains(ec.id_examen_padre))
                .ToListAsync();

            var hijosPorPadre = composiciones
                .GroupBy(c => c.id_examen_padre)
                .ToDictionary(g => g.Key, g => g.Select(x => x.id_examen_hijo).Distinct().ToList());

            var resultadosExamen = await _context.DetalleResultado
                .Where(d => d.resultado_navigation.id_orden == idOrden && d.resultado_navigation.activo)
                .Select(d => new
                {
                    d.id_examen,
                    d.resultado_navigation.id_resultado,
                    d.resultado_navigation.id_estado_resultado,
                    d.resultado_navigation.numero_resultado,
                    d.resultado_navigation.fecha_resultado,
                    Estado = d.resultado_navigation.estado_resultado_navigation.nombre
                })
                .ToListAsync();

            var resultadoPorExamen = resultadosExamen
                .GroupBy(x => x.id_examen)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.fecha_resultado).First());

            if (entidadOrden.id_estado_orden != EstadoOrdenAnuladaId)
            {
                var examenesRequeridos = CalcularExamenesRequeridos(examenesOrden, composiciones);

                var examenesAprobados = resultadosExamen
                    .Where(x => EsEstadoAprobado(x.id_estado_resultado))
                    .Select(x => x.id_examen)
                    .Distinct()
                    .ToHashSet();

                var todosAprobados = examenesRequeridos.Any() && examenesRequeridos.All(examenesAprobados.Contains);
                var pagada = entidadOrden.id_estado_pago == EstadoPagoPagadoId;

                var estadoEsperado = (todosAprobados && pagada)
                    ? EstadoOrdenFinalizadaId
                    : EstadoOrdenEnProcesoId;

                if (entidadOrden.id_estado_orden != estadoEsperado)
                {
                    entidadOrden.id_estado_orden = estadoEsperado;
                    entidadOrden.fecha_completado = estadoEsperado == EstadoOrdenFinalizadaId
                        ? (entidadOrden.fecha_completado ?? DateTime.UtcNow)
                        : null;
                    await _context.SaveChangesAsync();
                    await _context.Entry(entidadOrden).Reference(o => o.estado_orden_navigation).LoadAsync();
                }
            }

            return new OrdenDetalleDto
            {
                IdOrden = entidadOrden.id_orden,
                NumeroOrden = entidadOrden.numero_orden,
                FechaOrden = entidadOrden.fecha_orden,
                IdEstadoPago = entidadOrden.id_estado_pago,
                EstadoPago = entidadOrden.estado_pago_navigation?.nombre,
                IdEstadoOrden = entidadOrden.id_estado_orden,
                EstadoOrden = ObtenerNombreEstadoOrden(entidadOrden.id_estado_orden),
                ResultadosHabilitados = EsOrdenFinalizada(entidadOrden),
                IdPaciente = entidadOrden.id_paciente ?? 0,
                CedulaPaciente = entidadOrden.paciente_navigation?.persona_navigation?.cedula,
                NombrePaciente = $"{entidadOrden.paciente_navigation?.persona_navigation?.nombres} {entidadOrden.paciente_navigation?.persona_navigation?.apellidos}",
                DireccionPaciente = entidadOrden.paciente_navigation?.persona_navigation?.direccion,
                CorreoPaciente = await _context.Usuario
                    .Where(u => u.id_persona == entidadOrden.paciente_navigation!.id_persona && u.activo == true)
                    .Select(u => u.correo)
                    .FirstOrDefaultAsync(),
                TelefonoPaciente = entidadOrden.paciente_navigation?.persona_navigation?.telefono,
                GeneroPaciente = entidadOrden.paciente_navigation?.persona_navigation?.genero_navigation?.nombre,
                IdMedico = entidadOrden.id_medico,
                NombreMedico = entidadOrden.medico_navigation?.nombre_medico,
                Anulado = !entidadOrden.activo,
                Examenes = entidadOrden.detalle_orden.Select(d =>
                {
                    resultadoPorExamen.TryGetValue(d.id_examen, out var info);

                    int? idResultado = info?.id_resultado;
                    string? numeroResultado = info?.numero_resultado;
                    int? idEstadoResultado = info?.id_estado_resultado;
                    string estadoResultado = info?.Estado ?? "PENDIENTE";

                    if (hijosPorPadre.TryGetValue(d.id_examen, out var hijosIds) && hijosIds.Any())
                    {
                        var resultadosHijos = resultadosExamen
                            .Where(r => hijosIds.Contains(r.id_examen))
                            .ToList();

                        if (resultadosHijos.Any())
                        {
                            var ultimoHijo = resultadosHijos
                                .OrderByDescending(r => r.fecha_resultado)
                                .First();

                            var hijosAprobados = resultadosHijos
                                .Where(r => EsEstadoAprobado(r.id_estado_resultado))
                                .Select(r => r.id_examen)
                                .Distinct()
                                .ToHashSet();

                            idResultado = ultimoHijo.id_resultado;
                            numeroResultado = ultimoHijo.numero_resultado;
                            idEstadoResultado = hijosIds.All(hijosAprobados.Contains)
                                ? EstadoResultadoAprobadoId
                                : EstadoResultadoRevisionId;
                            estadoResultado = hijosIds.All(hijosAprobados.Contains) ? "APROBADO" : "EN REVISION";
                        }
                        else
                        {
                            idResultado = null;
                            numeroResultado = null;
                            idEstadoResultado = EstadoResultadoPendienteId;
                            estadoResultado = "PENDIENTE";
                        }
                    }
                    else if (info == null)
                    {
                        idResultado = null;
                        numeroResultado = null;
                        idEstadoResultado = EstadoResultadoPendienteId;
                        estadoResultado = "PENDIENTE";
                    }

                    return new ExamenDetalleDto
                    {
                        IdExamen = d.id_examen,
                        NombreExamen = d.examen_navigation!.nombre_examen ?? string.Empty,
                        NombreEstudio = d.examen_navigation!.estudio_navigation?.nombre,
                        IdResultado = idResultado,
                        NumeroResultado = numeroResultado,
                        IdEstadoResultado = idEstadoResultado,
                        EstadoResultado = estadoResultado
                    };
                }).ToList()
            };
        }

        public async Task<bool> AnularOrdenAsync(int idOrden)
        {
            var entidadOrden = await _context.Orden.FirstOrDefaultAsync(o => o.id_orden == idOrden);
            if (entidadOrden == null) return false;
            if (!entidadOrden.activo) return true;
            entidadOrden.activo = false;
            entidadOrden.fecha_fin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<OrdenRespuestaDto?> GuardarOrdenAsync(OrdenCompletaDto datosOrden)
        {
            var ultima = await _context.Orden.OrderByDescending(o => o.id_orden).FirstOrDefaultAsync();
            int siguiente = 1;
            if (ultima != null && !string.IsNullOrEmpty(ultima.numero_orden))
            {
                var partes = ultima.numero_orden.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimo)) siguiente = ultimo + 1;
            }
            string numeroOrden = $"ORD-{siguiente:D5}";
            var dto = datosOrden.Orden;
            var entidadOrden = new orden
            {
                id_paciente = dto.IdPaciente,
                fecha_orden = dto.FechaOrden,
                id_medico = dto.IdMedico,
                observacion = dto.Observacion,
                id_estado_pago = EstadoPagoPendienteId,
                id_estado_orden = EstadoOrdenEnProcesoId,
                activo = true,
                numero_orden = numeroOrden,
                total = dto.Total,
                saldo_pendiente = dto.Total,
                detalle_orden = dto.Detalles.Select(d => new detalle_orden { id_examen = d.IdExamen, precio = d.Precio }).ToList()
            };
            _context.Orden.Add(entidadOrden);
            await _context.SaveChangesAsync();

            await NotificarOrdenRegistradaAsync(entidadOrden.id_orden, entidadOrden.id_paciente, entidadOrden.numero_orden, entidadOrden.fecha_orden);

            return new OrdenRespuestaDto { IdOrden = entidadOrden.id_orden, NumeroOrden = entidadOrden.numero_orden };
        }

        public async Task<OrdenRespuestaDto?> GuardarOrdenConPagoAsync(OrdenPagoGuardarDto datos)
        {
            if (datos?.Orden?.Orden == null || datos.Pago == null)
                throw new InvalidOperationException("Datos incompletos para registrar orden y pago.");

            await using var tx = await _context.Database.BeginTransactionAsync();

            var ultima = await _context.Orden.OrderByDescending(o => o.id_orden).FirstOrDefaultAsync();
            int siguiente = 1;
            if (ultima != null && !string.IsNullOrEmpty(ultima.numero_orden))
            {
                var partes = ultima.numero_orden.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimo)) siguiente = ultimo + 1;
            }
            string numeroOrden = $"ORD-{siguiente:D5}";

            var dto = datos.Orden.Orden;
            var entidadOrden = new orden
            {
                id_paciente = dto.IdPaciente,
                fecha_orden = dto.FechaOrden,
                id_medico = dto.IdMedico,
                observacion = dto.Observacion,
                id_estado_pago = EstadoPagoPendienteId,
                id_estado_orden = EstadoOrdenEnProcesoId,
                activo = true,
                numero_orden = numeroOrden,
                total = dto.Total,
                saldo_pendiente = dto.Total,
                detalle_orden = dto.Detalles.Select(d => new detalle_orden { id_examen = d.IdExamen, precio = d.Precio }).ToList()
            };
            _context.Orden.Add(entidadOrden);
            await _context.SaveChangesAsync();

            var detallesPago = (datos.Pago.DetallePagos ?? new List<DetallePagoDto>())
                .Where(d => d.Monto > 0)
                .Select(d => new detalle_pago
                {
                    id_tipo_pago = d.IdTipoPago ?? MapearTipoPago(d.TipoPago),
                    numero_comprobante = d.NumeroComprobante,
                    monto = d.Monto,
                    activo = !(d.Anulado ?? false),
                    fecha_creacion = DateTime.UtcNow
                })
                .ToList();

            if (!detallesPago.Any())
                throw new InvalidOperationException("Debe registrar al menos un detalle de pago con monto mayor a cero.");

            if (detallesPago.Any(d => !d.id_tipo_pago.HasValue))
                throw new InvalidOperationException("Uno o más tipos de pago no son válidos.");

            var montoRecibido = detallesPago.Sum(d => d.monto ?? 0m);
            var montoAplicado = Math.Min(montoRecibido, entidadOrden.saldo_pendiente ?? entidadOrden.total);
            var montoVuelto = Math.Max(0, montoRecibido - montoAplicado);

            var entidadPago = new pago
            {
                id_orden = entidadOrden.id_orden,
                fecha_pago = datos.Pago.FechaPago ?? DateTime.UtcNow,
                monto_recibido = montoRecibido,
                monto_aplicado = montoAplicado,
                monto_vuelto = montoVuelto,
                observacion = string.IsNullOrWhiteSpace(datos.Pago.Observacion) ? "PAGO DESDE ORDEN" : datos.Pago.Observacion,
                activo = true,
                detalle_pago = detallesPago
            };
            _context.Pago.Add(entidadPago);
            await _context.SaveChangesAsync();

            entidadOrden.saldo_pendiente = Math.Max(0, (entidadOrden.saldo_pendiente ?? entidadOrden.total) - montoAplicado);
            if (entidadOrden.saldo_pendiente <= 0)
                entidadOrden.id_estado_pago = EstadoPagoPagadoId;
            else if (montoAplicado > 0)
                entidadOrden.id_estado_pago = EstadoPagoAbonadoId;
            else
                entidadOrden.id_estado_pago = EstadoPagoPendienteId;

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            await NotificarOrdenRegistradaAsync(entidadOrden.id_orden, entidadOrden.id_paciente, entidadOrden.numero_orden, entidadOrden.fecha_orden);

            return new OrdenRespuestaDto { IdOrden = entidadOrden.id_orden, NumeroOrden = entidadOrden.numero_orden };
        }

        private async Task NotificarOrdenRegistradaAsync(int idOrden, int? idPaciente, string numeroOrden, DateOnly fechaOrden)
        {
            try
            {
                var paciente = await _context.Paciente
                    .Include(p => p.persona_navigation)
                    .FirstOrDefaultAsync(p => p.id_paciente == idPaciente);

                var correoDestino = paciente == null
                    ? null
                    : await _context.Usuario
                        .Where(u => u.id_persona == paciente.id_persona)
                        .OrderByDescending(u => u.activo == true)
                        .Select(u => u.correo)
                        .FirstOrDefaultAsync();

                if (paciente != null && !string.IsNullOrWhiteSpace(correoDestino))
                {
                    string asunto = "Orden registrada - Laboratorio La Inmaculada";
                    string cuerpo = $@"
                        <div style=""font-family:Arial, sans-serif; color:#333;"">
                            <h3>Estimado/a {paciente.persona_navigation?.nombres} {paciente.persona_navigation?.apellidos},</h3>
                            <p>Su orden <strong>{numeroOrden}</strong> ha sido registrada.</p>
                            <p>Fecha de orden: {fechaOrden:dd/MM/yyyy}</p>
                            <p>Gracias por confiar en nosotros.</p>
                            <p style=""margin-top:20px;""><strong>Laboratorio Clínico La Inmaculada</strong></p>
                        </div>";

                    await _emailService.EnviarCorreoAsync(correoDestino, $"{paciente.persona_navigation?.nombres} {paciente.persona_navigation?.apellidos}".Trim(), asunto, cuerpo);
                }
                else
                {
                    _logger.LogWarning("No se encontró correo para notificación de orden registrada. IdOrden: {IdOrden}, IdPaciente: {IdPaciente}", idOrden, idPaciente);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando correo de orden registrada para la orden {IdOrden}", idOrden);
            }
        }

        public async Task<byte[]?> GenerarOrdenTicketPdfAsync(int idOrden)
        {
            var entidadOrden = await _context.Orden
                .Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(o => o.medico_navigation)
                .Include(o => o.estado_pago_navigation)
                .Include(o => o.detalle_orden).ThenInclude(d => d.examen_navigation)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);
            if (entidadOrden == null) return null;

            var tiposPago = await _context.Pago
                .Where(p => p.id_orden == idOrden && p.activo)
                .SelectMany(p => p.detalle_pago)
                .Where(dp => dp.activo && dp.id_tipo_pago != null && dp.tipo_pago_navigation != null)
                .Select(dp => dp.tipo_pago_navigation!.nombre)
                .Distinct()
                .ToListAsync();

            int edadPaciente = 0;
            if (entidadOrden.paciente_navigation?.persona_navigation?.fecha_nacimiento is DateOnly fechaNacimiento)
            {
                var hoy = DateTime.Today;
                var fechaNac = fechaNacimiento.ToDateTime(TimeOnly.MinValue);
                edadPaciente = hoy.Year - fechaNac.Year;
                if (fechaNac > hoy.AddYears(-edadPaciente)) edadPaciente--;
            }
            var ticket = new OrdenTicketDto
            {
                NumeroOrden = entidadOrden.numero_orden,
                FechaOrden = entidadOrden.fecha_orden.ToDateTime(TimeOnly.MinValue),
                NombrePaciente = $"{entidadOrden.paciente_navigation?.persona_navigation?.nombres} {entidadOrden.paciente_navigation?.persona_navigation?.apellidos}" ?? "(Sin nombre)",
                CedulaPaciente = entidadOrden.paciente_navigation?.persona_navigation?.cedula ?? "(Sin cédula)",
                EdadPaciente = edadPaciente,
                NombreMedico = entidadOrden.medico_navigation?.nombre_medico ?? "(Sin médico)",
                Total = entidadOrden.total,
                TotalPagado = entidadOrden.total - (entidadOrden.saldo_pendiente ?? 0m),
                SaldoPendiente = entidadOrden.saldo_pendiente ?? 0m,
                TipoPago = tiposPago.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()!,
                Examenes = entidadOrden.detalle_orden.Select(d => new ExamenTicketDto { NombreExamen = d.examen_navigation?.nombre_examen ?? "(Sin examen)", Precio = d.precio ?? 0m }).ToList()
            };
            return _pdfTicketService.GenerarTicketOrden(ticket);
        }

        private static int? MapearTipoPago(string? tipoPago)
        {
            if (string.IsNullOrWhiteSpace(tipoPago)) return null;
            return tipoPago.ToUpperInvariant() switch
            {
                "EFECTIVO" => 1,
                "TRANSFERENCIA" => 2,
                _ => null
            };
        }

        public async Task<bool> AnularOrdenCompletaAsync(int idOrden)
        {
            return await AnularOrdenAsync(idOrden);
        }

        public async Task<List<object>> ListarOrdenesPorPacienteAsync(int idPaciente)
        {
            var lista = await _context.Orden
                .Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(o => o.estado_pago_navigation)
                .Where(o => o.id_paciente == idPaciente)
                .Select(entidadOrden => new
                {
                    IdOrden = entidadOrden.id_orden,
                    NumeroOrden = entidadOrden.numero_orden,
                    FechaOrden = entidadOrden.fecha_orden,
                    Total = entidadOrden.total,
                    TotalPagado = entidadOrden.total - (entidadOrden.saldo_pendiente ?? 0m),
                    SaldoPendiente = entidadOrden.saldo_pendiente ?? 0m,
                    EstadoPago = entidadOrden.estado_pago_navigation != null ? entidadOrden.estado_pago_navigation.nombre : null,
                    Anulado = !entidadOrden.activo
                })
                .OrderByDescending(x => x.IdOrden)
                .ToListAsync();
            return lista.Cast<object>().ToList();
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenPacienteAsync(int idOrden) => await ObtenerDetalleOrdenAsync(idOrden);

        public async Task<PacienteDashboardDto> ObtenerDashboardPacienteAsync(int idPaciente)
        {
            var ordenesPaciente = _context.Orden
                .Where(o => o.id_paciente == idPaciente && o.activo);

            var ordenesActivas = await ordenesPaciente.CountAsync();
            var ultimaOrden = await ordenesPaciente
                .OrderByDescending(o => o.fecha_orden)
                .Select(o => new { o.fecha_orden, o.numero_orden })
                .FirstOrDefaultAsync();

            var resultadosDisponibles = await _context.Resultado
                .Include(r => r.orden_navigation)
                .Where(r => r.activo && r.orden_navigation != null && r.orden_navigation.id_paciente == idPaciente)
                .CountAsync();

            return new PacienteDashboardDto
            {
                OrdenesActivas = ordenesActivas,
                ResultadosDisponibles = resultadosDisponibles,
                FechaUltimaOrden = ultimaOrden?.fecha_orden,
                NumeroUltimaOrden = ultimaOrden?.numero_orden
            };
        }

        public async Task<LaboratoristaHomeDto> ObtenerDashboardLaboratoristaAsync()
        {
            const decimal stockMinimo = 5m;
            var resumen = new LaboratoristaDashboardDto
            {
                OrdenesPendientes = await _context.Orden.CountAsync(o => o.activo),
                ResultadosPorRegistrar = await _context.Orden
                    .Where(o => o.activo && !_context.Resultado.Any(r => r.id_orden == o.id_orden && r.activo))
                    .CountAsync(),
                ReactivosStockBajo = await _context.Reactivo
                    .Where(r => r.activo && (r.cantidad_disponible ?? 0m) < stockMinimo)
                    .CountAsync()
            };

            var ordenesRecientes = await _context.Orden
                .Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(o => o.medico_navigation)
                .Where(o => o.activo)
                .OrderByDescending(o => o.fecha_orden)
                .Take(5)
                .Select(o => new LaboratoristaOrdenRecienteDto
                {
                    IdOrden = o.id_orden,
                    NumeroOrden = o.numero_orden ?? string.Empty,
                    NombrePaciente = $"{o.paciente_navigation!.persona_navigation!.nombres} {o.paciente_navigation!.persona_navigation!.apellidos}",
                    Medico = o.medico_navigation != null ? o.medico_navigation.nombre_medico : string.Empty,
                    EstadoPago = o.estado_pago_navigation != null ? o.estado_pago_navigation.nombre ?? string.Empty : string.Empty,
                    IdEstadoPago = o.id_estado_pago,
                    ListoParaEntrega = o.id_estado_orden == EstadoOrdenFinalizadaId
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
                .Where(r => r.activo)
                .Include(r => r.estado_resultado_navigation)
                .Include(r => r.orden_navigation)!.ThenInclude(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(r => r.detalle_resultado)!.ThenInclude(d => d.examen_navigation)
                .OrderByDescending(r => r.fecha_resultado)
                .ToListAsync();

            var resumen = new AdminDashboardDto
            {
                OrdenesHoy = await _context.Orden.CountAsync(o => o.activo && o.fecha_orden == hoy),
                ResultadosPendientesAprobacion = resultados.Count(r => r.id_estado_resultado == EstadoResultadoRevisionId),
                ResultadosCorreccion = resultados.Count(r => r.id_estado_resultado == EstadoResultadoCorreccionId),
                ReactivosCriticos = await _context.Reactivo.CountAsync(r => r.activo && (r.cantidad_disponible ?? 0m) < 3m),
                UsuariosTotales = await _context.Usuario.CountAsync(u => u.activo == true && u.rol_navigation.nombre != "paciente")
            };

            var examenesHijosIds = resultados
                .SelectMany(r => r.detalle_resultado.Select(d => d.id_examen))
                .Distinct()
                .ToList();

            var composiciones = await _context.ExamenComposicion
                .Where(ec => ec.activo && examenesHijosIds.Contains(ec.id_examen_hijo))
                .Include(ec => ec.examen_padre_navigation)
                .ToListAsync();

            var pendientes = resultados
                .Where(r => r.id_estado_resultado == EstadoResultadoPendienteId
                    || r.id_estado_resultado == EstadoResultadoRevisionId
                    || r.id_estado_resultado == EstadoResultadoCorreccionId)
                .Take(5)
                .Select(r => {
                    var detalle = r.detalle_resultado.FirstOrDefault();
                    string tipoExamen = string.Empty;
                    if (detalle != null)
                    {
                        var comp = composiciones.FirstOrDefault(c => c.id_examen_hijo == detalle.id_examen);
                        if (comp != null && comp.examen_padre_navigation != null)
                        {
                            tipoExamen = comp.examen_padre_navigation.titulo_examen ?? comp.examen_padre_navigation.nombre_examen ?? string.Empty;
                        }
                        else
                        {
                            tipoExamen = detalle.examen_navigation?.titulo_examen ?? detalle.examen_navigation?.nombre_examen ?? string.Empty;
                        }
                    }
                    return new AdminResultadoPendienteDto
                    {
                        IdResultado = r.id_resultado,
                        IdOrden = r.id_orden,
                        NumeroOrden = r.orden_navigation!.numero_orden ?? string.Empty,
                        Paciente = r.orden_navigation!.paciente_navigation != null
                            ? $"{r.orden_navigation.paciente_navigation.persona_navigation!.nombres} {r.orden_navigation.paciente_navigation.persona_navigation!.apellidos}"
                            : string.Empty,
                        TipoExamen = tipoExamen,
                        Estado = r.estado_resultado_navigation?.nombre ?? "PENDIENTE"
                    };
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
                .Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(o => o.detalle_orden)
                .Include(o => o.estado_pago_navigation)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);
            if (orden == null) return;

            var examenesOrden = orden.detalle_orden.Select(d => d.id_examen).Distinct().ToList();
            if (!examenesOrden.Any()) return;

            var composicionesActivas = await _context.ExamenComposicion
                .Where(ec => ec.activo && examenesOrden.Contains(ec.id_examen_padre))
                .ToListAsync();

            var examenesRequeridos = CalcularExamenesRequeridos(examenesOrden, composicionesActivas);
            if (!examenesRequeridos.Any()) return;

            var resultadosOrden = await _context.Resultado
                .Include(r => r.estado_resultado_navigation)
                .Include(r => r.detalle_resultado)
                .Where(r => r.id_orden == idOrden && r.activo)
                .ToListAsync();

            var examenesAprobados = resultadosOrden
                .Where(r => EsEstadoAprobado(r.id_estado_resultado))
                .SelectMany(r => r.detalle_resultado)
                .Select(dr => dr.id_examen)
                .Distinct()
                .ToHashSet();

            bool todosAprobados = examenesRequeridos.All(examenesAprobados.Contains);
            bool pagada = orden.id_estado_pago == EstadoPagoPagadoId;
            bool estabaFinalizada = orden.id_estado_orden == EstadoOrdenFinalizadaId;

            bool finalizada = todosAprobados && pagada;
            orden.id_estado_orden = finalizada ? EstadoOrdenFinalizadaId : EstadoOrdenEnProcesoId;
            orden.fecha_completado = finalizada
                ? (orden.fecha_completado ?? DateTime.UtcNow)
                : null;

            bool debeNotificar = finalizada && !estabaFinalizada && !_ordenesNotificadas.ContainsKey(idOrden);

            await _context.SaveChangesAsync();

            if (debeNotificar)
            {
                var correo = await _context.Usuario
                    .Where(u => u.id_persona == orden.paciente_navigation!.id_persona)
                    .OrderByDescending(u => u.activo == true)
                    .Select(u => u.correo)
                    .FirstOrDefaultAsync();

                var nombre = $"{orden.paciente_navigation?.persona_navigation?.nombres} {orden.paciente_navigation?.persona_navigation?.apellidos}".Trim();
                if (!string.IsNullOrWhiteSpace(correo))
                {
                    string asunto = "Resultados habilitados - Laboratorio La Inmaculada";
                    string cuerpo = $@"<div style='font-family:Arial,sans-serif;color:#333;'><h3>Estimado/a {nombre},</h3><p>Su orden <strong>{orden.numero_orden}</strong> ha sido finalizada y los resultados ya están habilitados.</p><p>Puede consultarlos ingresando a su cuenta.</p><p style='margin-top:20px;'>Gracias por confiar en nosotros.<br><strong>Laboratorio Clínico La Inmaculada</strong></p></div>";

                    await _emailService.EnviarCorreoAsync(correo, string.IsNullOrWhiteSpace(nombre) ? "Paciente" : nombre, asunto, cuerpo);
                    _ordenesNotificadas.TryAdd(idOrden, true);
                }
                else
                {
                    _logger.LogWarning("No se encontró correo para notificación de resultados finalizados. IdOrden: {IdOrden}, IdPaciente: {IdPaciente}", idOrden, orden.id_paciente);
                }
            }
        }

        public async Task<RecepcionistaHomeDto> ObtenerDashboardRecepcionistaAsync()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var resumen = new RecepcionistaDashboardDto
            {
                OrdenesRegistradas = await _context.Orden.CountAsync(o => o.activo && o.fecha_orden == hoy),
                CuentasPorCobrar = await _context.Orden.CountAsync(o => o.activo && (o.saldo_pendiente ?? 0m) > 0m),
                ResultadosDisponibles = await _context.Orden.CountAsync(o => o.activo && o.id_estado_orden == EstadoOrdenFinalizadaId)
            };

            var ordenesRecientes = await _context.Orden
                .AsNoTracking()
                .Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(o => o.medico_navigation)
                .Include(o => o.estado_pago_navigation)
                .Where(o => o.activo)
                .OrderByDescending(o => o.fecha_orden)
                .Take(5)
                .Select(o => new RecepcionistaOrdenRecienteDto
                {
                    IdOrden = o.id_orden,
                    NumeroOrden = o.numero_orden ?? string.Empty,
                    Paciente = o.paciente_navigation != null ? $"{o.paciente_navigation.persona_navigation!.nombres} {o.paciente_navigation.persona_navigation!.apellidos}" : string.Empty,
                    Medico = o.medico_navigation != null ? o.medico_navigation.nombre_medico : string.Empty,
                    EstadoPago = o.estado_pago_navigation != null ? o.estado_pago_navigation.nombre ?? "-" : "-",
                    IdEstadoPago = o.id_estado_pago,
                    ListoParaEntrega = o.id_estado_orden == EstadoOrdenFinalizadaId
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
