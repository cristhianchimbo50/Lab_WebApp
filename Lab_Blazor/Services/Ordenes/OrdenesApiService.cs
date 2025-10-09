using Lab_Contracts.Ordenes;

namespace Lab_Blazor.Services.Ordenes
{
    public class OrdenesApiService : IOrdenesApiService
    {
        private readonly HttpClient _http;

        public OrdenesApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<List<OrdenDto>> GetOrdenesAsync()
        {
            return await _http.GetFromJsonAsync<List<OrdenDto>>("api/ordenes") ?? new();
        }

        public async Task<OrdenDto?> GetOrdenPorIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<OrdenDto>($"api/ordenes/{id}");
        }

        public async Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto orden)
        {
            var resp = await _http.PostAsJsonAsync("api/ordenes", orden);
            return await resp.Content.ReadFromJsonAsync<OrdenRespuestaDto>();
        }
    }


}