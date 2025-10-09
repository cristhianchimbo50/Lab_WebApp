using Lab_Contracts.Examenes;

namespace Lab_Blazor.Services.Examenes
{
    public interface IExamenReactivoAsociacionesApiService
    {
        Task<List<AsociacionReactivoDto>> ObtenerTodasAsync();
        Task<List<AsociacionReactivoDto>> BuscarPorExamenAsync(string nombre);
        Task<List<AsociacionReactivoDto>> BuscarPorReactivoAsync(string nombre);
        Task<AsociacionReactivoDto?> ObtenerPorIdAsync(int id);
        Task<AsociacionReactivoDto?> CrearAsync(AsociacionReactivoDto dto);
        Task<bool> EditarAsync(int id, AsociacionReactivoDto dto);
        Task<bool> EliminarAsync(int id);
        Task<List<AsociacionReactivoDto>> ObtenerPorExamenIdAsync(int idExamen);
        Task<bool> GuardarPorExamenAsync(int idExamen, List<AsociacionReactivoDto> asociaciones);
    }
}
