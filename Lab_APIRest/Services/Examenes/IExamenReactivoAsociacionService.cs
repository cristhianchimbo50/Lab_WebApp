using Lab_Contracts.Examenes;

namespace Lab_APIRest.Services.Examenes
{
    public interface IExamenReactivoAsociacionService
    {
        Task<List<AsociacionReactivoDto>> ListarAsociacionesAsync();
        Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenAsync(string nombreExamen);
        Task<List<AsociacionReactivoDto>> ListarAsociacionesPorReactivoAsync(string nombreReactivo);
        Task<AsociacionReactivoDto?> ObtenerDetalleAsociacionAsync(int idExamenReactivo);
        Task<AsociacionReactivoDto> GuardarAsociacionAsync(AsociacionReactivoDto asociacionDto);
        Task<bool> GuardarAsociacionAsync(int idExamenReactivo, AsociacionReactivoDto asociacionDto);
        Task<bool> AnularAsociacionAsync(int idExamenReactivo);
        Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenIdAsync(int idExamen);
        Task<bool> GuardarAsociacionesPorExamenAsync(int idExamen, List<AsociacionReactivoDto> asociaciones);
    }
}
