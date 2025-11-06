using Lab_Contracts.Resultados;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Resultados
{
    public interface IResultadosApiService
    {
        Task<List<ResultadoListadoDto>> ListarResultadosAsync(ResultadoFiltroDto filtro);
        Task<ResultadoPaginadoDto<ResultadoListadoDto>> ListarResultadosPaginadosAsync(ResultadoFiltroDto filtro);
        Task<ResultadoDetalleDto?> ObtenerDetalleResultadoAsync(int idResultado);
        Task<byte[]> GenerarResultadosPdfAsync(List<int> idsResultados);
        Task<bool> AnularResultadoAsync(int idResultado);
        Task<List<ResultadoListadoDto>> ListarResultadosPacienteAsync();
        Task<ResultadoDetalleDto?> ObtenerDetalleResultadoPacienteAsync(int idResultado);
    }
}
