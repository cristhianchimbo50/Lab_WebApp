using Lab_Contracts.Auth;

namespace Lab_APIRest.Services.Auth
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> IniciarSesionAsync(LoginRequestDto solicitud, CancellationToken ct);
        Task<CambiarContraseniaResponseDto> CambiarContraseniaAsync(CambiarContraseniaDto cambio, CancellationToken ct);

    }
}
