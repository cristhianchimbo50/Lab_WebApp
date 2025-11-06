using Lab_Contracts.Reactivos;
using Lab_Contracts.Common;

namespace Lab_APIRest.Services.Reactivos
{
    public interface IReactivoService
    {
        Task<List<ReactivoDto>> ListarReactivosAsync();
        Task<ReactivoDto?> ObtenerDetalleReactivoAsync(int idReactivo);
        Task<ReactivoDto> GuardarReactivoAsync(ReactivoDto reactivo);
        Task<bool> GuardarReactivoAsync(int idReactivo, ReactivoDto reactivo);
        Task<bool> AnularReactivoAsync(int idReactivo);
        Task<bool> RegistrarIngresosReactivosAsync(IEnumerable<MovimientoReactivoIngresoDto> ingresos);
        Task<bool> RegistrarEgresosReactivosAsync(IEnumerable<MovimientoReactivoEgresoDto> egresos);
        Task<ResultadoPaginadoDto<ReactivoDto>> ListarReactivosPaginadosAsync(ReactivoFiltroDto filtro);
    }
}
