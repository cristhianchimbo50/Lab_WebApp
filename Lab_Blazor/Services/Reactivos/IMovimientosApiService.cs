using Lab_Contracts.Reactivos;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Reactivos
{
    public interface IMovimientosApiService
    {
        Task<List<MovimientoReactivoDto>> FiltrarMovimientosAsync(MovimientoReactivoFiltroDto Filtro);
        Task<ResultadoPaginadoDto<MovimientoReactivoDto>> BuscarMovimientosAsync(MovimientoReactivoFiltroDto Filtro);
    }
}
