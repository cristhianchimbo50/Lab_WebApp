using Lab_Contracts.Pacientes;
using System.Net.Http.Json;
using System.Text.Json;

namespace Lab_Blazor.Services.Pacientes
{
    public class PacientesApiService : IPacientesApiService
    {
        private readonly HttpClient _http;

        public PacientesApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<List<PacienteDto>> GetPacientesAsync()
        {
            return await _http.GetFromJsonAsync<List<PacienteDto>>("api/pacientes") ?? new();
        }

        public async Task<PacienteDto?> GetPacientePorIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<PacienteDto>($"api/pacientes/{id}");
        }

        public async Task<List<PacienteDto>> BuscarPacientesAsync(string campo, string valor)
        {
            return await _http.GetFromJsonAsync<List<PacienteDto>>($"api/pacientes/buscar?campo={campo}&valor={valor}") ?? new();
        }

        public async Task<(bool Exito, string Mensaje, PacienteDto? Paciente)> CrearPacienteAsync(PacienteDto paciente)
        {
            var response = await _http.PostAsJsonAsync("api/pacientes", paciente);

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

            string errorMsg;
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                errorMsg = await response.Content.ReadAsStringAsync();
            else
                errorMsg = $"Error {response.StatusCode}: {await response.Content.ReadAsStringAsync()}";

            return (false, errorMsg, null);
        }


        public async Task<HttpResponseMessage> EditarPacienteAsync(int id, PacienteDto paciente)
        {
            return await _http.PutAsJsonAsync($"api/pacientes/{id}", paciente);
        }

        public async Task<HttpResponseMessage> AnularPacienteAsync(int id)
        {
            return await _http.PutAsync($"api/pacientes/anular/{id}", null);
        }

        public async Task<PacienteDto?> ObtenerPacientePorCedulaAsync(string cedula)
        {
            var url = $"api/pacientes/buscar?campo=cedula&valor={cedula}";
            var pacientes = await _http.GetFromJsonAsync<List<PacienteDto>>(url);
            return pacientes?.FirstOrDefault();
        }

        public async Task<(bool Exito, string Mensaje)> ReenviarTemporalAsync(int idPaciente)
        {
            var resp = await _http.PostAsync($"api/pacientes/{idPaciente}/reenviar-temporal", null);
            if (resp.IsSuccessStatusCode)
            {
                var txt = await resp.Content.ReadAsStringAsync();
                return (true, "Se envió una nueva contraseña temporal al correo del paciente.");
            }
            var err = await resp.Content.ReadAsStringAsync();
            return (false, string.IsNullOrWhiteSpace(err) ? "No se pudo reenviar la contraseña temporal." : err);
        }


    }
}
