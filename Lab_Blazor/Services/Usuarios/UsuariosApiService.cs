using Lab_Contracts.Usuarios;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Usuarios
{
    public class UsuariosApiService : BaseApiService, IUsuariosApiService
    {
        public UsuariosApiService(IHttpClientFactory Factory, ProtectedSessionStorage Sesion, IJSRuntime Js)
            : base(Factory, Sesion, Js) { }

        public async Task<List<UsuarioListadoDto>> ObtenerUsuariosAsync(UsuarioFiltroDto Filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Parametros = new List<string>();
            if (!string.IsNullOrWhiteSpace(Filtro.Nombre))
                Parametros.Add($"nombre={Uri.EscapeDataString(Filtro.Nombre)}");
            if (!string.IsNullOrWhiteSpace(Filtro.Correo))
                Parametros.Add($"correo={Uri.EscapeDataString(Filtro.Correo)}");
            if (!string.IsNullOrWhiteSpace(Filtro.Rol))
                Parametros.Add($"rol={Uri.EscapeDataString(Filtro.Rol)}");
            if (Filtro.Activo.HasValue)
                Parametros.Add($"activo={Filtro.Activo.Value.ToString().ToLower()}");

            var Url = "api/usuarios";
            if (Parametros.Count > 0)
                Url += "?" + string.Join("&", Parametros);

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, Url);
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<List<UsuarioListadoDto>>() ?? new();
        }

        public async Task<UsuarioListadoDto?> ObtenerUsuarioPorIdAsync(int IdUsuario)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Get, $"api/usuarios/{IdUsuario}");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<UsuarioListadoDto>();
        }

        public async Task<int> CrearUsuarioAsync(UsuarioCrearDto Usuario)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Post, "api/usuarios")
            {
                Content = JsonContent.Create(Usuario)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<int>();
        }

        public async Task<bool> EditarUsuarioAsync(UsuarioEditarDto Usuario)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/usuarios/{Usuario.IdUsuario}")
            {
                Content = JsonContent.Create(Usuario)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            return Respuesta.IsSuccessStatusCode;
        }

        public async Task<bool> CambiarEstadoAsync(int IdUsuario, bool Activo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/usuarios/{IdUsuario}/estado")
            {
                Content = JsonContent.Create(Activo)
            };
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            if (Respuesta.IsSuccessStatusCode)
                return true;

            var Contenido = await Respuesta.Content.ReadAsStringAsync();
            try
            {
                var Error = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(Contenido);
                if (Error != null && Error.ContainsKey("Mensaje"))
                    throw new Exception(Error["Mensaje"]);
            }
            catch { }

            throw new Exception("Error al cambiar estado del usuario.");
        }


        public async Task<UsuarioReenviarDto?> ReenviarCredencialesTemporalesAsync(int IdUsuario)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Solicitud = new HttpRequestMessage(HttpMethod.Put, $"api/usuarios/{IdUsuario}/reenviar");
            AddTokenHeader(Solicitud);

            var Respuesta = await _http.SendAsync(Solicitud);
            Respuesta.EnsureSuccessStatusCode();

            return await Respuesta.Content.ReadFromJsonAsync<UsuarioReenviarDto>();
        }
    }
}
