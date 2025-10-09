using Lab_Contracts.Medicos;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Medicos
{
    public class MedicosApiService : IMedicosApiService
    {
        private readonly HttpClient _http;

        public MedicosApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<List<MedicoDto>> GetMedicosAsync()
        {
            return await _http.GetFromJsonAsync<List<MedicoDto>>("api/medicos") ?? new();
        }

        public async Task<MedicoDto?> GetMedicoPorIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<MedicoDto>($"api/medicos/{id}");
        }

        public async Task<List<MedicoDto>> BuscarMedicosAsync(string campo, string valor)
        {
            return await _http.GetFromJsonAsync<List<MedicoDto>>($"api/medicos/buscar?campo={campo}&valor={valor}") ?? new();
        }

        public async Task<HttpResponseMessage> CrearMedicoAsync(MedicoDto medico)
        {
            return await _http.PostAsJsonAsync("api/medicos", medico);
        }

        public async Task<HttpResponseMessage> EditarMedicoAsync(int id, MedicoDto medico)
        {
            return await _http.PutAsJsonAsync($"api/medicos/{id}", medico);
        }

        public async Task<HttpResponseMessage> AnularMedicoAsync(int id)
        {
            return await _http.PutAsync($"api/medicos/anular/{id}", null);
        }
    }
}
