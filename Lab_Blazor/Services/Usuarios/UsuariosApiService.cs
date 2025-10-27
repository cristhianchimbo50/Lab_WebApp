using Lab_Contracts.Usuarios;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Usuarios
{
    public class UsuariosApiService : BaseApiService, IUsuariosApiService
    {
        public UsuariosApiService(IHttpClientFactory factory, ProtectedSessionStorage session)
            : base(factory, session) { }

        public async Task<List<UsuarioListadoDto>> GetUsuariosAsync(UsuarioFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                query.Add($"nombre={Uri.EscapeDataString(filtro.Nombre)}");
            if (!string.IsNullOrWhiteSpace(filtro.Correo))
                query.Add($"correo={Uri.EscapeDataString(filtro.Correo)}");
            if (!string.IsNullOrWhiteSpace(filtro.Rol))
                query.Add($"rol={Uri.EscapeDataString(filtro.Rol)}");
            if (filtro.Activo.HasValue)
                query.Add($"activo={filtro.Activo.Value.ToString().ToLower()}");

            var url = "api/usuarios";
            if (query.Count > 0)
                url += "?" + string.Join("&", query);

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<UsuarioListadoDto>>() ?? new();
        }

        public async Task<UsuarioListadoDto?> GetUsuarioPorIdAsync(int idUsuario)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/usuarios/{idUsuario}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UsuarioListadoDto>();
        }

        public async Task<int> CrearUsuarioAsync(UsuarioCrearDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/usuarios")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<int>();
        }

        public async Task<bool> EditarUsuarioAsync(UsuarioEditarDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/usuarios/{dto.IdUsuario}")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CambiarEstadoAsync(int idUsuario, bool activo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/usuarios/{idUsuario}/estado")
            {
                Content = JsonContent.Create(activo)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return true;

            var contenido = await response.Content.ReadAsStringAsync();
            try
            {
                var error = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(contenido);
                if (error != null && error.ContainsKey("Mensaje"))
                    throw new Exception(error["Mensaje"]);
            }
            catch { }

            throw new Exception("Error al cambiar estado del usuario.");
        }


        public async Task<UsuarioReenviarDto?> ReenviarCredencialesTemporalesAsync(int idUsuario)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/usuarios/{idUsuario}/reenviar");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UsuarioReenviarDto>();
        }
    }
}
