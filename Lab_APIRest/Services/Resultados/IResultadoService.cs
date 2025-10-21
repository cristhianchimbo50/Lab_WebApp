using Lab_Contracts.Resultados;

namespace Lab_APIRest.Services.Resultados
{
    public interface IResultadoService
    {
        Task<bool> GuardarResultadosAsync(ResultadoGuardarDto dto);
        Task<List<ResultadoListadoDto>> ListarResultadosAsync(ResultadoFiltroDto filtro);
        Task<ResultadoDetalleDto?> ObtenerDetalleResultadoAsync(int idResultado);
        Task<ResultadoCompletoDto?> ObtenerResultadoCompletoAsync(int idResultado);
        Task<byte[]?> GenerarResultadosPdfAsync(List<int> ids);
        Task<bool> AnularResultadoAsync(int idResultado);
    }
}
   