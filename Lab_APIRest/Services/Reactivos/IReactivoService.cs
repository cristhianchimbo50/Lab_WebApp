using Lab_Contracts.Reactivos;

namespace Lab_APIRest.Services.Reactivos
{
    public interface IReactivoService
    {
        Task<List<ReactivoDto>> ObtenerReactivosAsync();
        Task<ReactivoDto?> ObtenerReactivoPorIdAsync(int IdReactivo);
        Task<ReactivoDto> CrearReactivoAsync(ReactivoDto Reactivo);
        Task<bool> EditarReactivoAsync(int IdReactivo, ReactivoDto Reactivo);
        Task<bool> AnularReactivoAsync(int IdReactivo);
        Task<bool> RegistrarIngresosAsync(IEnumerable<MovimientoReactivoIngresoDto> Ingresos);
        Task<bool> RegistrarEgresosAsync(IEnumerable<MovimientoReactivoEgresoDto> Egresos);
    }
}
