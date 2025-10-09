using Lab_Contracts.Pacientes;
using System.Net.Http.Json;

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

        public async Task<HttpResponseMessage> CrearPacienteAsync(PacienteDto paciente)
        {
            return await _http.PostAsJsonAsync("api/pacientes", paciente);
        }

        public async Task<HttpResponseMessage> EditarPacienteAsync(int id, PacienteDto paciente)
        {
            return await _http.PutAsJsonAsync($"api/pacientes/{id}", paciente);
        }

        public async Task<HttpResponseMessage> AnularPacienteAsync(int id)
        {
            return await _http.PutAsync($"api/pacientes/anular/{id}", null);
        }
    }
}
