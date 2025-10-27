using Lab_Contracts.Usuarios;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Usuarios
{
    public class UsuariosApiService : IUsuariosApiService
    {
        private readonly HttpClient _http;

        public UsuariosApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<List<UsuarioListadoDto>> GetUsuariosAsync(UsuarioFiltroDto filtro)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                query.Add($"nombre={Uri.EscapeDataString(filtro.Nombre)}");
            if (!string.IsNullOrWhiteSpace(filtro.Correo))
                query.Add($"correo={Uri.EscapeDataString(filtro.Correo)}");
            if (!string.IsNullOrWhiteSpace(filtro.Rol))
                query.Add($"rol={Uri.EscapeDataString(filtro.Rol)}");
            if (filtro.Activo.HasValue)
                query.Add($"activo={filtro.Activo.Value}");

            var url = "api/usuarios";
            if (query.Any())
                url += "?" + string.Join("&", query);

            return await _http.GetFromJsonAsync<List<UsuarioListadoDto>>(url) ?? new();
        }

        public async Task<UsuarioListadoDto?> GetUsuarioPorIdAsync(int idUsuario)
        {
            return await _http.GetFromJsonAsync<UsuarioListadoDto>($"api/usuarios/{idUsuario}");
        }

        public async Task<int> CrearUsuarioAsync(UsuarioCrearDto dto)
        {
            var resp = await _http.PostAsJsonAsync("api/usuarios", dto);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<int>();
        }

        public async Task<bool> EditarUsuarioAsync(UsuarioEditarDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"api/usuarios/{dto.IdUsuario}", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> CambiarEstadoAsync(int idUsuario, bool activo)
        {
            var resp = await _http.PutAsJsonAsync($"api/usuarios/{idUsuario}/estado", activo);
            return resp.IsSuccessStatusCode;
        }

        public async Task<UsuarioReenviarDto?> ReenviarCredencialesTemporalesAsync(int idUsuario)
        {
            var resp = await _http.PutAsync($"api/usuarios/{idUsuario}/reenviar", null);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<UsuarioReenviarDto>();
        }
    }
}
