using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Services.PDF;
using Lab_Contracts.Resultados;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Lab_Contracts.Common;
using Lab_APIRest.Infrastructure.Services;

namespace Lab_APIRest.Services.Resultados
{
    public class ResultadoService : IResultadoService
    {
        private readonly LabDbContext _context;
        private readonly PdfResultadoService _pdfResultadoService;
        private readonly ILogger<ResultadoService> _logger;
        private const int EstadoResultadoPendienteId = 1;
        private const int EstadoResultadoRevisionId = 2;
        private const int EstadoResultadoCorreccionId = 3;
        private const int EstadoResultadoAprobadoId = 4;
        private const int EstadoResultadoAnuladoId = 5;

        private const int EstadoOrdenEnProcesoId = 1;
        private const int EstadoOrdenFinalizadaId = 2;
        private const int EstadoOrdenAnuladaId = 3;

        public ResultadoService(LabDbContext context, PdfResultadoService pdfResultadoService, ILogger<ResultadoService> logger)
        {
            _context = context;
            _pdfResultadoService = pdfResultadoService;
            _logger = logger;
        }

        private static ResultadoListadoDto MapListado(resultado r) => new()
        {
            IdResultado = r.id_resultado,
            NumeroResultado = r.numero_resultado,
            NumeroOrden = r.orden_navigation?.numero_orden ?? string.Empty,
            CedulaPaciente = r.orden_navigation?.paciente_navigation?.persona_navigation?.cedula ?? string.Empty,
            NombrePaciente = $"{r.orden_navigation?.paciente_navigation?.persona_navigation?.nombres} {r.orden_navigation?.paciente_navigation?.persona_navigation?.apellidos}",
            FechaResultado = r.fecha_resultado,
            Anulado = !r.activo,
            Observaciones = r.observaciones ?? string.Empty,
            IdPaciente = r.orden_navigation?.paciente_navigation?.id_paciente ?? 0,
            EstadoResultado = r.estado_resultado_navigation?.nombre ?? "REVISION"
        };

        private static DetalleResultadoDto MapDetalle(detalle_resultado d)
        {
            var referencia = d.examen_navigation?.referencia_examen?.FirstOrDefault(r => r.activo);
            var valorRef = referencia?.valor_texto ??
                (referencia?.valor_min.HasValue == true || referencia?.valor_max.HasValue == true
                    ? $"{referencia?.valor_min}-{referencia?.valor_max}"
                    : string.Empty);

            return new DetalleResultadoDto
            {
                IdExamen = d.id_examen,
                NombreExamen = d.examen_navigation?.nombre_examen ?? string.Empty,
                Valor = d.valor,
                Unidad = referencia?.unidad ?? string.Empty,
                Observacion = string.Empty,
                ValorReferencia = valorRef ?? string.Empty,
                Anulado = false,
                TituloExamen = d.examen_navigation?.titulo_examen
            };
        }

        private static ResultadoDetalleDto MapDetalleResultado(resultado r) => new()
        {
            IdResultado = r.id_resultado,
            NumeroResultado = r.numero_resultado,
            CedulaPaciente = r.orden_navigation?.paciente_navigation?.persona_navigation?.cedula ?? string.Empty,
            NombrePaciente = $"{r.orden_navigation?.paciente_navigation?.persona_navigation?.nombres} {r.orden_navigation?.paciente_navigation?.persona_navigation?.apellidos}",
            GeneroPaciente = r.orden_navigation?.paciente_navigation?.persona_navigation?.genero_navigation?.nombre,
            FechaResultado = r.fecha_resultado,
            Observaciones = r.observaciones ?? string.Empty,
            Anulado = !r.activo,
            Detalles = r.detalle_resultado.Select(MapDetalle).ToList(),
            IdPaciente = r.orden_navigation?.paciente_navigation?.id_paciente ?? 0,
            NumeroOrden = r.orden_navigation?.numero_orden ?? string.Empty,
            EstadoPago = r.orden_navigation?.estado_pago_navigation?.nombre ?? string.Empty,
            EstadoResultado = r.estado_resultado_navigation?.nombre ?? "REVISION",
            ObservacionRevision = r.observacion_revision,
            FechaRevision = r.fecha_revision,
            IdRevisor = r.id_revisor,
            NombreRevisor = r.revisor_navigation?.persona_navigation != null
                ? $"{r.revisor_navigation.persona_navigation.nombres} {r.revisor_navigation.persona_navigation.apellidos}"
                : null,
            IdOrden = r.id_orden,
            ResultadosHabilitados = r.orden_navigation?.resultados_habilitados ?? false
        };

