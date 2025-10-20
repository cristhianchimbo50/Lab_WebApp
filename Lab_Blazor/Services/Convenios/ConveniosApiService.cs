using Lab_Contracts.Convenios;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Convenios
{
    public class ConveniosApiService : IConveniosApiService
    {
        private readonly HttpClient _http;

        public ConveniosApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<List<ConvenioDto>> ObtenerConveniosAsync()
        {
            var result = await _http.GetFromJsonAsync<List<ConvenioDto>>("api/convenios");
            return result ?? new List<ConvenioDto>();
        }

        public async Task<ConvenioDetalleDto?> ObtenerDetalleAsync(int id)
        {
            return await _http.GetFromJsonAsync<ConvenioDetalleDto>($"api/convenios/{id}");
        }

        public async Task<List<OrdenDisponibleDto>> ObtenerOrdenesDisponiblesAsync(int idMedico)
        {
            var result = await _http.GetFromJsonAsync<List<OrdenDisponibleDto>>(
                $"api/convenios/ordenes-disponibles/{idMedico}");
            return result ?? new List<OrdenDisponibleDto>();
        }

        public async Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/convenios", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AnularConvenioAsync(int id)
        {
            var response = await _http.PutAsync($"api/convenios/{id}/anular", null);
            return response.IsSuccessStatusCode;
        }
    }
}
