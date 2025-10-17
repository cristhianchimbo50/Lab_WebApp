using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_Contracts.Resultados;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Resultados
{
    public class ResultadoService : IResultadoService
    {
        private readonly LabDbContext _context;

        public ResultadoService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<bool> GuardarResultadosAsync(ResultadoGuardarDto dto)
        {
            var lastResult = await _context.resultados.OrderByDescending(r => r.id_resultado).FirstOrDefaultAsync();
            int nextNum = lastResult?.id_resultado + 1 ?? 1;
            string numeroResultado = $"RES-{nextNum.ToString("D5")}";

            var resultado = new resultado
            {
                numero_resultado = numeroResultado,
                id_paciente = dto.IdPaciente,
                fecha_resultado = dto.FechaResultado,
                observaciones = dto.ObservacionesGenerales,
                id_orden = dto.IdOrden,
                anulado = false
            };
            _context.resultados.Add(resultado);
            await _context.SaveChangesAsync();

            foreach (var ex in dto.Examenes)
            {
                var detRes = new detalle_resultado
                {
                    id_resultado = resultado.id_resultado,
                    id_examen = ex.IdExamen,
                    valor = ex.Valor,
                    unidad = ex.Unidad,
                    observacion = ex.Observacion,
                    valor_referencia = ex.ValorReferencia,
                    anulado = false
                };
                _context.detalle_resultados.Add(detRes);

                var detalleOrden = await _context.detalle_ordens
                    .FirstOrDefaultAsync(detalle => detalle.id_orden == dto.IdOrden && detalle.id_examen == ex.IdExamen);


                if (detalleOrden != null)
                {
                    detalleOrden.id_resultado = resultado.id_resultado;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ResultadoListadoDto>> ListarResultadosAsync(ResultadoFiltroDto filtro)
        {
            var query = _context.resultados
                .Include(r => r.id_pacienteNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro.NumeroResultado))
                query = query.Where(r => r.numero_resultado.Contains(filtro.NumeroResultado));
            if (!string.IsNullOrWhiteSpace(filtro.Cedula))
                query = query.Where(r => r.id_pacienteNavigation.cedula_paciente.Contains(filtro.Cedula));
            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                query = query.Where(r => r.id_pacienteNavigation.nombre_paciente.Contains(filtro.Nombre));
            if (filtro.FechaDesde != null)
                query = query.Where(r => r.fecha_resultado >= filtro.FechaDesde);
            if (filtro.FechaHasta != null)
                query = query.Where(r => r.fecha_resultado <= filtro.FechaHasta);
            if (filtro.Anulado != null)
                query = query.Where(r => r.anulado == filtro.Anulado);

            return await query
                .OrderByDescending(r => r.id_resultado)
                .Select(r => new ResultadoListadoDto
                {
                    IdResultado = r.id_resultado,
                    NumeroResultado = r.numero_resultado,
                    CedulaPaciente = r.id_pacienteNavigation.cedula_paciente,
                    NombrePaciente = r.id_pacienteNavigation.nombre_paciente,
                    FechaResultado = r.fecha_resultado,
                    Anulado = (bool)r.anulado,
                    Observaciones = r.observaciones
                }).ToListAsync();
        }

        public async Task<ResultadoDetalleDto?> ObtenerDetalleResultadoAsync(int idResultado)
        {
            var r = await _context.resultados
                .Include(res => res.id_pacienteNavigation)
                .Include(res => res.detalle_resultados)
                    .ThenInclude(dr => dr.id_examenNavigation)
                .FirstOrDefaultAsync(res => res.id_resultado == idResultado);

            if (r == null) return null;

            return new ResultadoDetalleDto
            {
                IdResultado = r.id_resultado,
                NumeroResultado = r.numero_resultado,
                CedulaPaciente = r.id_pacienteNavigation?.cedula_paciente ?? "",
                NombrePaciente = r.id_pacienteNavigation?.nombre_paciente ?? "",
                FechaResultado = r.fecha_resultado,
                Observaciones = r.observaciones,
                Anulado = (bool)r.anulado,
                Detalles = r.detalle_resultados.Select(dr => new DetalleResultadoDto
                {
                    IdDetalleResultado = dr.id_detalle_resultado,
                    NombreExamen = dr.id_examenNavigation?.nombre_examen ?? "",
                    Valor = dr.valor.ToString() ?? "",
                    Unidad = dr.unidad ?? "",
                    Observacion = dr.observacion ?? "",
                    ValorReferencia = dr.valor_referencia ?? "",
                    Anulado = dr.anulado ?? false
                }).ToList()
            };
        }

        public async Task<ResultadoCompletoDto?> ObtenerResultadoCompletoAsync(int idResultado)
        {
            var resultado = await _context.resultados
                .Include(r => r.id_pacienteNavigation)
                .Include(r => r.id_ordenNavigation)
                    .ThenInclude(o => o.id_medicoNavigation)
                .Include(r => r.detalle_resultados)
                    .ThenInclude(d => d.id_examenNavigation)
                .FirstOrDefaultAsync(r => r.id_resultado == idResultado);


            if (resultado == null)
                return null;

            return new ResultadoCompletoDto
            {
                NumeroOrden = resultado.id_ordenNavigation?.numero_orden ?? "",
                NumeroResultado = resultado.numero_resultado,
                FechaResultado = resultado.fecha_resultado,
                NombrePaciente = resultado.id_pacienteNavigation?.nombre_paciente ?? "",
                CedulaPaciente = resultado.id_pacienteNavigation?.cedula_paciente ?? "",
                EdadPaciente = resultado.id_pacienteNavigation != null ? CalcularEdad(resultado.id_pacienteNavigation.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue)) : 0,
                MedicoSolicitante = resultado.id_ordenNavigation?.id_medicoNavigation?.nombre_medico ?? "",
                Detalles = new List<ResultadoDetalleDto>
                {
                    new ResultadoDetalleDto
                    {
                        IdResultado = resultado.id_resultado,
                        NumeroResultado = resultado.numero_resultado,
                        CedulaPaciente = resultado.id_pacienteNavigation?.cedula_paciente ?? "",
                        NombrePaciente = resultado.id_pacienteNavigation?.nombre_paciente ?? "",
                        FechaResultado = resultado.fecha_resultado,
                        Anulado = resultado.anulado ?? false,
                        Detalles = resultado.detalle_resultados.Select(d => new DetalleResultadoDto
                        {
                            IdDetalleResultado = d.id_detalle_resultado,
                            NombreExamen = d.id_examenNavigation.nombre_examen,
                            Valor = d.valor,
                            Unidad = d.unidad,
                            Observacion = d.observacion,
                            TituloExamen = d.id_examenNavigation.titulo_examen,
                            ValorReferencia = d.valor_referencia ?? "",
                            Anulado = d.anulado ?? false
                        }).ToList()

                    }
                }
            };
        }

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        public async Task<bool> AnularResultadoAsync(int idResultado)
        {
            var resultado = await _context.resultados
                .Include(r => r.detalle_resultados)
                .FirstOrDefaultAsync(r => r.id_resultado == idResultado);

            if (resultado == null)
                return false;

            resultado.anulado = true;

            foreach (var dr in resultado.detalle_resultados)
            {
                dr.anulado = true;
            }

            var detallesOrden = await _context.detalle_ordens
                .Where(d => d.id_resultado == idResultado)
                .ToListAsync();

            foreach (var det in detallesOrden)
            {
                det.id_resultado = null;
            }

            await _context.SaveChangesAsync();
            return true;
        }


    }
}
