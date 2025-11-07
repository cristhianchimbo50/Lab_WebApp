using Lab_Contracts.Usuarios;

namespace Lab_Blazor.Services.Usuarios
{
    public interface IUsuariosApiService
    {
        Task<List<UsuarioListadoDto>> ListarUsuariosAsync(UsuarioFiltroDto filtro);
        Task<UsuarioListadoDto?> ObtenerDetalleUsuarioAsync(int idUsuario);
        Task<int> GuardarUsuarioAsync(UsuarioCrearDto usuario);
        Task<bool> GuardarUsuarioAsync(UsuarioEditarDto usuario);
        Task<bool> CambiarEstadoUsuarioAsync(int idUsuario, bool activo);
    }
}
