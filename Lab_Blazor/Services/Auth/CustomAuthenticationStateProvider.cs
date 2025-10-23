using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Lab_Blazor.Services.Auth;

/// <summary>
/// Proveedor de estado de autenticación personalizado para Blazor Server.
/// Solo maneja lógica de sesión local y claims, nunca lógica de negocio ni acceso a datos.
/// </summary>
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
            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }
    /// <summary>
    /// Almacena el token de sesión y notifica el cambio de autenticación.
    /// </summary>
    public async Task SignInAsync(LoginResponseDto session)
    {
        if (string.IsNullOrEmpty(session.AccessToken))
            return;

        await _session.SetAsync("jwt", session.AccessToken);
        await _session.SetAsync("usuario", session);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    /// Elimina el token de sesión y notifica el cambio de autenticación.
    /// </summary>
    public async Task SignOutAsync()
    {
        await _session.DeleteAsync("jwt");
        await _session.DeleteAsync("usuario");

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

