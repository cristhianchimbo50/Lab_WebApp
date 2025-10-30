using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Services.PDF;
using Lab_Contracts.Resultados;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Lab_APIRest.Services.Resultados
{
    public class ResultadoService : IResultadoService
    {
        private readonly LabDbContext _context;
        private readonly PdfResultadoService _pdf;
        private readonly ILogger<ResultadoService> _logger;

        public ResultadoService(LabDbContext context, PdfResultadoService pdf, ILogger<ResultadoService> logger)
        {
            _context = context;
            _pdf = pdf;
            _logger = logger;
        }

        public async Task<bool> GuardarResultadosAsync(ResultadoGuardarDto dto)
        {
            using var trx = await _context.Database.BeginTransactionAsync();
            try
            {
                var last = await _context.resultados.OrderByDescending(r => r.id_resultado).FirstOrDefaultAsync();
                string numero = $"RES-{((last?.id_resultado ?? 0) + 1):D5}";

                var res = new resultado
                {
                    numero_resultado = numero,
                    id_paciente = dto.IdPaciente,
                    fecha_resultado = (DateTime)dto.FechaResultado,
                    observaciones = dto.ObservacionesGenerales,
                    id_orden = dto.IdOrden,
                    anulado = false
                };
                _context.resultados.Add(res);
                await _context.SaveChangesAsync();

                foreach (var ex in dto.Examenes)
                {
                    var det = new detalle_resultado
                    {
                        id_resultado = res.id_resultado,
                        id_examen = ex.IdExamen,
                        valor = ex.Valor,
                        unidad = ex.Unidad,
                        observacion = ex.Observacion,
                        valor_referencia = ex.ValorReferencia,
                        anulado = false
                    };
                    _context.detalle_resultados.Add(det);
                    await _context.SaveChangesAsync();

                    var idPadre = await _context.examen_composicion
                        .Where(c => c.id_examen_hijo == ex.IdExamen)
                        .Select(c => c.id_examen_padre)
                        .FirstOrDefaultAsync();

                    if (idPadre != 0)
                    {
                        var detOrdenPadre = await _context.detalle_ordens
                            .FirstOrDefaultAsync(d => d.id_orden == dto.IdOrden && d.id_examen == idPadre);
                        if (detOrdenPadre != null)
                            detOrdenPadre.id_resultado = res.id_resultado;
                    }
                    else
                    {
                        var detOrden = await _context.detalle_ordens
                            .FirstOrDefaultAsync(d => d.id_orden == dto.IdOrden && d.id_examen == ex.IdExamen);
                        if (detOrden != null)
                            detOrden.id_resultado = res.id_resultado;
                    }

                    var examReactivos = await _context.examen_reactivos
                        .Where(er => er.id_examen == ex.IdExamen)
                        .Include(er => er.id_reactivoNavigation)
                        .ToListAsync();

                    if (idPadre != 0)
                    {
                        var reactivosPadre = await _context.examen_reactivos
                            .Where(er => er.id_examen == idPadre)
                            .Include(er => er.id_reactivoNavigation)
                            .ToListAsync();
                        examReactivos.AddRange(reactivosPadre);
                    }

                    foreach (var er in examReactivos)
                    {
                        var reactivo = er.id_reactivoNavigation;
                        if (reactivo == null) continue;

                        if (reactivo.cantidad_disponible < er.cantidad_usada)
                            throw new InvalidOperationException($"Stock insuficiente para {reactivo.nombre_reactivo}");

                        var movimiento = new movimiento_reactivo
                        {
                            id_reactivo = reactivo.id_reactivo,
                            tipo_movimiento = "EGRESO",
                            cantidad = er.cantidad_usada,
                            fecha_movimiento = (DateTime)dto.FechaResultado,
                            observacion = $"Egreso por examen {ex.NombreExamen} en resultado {numero}",
                            id_detalle_resultado = det.id_detalle_resultado
                        };
                        _context.movimiento_reactivos.Add(movimiento);

                        reactivo.cantidad_disponible -= er.cantidad_usada;
                    }
                }

                await _context.SaveChangesAsync();
                await trx.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
               
                _logger.LogError(ex, $"Error guardando resultados de orden {dto.IdOrden} - paciente {dto.IdPaciente}");
                return false;
            }
        }

        public async Task<List<ResultadoListadoDto>> ListarResultadosAsync(ResultadoFiltroDto f)
        {
            var q = _context.resultados.Include(r => r.id_pacienteNavigation).AsQueryable();

            if (!string.IsNullOrWhiteSpace(f.NumeroResultado))
                q = q.Where(r => r.numero_resultado.Contains(f.NumeroResultado));
            if (!string.IsNullOrWhiteSpace(f.Cedula))
                q = q.Where(r => r.id_pacienteNavigation.cedula_paciente.Contains(f.Cedula));
            if (!string.IsNullOrWhiteSpace(f.Nombre))
                q = q.Where(r => r.id_pacienteNavigation.nombre_paciente.Contains(f.Nombre));
            if (f.FechaDesde != null)
                q = q.Where(r => r.fecha_resultado >= f.FechaDesde);
            if (f.FechaHasta != null)
                q = q.Where(r => r.fecha_resultado <= f.FechaHasta);
            if (f.Anulado != null)
                q = q.Where(r => r.anulado == f.Anulado);

            return await q.OrderByDescending(r => r.id_resultado)
                .Select(r => new ResultadoListadoDto
                {
                    IdResultado = r.id_resultado,
                    NumeroResultado = r.numero_resultado,
                    CedulaPaciente = r.id_pacienteNavigation.cedula_paciente,
                    NombrePaciente = r.id_pacienteNavigation.nombre_paciente,
                    FechaResultado = r.fecha_resultado,
                    Anulado = r.anulado ?? false,
                    IdPaciente = (int)r.id_paciente,
                    Observaciones = r.observaciones
                }).ToListAsync();
        }

        public async Task<ResultadoDetalleDto?> ObtenerDetalleResultadoAsync(int id)
        {
            var r = await _context.resultados
                .Include(x => x.id_pacienteNavigation)
                .Include(x => x.detalle_resultados).ThenInclude(d => d.id_examenNavigation)
                .FirstOrDefaultAsync(x => x.id_resultado == id);

            if (r == null) return null;

            return new ResultadoDetalleDto
            {
                IdResultado = r.id_resultado,
                NumeroResultado = r.numero_resultado,
                CedulaPaciente = r.id_pacienteNavigation?.cedula_paciente ?? "",
                NombrePaciente = r.id_pacienteNavigation?.nombre_paciente ?? "",
                FechaResultado = r.fecha_resultado,
                IdPaciente = r.id_paciente ?? 0,
                Observaciones = r.observaciones,
                Anulado = r.anulado ?? false,
                Detalles = r.detalle_resultados.Select(d => new DetalleResultadoDto
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

        public async Task<ResultadoCompletoDto?> ObtenerResultadoCompletoAsync(int id)
        {
            var r = await _context.resultados
                .Include(x => x.id_pacienteNavigation)
                .Include(x => x.id_ordenNavigation).ThenInclude(o => o.id_medicoNavigation)
                .Include(x => x.detalle_resultados).ThenInclude(d => d.id_examenNavigation)
                .FirstOrDefaultAsync(x => x.id_resultado == id);

            if (r == null) return null;

            return new ResultadoCompletoDto
            {
                NumeroOrden = r.id_ordenNavigation?.numero_orden ?? "",
                NumeroResultado = r.numero_resultado,
                FechaResultado = r.fecha_resultado,
                NombrePaciente = r.id_pacienteNavigation?.nombre_paciente ?? "",
                CedulaPaciente = r.id_pacienteNavigation?.cedula_paciente ?? "",
                EdadPaciente = CalcularEdad(r.id_pacienteNavigation?.fecha_nac_paciente),
                MedicoSolicitante = r.id_ordenNavigation?.id_medicoNavigation?.nombre_medico ?? "",
                Detalles = new List<ResultadoDetalleDto>
                {
                    new ResultadoDetalleDto
                    {
                        IdResultado = r.id_resultado,
                        NumeroResultado = r.numero_resultado,
                        CedulaPaciente = r.id_pacienteNavigation?.cedula_paciente ?? "",
                        NombrePaciente = r.id_pacienteNavigation?.nombre_paciente ?? "",
                        FechaResultado = r.fecha_resultado,
                        Observaciones = r.observaciones,
                        Anulado = r.anulado ?? false,
                        Detalles = r.detalle_resultados.Select(d => new DetalleResultadoDto
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
            foreach (var id in ids)
            {
                var r = await ObtenerResultadoCompletoAsync(id);
                if (r != null) resultados.Add(r);
            }

            if (!resultados.Any()) return null;
            return _pdf.GenerarResultadosPdf(resultados);
        }

        public async Task<bool> AnularResultadoAsync(int id)
        {
            var r = await _context.resultados
                .Include(x => x.detalle_resultados)
                .FirstOrDefaultAsync(x => x.id_resultado == id);

            if (r == null || r.anulado == true) return false;

            r.anulado = true;
            foreach (var dr in r.detalle_resultados)
                dr.anulado = true;

            var detOrden = await _context.detalle_ordens
                .Where(d => d.id_resultado == id)
                .ToListAsync();
            foreach (var d in detOrden)
                d.id_resultado = null;

            var examenesPadres = await _context.examen_composicion
                .Select(c => c.id_examen_padre)
                .Distinct()
                .ToListAsync();

            var hijosDelResultado = r.detalle_resultados
                .Select(dr => dr.id_examen)
                .ToList();

            var padresAsociados = await _context.examen_composicion
                .Where(c => hijosDelResultado.Contains(c.id_examen_hijo))
                .Select(c => c.id_examen_padre)
                .Distinct()
                .ToListAsync();

            if (padresAsociados.Any())
            {
                var detOrdenPadres = await _context.detalle_ordens
                    .Where(d => d.id_orden == r.id_orden && padresAsociados.Contains((int)d.id_examen))
                    .ToListAsync();

                foreach (var dp in detOrdenPadres)
                    dp.id_resultado = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private int CalcularEdad(DateOnly? fn)
        {
            if (fn == null) return 0;
            var f = fn.Value.ToDateTime(TimeOnly.MinValue);
            var h = DateTime.Today;
            var edad = h.Year - f.Year;
            if (f > h.AddYears(-edad)) edad--;
            return edad;
        }
    }
}
