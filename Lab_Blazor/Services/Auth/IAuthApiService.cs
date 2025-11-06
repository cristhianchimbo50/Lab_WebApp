using Lab_Contracts.Auth;

namespace Lab_Blazor.Services.Auth
{
    public interface IAuthApiService
    {
        Task<(bool Exito, string Mensaje, LoginResponseDto? Usuario, bool RequiereCambioClave)> LoginAsync(LoginRequestDto Solicitud, CancellationToken Ct = default);
        Task LogoutAsync(CancellationToken Ct = default);
        Task<(bool Exito, string Mensaje)> CambiarContraseniaAsync(CambiarContraseniaDto Solicitud, CancellationToken Ct = default);
        Task<bool> RegisterAsync(RegisterRequestDto Solicitud, CancellationToken Ct = default);
        Task<bool> ActivarCuentaAsync(ActivateAccountDto Solicitud, CancellationToken Ct = default);
        Task<bool> VerificarSesionAsync(CancellationToken Ct = default);
        Task<string?> ObtenerTokenAsync();
        Task<LoginResponseDto?> ObtenerUsuarioAsync();
    }
}
