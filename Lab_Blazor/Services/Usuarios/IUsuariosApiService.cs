using Lab_Contracts.Usuarios;

namespace Lab_Blazor.Services.Usuarios
{
    public interface IUsuariosApiService
    {
        Task<List<UsuarioListadoDto>> ObtenerUsuariosAsync(UsuarioFiltroDto Filtro);
        Task<UsuarioListadoDto?> ObtenerUsuarioPorIdAsync(int IdUsuario);
        Task<int> CrearUsuarioAsync(UsuarioCrearDto Usuario);
        Task<bool> EditarUsuarioAsync(UsuarioEditarDto Usuario);
        Task<bool> CambiarEstadoAsync(int IdUsuario, bool Activo);
        Task<UsuarioReenviarDto?> ReenviarCredencialesTemporalesAsync(int IdUsuario);
    }

}
