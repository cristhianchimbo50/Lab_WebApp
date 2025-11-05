using Lab_Contracts.Examenes;

namespace Lab_APIRest.Services.Examenes
{
    public interface IExamenReactivoAsociacionService
    {
        Task<List<AsociacionReactivoDto>> ObtenerTodas();
        Task<List<AsociacionReactivoDto>> BuscarPorExamen(string NombreExamen);
        Task<List<AsociacionReactivoDto>> BuscarPorReactivo(string NombreReactivo);
        Task<AsociacionReactivoDto?> ObtenerPorId(int IdExamenReactivo);
        Task<AsociacionReactivoDto> Crear(AsociacionReactivoDto AsociacionDto);
        Task<bool> Editar(int IdExamenReactivo, AsociacionReactivoDto AsociacionDto);
        Task<bool> Eliminar(int IdExamenReactivo);
        Task<List<AsociacionReactivoDto>> ObtenerPorExamenId(int IdExamen);
        Task<bool> GuardarPorExamen(int IdExamen, List<AsociacionReactivoDto> Asociaciones);
    }
}
