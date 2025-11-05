using Lab_Contracts.Auth;

namespace Lab_APIRest.Services.Auth
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> IniciarSesionAsync(LoginRequestDto Solicitud, CancellationToken Ct);
        Task<CambiarContraseniaResponseDto> CambiarContraseniaAsync(CambiarContraseniaDto Cambio, CancellationToken Ct);
        //Task<bool> ReenviarContraseniaTemporalAsync(string Correo, CancellationToken Ct);

    }
}
