using Lab_Contracts.Examenes;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Examenes
{
    public class ExamenComposicionApiService : IExamenComposicionApiService
    {
        private readonly HttpClient _http;

        public ExamenComposicionApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<List<ExamenComposicionDto>> GetComposicionesPorExamenPadreAsync(int idExamenPadre)
        {
            return await _http.GetFromJsonAsync<List<ExamenComposicionDto>>($"api/examencomposicion/padre/{idExamenPadre}") ?? new();
        }

        public async Task<List<ExamenComposicionDto>> GetComposicionesPorExamenHijoAsync(int idExamenHijo)
        {
            return await _http.GetFromJsonAsync<List<ExamenComposicionDto>>($"api/examencomposicion/hijo/{idExamenHijo}") ?? new();
        }

        public async Task<HttpResponseMessage> CrearComposicionAsync(ExamenComposicionDto dto)
        {
            return await _http.PostAsJsonAsync("api/examencomposicion", dto);
        }

        public async Task<HttpResponseMessage> EliminarComposicionAsync(int idExamenPadre, int idExamenHijo)
        {
            return await _http.DeleteAsync($"api/examencomposicion?idExamenPadre={idExamenPadre}&idExamenHijo={idExamenHijo}");
        }
    }
}
