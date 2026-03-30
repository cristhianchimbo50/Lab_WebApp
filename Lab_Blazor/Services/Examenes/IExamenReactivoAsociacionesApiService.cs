using Lab_Contracts.Examenes;

namespace Lab_Blazor.Services.Examenes
{
    public interface IExamenReactivoAsociacionesApiService
    {
        Task<List<AsociacionReactivoDto>> ListarAsociacionesAsync();
        Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenAsync(string nombreExamen);
        Task<List<AsociacionReactivoDto>> ListarAsociacionesPorReactivoAsync(string nombreReactivo);
        Task<AsociacionReactivoDto?> ObtenerDetalleAsociacionAsync(int idAsociacion);
        Task<AsociacionReactivoDto?> CrearAsociacionAsync(AsociacionReactivoDto asociacionReactivo);
        Task<bool> EditarAsociacionAsync(int idAsociacion, AsociacionReactivoDto asociacionReactivo);
        Task<bool> AnularAsociacionAsync(int idAsociacion);
        // Task<bool> EliminarAsociacionAsync(int idAsociacion); // Eliminado, se usa AnularAsociacionAsync
        Task<bool> ActivarAsociacionAsync(int idAsociacion, AsociacionReactivoDto asociacionReactivo);
        Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenIdAsync(int idExamen);
        Task<bool> GuardarAsociacionesPorExamenAsync(int idExamen, List<AsociacionReactivoDto> asociaciones);
    }
}
