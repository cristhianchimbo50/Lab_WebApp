using Lab_Contracts.Resultados;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Resultados
{
    public interface IResultadosApiService
    {
        Task<List<ResultadoListadoDto>> ObtenerResultadosAsync(ResultadoFiltroDto Filtro);
        Task<ResultadoDetalleDto?> ObtenerDetalleResultadoAsync(int IdResultado);
        Task<byte[]> ObtenerResultadosPdfAsync(List<int> IdsResultados);
        Task<bool> AnularResultadoAsync(int IdResultado);
        Task<List<ResultadoListadoDto>> ObtenerResultadosPacienteAsync();
        Task<ResultadoDetalleDto?> ObtenerDetalleResultadoPacienteAsync(int IdResultado);
        Task<ResultadoPaginadoDto<ResultadoListadoDto>> BuscarResultadosPaginadosAsync(ResultadoFiltroDto Filtro);
    }
}
