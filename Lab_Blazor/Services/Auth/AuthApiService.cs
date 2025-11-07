using Lab_Contracts.Auth;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Auth
{
    public class AuthApiService : BaseApiService, IAuthApiService
    {
        public AuthApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

        public async Task<(bool Exito, string Mensaje, LoginResponseDto? Usuario, bool RequiereCambioClave)> IniciarSesionAsync(LoginRequestDto solicitud, CancellationToken ct = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/login") { Content = JsonContent.Create(solicitud) };
            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return (false, "Credenciales inválidas o la cuenta está bloqueada.", null, false);
            var usuario = await resp.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: ct);
            return (true, "Inicio de sesión exitoso.", usuario, false);
        }

        public Task LogoutAsync(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public async Task<(bool Exito, string Mensaje)> CambiarContraseniaAsync(CambiarContraseniaDto solicitud, CancellationToken ct = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/cambiar-contrasenia") { Content = JsonContent.Create(solicitud) };
            var resp = await _http.SendAsync(req, ct);
            var resultado = await resp.Content.ReadFromJsonAsync<CambiarContraseniaResponseDto>(cancellationToken: ct);
            return resultado != null ? (resultado.Exito, resultado.Mensaje) : (false, "Error de red");
        }

        public Task<bool> RegistrarUsuarioAsync(RegisterRequestDto solicitud, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public async Task<RespuestaMensajeDto> ActivarCuentaAsync(RestablecerContraseniaDto solicitud, CancellationToken ct = default)
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/activar-cuenta")
            {
                Content = JsonContent.Create(solicitud)
            };

            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
                return new RespuestaMensajeDto { Exito = false, Mensaje = "Error al activar la cuenta." };

            var resultado = await resp.Content.ReadFromJsonAsync<RespuestaMensajeDto>(cancellationToken: ct);
            return resultado ?? new RespuestaMensajeDto { Exito = false, Mensaje = "Respuesta inválida del servidor." };
        }


        public async Task<bool> VerificarSesionAsync(CancellationToken ct = default)
        {
            if (!await SetAuthHeaderAsync()) return false;
            var req = new HttpRequestMessage(HttpMethod.Get, "api/auth/verificar-sesion");
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return false;
            var resultado = await resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>(cancellationToken: ct);
            if (resultado.ValueKind != System.Text.Json.JsonValueKind.Object) return false;
            if (resultado.TryGetProperty("Activa", out var activaProp) && activaProp.ValueKind == System.Text.Json.JsonValueKind.True)
                return true;
            return false;
        }

        public Task<string?> ObtenerTokenAsync()
        {
            throw new NotImplementedException();
        }

        public Task<LoginResponseDto?> ObtenerUsuarioAsync()
        {
            throw new NotImplementedException();
        }


    }
}
