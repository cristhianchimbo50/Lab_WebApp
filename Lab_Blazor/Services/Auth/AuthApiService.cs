using Lab_Contracts.Auth;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace Lab_Blazor.Services.Auth
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _http;
        private readonly IJSRuntime _js;

        public AuthApiService(IHttpClientFactory factory, IJSRuntime js)
        {
            _http = factory.CreateClient("Api");
            _js = js;
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
                        return (true, "Debe cambiar su contraseña antes de continuar.", loginResp, true);

                    await _js.InvokeVoidAsync("localStorage.setItem", "jwt", loginResp.AccessToken);
                    await _js.InvokeVoidAsync("localStorage.setItem", "usuario", JsonSerializer.Serialize(loginResp));

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
            await _js.InvokeVoidAsync("localStorage.removeItem", "jwt");
            await _js.InvokeVoidAsync("localStorage.removeItem", "usuario");
        }

        public async Task<(bool Exito, string Mensaje)> CambiarContraseniaAsync(CambiarContraseniaDto dto, CancellationToken ct = default)
        {
            var resp = await _http.PostAsJsonAsync("api/auth/cambiar-contrasenia", dto, ct);
            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadFromJsonAsync<CambiarContraseniaResponseDto>(cancellationToken: ct);
                return (data?.Exito ?? true, data?.Mensaje ?? "Contraseña actualizada correctamente.");
            }

            var error = await resp.Content.ReadAsStringAsync(ct);
            return (false, string.IsNullOrWhiteSpace(error) ? "No se pudo cambiar la contraseña." : error);
        }

        public async Task<bool> VerificarSesionAsync(CancellationToken ct = default)
        {
            try
            {
                var token = await _js.InvokeAsync<string>("localStorage.getItem", "jwt");
                if (string.IsNullOrWhiteSpace(token))
                    return false;

                _http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var resp = await _http.GetAsync("api/auth/verificar-sesion", ct);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> ObtenerTokenAsync()
        {
            return await _js.InvokeAsync<string>("localStorage.getItem", "jwt");
        }

        public async Task<LoginResponseDto?> ObtenerUsuarioAsync()
        {
            var data = await _js.InvokeAsync<string>("localStorage.getItem", "usuario");
            return string.IsNullOrWhiteSpace(data)
                ? null
                : JsonSerializer.Deserialize<LoginResponseDto>(data);
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
