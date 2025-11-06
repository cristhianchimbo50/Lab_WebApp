using Lab_Contracts.Auth;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace Lab_Blazor.Services.Auth
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient _clienteHttp;
        private readonly IJSRuntime _jsRuntime;

        public AuthApiService(IHttpClientFactory factoryHttp, IJSRuntime jsRuntime)
        {
            _clienteHttp = factoryHttp.CreateClient("Api");
            _jsRuntime = jsRuntime;
        }

        public async Task<(bool Exito, string Mensaje, LoginResponseDto? Usuario, bool RequiereCambioClave)> IniciarSesionAsync(LoginRequestDto solicitud, CancellationToken ct = default)
        {
            var respuesta = await _clienteHttp.PostAsJsonAsync("api/auth/login", solicitud, ct);

            if (respuesta.IsSuccessStatusCode)
            {
                var respuestaLogin = await respuesta.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: ct);
                if (respuestaLogin is not null)
                {
                    if (respuestaLogin.EsContraseniaTemporal && string.IsNullOrEmpty(respuestaLogin.AccessToken))
                        return (true, "Debe cambiar su contraseña antes de continuar.", respuestaLogin, true);

                    return (true, respuestaLogin.Mensaje ?? "Inicio de sesión exitoso.", respuestaLogin, false);
                }
            }

            string mensajeError = "Error desconocido.";
            try
            {
                var contenido = await respuesta.Content.ReadAsStringAsync(ct);
                if (!string.IsNullOrWhiteSpace(contenido))
                {
                    var documentoJson = JsonDocument.Parse(contenido);
                    string? mensajeServidor = null;
                    string? expiracion = null;

                    foreach (var propiedad in documentoJson.RootElement.EnumerateObject())
                    {
                        if (propiedad.Name.Equals("mensaje", StringComparison.OrdinalIgnoreCase))
                            mensajeServidor = propiedad.Value.GetString();
                        else if (propiedad.Name.Equals("expiracion", StringComparison.OrdinalIgnoreCase))
                            expiracion = propiedad.Value.GetString();
                    }

                    mensajeError = mensajeServidor ?? mensajeError;
                    if (!string.IsNullOrEmpty(expiracion))
                        mensajeError += $" (Expiró el {expiracion})";
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
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwt");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "usuario");
        }

        public async Task<(bool Exito, string Mensaje)> CambiarContraseniaAsync(CambiarContraseniaDto solicitud, CancellationToken ct = default)
        {
            var respuesta = await _clienteHttp.PostAsJsonAsync("api/auth/cambiar-contrasenia", solicitud, ct);
            if (respuesta.IsSuccessStatusCode)
            {
                var respuestaCambio = await respuesta.Content.ReadFromJsonAsync<CambiarContraseniaResponseDto>(cancellationToken: ct);
                return (respuestaCambio?.Exito ?? true, respuestaCambio?.Mensaje ?? "Contraseña actualizada correctamente.");
            }

            var errorRespuesta = await respuesta.Content.ReadAsStringAsync(ct);
            return (false, string.IsNullOrWhiteSpace(errorRespuesta) ? "No se pudo cambiar la contraseña." : errorRespuesta);
        }

        public async Task<bool> VerificarSesionAsync(CancellationToken ct = default)
        {
            try
            {
                var tokenJwt = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwt");
                if (string.IsNullOrWhiteSpace(tokenJwt))
                    return false;

                _clienteHttp.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenJwt);

                var respuesta = await _clienteHttp.GetAsync("api/auth/verificar-sesion", ct);
                return respuesta.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> ObtenerTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "jwt");
        }

        public async Task<LoginResponseDto?> ObtenerUsuarioAsync()
        {
            var datosUsuario = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "usuario");
            return string.IsNullOrWhiteSpace(datosUsuario)
                ? null
                : JsonSerializer.Deserialize<LoginResponseDto>(datosUsuario);
        }

        public async Task<bool> RegistrarUsuarioAsync(RegisterRequestDto solicitud, CancellationToken ct = default)
        {
            var respuesta = await _clienteHttp.PostAsJsonAsync("api/auth/register", solicitud, ct);
            return respuesta.IsSuccessStatusCode;
        }

        public async Task<bool> ActivarCuentaAsync(ActivateAccountDto solicitud, CancellationToken ct = default)
        {
            var respuesta = await _clienteHttp.PostAsJsonAsync("api/auth/activate", solicitud, ct);
            return respuesta.IsSuccessStatusCode;
        }
    }
}
