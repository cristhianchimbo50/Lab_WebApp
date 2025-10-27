using Lab_Contracts.Pacientes;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;
using System.Text.Json;

namespace Lab_Blazor.Services.Pacientes
{
    public class PacientesApiService : BaseApiService, IPacientesApiService
    {
        public PacientesApiService(IHttpClientFactory factory, ProtectedSessionStorage session)
            : base(factory, session) { }

        public async Task<List<PacienteDto>> GetPacientesAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/pacientes");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<PacienteDto>>() ?? new();
        }

        public async Task<PacienteDto?> GetPacientePorIdAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/pacientes/{id}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PacienteDto>();
        }

        public async Task<List<PacienteDto>> BuscarPacientesAsync(string campo, string valor)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/pacientes/buscar?campo={campo}&valor={valor}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<PacienteDto>>() ?? new();
        }

        public async Task<(bool Exito, string Mensaje, PacienteDto? Paciente)> CrearPacienteAsync(PacienteDto paciente)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/pacientes")
            {
                Content = JsonContent.Create(paciente)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var contenido = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var dto = new PacienteDto();

                    if (contenido.TryGetProperty("idPaciente", out var id))
                        dto.IdPaciente = id.GetInt32();
                    if (contenido.TryGetProperty("nombrePaciente", out var nombre))
                        dto.NombrePaciente = nombre.GetString() ?? "";
                    if (contenido.TryGetProperty("correoElectronicoPaciente", out var correo))
                        dto.CorreoElectronicoPaciente = correo.GetString() ?? "";
                    if (contenido.TryGetProperty("contraseñaTemporal", out var tempPass))
                        dto.ContraseñaTemporal = tempPass.GetString();

                    string mensaje = contenido.TryGetProperty("mensaje", out var msg)
                        ? msg.GetString() ?? "Paciente registrado correctamente."
                        : "Paciente registrado correctamente.";

                    return (true, mensaje, dto);
                }
                catch
                {
                    return (true, "Paciente registrado correctamente.", null);
                }
            }

            string errorMsg = response.StatusCode == System.Net.HttpStatusCode.Conflict
                ? await response.Content.ReadAsStringAsync()
                : $"Error {response.StatusCode}: {await response.Content.ReadAsStringAsync()}";

            return (false, errorMsg, null);
        }

        public async Task<HttpResponseMessage> EditarPacienteAsync(int id, PacienteDto paciente)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/pacientes/{id}")
            {
                Content = JsonContent.Create(paciente)
            };
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> AnularPacienteAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/pacientes/anular/{id}");
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<PacienteDto?> ObtenerPacientePorCedulaAsync(string cedula)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/pacientes/buscar?campo=cedula&valor={cedula}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var pacientes = await response.Content.ReadFromJsonAsync<List<PacienteDto>>();
            return pacientes?.FirstOrDefault();
        }

        public async Task<(bool Exito, string Mensaje)> ReenviarTemporalAsync(int idPaciente)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/pacientes/{idPaciente}/reenviar-temporal");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
                return (true, "Se envió una nueva contraseña temporal al correo del paciente.");

            var err = await response.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(err) ? "No se pudo reenviar la contraseña temporal." : err);
        }
    }
}
