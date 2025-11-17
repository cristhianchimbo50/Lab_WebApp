using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Services.PDF;
using Lab_Contracts.Resultados;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Lab_Contracts.Common;

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

        public async Task<bool> GuardarResultadosAsync(ResultadoGuardarDto resultado)
        {
            using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var ultimoResultado = await _context.resultados.OrderByDescending(r => r.id_resultado).FirstOrDefaultAsync();
                string numeroGenerado = $"RES-{((ultimoResultado?.id_resultado ?? 0) + 1):D5}";

                var entidadResultado = new resultado
                {
                    numero_resultado = numeroGenerado,
                    fecha_resultado = (DateTime)resultado.FechaResultado!,
                    observaciones = resultado.ObservacionesGenerales,
                    id_orden = resultado.IdOrden,
                    anulado = false
                };
                _context.resultados.Add(entidadResultado);
                await _context.SaveChangesAsync();

                foreach (var examen in resultado.Examenes)
                {
                    var detalleResultado = new detalle_resultado
                    {
                        id_resultado = entidadResultado.id_resultado,
                        id_examen = examen.IdExamen,
                        valor = examen.Valor,
                        unidad = examen.Unidad,
                        observacion = examen.Observacion,
                        valor_referencia = examen.ValorReferencia,
                        anulado = false
                    };
                    _context.detalle_resultados.Add(detalleResultado);
                    await _context.SaveChangesAsync();

                    var idExamenPadre = await _context.examen_composicion
                        .Where(c => c.id_examen_hijo == examen.IdExamen)
                        .Select(c => c.id_examen_padre)
                        .FirstOrDefaultAsync();

                    if (idExamenPadre != 0)
                    {
                        var detalleOrdenPadre = await _context.detalle_ordens
                            .FirstOrDefaultAsync(d => d.id_orden == resultado.IdOrden && d.id_examen == idExamenPadre);
                        if (detalleOrdenPadre != null)
                            detalleOrdenPadre.id_resultado = entidadResultado.id_resultado;
                    }
                    else
                    {
                        var detalleOrden = await _context.detalle_ordens
                            .FirstOrDefaultAsync(d => d.id_orden == resultado.IdOrden && d.id_examen == examen.IdExamen);
                        if (detalleOrden != null)
                            detalleOrden.id_resultado = entidadResultado.id_resultado;
                    }

                    var reactivosDelExamen = await _context.examen_reactivos
                        .Where(er => er.id_examen == examen.IdExamen)
                        .Include(er => er.id_reactivoNavigation)
                        .ToListAsync();

                    if (idExamenPadre != 0)
                    {
                        var reactivosDelPadre = await _context.examen_reactivos
                            .Where(er => er.id_examen == idExamenPadre)
                            .Include(er => er.id_reactivoNavigation)
                            .ToListAsync();
                        reactivosDelExamen.AddRange(reactivosDelPadre);
                    }

                    foreach (var asociacion in reactivosDelExamen)
                    {
                        var reactivoAsociado = asociacion.id_reactivoNavigation;
                        if (reactivoAsociado == null) continue;

                        if (reactivoAsociado.cantidad_disponible < asociacion.cantidad_usada)
                            throw new InvalidOperationException($"Stock insuficiente para {reactivoAsociado.nombre_reactivo}");

                        var movimientoEntidad = new movimiento_reactivo
                        {
                            id_reactivo = reactivoAsociado.id_reactivo,
                            tipo_movimiento = "EGRESO",
                            cantidad = asociacion.cantidad_usada,
                            fecha_movimiento = (DateTime)resultado.FechaResultado!,
                            observacion = $"Egreso por examen {examen.NombreExamen} en resultado {numeroGenerado}",
                            id_detalle_resultado = detalleResultado.id_detalle_resultado
                        };
                        _context.movimiento_reactivos.Add(movimientoEntidad);

                        reactivoAsociado.cantidad_disponible -= asociacion.cantidad_usada;
                    }
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaccion.RollbackAsync();
                _logger.LogError(ex, $"Error guardando resultados de orden {resultado.IdOrden}");
                return false;
            }
        }

        public async Task<List<ResultadoListadoDto>> ListarResultadosAsync(ResultadoFiltroDto filtro)
        {
            var consulta = _context.resultados
                .Include(r => r.id_ordenNavigation)
                .ThenInclude(o => o.id_pacienteNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.NumeroResultado))
                consulta = consulta.Where(r => r.numero_resultado.Contains(filtro.NumeroResultado));
            if (!string.IsNullOrWhiteSpace(filtro.NumeroOrden))
                consulta = consulta.Where(r => r.id_ordenNavigation != null && r.id_ordenNavigation.numero_orden.Contains(filtro.NumeroOrden));
            if (!string.IsNullOrWhiteSpace(filtro.Cedula))
                consulta = consulta.Where(r => r.id_ordenNavigation != null && r.id_ordenNavigation.id_pacienteNavigation.cedula_paciente.Contains(filtro.Cedula));
            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                consulta = consulta.Where(r => r.id_ordenNavigation != null && r.id_ordenNavigation.id_pacienteNavigation.nombre_paciente.Contains(filtro.Nombre));
            if (filtro.FechaDesde != null)
                consulta = consulta.Where(r => r.fecha_resultado >= filtro.FechaDesde);
            if (filtro.FechaHasta != null)
                consulta = consulta.Where(r => r.fecha_resultado <= filtro.FechaHasta);
            if (filtro.Anulado != null)
                consulta = consulta.Where(r => r.anulado == filtro.Anulado);

            return await consulta.OrderByDescending(r => r.id_resultado)
                .Select(r => new ResultadoListadoDto
                {
                    IdResultado = r.id_resultado,
                    NumeroResultado = r.numero_resultado,
                    NumeroOrden = r.id_ordenNavigation!.numero_orden,
                    CedulaPaciente = r.id_ordenNavigation!.id_pacienteNavigation.cedula_paciente,
                    NombrePaciente = r.id_ordenNavigation!.id_pacienteNavigation.nombre_paciente,
                    FechaResultado = r.fecha_resultado,
                    Anulado = r.anulado ?? false,
                    IdPaciente = r.id_ordenNavigation!.id_pacienteNavigation.id_paciente,
                    Observaciones = r.observaciones
                })
                .ToListAsync();
        }

        public async Task<ResultadoPaginadoDto<ResultadoListadoDto>> ListarResultadosPaginadosAsync(ResultadoFiltroDto filtro)
        {
            var consulta = _context.resultados
                .Include(r => r.id_ordenNavigation)
                .ThenInclude(o => o.id_pacienteNavigation)
                .AsNoTracking()
                .AsQueryable();

            if (filtro.IdPaciente.HasValue)
                consulta = consulta.Where(r => r.id_ordenNavigation != null && r.id_ordenNavigation.id_paciente == filtro.IdPaciente.Value);
            if (!string.IsNullOrWhiteSpace(filtro.NumeroResultado))
                consulta = consulta.Where(r => r.numero_resultado.Contains(filtro.NumeroResultado));
            if (!string.IsNullOrWhiteSpace(filtro.NumeroOrden))
                consulta = consulta.Where(r => r.id_ordenNavigation != null && r.id_ordenNavigation.numero_orden.Contains(filtro.NumeroOrden));
            if (!string.IsNullOrWhiteSpace(filtro.Cedula))
                consulta = consulta.Where(r => r.id_ordenNavigation != null && r.id_ordenNavigation.id_pacienteNavigation.cedula_paciente.Contains(filtro.Cedula));
            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                consulta = consulta.Where(r => r.id_ordenNavigation != null && r.id_ordenNavigation.id_pacienteNavigation.nombre_paciente.Contains(filtro.Nombre));
            if (filtro.FechaDesde.HasValue)
                consulta = consulta.Where(r => r.fecha_resultado >= filtro.FechaDesde);
            if (filtro.FechaHasta.HasValue)
                consulta = consulta.Where(r => r.fecha_resultado <= filtro.FechaHasta);
            if (filtro.Anulado.HasValue)
                consulta = consulta.Where(r => r.anulado == filtro.Anulado);

            var total = await consulta.CountAsync();

            bool asc = filtro.SortAsc;
            consulta = filtro.SortBy switch
            {
                nameof(ResultadoListadoDto.NumeroResultado) => asc ? consulta.OrderBy(r => r.numero_resultado) : consulta.OrderByDescending(r => r.numero_resultado),
                nameof(ResultadoListadoDto.NumeroOrden) => asc ? consulta.OrderBy(r => r.id_ordenNavigation!.numero_orden) : consulta.OrderByDescending(r => r.id_ordenNavigation!.numero_orden),
                nameof(ResultadoListadoDto.CedulaPaciente) => asc ? consulta.OrderBy(r => r.id_ordenNavigation!.id_pacienteNavigation.cedula_paciente) : consulta.OrderByDescending(r => r.id_ordenNavigation!.id_pacienteNavigation.cedula_paciente),
                nameof(ResultadoListadoDto.NombrePaciente) => asc ? consulta.OrderBy(r => r.id_ordenNavigation!.id_pacienteNavigation.nombre_paciente) : consulta.OrderByDescending(r => r.id_ordenNavigation!.id_pacienteNavigation.nombre_paciente),
                _ => asc ? consulta.OrderBy(r => r.fecha_resultado) : consulta.OrderByDescending(r => r.fecha_resultado)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await consulta.Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ResultadoListadoDto
                {
                    IdResultado = r.id_resultado,
                    NumeroResultado = r.numero_resultado,
                    NumeroOrden = r.id_ordenNavigation!.numero_orden,
                    CedulaPaciente = r.id_ordenNavigation!.id_pacienteNavigation.cedula_paciente,
                    NombrePaciente = r.id_ordenNavigation!.id_pacienteNavigation.nombre_paciente,
                    FechaResultado = r.fecha_resultado,
                    Anulado = r.anulado ?? false,
                    IdPaciente = r.id_ordenNavigation!.id_pacienteNavigation.id_paciente,
                    Observaciones = r.observaciones
                })
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
            var entidad = await _context.resultados
                .Include(r => r.id_ordenNavigation)
                .ThenInclude(o => o.id_pacienteNavigation)
                .Include(r => r.detalle_resultados).ThenInclude(d => d.id_examenNavigation)
                .FirstOrDefaultAsync(r => r.id_resultado == idResultado);

            if (entidad == null) return null;

            return new ResultadoDetalleDto
            {
                IdResultado = entidad.id_resultado,
                NumeroResultado = entidad.numero_resultado,
                CedulaPaciente = entidad.id_ordenNavigation?.id_pacienteNavigation?.cedula_paciente ?? "",
                NombrePaciente = entidad.id_ordenNavigation?.id_pacienteNavigation?.nombre_paciente ?? "",
                FechaResultado = entidad.fecha_resultado,
                IdPaciente = entidad.id_ordenNavigation?.id_pacienteNavigation?.id_paciente ?? 0,
                Observaciones = entidad.observaciones,
                NumeroOrden = entidad.id_ordenNavigation?.numero_orden ?? "(Sin orden)",
                EstadoPago = entidad.id_ordenNavigation?.estado_pago ?? "DESCONOCIDO",
                Anulado = entidad.anulado ?? false,
                Detalles = entidad.detalle_resultados.Select(d => new DetalleResultadoDto
                {
                    IdDetalleResultado = d.id_detalle_resultado,
                    NombreExamen = d.id_examenNavigation?.nombre_examen ?? "",
                    Valor = d.valor,
                    Unidad = d.unidad ?? "",
                    Observacion = d.observacion ?? "",
                    ValorReferencia = d.valor_referencia ?? "",
                    Anulado = d.anulado ?? false
                }).ToList()
            };
        }

        public async Task<ResultadoCompletoDto?> ObtenerResultadoCompletoAsync(int idResultado)
        {
            var entidad = await _context.resultados
                .Include(r => r.id_ordenNavigation).ThenInclude(o => o.id_pacienteNavigation)
                .Include(r => r.id_ordenNavigation).ThenInclude(o => o.id_medicoNavigation)
                .Include(r => r.detalle_resultados).ThenInclude(d => d.id_examenNavigation)
                .FirstOrDefaultAsync(r => r.id_resultado == idResultado);

            if (entidad == null) return null;

            return new ResultadoCompletoDto
            {
                NumeroOrden = entidad.id_ordenNavigation?.numero_orden ?? "",
                NumeroResultado = entidad.numero_resultado,
                FechaResultado = entidad.fecha_resultado,
                NombrePaciente = entidad.id_ordenNavigation?.id_pacienteNavigation?.nombre_paciente ?? "",
                CedulaPaciente = entidad.id_ordenNavigation?.id_pacienteNavigation?.cedula_paciente ?? "",
                EdadPaciente = CalcularEdad(entidad.id_ordenNavigation?.id_pacienteNavigation?.fecha_nac_paciente),
                MedicoSolicitante = entidad.id_ordenNavigation?.id_medicoNavigation?.nombre_medico ?? "",
                Detalles = new List<ResultadoDetalleDto>
                {
                    new ResultadoDetalleDto
                    {
                        IdResultado = entidad.id_resultado,
                        NumeroResultado = entidad.numero_resultado,
                        CedulaPaciente = entidad.id_ordenNavigation?.id_pacienteNavigation?.cedula_paciente ?? "",
                        NombrePaciente = entidad.id_ordenNavigation?.id_pacienteNavigation?.nombre_paciente ?? "",
                        FechaResultado = entidad.fecha_resultado,
                        Observaciones = entidad.observaciones,
                        Anulado = entidad.anulado ?? false,
                        Detalles = entidad.detalle_resultados.Select(d => new DetalleResultadoDto
                        {
                            IdDetalleResultado = d.id_detalle_resultado,
                            NombreExamen = d.id_examenNavigation?.nombre_examen ?? "",
                            Valor = d.valor,
                            Unidad = d.unidad ?? "",
                            Observacion = d.observacion ?? "",
                            ValorReferencia = d.valor_referencia ?? "",
                            Anulado = d.anulado ?? false
                        }).ToList()
                    }
                }
            };
        }

        public async Task<byte[]?> GenerarResultadosPdfAsync(List<int> ids)
        {
            var resultados = new List<ResultadoCompletoDto>();
            foreach (var idResultado in ids)
            {
                var resultadoCompleto = await ObtenerResultadoCompletoAsync(idResultado);
                if (resultadoCompleto != null) resultados.Add(resultadoCompleto);
            }

            if (!resultados.Any()) return null;
            return _pdfResultadoService.GenerarResultadosPdf(resultados);
        }

        public async Task<bool> AnularResultadoAsync(int idResultado)
        {
            var entidad = await _context.resultados
                .Include(r => r.detalle_resultados)
                .FirstOrDefaultAsync(r => r.id_resultado == idResultado);

            if (entidad == null || entidad.anulado == true) return false;

            entidad.anulado = true;
            foreach (var detalle in entidad.detalle_resultados)
                detalle.anulado = true;

            var detallesOrden = await _context.detalle_ordens
                .Where(d => d.id_resultado == idResultado)
                .ToListAsync();
            foreach (var detalleOrden in detallesOrden)
                detalleOrden.id_resultado = null;

            var hijosDelResultado = entidad.detalle_resultados
                .Select(d => d.id_examen)
                .ToList();

            var padresAsociados = await _context.examen_composicion
                .Where(c => hijosDelResultado.Contains(c.id_examen_hijo))
                .Select(c => c.id_examen_padre)
                .Distinct()
                .ToListAsync();

            if (padresAsociados.Any())
            {
                var detallesOrdenPadres = await _context.detalle_ordens
                    .Where(doe => doe.id_orden == entidad.id_orden && padresAsociados.Contains((int)doe.id_examen))
                    .ToListAsync();

                foreach (var detalleOrdenPadre in detallesOrdenPadres)
                    detalleOrdenPadre.id_resultado = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private int CalcularEdad(DateOnly? fechaNacimiento)
        {
            if (fechaNacimiento == null) return 0;
            var fechaNac = fechaNacimiento.Value.ToDateTime(TimeOnly.MinValue);
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNac.Year;
            if (fechaNac > hoy.AddYears(-edad)) edad--;
            return edad;
        }
    }
}
