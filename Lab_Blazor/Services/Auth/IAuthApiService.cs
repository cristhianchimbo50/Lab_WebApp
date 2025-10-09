using Lab_Contracts.Auth;
namespace Lab_Blazor.Services.Auth;

public interface IAuthApiService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken ct = default);
    Task<bool> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default);
    Task<bool> ActivateAccountAsync(ActivateAccountDto dto, CancellationToken ct = default);
}
