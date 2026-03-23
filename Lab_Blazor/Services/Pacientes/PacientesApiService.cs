using Lab_Contracts.Pacientes;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Lab_Contracts.Common;
using System.Linq;

namespace Lab_Blazor.Services.Pacientes
{
    public class PacientesApiService : BaseApiService, IPacientesApiService
    {
        public PacientesApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

        public async Task<List<PacienteDto>> ListarPacientesAsync()
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Get, "api/pacientes");
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<PacienteDto>>() ?? new();
        }

        public async Task<PacienteDto?> ObtenerDetallePacienteAsync(int idPaciente)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/pacientes/{idPaciente}");
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<PacienteDto>();
        }

        public async Task<List<PacienteDto>> ListarPacientesAsync(string criterio, string valor)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var c = Uri.EscapeDataString(criterio ?? string.Empty);
            var v = Uri.EscapeDataString(valor ?? string.Empty);
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/pacientes/buscar?criterio={c}&valor={v}");
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<PacienteDto>>() ?? new();
        }

        public async Task<ResultadoPaginadoDto<PacienteDto>> ListarPacientesPaginadosAsync(PacienteFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/pacientes/buscar") { Content = JsonContent.Create(filtro) };
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ResultadoPaginadoDto<PacienteDto>>()
                ?? new ResultadoPaginadoDto<PacienteDto> { Items = new List<PacienteDto>(), PageNumber = filtro.PageNumber, PageSize = filtro.PageSize };
        }

        public async Task<(bool Exito, string Mensaje, PacienteDto? Paciente)> GuardarPacienteAsync(PacienteDto paciente)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/pacientes") { Content = JsonContent.Create(paciente) };
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode)
            {
                var dto = await resp.Content.ReadFromJsonAsync<PacienteDto?>();
                return (true, "Paciente registrado correctamente.", dto);
            }
            string errorMsg = await ExtraerMensajeErrorAsync(resp, $"Error {resp.StatusCode} al guardar paciente.");
            return (false, errorMsg, null);
        }

        public async Task<HttpResponseMessage> GuardarPacienteAsync(int idPaciente, PacienteDto paciente)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Put, $"api/pacientes/{idPaciente}") { Content = JsonContent.Create(paciente) };
            AddTokenHeader(req);
            return await _http.SendAsync(req);
        }

        public async Task<HttpResponseMessage> AnularPacienteAsync(int idPaciente)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Put, $"api/pacientes/anular/{idPaciente}");
            AddTokenHeader(req);
            return await _http.SendAsync(req);
        }

        public async Task<PacienteDto?> ObtenerPacientePorCedulaAsync(string cedula)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var c = Uri.EscapeDataString(cedula ?? string.Empty);
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/pacientes/buscar?criterio=cedula&valor={c}");
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            var lista = await resp.Content.ReadFromJsonAsync<List<PacienteDto>>();
            return lista?.FirstOrDefault();
        }

        public async Task<PacienteDto?> ObtenerPersonaPorCedulaAsync(string cedula)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var c = Uri.EscapeDataString(cedula ?? string.Empty);
            var req = new HttpRequestMessage(HttpMethod.Get, $"api/pacientes/persona?cedula={c}");
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req);
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<PacienteDto>();
        }

        public async Task<List<GeneroDto>> ListarGenerosAsync()
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Get, "api/pacientes/generos");
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req); resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<List<GeneroDto>>() ?? new();
        }

        private static async Task<string> ExtraerMensajeErrorAsync(HttpResponseMessage resp, string predeterminado)
        {
            var contenido = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(contenido)) return predeterminado;

            try
            {
                using var doc = JsonDocument.Parse(contenido);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
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