        private static int CalcularEdad(DateOnly? fechaNac)
        {
            if (!fechaNac.HasValue) return 0;
            var nacimiento = fechaNac.Value.ToDateTime(TimeOnly.MinValue);
            var hoy = DateTime.Today;
            int edad = hoy.Year - nacimiento.Year;
            if (nacimiento > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        private const int EstadoPagoPagadoId = 3;

        private async Task ActualizarOrdenSegunResultadosAsync(int idOrden)
        {
            var orden = await _context.Orden
                .Include(o => o.paciente_navigation)
                .Include(o => o.detalle_orden)
                .Include(o => o.resultado.Where(r => r.activo))!.ThenInclude(r => r.detalle_resultado)
                .Include(o => o.resultado.Where(r => r.activo))!.ThenInclude(r => r.estado_resultado_navigation)
                .FirstOrDefaultAsync(o => o.id_orden == idOrden);
            if (orden == null) return;

            var examenesOrden = orden.detalle_orden.Select(d => d.id_examen).Distinct().ToList();
            var examenesAprobados = orden.resultado
                .Where(r => r.id_estado_resultado == EstadoResultadoAprobadoId)
                .SelectMany(r => r.detalle_resultado)
                .Select(d => d.id_examen)
                .Distinct()
                .ToList();  

            bool todosAprobados = examenesOrden.Any() && examenesOrden.All(examenesAprobados.Contains);
            orden.id_estado_orden = todosAprobados ? EstadoOrdenFinalizadaId : EstadoOrdenEnProcesoId;

            bool habilitar = todosAprobados && orden.id_estado_pago == EstadoPagoPagadoId;
            bool debeNotificar = habilitar && !orden.resultados_habilitados;
            orden.resultados_habilitados = habilitar;

            await _context.SaveChangesAsync();


        }

        public async Task<bool> GuardarResultadosAsync(ResultadoGuardarDto resultado)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var ultimo = await _context.Resultado.OrderByDescending(r => r.id_resultado).FirstOrDefaultAsync();
                var correlativo = (ultimo?.id_resultado ?? 0) + 1;

                string numeroGenerado = $"RES-{correlativo:D5}";

                var entidadResultado = new resultado
                {
                    id_orden = resultado.IdOrden,
                    numero_resultado = numeroGenerado,
                    fecha_resultado = resultado.FechaResultado ?? DateTime.UtcNow,
                    observaciones = resultado.ObservacionesGenerales,
                    activo = true,
                    id_estado_resultado = EstadoResultadoRevisionId
                };
                _context.Resultado.Add(entidadResultado);
                await _context.SaveChangesAsync();

                foreach (var examen in resultado.Examenes)
                {
                    var detalle = new detalle_resultado
                    {
                        id_resultado = entidadResultado.id_resultado,
                        id_examen = examen.IdExamen,
                        valor = examen.Valor ?? string.Empty
                    };
                    _context.DetalleResultado.Add(detalle);
                }

                await _context.SaveChangesAsync();

                await transaccion.CommitAsync();
                await ActualizarOrdenSegunResultadosAsync(resultado.IdOrden);
                return true;
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                _logger.LogError(ex, "Error guardando resultados");
                return false;
            }
        }

        public async Task<List<ResultadoListadoDto>> ListarResultadosAsync(ResultadoFiltroDto filtro)
        {
            var consulta = _context.Resultado
                .Include(r => r.estado_resultado_navigation)
                .Include(r => r.orden_navigation)!.ThenInclude(o => o.paciente_navigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.NumeroResultado))
                consulta = consulta.Where(r => r.numero_resultado.Contains(filtro.NumeroResultado));
            if (!string.IsNullOrWhiteSpace(filtro.NumeroOrden))
                consulta = consulta.Where(r => r.orden_navigation != null && r.orden_navigation.numero_orden.Contains(filtro.NumeroOrden));
            if (filtro.IdPaciente.HasValue)
                consulta = consulta.Where(r => r.orden_navigation != null && r.orden_navigation.id_paciente == filtro.IdPaciente.Value);
            if (filtro.FechaDesde.HasValue)
                consulta = consulta.Where(r => r.fecha_resultado >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue)
                consulta = consulta.Where(r => r.fecha_resultado <= filtro.FechaHasta.Value);
            if (filtro.Anulado.HasValue)
                consulta = consulta.Where(r => r.activo == !filtro.Anulado.Value);

