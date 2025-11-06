using Lab_Contracts.Examenes;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Lab_Blazor.Services.Examenes
{
    public class ExamenesApiService : BaseApiService, IExamenesApiService
    {
        public ExamenesApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

        public async Task<List<ExamenDto>> ListarExamenesAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/examenes");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<List<ExamenDto>> ListarExamenesAsync(string filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var url = string.IsNullOrWhiteSpace(filtro)
                ? "api/examenes"
                : $"api/examenes/buscar?nombre={filtro}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<ExamenDto?> ObtenerDetalleExamenAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/examenes/{id}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ExamenDto>();
        }

        public async Task<List<ExamenDto>> ListarExamenesPorNombreAsync(string nombre)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/examenes/buscar?nombre={nombre}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> GuardarExamenAsync(ExamenDto examen)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/examenes")
            {
                Content = JsonContent.Create(examen)
            };
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> GuardarExamenAsync(int id, ExamenDto examen)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/examenes/{id}")
            {
                Content = JsonContent.Create(examen)
            };
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> AnularExamenAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/examenes/anular/{id}");
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<List<ExamenDto>> ListarExamenesHijosAsync(int idPadre)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/examenes/{idPadre}/hijos");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> AsignarExamenHijoAsync(int idPadre, int idHijo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/examenes/{idPadre}/hijos/{idHijo}");
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> EliminarExamenHijoAsync(int idPadre, int idHijo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/examenes/{idPadre}/hijos/{idHijo}");
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }
    }
}
