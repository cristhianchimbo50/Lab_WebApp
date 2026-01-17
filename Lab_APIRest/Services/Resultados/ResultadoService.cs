using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Services.PDF;
using Lab_Contracts.Resultados;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Lab_Contracts.Common;
using Lab_APIRest.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace Lab_APIRest.Services.Resultados
{
    public class ResultadoService : IResultadoService
    {
        private readonly LabDbContext _context;
        private readonly PdfResultadoService _pdfResultadoService;
        private readonly ILogger<ResultadoService> _logger;

        public ResultadoService(LabDbContext context, PdfResultadoService pdfResultadoService, ILogger<ResultadoService> logger)
        {
            _context = context;
            _pdfResultadoService = pdfResultadoService;
            _logger = logger;
        }

        private static ResultadoListadoDto MapListado(Resultado r) => new()
        {
            IdResultado = r.IdResultado,
            NumeroResultado = r.NumeroResultado,
            NumeroOrden = r.IdOrdenNavigation?.NumeroOrden ?? string.Empty,
            CedulaPaciente = r.IdOrdenNavigation?.IdPacienteNavigation?.CedulaPaciente ?? string.Empty,
            NombrePaciente = r.IdOrdenNavigation?.IdPacienteNavigation?.NombrePaciente ?? string.Empty,
            FechaResultado = r.FechaResultado,
            Anulado = !r.Activo,
            Observaciones = r.Observaciones ?? string.Empty,
            IdPaciente = r.IdOrdenNavigation?.IdPacienteNavigation?.IdPaciente ?? 0,
            EstadoResultado = r.EstadoResultado ?? "REVISION"
        };

        private static DetalleResultadoDto MapDetalle(DetalleResultado d) => new()
        {
            IdExamen = d.IdExamen,
            NombreExamen = d.IdExamenNavigation?.NombreExamen ?? string.Empty,
            Valor = d.Valor,
            Unidad = d.IdExamenNavigation?.Unidad ?? string.Empty,
            Observacion = string.Empty,
            ValorReferencia = d.IdExamenNavigation?.ValorReferencia ?? string.Empty,
            Anulado = false,
            TituloExamen = d.IdExamenNavigation?.TituloExamen
        };

        private static ResultadoDetalleDto MapDetalleResultado(Resultado r) => new()
        {
            IdResultado = r.IdResultado,
            NumeroResultado = r.NumeroResultado,
            CedulaPaciente = r.IdOrdenNavigation?.IdPacienteNavigation?.CedulaPaciente ?? string.Empty,
            NombrePaciente = r.IdOrdenNavigation?.IdPacienteNavigation?.NombrePaciente ?? string.Empty,
            GeneroPaciente = r.IdOrdenNavigation?.IdPacienteNavigation?.IdGeneroNavigation?.Nombre,
            FechaResultado = r.FechaResultado,
            Observaciones = r.Observaciones ?? string.Empty,
            Anulado = !r.Activo,
            Detalles = r.DetalleResultado.Select(MapDetalle).ToList(),
            IdPaciente = r.IdOrdenNavigation?.IdPacienteNavigation?.IdPaciente ?? 0,
            NumeroOrden = r.IdOrdenNavigation?.NumeroOrden ?? string.Empty,
            EstadoPago = r.IdOrdenNavigation?.EstadoPago ?? string.Empty,
            EstadoResultado = r.EstadoResultado ?? "REVISION",
            ObservacionRevision = r.ObservacionRevision,
            FechaRevision = r.FechaRevision,
            IdRevisor = r.IdRevisor,
            NombreRevisor = r.IdRevisorNavigation?.Nombre,
            IdOrden = r.IdOrden,
            ResultadosHabilitados = r.IdOrdenNavigation?.ResultadosHabilitados ?? false
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

        private async Task ActualizarOrdenSegunResultadosAsync(int idOrden)
        {
            var orden = await _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .Include(o => o.DetalleOrden)
                .Include(o => o.Resultado.Where(r => r.Activo))!.ThenInclude(r => r.DetalleResultado)
                .FirstOrDefaultAsync(o => o.IdOrden == idOrden);
            if (orden == null) return;

            var examenesOrden = orden.DetalleOrden.Select(d => d.IdExamen).Distinct().ToList();
            var examenesAprobados = orden.Resultado
                .Where(r => string.Equals(r.EstadoResultado, "APROBADO", StringComparison.OrdinalIgnoreCase))
                .SelectMany(r => r.DetalleResultado)
                .Select(d => d.IdExamen)
                .Distinct()
                .ToList();  

            bool todosAprobados = examenesOrden.Any() && examenesOrden.All(examenesAprobados.Contains);
            orden.EstadoOrden = todosAprobados ? "FINALIZADA" : "EN_PROCESO";

            bool habilitar = todosAprobados && string.Equals(orden.EstadoPago, "PAGADO", StringComparison.OrdinalIgnoreCase);
            bool debeNotificar = habilitar && !orden.ResultadosHabilitados;
            orden.ResultadosHabilitados = habilitar;

            await _context.SaveChangesAsync();

            if (debeNotificar)
            {
                try
                {
                    var correo = orden.IdPacienteNavigation?.CorreoElectronicoPaciente;
                    var nombre = orden.IdPacienteNavigation?.NombrePaciente;
                    if (!string.IsNullOrWhiteSpace(correo))
                    {
                        string asunto = "Resultados habilitados - Laboratorio La Inmaculada";
                        string cuerpo = $"<div style='font-family:Arial,sans-serif;color:#333;'><h3>Estimado/a {nombre},</h3><p>Su orden <strong>{orden.NumeroOrden}</strong> ha sido finalizada y los resultados ya están habilitados.</p><p>Puede revisarlos ingresando a su cuenta.</p><p style='margin-top:20px;'>Gracias por confiar en nosotros.<br><strong>Laboratorio Clínico La Inmaculada</strong></p></div>";
                        var emailService = new EmailService(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
                        await emailService.EnviarCorreoAsync(correo, nombre ?? "Paciente", asunto, cuerpo);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error enviando notificación de resultados habilitados para la orden {IdOrden}", idOrden);
                }
            }
        }

        public async Task<bool> GuardarResultadosAsync(ResultadoGuardarDto resultado)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var ultimo = await _context.Resultado.OrderByDescending(r => r.IdResultado).FirstOrDefaultAsync();
                var correlativo = (ultimo?.IdResultado ?? 0) + 1;

                string numeroGenerado = $"RES-{correlativo:D5}";

                var entidadResultado = new Resultado
                {
                    IdOrden = resultado.IdOrden,
                    NumeroResultado = numeroGenerado,
                    FechaResultado = resultado.FechaResultado ?? DateTime.UtcNow,
                    Observaciones = resultado.ObservacionesGenerales,
                    Activo = true,
                    EstadoResultado = "REVISION"
                };
                _context.Resultado.Add(entidadResultado);
                await _context.SaveChangesAsync();

                foreach (var examen in resultado.Examenes)
                {
                    var detalle = new DetalleResultado
                    {
                        IdResultado = entidadResultado.IdResultado,
                        IdExamen = examen.IdExamen,
                        Valor = examen.Valor ?? string.Empty
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
                .Include(r => r.IdOrdenNavigation)!.ThenInclude(o => o.IdPacienteNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.NumeroResultado))
                consulta = consulta.Where(r => r.NumeroResultado.Contains(filtro.NumeroResultado));
            if (!string.IsNullOrWhiteSpace(filtro.NumeroOrden))
                consulta = consulta.Where(r => r.IdOrdenNavigation != null && r.IdOrdenNavigation.NumeroOrden.Contains(filtro.NumeroOrden));
            if (filtro.IdPaciente.HasValue)
                consulta = consulta.Where(r => r.IdOrdenNavigation != null && r.IdOrdenNavigation.IdPaciente == filtro.IdPaciente.Value);
            if (filtro.FechaDesde.HasValue)
                consulta = consulta.Where(r => r.FechaResultado >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue)
                consulta = consulta.Where(r => r.FechaResultado <= filtro.FechaHasta.Value);
            if (filtro.Anulado.HasValue)
                consulta = consulta.Where(r => r.Activo == !filtro.Anulado.Value);

            return await consulta
                .OrderByDescending(r => r.IdResultado)
                .Select(r => MapListado(r))
                .ToListAsync();
        }

        public async Task<ResultadoPaginadoDto<ResultadoListadoDto>> ListarResultadosPaginadosAsync(ResultadoFiltroDto filtro)
        {
            var consulta = _context.Resultado
                .Include(r => r.IdOrdenNavigation)!.ThenInclude(o => o.IdPacienteNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (filtro.IdPaciente.HasValue)
                consulta = consulta.Where(r => r.IdOrdenNavigation != null && r.IdOrdenNavigation.IdPaciente == filtro.IdPaciente.Value);
            if (!string.IsNullOrWhiteSpace(filtro.NumeroResultado))
                consulta = consulta.Where(r => r.NumeroResultado.Contains(filtro.NumeroResultado));
            if (!string.IsNullOrWhiteSpace(filtro.NumeroOrden))
                consulta = consulta.Where(r => r.IdOrdenNavigation != null && r.IdOrdenNavigation.NumeroOrden.Contains(filtro.NumeroOrden));
            if (filtro.FechaDesde.HasValue)
                consulta = consulta.Where(r => r.FechaResultado >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue)
                consulta = consulta.Where(r => r.FechaResultado <= filtro.FechaHasta.Value);
            if (filtro.Anulado.HasValue)
                consulta = consulta.Where(r => r.Activo == !filtro.Anulado.Value);

            var total = await consulta.CountAsync();
            bool asc = filtro.SortAsc;
            consulta = filtro.SortBy switch
            {
                nameof(ResultadoListadoDto.NumeroResultado) => asc ? consulta.OrderBy(r => r.NumeroResultado) : consulta.OrderByDescending(r => r.NumeroResultado),
                nameof(ResultadoListadoDto.NumeroOrden) => asc ? consulta.OrderBy(r => r.IdOrdenNavigation!.NumeroOrden) : consulta.OrderByDescending(r => r.IdOrdenNavigation!.NumeroOrden),
                nameof(ResultadoListadoDto.CedulaPaciente) => asc ? consulta.OrderBy(r => r.IdOrdenNavigation!.IdPacienteNavigation!.CedulaPaciente) : consulta.OrderByDescending(r => r.IdOrdenNavigation!.IdPacienteNavigation!.CedulaPaciente),
                nameof(ResultadoListadoDto.NombrePaciente) => asc ? consulta.OrderBy(r => r.IdOrdenNavigation!.IdPacienteNavigation!.NombrePaciente) : consulta.OrderByDescending(r => r.IdOrdenNavigation!.IdPacienteNavigation!.NombrePaciente),
                _ => asc ? consulta.OrderBy(r => r.FechaResultado) : consulta.OrderByDescending(r => r.FechaResultado)
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
                .Include(r => r.IdOrdenNavigation)!.ThenInclude(o => o.IdPacienteNavigation)!.ThenInclude(p => p.IdGeneroNavigation)
                .Include(r => r.IdOrdenNavigation)!.ThenInclude(o => o.IdMedicoNavigation)
                .Include(r => r.IdRevisorNavigation)
                .Include(r => r.DetalleResultado).ThenInclude(d => d.IdExamenNavigation)
                .FirstOrDefaultAsync(r => r.IdResultado == idResultado);
            return entidad == null ? null : MapDetalleResultado(entidad);
        }

        public async Task<ResultadoCompletoDto?> ObtenerResultadoCompletoAsync(int idResultado)
        {
            var entidad = await _context.Resultado
                .Include(r => r.IdOrdenNavigation)!.ThenInclude(o => o.IdPacienteNavigation)!.ThenInclude(p => p.IdGeneroNavigation)
                .Include(r => r.IdOrdenNavigation)!.ThenInclude(o => o.IdMedicoNavigation)
                .Include(r => r.IdRevisorNavigation)
                .Include(r => r.DetalleResultado).ThenInclude(d => d.IdExamenNavigation)
                .FirstOrDefaultAsync(r => r.IdResultado == idResultado);
            if (entidad == null) return null;

            int edad = CalcularEdad(entidad.IdOrdenNavigation?.IdPacienteNavigation?.FechaNacPaciente);

            return new ResultadoCompletoDto
            {
                NumeroOrden = entidad.IdOrdenNavigation?.NumeroOrden ?? string.Empty,
                NumeroResultado = entidad.NumeroResultado,
                FechaResultado = entidad.FechaResultado,
                NombrePaciente = entidad.IdOrdenNavigation?.IdPacienteNavigation?.NombrePaciente ?? string.Empty,
                CedulaPaciente = entidad.IdOrdenNavigation?.IdPacienteNavigation?.CedulaPaciente ?? string.Empty,
                GeneroPaciente = entidad.IdOrdenNavigation?.IdPacienteNavigation?.IdGeneroNavigation?.Nombre,
                EdadPaciente = edad,
                MedicoSolicitante = entidad.IdOrdenNavigation?.IdMedicoNavigation?.NombreMedico ?? string.Empty,
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
            var entidad = await _context.Resultado.FirstOrDefaultAsync(r => r.IdResultado == idResultado);
            if (entidad == null || !entidad.Activo) return false;
            entidad.Activo = false;
            entidad.FechaFin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RevisarResultadoAsync(int idResultado, string estado, string? observacion, int idRevisor)
        {
            if (string.IsNullOrWhiteSpace(estado)) throw new ArgumentException("Estado inválido", nameof(estado));
            var estadoNormalizado = estado.Trim().ToUpperInvariant();
            if (estadoNormalizado != "APROBADO" && estadoNormalizado != "CORRECCION")
                throw new ArgumentException("Estado de resultado no soportado", nameof(estado));

            var entidad = await _context.Resultado.FirstOrDefaultAsync(r => r.IdResultado == idResultado && r.Activo);
            if (entidad == null) return false;

            if (string.Equals(entidad.EstadoResultado, "APROBADO", StringComparison.OrdinalIgnoreCase)
                && estadoNormalizado == "CORRECCION")
                return false;

            entidad.EstadoResultado = estadoNormalizado;
            entidad.ObservacionRevision = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim();
            entidad.IdRevisor = idRevisor;
            entidad.FechaRevision = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await ActualizarOrdenSegunResultadosAsync(entidad.IdOrden);
            return true;
        }

        public async Task<bool> ActualizarResultadoAsync(ResultadoActualizarDto resultado)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var entidad = await _context.Resultado
                    .Include(r => r.DetalleResultado)
                    .FirstOrDefaultAsync(r => r.IdResultado == resultado.IdResultado && r.Activo);
                if (entidad == null) return false;
                if (!string.Equals(entidad.EstadoResultado, "CORRECCION", StringComparison.OrdinalIgnoreCase)) return false;

                entidad.FechaResultado = resultado.FechaResultado ?? DateTime.UtcNow;
                entidad.Observaciones = resultado.ObservacionesGenerales;
                entidad.EstadoResultado = "REVISION";
                entidad.ObservacionRevision = null;
                entidad.FechaRevision = null;
                entidad.IdRevisor = null;

                foreach (var ex in resultado.Examenes)
                {
                    var detalle = entidad.DetalleResultado.FirstOrDefault(d => d.IdExamen == ex.IdExamen);
                    if (detalle != null)
                        detalle.Valor = ex.Valor ?? string.Empty;
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                await ActualizarOrdenSegunResultadosAsync(entidad.IdOrden);
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
