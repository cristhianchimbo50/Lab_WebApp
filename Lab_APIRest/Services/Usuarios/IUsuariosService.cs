using Lab_Contracts.Usuarios;

namespace Lab_APIRest.Services.Usuarios
{
    public interface IUsuariosService
    {
        Task<List<UsuarioListadoDto>> GetUsuariosAsync(UsuarioFiltroDto filtro, CancellationToken ct = default);
        Task<UsuarioListadoDto?> GetUsuarioPorIdAsync(int idUsuario, CancellationToken ct = default);
        Task<int> CrearUsuarioAsync(UsuarioCrearDto dto, CancellationToken ct = default);
        Task<bool> EditarUsuarioAsync(UsuarioEditarDto dto, CancellationToken ct = default);
        Task<bool> CambiarEstadoAsync(int idUsuario, bool activo, string correoUsuarioActual, CancellationToken ct = default);
        Task<UsuarioReenviarDto?> ReenviarCredencialesTemporalesAsync(int idUsuario, CancellationToken ct = default);
    }
}
