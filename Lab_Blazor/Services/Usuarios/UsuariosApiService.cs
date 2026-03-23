using Lab_Contracts.Usuarios;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Usuarios
{
    public class UsuariosApiService : BaseApiService, IUsuariosApiService
    {
        public UsuariosApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

        public async Task<List<UsuarioListadoDto>> ListarUsuariosAsync(UsuarioFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var parametros = new List<string>();
            if (!string.IsNullOrWhiteSpace(filtro.Nombre)) parametros.Add($"nombre={Uri.EscapeDataString(filtro.Nombre)}");
            if (!string.IsNullOrWhiteSpace(filtro.Correo)) parametros.Add($"correo={Uri.EscapeDataString(filtro.Correo)}");
            if (filtro.IdRol.HasValue) parametros.Add($"idRol={filtro.IdRol.Value}");
            if (filtro.Activo.HasValue) parametros.Add($"activo={filtro.Activo.Value.ToString().ToLower()}");
            var url = "api/usuarios" + (parametros.Count > 0 ? "?" + string.Join("&", parametros) : "");
            var req = new HttpRequestMessage(HttpMethod.Get, url); AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<UsuarioListadoDto>>() ?? new();
        }

        public async Task<UsuarioListadoDto?> ObtenerDetalleUsuarioAsync(int idUsuario)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/usuarios/{idUsuario}"); AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<UsuarioListadoDto>();
        }

        public async Task<int> GuardarUsuarioAsync(UsuarioCrearDto usuario)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/usuarios") { Content = JsonContent.Create(usuario) }; AddTokenHeader(req);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                throw new Exception(await ExtraerMensajeErrorAsync(resp, "Error al guardar usuario."));
            return await resp.Content.ReadFromJsonAsync<int>();
        }

        public async Task<bool> GuardarUsuarioAsync(UsuarioEditarDto usuario)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Put, $"api/usuarios/{usuario.IdUsuario}") { Content = JsonContent.Create(usuario) }; AddTokenHeader(req);
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode)
                throw new Exception(await ExtraerMensajeErrorAsync(resp, "Error al actualizar usuario."));
            return true;
        }

        public async Task<bool> CambiarEstadoUsuarioAsync(int idUsuario, bool activo)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Put, $"api/usuarios/{idUsuario}/estado") { Content = JsonContent.Create(activo) }; AddTokenHeader(req);
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode) return true;
            var contenido = await resp.Content.ReadAsStringAsync();
            try
            {
                var error = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(contenido);
                if (error != null && error.ContainsKey("Mensaje")) throw new Exception(error["Mensaje"]);
            }
            catch { }
            throw new Exception("Error al cambiar estado del usuario.");
        }

        private static async Task<string> ExtraerMensajeErrorAsync(HttpResponseMessage resp, string predeterminado)
        {
            var contenido = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(contenido)) return predeterminado;

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(contenido);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("Mensaje", out var m)) return m.GetString() ?? predeterminado;
                    if (doc.RootElement.TryGetProperty("message", out var mm)) return mm.GetString() ?? predeterminado;
                    if (doc.RootElement.TryGetProperty("title", out var t)) return t.GetString() ?? predeterminado;
                }
            }
            catch { }

            return contenido;
        }
    }
}
