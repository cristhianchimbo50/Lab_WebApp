using Lab_Contracts.Reactivos;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Reactivos
{
    public interface IReactivosApiService
    {
        Task<List<ReactivoDto>> ListarReactivosAsync();
        Task<ReactivoDto?> ObtenerDetalleReactivoAsync(int idReactivo);
        Task<HttpResponseMessage> GuardarReactivoAsync(ReactivoDto reactivo);
        Task<HttpResponseMessage> GuardarReactivoAsync(int idReactivo, ReactivoDto reactivo);
        Task<HttpResponseMessage> AnularReactivoAsync(int idReactivo);
        Task<HttpResponseMessage> RegistrarIngresosReactivosAsync(IEnumerable<MovimientoReactivoIngresoDto> ingresos);
        Task<HttpResponseMessage> RegistrarEgresosReactivosAsync(IEnumerable<MovimientoReactivoEgresoDto> egresos);
        Task<ResultadoPaginadoDto<ReactivoDto>> ListarReactivosPaginadosAsync(ReactivoFiltroDto filtro);
    }
}
