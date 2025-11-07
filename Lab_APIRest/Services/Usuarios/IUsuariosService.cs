using Lab_Contracts.Usuarios;

namespace Lab_APIRest.Services.Usuarios
{
    public interface IUsuariosService
    {
        Task<List<UsuarioListadoDto>> ListarUsuariosAsync(UsuarioFiltroDto filtro, CancellationToken ct = default);
        Task<UsuarioListadoDto?> ObtenerDetalleUsuarioAsync(int idUsuario, CancellationToken ct = default);
        Task<int> GuardarUsuarioAsync(UsuarioCrearDto usuario, CancellationToken ct = default);
        Task<bool> GuardarUsuarioAsync(UsuarioEditarDto usuario, CancellationToken ct = default);
        Task<bool> CambiarEstadoUsuarioAsync(int idUsuario, bool activo, string correoUsuarioActual, CancellationToken ct = default);
    }
}
