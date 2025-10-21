using Lab_Contracts.Reactivos;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Reactivos
{
    public class ReactivosApiService : IReactivosApiService
    {
        private readonly HttpClient _http;

        public ReactivosApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<List<ReactivoDto>> GetReactivosAsync()
        {
            return await _http.GetFromJsonAsync<List<ReactivoDto>>("api/reactivos") ?? new();
        }

        public async Task<ReactivoDto?> GetReactivoPorIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<ReactivoDto>($"api/reactivos/{id}");
        }

        public async Task<HttpResponseMessage> CrearReactivoAsync(ReactivoDto dto)
        {
            return await _http.PostAsJsonAsync("api/reactivos", dto);
        }

        public async Task<HttpResponseMessage> EditarReactivoAsync(int id, ReactivoDto dto)
        {
            return await _http.PutAsJsonAsync($"api/reactivos/{id}", dto);
        }

        public async Task<HttpResponseMessage> AnularReactivoAsync(int id)
        {
            return await _http.PutAsync($"api/reactivos/anular/{id}", null);
        }

        public async Task<HttpResponseMessage> RegistrarIngresosAsync(IEnumerable<MovimientoReactivoIngresoDto> ingresos)
        {
            return await _http.PostAsJsonAsync("api/reactivos/ingresos", ingresos);
        }

        public async Task<HttpResponseMessage> RegistrarEgresosAsync(IEnumerable<MovimientoReactivoEgresoDto> egresos)
        {
            return await _http.PostAsJsonAsync("api/reactivos/egresos", egresos);
        }
    }
}
