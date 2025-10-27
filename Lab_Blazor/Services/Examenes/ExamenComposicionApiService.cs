using Lab_Contracts.Examenes;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Examenes
{
    public class ExamenComposicionApiService : BaseApiService, IExamenComposicionApiService
    {
        public ExamenComposicionApiService(IHttpClientFactory factory, ProtectedSessionStorage session)
            : base(factory, session) { }

        public async Task<List<ExamenComposicionDto>> GetComposicionesPorExamenPadreAsync(int idExamenPadre)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/examencomposicion/padre/{idExamenPadre}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ExamenComposicionDto>>() ?? new();
        }

        public async Task<List<ExamenComposicionDto>> GetComposicionesPorExamenHijoAsync(int idExamenHijo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/examencomposicion/hijo/{idExamenHijo}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ExamenComposicionDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> CrearComposicionAsync(ExamenComposicionDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/examencomposicion")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> EliminarComposicionAsync(int idExamenPadre, int idExamenHijo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/examencomposicion?idExamenPadre={idExamenPadre}&idExamenHijo={idExamenHijo}");
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }
    }
}
