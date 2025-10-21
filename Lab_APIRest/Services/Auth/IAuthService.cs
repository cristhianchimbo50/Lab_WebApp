using Lab_Contracts.Auth;

namespace Lab_APIRest.Services.Auth
{
    public interface IAuthService
    {
        /// <summary>
        /// Autentica un usuario y devuelve el token JWT si las credenciales son válidas.
        /// </summary>
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct);
    }
}