            return await consulta
                .OrderByDescending(r => r.id_resultado)
                .Select(r => MapListado(r))
                .ToListAsync();
        }

        public async Task<ResultadoPaginadoDto<ResultadoListadoDto>> ListarResultadosPaginadosAsync(ResultadoFiltroDto filtro)
        {
            var consulta = _context.Resultado
                .Include(r => r.estado_resultado_navigation)
                .Include(r => r.orden_navigation)!.ThenInclude(o => o.paciente_navigation)
                .AsNoTracking()
                .AsQueryable();

            if (filtro.IdPaciente.HasValue)
                consulta = consulta.Where(r => r.orden_navigation != null && r.orden_navigation.id_paciente == filtro.IdPaciente.Value);
            if (!string.IsNullOrWhiteSpace(filtro.NumeroResultado))
                consulta = consulta.Where(r => r.numero_resultado.Contains(filtro.NumeroResultado));
            if (!string.IsNullOrWhiteSpace(filtro.NumeroOrden))
                consulta = consulta.Where(r => r.orden_navigation != null && r.orden_navigation.numero_orden.Contains(filtro.NumeroOrden));
            if (filtro.FechaDesde.HasValue)
                consulta = consulta.Where(r => r.fecha_resultado >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue)
                consulta = consulta.Where(r => r.fecha_resultado <= filtro.FechaHasta.Value);
            if (filtro.Anulado.HasValue)
                consulta = consulta.Where(r => r.activo == !filtro.Anulado.Value);

            var total = await consulta.CountAsync();
            bool asc = filtro.SortAsc;
            consulta = filtro.SortBy switch
            {
                nameof(ResultadoListadoDto.NumeroResultado) => asc ? consulta.OrderBy(r => r.numero_resultado) : consulta.OrderByDescending(r => r.numero_resultado),
                nameof(ResultadoListadoDto.NumeroOrden) => asc ? consulta.OrderBy(r => r.orden_navigation!.numero_orden) : consulta.OrderByDescending(r => r.orden_navigation!.numero_orden),
                nameof(ResultadoListadoDto.CedulaPaciente) => asc ? consulta.OrderBy(r => r.orden_navigation!.paciente_navigation!.persona_navigation!.cedula) : consulta.OrderByDescending(r => r.orden_navigation!.paciente_navigation!.persona_navigation!.cedula),
                nameof(ResultadoListadoDto.NombrePaciente) => asc ? consulta.OrderBy(r => r.orden_navigation!.paciente_navigation!.persona_navigation!.nombres) : consulta.OrderByDescending(r => r.orden_navigation!.paciente_navigation!.persona_navigation!.nombres),
                _ => asc ? consulta.OrderBy(r => r.fecha_resultado) : consulta.OrderByDescending(r => r.fecha_resultado)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await consulta.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => MapListado(r))
                .ToListAsync();

            return new ResultadoPaginadoDto<ResultadoListadoDto>
            {
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task<ResultadoDetalleDto?> ObtenerDetalleResultadoAsync(int idResultado)
        {
            var entidad = await _context.Resultado
                .Include(r => r.estado_resultado_navigation)
                .Include(r => r.orden_navigation)!.ThenInclude(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)!.ThenInclude(per => per.genero_navigation)
                .Include(r => r.orden_navigation)!.ThenInclude(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(r => r.orden_navigation)!.ThenInclude(o => o.medico_navigation)
                .Include(r => r.revisor_navigation)
                .Include(r => r.detalle_resultado).ThenInclude(d => d.examen_navigation)!.ThenInclude(e => e.referencia_examen)
                .FirstOrDefaultAsync(r => r.id_resultado == idResultado);
            if (entidad == null) return null;

            int edad = CalcularEdad(entidad.orden_navigation?.paciente_navigation?.persona_navigation?.fecha_nacimiento);

            return MapDetalleResultado(entidad);
        }

        public async Task<ResultadoCompletoDto?> ObtenerResultadoCompletoAsync(int idResultado)
        {
            var entidad = await _context.Resultado
                .Include(r => r.estado_resultado_navigation)
                .Include(r => r.orden_navigation)!.ThenInclude(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)!.ThenInclude(per => per.genero_navigation)
                .Include(r => r.orden_navigation)!.ThenInclude(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Include(r => r.orden_navigation)!.ThenInclude(o => o.medico_navigation)
                .Include(r => r.revisor_navigation)
                .Include(r => r.detalle_resultado).ThenInclude(d => d.examen_navigation)!.ThenInclude(e => e.referencia_examen)
                .FirstOrDefaultAsync(r => r.id_resultado == idResultado);
            if (entidad == null) return null;

            int edad = CalcularEdad(entidad.orden_navigation?.paciente_navigation?.persona_navigation?.fecha_nacimiento);

            return new ResultadoCompletoDto
            {
                NumeroOrden = entidad.orden_navigation?.numero_orden ?? string.Empty,
                NumeroResultado = entidad.numero_resultado,
                FechaResultado = entidad.fecha_resultado,
                NombrePaciente = entidad.orden_navigation?.paciente_navigation != null
                    ? $"{entidad.orden_navigation.paciente_navigation.persona_navigation!.nombres} {entidad.orden_navigation.paciente_navigation.persona_navigation!.apellidos}"
                    : string.Empty,
                CedulaPaciente = entidad.orden_navigation?.paciente_navigation?.persona_navigation?.cedula ?? string.Empty,
                GeneroPaciente = entidad.orden_navigation?.paciente_navigation?.persona_navigation?.genero_navigation?.nombre,
                EdadPaciente = edad,
                MedicoSolicitante = entidad.orden_navigation?.medico_navigation?.nombre_medico ?? string.Empty,
                Detalles = new List<ResultadoDetalleDto> { MapDetalleResultado(entidad) }
            };
        }

        public async Task<byte[]?> GenerarResultadosPdfAsync(List<int> ids)
        {
            var resultados = new List<ResultadoCompletoDto>();
            foreach (var id in ids)
            {
                var r = await ObtenerResultadoCompletoAsync(id);
                if (r != null) resultados.Add(r);
            }
            if (!resultados.Any()) return null;
            return _pdfResultadoService.GenerarResultadosPdf(resultados);
        }

        public async Task<bool> AnularResultadoAsync(int idResultado)
        {
            var entidad = await _context.Resultado.FirstOrDefaultAsync(r => r.id_resultado == idResultado);
            if (entidad == null || !entidad.activo) return false;
            entidad.activo = false;
            entidad.fecha_fin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevisarResultadoAsync(int idResultado, string estado, string? observacion, int idRevisor)
        {
            if (string.IsNullOrWhiteSpace(estado)) throw new ArgumentException("Estado inválido", nameof(estado));
            var estadoNormalizado = estado.Trim().ToUpperInvariant();
            if (estadoNormalizado != "APROBADO" && estadoNormalizado != "CORRECCION")
                throw new ArgumentException("Estado de resultado no soportado", nameof(estado));

            var entidad = await _context.Resultado.FirstOrDefaultAsync(r => r.id_resultado == idResultado && r.activo);
            if (entidad == null) return false;

            if (entidad.id_estado_resultado == EstadoResultadoAprobadoId && estadoNormalizado == "CORRECCION")
                return false;

            entidad.id_estado_resultado = estadoNormalizado == "APROBADO" ? EstadoResultadoAprobadoId : EstadoResultadoCorreccionId;
            entidad.observacion_revision = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
            entidad.id_revisor = idRevisor;
            entidad.fecha_revision = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await ActualizarOrdenSegunResultadosAsync(entidad.id_orden);
            return true;
        }

        public async Task<bool> ActualizarResultadoAsync(ResultadoActualizarDto resultado)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var entidad = await _context.Resultado
                    .Include(r => r.detalle_resultado)
                    .FirstOrDefaultAsync(r => r.id_resultado == resultado.IdResultado && r.activo);
                if (entidad == null) return false;
                if (entidad.id_estado_resultado != EstadoResultadoCorreccionId) return false;

                entidad.fecha_resultado = resultado.FechaResultado ?? DateTime.UtcNow;
                entidad.observaciones = resultado.ObservacionesGenerales;
                entidad.id_estado_resultado = EstadoResultadoRevisionId;
                entidad.observacion_revision = null;
                entidad.fecha_revision = null;
                entidad.id_revisor = null;

                foreach (var ex in resultado.Examenes)
                {
                    var detalle = entidad.detalle_resultado.FirstOrDefault(d => d.id_examen == ex.IdExamen);
                    if (detalle != null)
                        detalle.valor = ex.Valor ?? string.Empty;
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                await ActualizarOrdenSegunResultadosAsync(entidad.id_orden);
                return true;
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                _logger.LogError(ex, "Error actualizando resultados");
                return false;
            }
        }
    }
}
