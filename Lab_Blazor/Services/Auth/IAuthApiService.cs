using Lab_Contracts.Auth;

namespace Lab_Blazor.Services.Auth
{
    public interface IAuthApiService
    {
        Task<(bool Exito, string Mensaje, LoginResponseDto? Usuario, bool RequiereCambioClave)> IniciarSesionAsync(LoginRequestDto solicitud, CancellationToken ct = default);
        Task LogoutAsync(CancellationToken ct = default);
        Task<(bool Exito, string Mensaje)> CambiarContraseniaAsync(CambiarContraseniaDto solicitud, CancellationToken ct = default);
        Task<bool> RegistrarUsuarioAsync(RegisterRequestDto solicitud, CancellationToken ct = default);
        Task<RespuestaMensajeDto> ActivarCuentaAsync(RestablecerContraseniaDto solicitud, CancellationToken ct = default);
        Task<bool> VerificarSesionAsync(CancellationToken ct = default);
        Task<string?> ObtenerTokenAsync();
        Task<LoginResponseDto?> ObtenerUsuarioAsync();
    }
}
