using Lab_Contracts.Resultados;

namespace Lab_APIRest.Services.Resultados
{
    public interface IResultadoService
    {
        Task<bool> GuardarResultadosAsync(ResultadoGuardarDto Resultado);
        Task<List<ResultadoListadoDto>> ListarResultadosAsync(ResultadoFiltroDto Filtro);
        Task<ResultadoDetalleDto?> ObtenerDetalleResultadoAsync(int IdResultado);
        Task<ResultadoCompletoDto?> ObtenerResultadoCompletoAsync(int IdResultado);
        Task<byte[]?> GenerarResultadosPdfAsync(List<int> Ids);
        Task<bool> AnularResultadoAsync(int IdResultado);
    }
}   