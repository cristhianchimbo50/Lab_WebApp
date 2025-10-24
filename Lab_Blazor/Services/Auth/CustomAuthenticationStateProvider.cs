using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Lab_Blazor.Services.Auth
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ProtectedSessionStorage _session;
        private readonly IJSRuntime _jsRuntime;

        public CustomAuthenticationStateProvider(ProtectedSessionStorage session, IJSRuntime jsRuntime)
        {
            _session = session;
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? token = null;

            try
            {
                var result = await _session.GetAsync<string>("jwt");
                token = result.Success ? result.Value : null;
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
                var claims = jwt.Claims.ToList();

                // Si el token expira, cerrar sesión automáticamente
                if (jwt.ValidTo < DateTime.UtcNow)
                {
                    await SignOutAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var identity = new ClaimsIdentity(claims, "jwt");
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

            await _session.SetAsync("jwt", session.AccessToken);
            await _session.SetAsync("usuario", session);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task SignOutAsync()
        {
            await _session.DeleteAsync("jwt");
            await _session.DeleteAsync("usuario");

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
