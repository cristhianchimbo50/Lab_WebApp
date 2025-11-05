using Lab_Contracts.Usuarios;

namespace Lab_APIRest.Services.Usuarios
{
    public interface IUsuariosService
    {
        Task<List<UsuarioListadoDto>> ListarUsuariosAsync(UsuarioFiltroDto filtro, CancellationToken ct = default);
        Task<UsuarioListadoDto?> ObtenerUsuarioPorIdAsync(int idUsuario, CancellationToken ct = default);
        Task<int> CrearUsuarioAsync(UsuarioCrearDto usuario, CancellationToken ct = default);
        Task<bool> EditarUsuarioAsync(UsuarioEditarDto usuario, CancellationToken ct = default);
        Task<bool> CambiarEstadoAsync(int idUsuario, bool activo, string correoUsuarioActual, CancellationToken ct = default);
        Task<UsuarioReenviarDto?> ReenviarCredencialesTemporalesAsync(int idUsuario, CancellationToken ct = default);
    }
}
