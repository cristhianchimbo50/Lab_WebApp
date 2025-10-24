
using Lab_Contracts.Auth;

namespace Lab_APIRest.Services.Auth
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct);
        Task<CambiarContraseniaResponseDto> CambiarContraseniaAsync(CambiarContraseniaDto dto, CancellationToken ct);
        Task<bool> ReenviarContraseniaTemporalAsync(string correo, CancellationToken ct);

    }
}
