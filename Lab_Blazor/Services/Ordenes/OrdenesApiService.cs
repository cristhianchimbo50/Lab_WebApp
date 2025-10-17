using Lab_Contracts.Examenes;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Resultados;

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

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenAsync(int idOrden)
        {
            return await _http.GetFromJsonAsync<OrdenDetalleDto>($"api/ordenes/detalle/{idOrden}");
        }

        public async Task<HttpResponseMessage> AnularOrdenAsync(int idOrden)
        {
            return await _http.PutAsync($"api/ordenes/anular/{idOrden}", null);
        }

        //PARA PRUEBASS
        public async Task<HttpResponseMessage> CrearOrdenHttpResponseAsync(OrdenCompletaDto orden)
        {
            return await _http.PostAsJsonAsync("api/ordenes", orden);
        }

        public async Task<byte[]> ObtenerTicketOrdenPdfAsync(int idOrden)
        {
            try
            {
                return await _http.GetByteArrayAsync($"api/ordenes/{idOrden}/ticket-pdf");
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        //para resultados

        public async Task<HttpResponseMessage> GuardarResultadosAsync(ResultadoGuardarDto dto)
        {
            return await _http.PostAsJsonAsync("api/Ordenes/ingresar-resultado", dto);
        }

        public async Task<List<ExamenDto>> ObtenerExamenesPorOrdenAsync(int idOrden)
        {
            return await _http.GetFromJsonAsync<List<ExamenDto>>($"api/Ordenes/{idOrden}/examenes") ?? new();
        }


    }


}