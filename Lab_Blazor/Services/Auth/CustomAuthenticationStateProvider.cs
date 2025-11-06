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
        private readonly IJSRuntime _jsRuntime;
        private readonly IAuthApiService _authService;

        public CustomAuthenticationStateProvider(IJSRuntime jsRuntime, IAuthApiService authService)
        {
            _jsRuntime = jsRuntime;
            _authService = authService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? tokenJwt = null;
            try
            {
                tokenJwt = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwt");
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            if (string.IsNullOrWhiteSpace(tokenJwt))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var decodedToken = jwtHandler.ReadJwtToken(tokenJwt);

                if (decodedToken.ValidTo < DateTime.UtcNow)
                {
                    await CerrarSesionAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var claimsNormalized = new List<Claim>();
                foreach (var claim in decodedToken.Claims)
                {
                    if (claim.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                        claim.Type.Equals("roles", StringComparison.OrdinalIgnoreCase) ||
                        claim.Type.Equals("rol", StringComparison.OrdinalIgnoreCase))
                        claimsNormalized.Add(new Claim(ClaimTypes.Role, claim.Value));
                    else
                        claimsNormalized.Add(claim);
                }
                var identity = new ClaimsIdentity(claimsNormalized, "jwt");
                var principal = new ClaimsPrincipal(identity);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var sesionValida = await _authService.VerificarSesionAsync();
                        if (!sesionValida)
                            await CerrarSesionAsync();
                    }
                    catch { }
                });

                return new AuthenticationState(principal);
            }
            catch
            {
                await CerrarSesionAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task IniciarSesionAsync(LoginResponseDto sesion)
        {
            if (string.IsNullOrEmpty(sesion.AccessToken))
                return;

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "jwt", sesion.AccessToken);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "usuario", JsonSerializer.Serialize(sesion));

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task CerrarSesionAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwt");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "usuario");

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
