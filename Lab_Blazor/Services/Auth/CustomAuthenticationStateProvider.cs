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
        // Protección: si es prerendering, JS interop no está disponible aún.
        // Try/catch simple: si ProtectedSessionStorage falla, devuelve usuario anónimo.
        string? token = null;
        try
        {
            var result = await _session.GetAsync<string>("jwt");
            token = result.Success ? result.Value : null;
        }
        catch
        {
            // Si ProtectedSessionStorage falla por prerendering o por otro motivo,
            // se retorna un usuario anónimo (sin claims)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        // Decodifica el JWT y extrae claims
        JwtSecurityToken? jwt = null;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            jwt = handler.ReadJwtToken(token);
        }
        catch
        {
            // Token corrupto o inválido: sesión inválida
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = jwt?.Claims ?? Enumerable.Empty<Claim>();
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationState(principal);
    }

    /// <summary>
    /// Almacena el token de sesión y notifica el cambio de autenticación.
    /// </summary>
    public async Task SignInAsync(LoginResponseDto session)
    {
        await _session.SetAsync("jwt", session.AccessToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    /// <summary>
    /// Elimina el token de sesión y notifica el cambio de autenticación.
    /// </summary>
    public async Task SignOutAsync()
    {
        await _session.DeleteAsync("jwt");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}

