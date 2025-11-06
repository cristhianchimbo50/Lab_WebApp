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
        private readonly IJSRuntime JsRuntime;
        private readonly IAuthApiService AuthService;

        public CustomAuthenticationStateProvider(IJSRuntime JsRuntime, IAuthApiService AuthService)
        {
            this.JsRuntime = JsRuntime;
            this.AuthService = AuthService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string? TokenJwt = null;
            try
            {
                TokenJwt = await JsRuntime.InvokeAsync<string>("localStorage.getItem", "jwt");
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            if (string.IsNullOrWhiteSpace(TokenJwt))
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

            try
            {
                var ManejadorJwt = new JwtSecurityTokenHandler();
                var TokenDecodificado = ManejadorJwt.ReadJwtToken(TokenJwt);

                if (TokenDecodificado.ValidTo < DateTime.UtcNow)
                {
                    await SignOutAsync();
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                var ReclamacionesNormalizadas = new List<Claim>();
                foreach (var Reclamacion in TokenDecodificado.Claims)
                {
                    if (Reclamacion.Type.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                        Reclamacion.Type.Equals("roles", StringComparison.OrdinalIgnoreCase) ||
                        Reclamacion.Type.Equals("rol", StringComparison.OrdinalIgnoreCase))
                        ReclamacionesNormalizadas.Add(new Claim(ClaimTypes.Role, Reclamacion.Value));
                    else
                        ReclamacionesNormalizadas.Add(Reclamacion);
                }
                var Identidad = new ClaimsIdentity(ReclamacionesNormalizadas, "jwt");
                var UsuarioPrincipal = new ClaimsPrincipal(Identidad);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var EsSesionValidaEnServidor = await AuthService.VerificarSesionAsync();
                        if (!EsSesionValidaEnServidor)
                            await SignOutAsync();
                    }
                    catch
                    {

                    }
                });

                return new AuthenticationState(UsuarioPrincipal);
            }
            catch
            {
                await SignOutAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task SignInAsync(LoginResponseDto Sesion)
        {
            if (string.IsNullOrEmpty(Sesion.AccessToken))
                return;

            await JsRuntime.InvokeVoidAsync("localStorage.setItem", "jwt", Sesion.AccessToken);
            await JsRuntime.InvokeVoidAsync("localStorage.setItem", "usuario", JsonSerializer.Serialize(Sesion));

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task SignOutAsync()
        {
            await JsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwt");
            await JsRuntime.InvokeVoidAsync("localStorage.removeItem", "usuario");

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
