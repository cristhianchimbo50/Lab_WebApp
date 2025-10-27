using Lab_Contracts.Usuarios;

namespace Lab_Blazor.Services.Usuarios
{
    public interface IUsuariosApiService
    {
        Task<List<UsuarioListadoDto>> GetUsuariosAsync(UsuarioFiltroDto filtro);
        Task<UsuarioListadoDto?> GetUsuarioPorIdAsync(int idUsuario);
        Task<int> CrearUsuarioAsync(UsuarioCrearDto dto);
        Task<bool> EditarUsuarioAsync(UsuarioEditarDto dto);
        Task<bool> CambiarEstadoAsync(int idUsuario, bool activo);
        Task<UsuarioReenviarDto?> ReenviarCredencialesTemporalesAsync(int idUsuario);
    }

}
