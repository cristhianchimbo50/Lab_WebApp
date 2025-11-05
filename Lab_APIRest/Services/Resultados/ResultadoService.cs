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
        private readonly LabDbContext Contexto;
        private readonly PdfResultadoService PdfResultadoService;
        private readonly ILogger<ResultadoService> Logger;

        public ResultadoService(LabDbContext Contexto, PdfResultadoService PdfResultadoService, ILogger<ResultadoService> Logger)
        {
            this.Contexto = Contexto;
            this.PdfResultadoService = PdfResultadoService;
            this.Logger = Logger;
        }

        public async Task<bool> GuardarResultadosAsync(ResultadoGuardarDto Resultado)
        {
            using var Transaccion = await Contexto.Database.BeginTransactionAsync();
            try
            {
                var UltimoResultado = await Contexto.resultados.OrderByDescending(r => r.id_resultado).FirstOrDefaultAsync();
                string NumeroGenerado = $"RES-{((UltimoResultado?.id_resultado ?? 0) + 1):D5}";

                var EntidadResultado = new resultado
                {
                    numero_resultado = NumeroGenerado,
                    id_paciente = Resultado.IdPaciente,
                    fecha_resultado = (DateTime)Resultado.FechaResultado,
                    observaciones = Resultado.ObservacionesGenerales,
                    id_orden = Resultado.IdOrden,
                    anulado = false
                };
                Contexto.resultados.Add(EntidadResultado);
                await Contexto.SaveChangesAsync();

                foreach (var Examen in Resultado.Examenes)
                {
                    var DetalleResultado = new detalle_resultado
                    {
                        id_resultado = EntidadResultado.id_resultado,
                        id_examen = Examen.IdExamen,
                        valor = Examen.Valor,
                        unidad = Examen.Unidad,
                        observacion = Examen.Observacion,
                        valor_referencia = Examen.ValorReferencia,
                        anulado = false
                    };
                    Contexto.detalle_resultados.Add(DetalleResultado);
                    await Contexto.SaveChangesAsync();

                    var IdExamenPadre = await Contexto.examen_composicion
                        .Where(c => c.id_examen_hijo == Examen.IdExamen)
                        .Select(c => c.id_examen_padre)
                        .FirstOrDefaultAsync();

                    if (IdExamenPadre != 0)
                    {
                        var DetalleOrdenPadre = await Contexto.detalle_ordens
                            .FirstOrDefaultAsync(d => d.id_orden == Resultado.IdOrden && d.id_examen == IdExamenPadre);
                        if (DetalleOrdenPadre != null)
                            DetalleOrdenPadre.id_resultado = EntidadResultado.id_resultado;
                    }
                    else
                    {
                        var DetalleOrden = await Contexto.detalle_ordens
                            .FirstOrDefaultAsync(d => d.id_orden == Resultado.IdOrden && d.id_examen == Examen.IdExamen);
                        if (DetalleOrden != null)
                            DetalleOrden.id_resultado = EntidadResultado.id_resultado;
                    }

                    var ReactivosDelExamen = await Contexto.examen_reactivos
                        .Where(er => er.id_examen == Examen.IdExamen)
                        .Include(er => er.id_reactivoNavigation)
                        .ToListAsync();

                    if (IdExamenPadre != 0)
                    {
                        var ReactivosDelPadre = await Contexto.examen_reactivos
                            .Where(er => er.id_examen == IdExamenPadre)
                            .Include(er => er.id_reactivoNavigation)
                            .ToListAsync();
                        ReactivosDelExamen.AddRange(ReactivosDelPadre);
                    }

                    foreach (var Asociacion in ReactivosDelExamen)
                    {
                        var ReactivoAsociado = Asociacion.id_reactivoNavigation;
                        if (ReactivoAsociado == null) continue;

                        if (ReactivoAsociado.cantidad_disponible < Asociacion.cantidad_usada)
                            throw new InvalidOperationException($"Stock insuficiente para {ReactivoAsociado.nombre_reactivo}");

                        var MovimientoEntidad = new movimiento_reactivo
                        {
                            id_reactivo = ReactivoAsociado.id_reactivo,
                            tipo_movimiento = "EGRESO",
                            cantidad = Asociacion.cantidad_usada,
                            fecha_movimiento = (DateTime)Resultado.FechaResultado,
                            observacion = $"Egreso por examen {Examen.NombreExamen} en resultado {NumeroGenerado}",
                            id_detalle_resultado = DetalleResultado.id_detalle_resultado
                        };
                        Contexto.movimiento_reactivos.Add(MovimientoEntidad);

                        ReactivoAsociado.cantidad_disponible -= Asociacion.cantidad_usada;
                    }
                }

                await Contexto.SaveChangesAsync();
                await Transaccion.CommitAsync();
                return true;
            }
            catch (Exception Ex)
            {
                await Transaccion.RollbackAsync();
                
                Logger.LogError(Ex, $"Error guardando resultados de orden {Resultado.IdOrden} - paciente {Resultado.IdPaciente}");
                return false;
            }
        }

        public async Task<List<ResultadoListadoDto>> ListarResultadosAsync(ResultadoFiltroDto Filtro)
        {
            var Consulta = Contexto.resultados
                .Include(Resultado => Resultado.id_pacienteNavigation)
                .Include(Resultado => Resultado.id_ordenNavigation)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(Filtro.NumeroResultado))
                Consulta = Consulta.Where(Resultado => Resultado.numero_resultado.Contains(Filtro.NumeroResultado));
            if (!string.IsNullOrWhiteSpace(Filtro.NumeroOrden))
                Consulta = Consulta.Where(Resultado => Resultado.id_ordenNavigation != null &&
                                 Resultado.id_ordenNavigation.numero_orden.Contains(Filtro.NumeroOrden));
            if (!string.IsNullOrWhiteSpace(Filtro.Cedula))
                Consulta = Consulta.Where(Resultado => Resultado.id_pacienteNavigation.cedula_paciente.Contains(Filtro.Cedula));
            if (!string.IsNullOrWhiteSpace(Filtro.Nombre))
                Consulta = Consulta.Where(Resultado => Resultado.id_pacienteNavigation.nombre_paciente.Contains(Filtro.Nombre));
            if (Filtro.FechaDesde != null)
                Consulta = Consulta.Where(Resultado => Resultado.fecha_resultado >= Filtro.FechaDesde);
            if (Filtro.FechaHasta != null)
                Consulta = Consulta.Where(Resultado => Resultado.fecha_resultado <= Filtro.FechaHasta);
            if (Filtro.Anulado != null)
                Consulta = Consulta.Where(Resultado => Resultado.anulado == Filtro.Anulado);

            return await Consulta.OrderByDescending(Resultado => Resultado.id_resultado)
                .Select(Resultado => new ResultadoListadoDto
                {
                    IdResultado = Resultado.id_resultado,
                    NumeroResultado = Resultado.numero_resultado,
                    NumeroOrden = Resultado.id_ordenNavigation!.numero_orden,
                    CedulaPaciente = Resultado.id_pacienteNavigation.cedula_paciente,
                    NombrePaciente = Resultado.id_pacienteNavigation.nombre_paciente,
                    FechaResultado = Resultado.fecha_resultado,
                    Anulado = Resultado.anulado ?? false,
                    IdPaciente = (int)Resultado.id_paciente,
                    Observaciones = Resultado.observaciones
                }).ToListAsync();
        }

        public async Task<ResultadoDetalleDto?> ObtenerDetalleResultadoAsync(int IdResultado)
        {
            var ResultadoEntidad = await Contexto.resultados
                .Include(Resultado => Resultado.id_pacienteNavigation)
                .Include(Resultado => Resultado.id_ordenNavigation)
                .Include(Resultado => Resultado.detalle_resultados).ThenInclude(Detalle => Detalle.id_examenNavigation)
                .FirstOrDefaultAsync(Resultado => Resultado.id_resultado == IdResultado);

            if (ResultadoEntidad == null) return null;

            return new ResultadoDetalleDto
            {
                IdResultado = ResultadoEntidad.id_resultado,
                NumeroResultado = ResultadoEntidad.numero_resultado,
                CedulaPaciente = ResultadoEntidad.id_pacienteNavigation?.cedula_paciente ?? "",
                NombrePaciente = ResultadoEntidad.id_pacienteNavigation?.nombre_paciente ?? "",
                FechaResultado = ResultadoEntidad.fecha_resultado,
                IdPaciente = ResultadoEntidad.id_paciente ?? 0,
                Observaciones = ResultadoEntidad.observaciones,
                NumeroOrden = ResultadoEntidad.id_ordenNavigation?.numero_orden ?? "(Sin orden)",
                EstadoPago = ResultadoEntidad.id_ordenNavigation?.estado_pago ?? "DESCONOCIDO",
                Anulado = ResultadoEntidad.anulado ?? false,
                Detalles = ResultadoEntidad.detalle_resultados.Select(Detalle => new DetalleResultadoDto
                {
                    IdDetalleResultado = Detalle.id_detalle_resultado,
                    NombreExamen = Detalle.id_examenNavigation?.nombre_examen ?? "",
                    Valor = Detalle.valor,
                    Unidad = Detalle.unidad ?? "",
                    Observacion = Detalle.observacion ?? "",
                    ValorReferencia = Detalle.valor_referencia ?? "",
                    Anulado = Detalle.anulado ?? false
                }).ToList()
            };
        }

        public async Task<ResultadoCompletoDto?> ObtenerResultadoCompletoAsync(int IdResultado)
        {
            var ResultadoEntidad = await Contexto.resultados
                .Include(Resultado => Resultado.id_pacienteNavigation)
                .Include(Resultado => Resultado.id_ordenNavigation).ThenInclude(Orden => Orden.id_medicoNavigation)
                .Include(Resultado => Resultado.detalle_resultados).ThenInclude(Detalle => Detalle.id_examenNavigation)
                .FirstOrDefaultAsync(Resultado => Resultado.id_resultado == IdResultado);

            if (ResultadoEntidad == null) return null;

            return new ResultadoCompletoDto
            {
                NumeroOrden = ResultadoEntidad.id_ordenNavigation?.numero_orden ?? "",
                NumeroResultado = ResultadoEntidad.numero_resultado,
                FechaResultado = ResultadoEntidad.fecha_resultado,
                NombrePaciente = ResultadoEntidad.id_pacienteNavigation?.nombre_paciente ?? "",
                CedulaPaciente = ResultadoEntidad.id_pacienteNavigation?.cedula_paciente ?? "",
                EdadPaciente = CalcularEdad(ResultadoEntidad.id_pacienteNavigation?.fecha_nac_paciente),
                MedicoSolicitante = ResultadoEntidad.id_ordenNavigation?.id_medicoNavigation?.nombre_medico ?? "",
                Detalles = new List<ResultadoDetalleDto>
                {
                    new ResultadoDetalleDto
                    {
                        IdResultado = ResultadoEntidad.id_resultado,
                        NumeroResultado = ResultadoEntidad.numero_resultado,
                        CedulaPaciente = ResultadoEntidad.id_pacienteNavigation?.cedula_paciente ?? "",
                        NombrePaciente = ResultadoEntidad.id_pacienteNavigation?.nombre_paciente ?? "",
                        FechaResultado = ResultadoEntidad.fecha_resultado,
                        Observaciones = ResultadoEntidad.observaciones,
                        Anulado = ResultadoEntidad.anulado ?? false,
                        Detalles = ResultadoEntidad.detalle_resultados.Select(Detalle => new DetalleResultadoDto
                        {
                            IdDetalleResultado = Detalle.id_detalle_resultado,
                            NombreExamen = Detalle.id_examenNavigation?.nombre_examen ?? "",
                            Valor = Detalle.valor,
                            Unidad = Detalle.unidad ?? "",
                            Observacion = Detalle.observacion ?? "",
                            ValorReferencia = Detalle.valor_referencia ?? "",
                            Anulado = Detalle.anulado ?? false
                        }).ToList()
                    }
                }

            };
        }

        public async Task<byte[]?> GenerarResultadosPdfAsync(List<int> Ids)
        {
            var Resultados = new List<ResultadoCompletoDto>();
            foreach (var IdResultado in Ids)
            {
                var ResultadoCompleto = await ObtenerResultadoCompletoAsync(IdResultado);
                if (ResultadoCompleto != null) Resultados.Add(ResultadoCompleto);
            }

            if (!Resultados.Any()) return null;
            return PdfResultadoService.GenerarResultadosPdf(Resultados);
        }

        public async Task<bool> AnularResultadoAsync(int IdResultado)
        {
            var ResultadoEntidad = await Contexto.resultados
                .Include(Resultado => Resultado.detalle_resultados)
                .FirstOrDefaultAsync(Resultado => Resultado.id_resultado == IdResultado);

            if (ResultadoEntidad == null || ResultadoEntidad.anulado == true) return false;

            ResultadoEntidad.anulado = true;
            foreach (var Detalle in ResultadoEntidad.detalle_resultados)
                Detalle.anulado = true;

            var DetallesOrden = await Contexto.detalle_ordens
                .Where(Detalle => Detalle.id_resultado == IdResultado)
                .ToListAsync();
            foreach (var DetalleOrden in DetallesOrden)
                DetalleOrden.id_resultado = null;

            var ExamenesPadres = await Contexto.examen_composicion
                .Select(Composicion => Composicion.id_examen_padre)
                .Distinct()
                .ToListAsync();

            var HijosDelResultado = ResultadoEntidad.detalle_resultados
                .Select(Detalle => Detalle.id_examen)
                .ToList();

            var PadresAsociados = await Contexto.examen_composicion
                .Where(Composicion => HijosDelResultado.Contains(Composicion.id_examen_hijo))
                .Select(Composicion => Composicion.id_examen_padre)
                .Distinct()
                .ToListAsync();

            if (PadresAsociados.Any())
            {
                var DetallesOrdenPadres = await Contexto.detalle_ordens
                    .Where(DetalleOrden => DetalleOrden.id_orden == ResultadoEntidad.id_orden && PadresAsociados.Contains((int)DetalleOrden.id_examen))
                    .ToListAsync();

                foreach (var DetalleOrdenPadre in DetallesOrdenPadres)
                    DetalleOrdenPadre.id_resultado = null;
            }

            await Contexto.SaveChangesAsync();
            return true;
        }

        private int CalcularEdad(DateOnly? FechaNacimiento)
        {
            if (FechaNacimiento == null) return 0;
            var FechaNac = FechaNacimiento.Value.ToDateTime(TimeOnly.MinValue);
            var Hoy = DateTime.Today;
            var Edad = Hoy.Year - FechaNac.Year;
            if (FechaNac > Hoy.AddYears(-Edad)) Edad--;
            return Edad;
        }
    }
}
