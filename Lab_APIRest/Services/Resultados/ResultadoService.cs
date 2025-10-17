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

    }
}
