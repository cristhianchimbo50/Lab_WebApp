using Lab_Contracts.Resultados;

namespace Lab_Blazor.Services.Resultados
{
    public interface IResultadosApiService
    {
        Task<List<ResultadoListadoDto>> GetResultadosAsync(ResultadoFiltroDto filtro);
        Task<ResultadoDetalleDto?> GetDetalleResultadoAsync(int idResultado);
        Task<byte[]> ObtenerResultadosPdfAsync(List<int> idsResultados);
        Task<bool> AnularResultadoAsync(int idResultado);
    }
}
