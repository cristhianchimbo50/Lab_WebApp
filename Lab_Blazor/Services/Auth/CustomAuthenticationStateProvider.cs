using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Lab_Blazor.Services.Auth
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;
        private readonly IAuthApiService _authApi;

        public CustomAuthenticationStateProvider(IJSRuntime js, IAuthApiService authApi)
        {
            _js = js;
            _authApi = authApi;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? token = null;
            try
            {
                token = await _js.InvokeAsync<string>("localStorage.getItem", "jwt");
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(token);

                if (jwt.ValidTo < DateTime.UtcNow)
                {
                    await SignOutAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var serverOk = await _authApi.VerificarSesionAsync();
                if (!serverOk)
                {
                    await SignOutAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var normalizedClaims = new List<Claim>();
                foreach (var c in jwt.Claims)
                {
                    if (c.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                        c.Type.Equals("roles", StringComparison.OrdinalIgnoreCase) ||
                        c.Type.Equals("rol", StringComparison.OrdinalIgnoreCase))
                        normalizedClaims.Add(new Claim(ClaimTypes.Role, c.Value));
                    else
                        normalizedClaims.Add(c);
                }

                var identity = new ClaimsIdentity(normalizedClaims, "jwt");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                await SignOutAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task SignInAsync(LoginResponseDto session)
        {
            if (string.IsNullOrEmpty(session.AccessToken))
                return;

            await _js.InvokeVoidAsync("localStorage.setItem", "jwt", session.AccessToken);
            await _js.InvokeVoidAsync("localStorage.setItem", "usuario", JsonSerializer.Serialize(session));

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task SignOutAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "jwt");
            await _js.InvokeVoidAsync("localStorage.removeItem", "usuario");

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
