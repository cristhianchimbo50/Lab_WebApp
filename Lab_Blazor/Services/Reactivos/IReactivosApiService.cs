using Lab_Contracts.Reactivos;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Reactivos
{
    public interface IReactivosApiService
    {
        Task<List<ReactivoDto>> ObtenerReactivosAsync();
        Task<ReactivoDto?> ObtenerReactivoPorIdAsync(int IdReactivo);
        Task<HttpResponseMessage> CrearReactivoAsync(ReactivoDto Reactivo);
        Task<HttpResponseMessage> EditarReactivoAsync(int IdReactivo, ReactivoDto Reactivo);
        Task<HttpResponseMessage> AnularReactivoAsync(int IdReactivo);
        Task<HttpResponseMessage> RegistrarIngresosAsync(IEnumerable<MovimientoReactivoIngresoDto> Ingresos);
        Task<HttpResponseMessage> RegistrarEgresosAsync(IEnumerable<MovimientoReactivoEgresoDto> Egresos);
        Task<ResultadoPaginadoDto<ReactivoDto>> BuscarReactivosAsync(ReactivoFiltroDto filtro);
    }
}
