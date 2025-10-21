using Lab_Contracts.Reactivos;

namespace Lab_Blazor.Services.Reactivos
{
    public interface IReactivosApiService
    {
        Task<List<ReactivoDto>> GetReactivosAsync();
        Task<ReactivoDto?> GetReactivoPorIdAsync(int id);
        Task<HttpResponseMessage> CrearReactivoAsync(ReactivoDto dto);
        Task<HttpResponseMessage> EditarReactivoAsync(int id, ReactivoDto dto);
        Task<HttpResponseMessage> AnularReactivoAsync(int id);

        Task<HttpResponseMessage> RegistrarIngresosAsync(IEnumerable<MovimientoReactivoIngresoDto> ingresos);
        Task<HttpResponseMessage> RegistrarEgresosAsync(IEnumerable<MovimientoReactivoEgresoDto> egresos);

    }
}
