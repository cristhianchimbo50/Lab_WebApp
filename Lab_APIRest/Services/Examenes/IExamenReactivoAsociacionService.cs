using Lab_Contracts.Examenes;

namespace Lab_APIRest.Services.Examenes
{
    public interface IExamenReactivoAsociacionService
    {
        Task<List<AsociacionReactivoDto>> ObtenerTodasAsync();
        Task<List<AsociacionReactivoDto>> BuscarPorExamenAsync(string nombreExamen);
        Task<List<AsociacionReactivoDto>> BuscarPorReactivoAsync(string nombreReactivo);
        Task<AsociacionReactivoDto?> ObtenerPorIdAsync(int idExamenReactivo);
        Task<AsociacionReactivoDto> CrearAsync(AsociacionReactivoDto dto);
        Task<bool> EditarAsync(int id, AsociacionReactivoDto dto);
        Task<bool> EliminarAsync(int id);
        Task<List<AsociacionReactivoDto>> ObtenerPorExamenIdAsync(int idExamen);
        Task<bool> GuardarPorExamenAsync(int idExamen, List<AsociacionReactivoDto> asociaciones);


    }
}
