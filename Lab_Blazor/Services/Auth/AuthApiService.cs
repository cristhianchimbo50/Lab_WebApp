using Lab_Contracts.Auth;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;

namespace Lab_Blazor.Services.Auth
{
    public class AuthApiService : IAuthApiService
    {
        private readonly HttpClient ClienteHttp;
        private readonly IJSRuntime JsRuntime;

        public AuthApiService(IHttpClientFactory FactoryHttp, IJSRuntime JsRuntime)
        {
            ClienteHttp = FactoryHttp.CreateClient("Api");
            this.JsRuntime = JsRuntime;
        }

        public async Task<(bool Exito, string Mensaje, LoginResponseDto? Usuario, bool RequiereCambioClave)> LoginAsync(LoginRequestDto Solicitud, CancellationToken Ct = default)
        {
            var Respuesta = await ClienteHttp.PostAsJsonAsync("api/auth/login", Solicitud, Ct);

            if (Respuesta.IsSuccessStatusCode)
            {
                var RespuestaLogin = await Respuesta.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: Ct);
                if (RespuestaLogin is not null)
                {
                    if (RespuestaLogin.EsContraseniaTemporal && string.IsNullOrEmpty(RespuestaLogin.AccessToken))
                        return (true, "Debe cambiar su contraseña antes de continuar.", RespuestaLogin, true);

                    return (true, RespuestaLogin.Mensaje ?? "Inicio de sesión exitoso.", RespuestaLogin, false);
                }
            }

            string MensajeError = "Error desconocido.";
            try
            {
                var Contenido = await Respuesta.Content.ReadAsStringAsync(Ct);
                if (!string.IsNullOrWhiteSpace(Contenido))
                {
                    var DocumentoJson = JsonDocument.Parse(Contenido);
                    string? MensajeServidor = null;
                    string? Expiracion = null;

                    foreach (var Propiedad in DocumentoJson.RootElement.EnumerateObject())
                    {
                        if (Propiedad.Name.Equals("mensaje", StringComparison.OrdinalIgnoreCase))
                            MensajeServidor = Propiedad.Value.GetString();
                        else if (Propiedad.Name.Equals("expiracion", StringComparison.OrdinalIgnoreCase))
                            Expiracion = Propiedad.Value.GetString();
                    }

                    MensajeError = MensajeServidor ?? MensajeError;
                    if (!string.IsNullOrEmpty(Expiracion))
                        MensajeError += $" (Expiró el {Expiracion})";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al leer la respuesta del servidor: {ex.Message}";
            }

            return (false, MensajeError, null, false);
        }

        public async Task LogoutAsync(CancellationToken Ct = default)
        {
            await JsRuntime.InvokeVoidAsync("localStorage.removeItem", "jwt");
            await JsRuntime.InvokeVoidAsync("localStorage.removeItem", "usuario");
        }

        public async Task<(bool Exito, string Mensaje)> CambiarContraseniaAsync(CambiarContraseniaDto Solicitud, CancellationToken Ct = default)
        {
            var Respuesta = await ClienteHttp.PostAsJsonAsync("api/auth/cambiar-contrasenia", Solicitud, Ct);
            if (Respuesta.IsSuccessStatusCode)
            {
                var RespuestaCambio = await Respuesta.Content.ReadFromJsonAsync<CambiarContraseniaResponseDto>(cancellationToken: Ct);
                return (RespuestaCambio?.Exito ?? true, RespuestaCambio?.Mensaje ?? "Contraseña actualizada correctamente.");
            }

            var ErrorRespuesta = await Respuesta.Content.ReadAsStringAsync(Ct);
            return (false, string.IsNullOrWhiteSpace(ErrorRespuesta) ? "No se pudo cambiar la contraseña." : ErrorRespuesta);
        }

        public async Task<bool> VerificarSesionAsync(CancellationToken Ct = default)
        {
            try
            {
                var TokenJwt = await JsRuntime.InvokeAsync<string>("localStorage.getItem", "jwt");
                if (string.IsNullOrWhiteSpace(TokenJwt))
                    return false;

                ClienteHttp.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenJwt);

                var Respuesta = await ClienteHttp.GetAsync("api/auth/verificar-sesion", Ct);
                return Respuesta.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> ObtenerTokenAsync()
        {
            return await JsRuntime.InvokeAsync<string>("localStorage.getItem", "jwt");
        }

        public async Task<LoginResponseDto?> ObtenerUsuarioAsync()
        {
            var DatosUsuario = await JsRuntime.InvokeAsync<string>("localStorage.getItem", "usuario");
            return string.IsNullOrWhiteSpace(DatosUsuario)
                ? null
                : JsonSerializer.Deserialize<LoginResponseDto>(DatosUsuario);
        }

        public async Task<bool> RegisterAsync(RegisterRequestDto Solicitud, CancellationToken Ct = default)
        {
            var Respuesta = await ClienteHttp.PostAsJsonAsync("api/auth/register", Solicitud, Ct);
            return Respuesta.IsSuccessStatusCode;
        }

        public async Task<bool> ActivarCuentaAsync(ActivateAccountDto Solicitud, CancellationToken Ct = default)
        {
            var Respuesta = await ClienteHttp.PostAsJsonAsync("api/auth/activate", Solicitud, Ct);
            return Respuesta.IsSuccessStatusCode;
        }
    }
}
