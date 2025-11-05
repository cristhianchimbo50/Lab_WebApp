using Lab_Contracts.Reactivos;

namespace Lab_APIRest.Services.Reactivos
{
    public interface IMovimientoService
    {
        Task<List<MovimientoReactivoDto>> ObtenerMovimientosAsync(MovimientoReactivoFiltroDto Filtro);
    }
}
