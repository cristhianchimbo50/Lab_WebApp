using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Auth
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _http;
        private readonly ProtectedSessionStorage _session;

        public AuthApiService(IHttpClientFactory factory, ProtectedSessionStorage session)
        {
            _http = factory.CreateClient("Api");
            _session = session;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/auth/login", dto, ct);

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                var loginResp = await resp.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: ct);
                if (loginResp is not null)
                {
                    await _session.SetAsync("jwt", loginResp.AccessToken);
                    await _session.SetAsync("usuario", loginResp);
                }
                return loginResp;
            }

            if (resp.StatusCode == HttpStatusCode.Locked)
                throw new InvalidOperationException("Cuenta bloqueada temporalmente. Intente más tarde.");
            if (resp.StatusCode == HttpStatusCode.Forbidden)
                throw new InvalidOperationException("Cuenta no activa. Debe activar su cuenta.");

            return null;
        }

        public async Task LogoutAsync(CancellationToken ct = default)
        {
            await _session.DeleteAsync("jwt");
            await _session.DeleteAsync("usuario");
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/auth/change-password", dto, ct);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto dto, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/auth/register", dto, ct);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> ActivateAccountAsync(ActivateAccountDto dto, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/auth/activate", dto, ct);
            return resp.IsSuccessStatusCode;
        }
    }
}
