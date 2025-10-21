using Lab_Contracts.Reactivos;

namespace Lab_Blazor.Services.Reactivos
{
    public interface IMovimientosApiService
    {
        Task<List<MovimientoReactivoDto>> FiltrarMovimientosAsync(MovimientoReactivoFiltroDto filtro);
    }
}
