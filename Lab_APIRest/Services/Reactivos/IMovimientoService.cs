using Lab_Contracts.Reactivos;
using Lab_Contracts.Common;

namespace Lab_APIRest.Services.Reactivos
{
    public interface IMovimientoService
    {
        Task<List<MovimientoReactivoDto>> ListarMovimientosAsync(MovimientoReactivoFiltroDto filtro);
        Task<ResultadoPaginadoDto<MovimientoReactivoDto>> ListarMovimientosPaginadosAsync(MovimientoReactivoFiltroDto filtro);
    }
}
