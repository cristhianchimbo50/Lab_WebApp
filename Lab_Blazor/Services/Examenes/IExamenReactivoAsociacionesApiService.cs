using Lab_Contracts.Examenes;

namespace Lab_Blazor.Services.Examenes
{
    public interface IExamenReactivoAsociacionesApiService
    {
        Task<List<AsociacionReactivoDto>> ObtenerTodasAsync();
        Task<List<AsociacionReactivoDto>> BuscarPorExamenAsync(string NombreExamen);
        Task<List<AsociacionReactivoDto>> BuscarPorReactivoAsync(string NombreReactivo);
        Task<AsociacionReactivoDto?> ObtenerPorIdAsync(int IdAsociacion);
        Task<AsociacionReactivoDto?> CrearAsync(AsociacionReactivoDto AsociacionReactivo);
        Task<bool> EditarAsync(int IdAsociacion, AsociacionReactivoDto AsociacionReactivo);
        Task<bool> EliminarAsync(int IdAsociacion);
        Task<List<AsociacionReactivoDto>> ObtenerPorExamenIdAsync(int IdExamen);
        Task<bool> GuardarPorExamenAsync(int IdExamen, List<AsociacionReactivoDto> Asociaciones);
    }
}
