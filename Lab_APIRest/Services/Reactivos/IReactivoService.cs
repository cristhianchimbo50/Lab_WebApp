using Lab_Contracts.Reactivos;

namespace Lab_APIRest.Services.Reactivos
{
    public interface IReactivoService
    {
        Task<List<ReactivoDto>> GetReactivosAsync();
        Task<ReactivoDto?> GetReactivoPorIdAsync(int id);
        Task<ReactivoDto> CrearReactivoAsync(ReactivoDto dto);
        Task<bool> EditarReactivoAsync(int id, ReactivoDto dto);
        Task<bool> AnularReactivoAsync(int id);
        Task<bool> RegistrarIngresosAsync(IEnumerable<MovimientoReactivoIngresoDto> ingresos);
        Task<bool> RegistrarEgresosAsync(IEnumerable<MovimientoReactivoEgresoDto> egresos);
    }
}
