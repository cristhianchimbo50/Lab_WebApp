using Lab_Contracts.Auth;

namespace Lab_Blazor.Services.Auth
{
    public interface IAuthApiService
    {
        Task<(bool Exito, string Mensaje, LoginResponseDto? Usuario, bool RequiereCambioClave)> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
        Task LogoutAsync(CancellationToken ct = default);
        Task<(bool Exito, string Mensaje)> CambiarContraseniaAsync(CambiarContraseniaDto dto, CancellationToken ct = default);
        Task<bool> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
        Task<bool> ActivateAccountAsync(ActivateAccountDto dto, CancellationToken ct = default);
    }
}
