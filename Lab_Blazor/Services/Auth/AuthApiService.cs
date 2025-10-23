using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

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

        public async Task<(bool Exito, string Mensaje, LoginResponseDto? Usuario, bool RequiereCambioClave)> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/auth/login", dto, ct);

            if (resp.IsSuccessStatusCode)
            {
                var loginResp = await resp.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: ct);

                if (loginResp is not null)
                {
                    if (loginResp.EsContraseñaTemporal && string.IsNullOrEmpty(loginResp.AccessToken))
                        return (true, "Debe cambiar su contraseña temporal antes de continuar.", loginResp, true);

                    await _session.SetAsync("jwt", loginResp.AccessToken);
                    await _session.SetAsync("usuario", loginResp);
                    return (true, loginResp.Mensaje ?? "Inicio de sesión exitoso.", loginResp, false);
                }
            }

            string mensajeError = "Error desconocido.";
            try
            {
                var contenido = await resp.Content.ReadAsStringAsync(ct);
                if (!string.IsNullOrWhiteSpace(contenido))
                {
                    var json = JsonDocument.Parse(contenido);
                    string? msg = null;
                    string? exp = null;

                    // Soporta tanto "Mensaje" como "mensaje" (insensible a mayúsculas)
                    foreach (var prop in json.RootElement.EnumerateObject())
                    {
                        if (prop.Name.Equals("mensaje", StringComparison.OrdinalIgnoreCase))
                            msg = prop.Value.GetString();
                        else if (prop.Name.Equals("expiracion", StringComparison.OrdinalIgnoreCase))
                            exp = prop.Value.GetString();
                    }

                    mensajeError = msg ?? mensajeError;
                    if (!string.IsNullOrEmpty(exp))
                        mensajeError += $" (Expiró el {exp})";
                }
            }
            catch (Exception ex)
            {
                mensajeError = $"Error al leer la respuesta del servidor: {ex.Message}";
            }


            return (false, mensajeError, null, false);
        }

        public async Task LogoutAsync(CancellationToken ct = default)
        {
            await _session.DeleteAsync("jwt");
            await _session.DeleteAsync("usuario");
        }

        public async Task<(bool Exito, string Mensaje)> CambiarClaveAsync(ChangePasswordDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/auth/change-password", dto);
            if (response.IsSuccessStatusCode)
                return (true, "Contraseña actualizada correctamente.");

            var msg = await response.Content.ReadAsStringAsync();
            return (false, msg);
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
