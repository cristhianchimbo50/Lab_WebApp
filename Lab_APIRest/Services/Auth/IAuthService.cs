using Lab_Contracts.Auth;

namespace Lab_APIRest.Services.Auth
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct);
        Task<bool> CambiarClaveAsync(ChangePasswordDto dto, CancellationToken ct);
        Task<bool> ReenviarContraseñaTemporalAsync(string correo, CancellationToken ct);

    }
}
