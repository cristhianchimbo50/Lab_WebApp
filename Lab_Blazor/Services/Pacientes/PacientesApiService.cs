using Lab_Contracts.Pacientes;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using Lab_Contracts.Common;

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
                try
                {
                    var contenido = await resp.Content.ReadFromJsonAsync<JsonElement>();
                    var dto = new PacienteDto();
                    if (contenido.TryGetProperty("idPaciente", out var Id)) dto.IdPaciente = Id.GetInt32();
                    if (contenido.TryGetProperty("nombrePaciente", out var Nombre)) dto.NombrePaciente = Nombre.GetString() ?? "";
                    if (contenido.TryGetProperty("correoElectronicoPaciente", out var Correo)) dto.CorreoElectronicoPaciente = Correo.GetString() ?? "";
                    if (contenido.TryGetProperty("contraseniaTemporal", out var Temporal)) dto.ContraseniaTemporal = Temporal.GetString();
                    string mensaje = contenido.TryGetProperty("mensaje", out var Msg) ? Msg.GetString() ?? "Paciente registrado correctamente." : "Paciente registrado correctamente.";
                    return (true, mensaje, dto);
                }
                catch { return (true, "Paciente registrado correctamente.", null); }
            }
            string errorMsg = resp.StatusCode == System.Net.HttpStatusCode.Conflict ? await resp.Content.ReadAsStringAsync() : $"Error {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}";
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

        public async Task<(bool Exito, string Mensaje)> ReenviarCredencialesTemporalesAsync(int idPaciente)
        {
            if (!await SetAuthHeaderAsync()) throw new HttpRequestException("Token no disponible o sesión expirada.");
            var req = new HttpRequestMessage(HttpMethod.Post, $"api/pacientes/{idPaciente}/reenviar-temporal");
            AddTokenHeader(req);
            var resp = await _http.SendAsync(req);
            if (resp.IsSuccessStatusCode) return (true, "Se envió una nueva contraseña temporal al correo del paciente.");
            var err = await resp.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(err) ? "No se pudo reenviar la contraseña temporal." : err);
        }
    }
}
