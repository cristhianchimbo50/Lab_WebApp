using Lab_Contracts.Examenes;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Examenes
{
    public class ExamenesApiService : BaseApiService, IExamenesApiService
    {
        public ExamenesApiService(IHttpClientFactory factory, ProtectedSessionStorage session)
            : base(factory, session) { }

        public async Task<List<ExamenDto>> GetExamenesAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/examenes");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<ExamenDto?> GetExamenPorIdAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/examenes/{id}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ExamenDto>();
        }

        public async Task<List<ExamenDto>> BuscarExamenesPorNombreAsync(string nombre)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/examenes/buscar?nombre={nombre}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> CrearExamenAsync(ExamenDto examen)
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

        public async Task<HttpResponseMessage> EditarExamenAsync(int id, ExamenDto examen)
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

        public async Task<List<ExamenDto>> ObtenerHijosDeExamenAsync(int idExamenPadre)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/examenes/{idExamenPadre}/hijos");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> AgregarExamenHijoAsync(int idPadre, int idHijo)
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

        public async Task<List<AsociacionReactivoDto>> GetReactivosAsociadosPorExamenAsync(int idExamen)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivos/por-examen/{idExamen}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> GuardarReactivosPorExamenAsync(int idExamen, List<AsociacionReactivoDto> reactivos)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/ExamenReactivos/guardar-masivo/{idExamen}")
            {
                Content = JsonContent.Create(reactivos)
            };
            AddTokenHeader(request);

            return await _http.SendAsync(request);
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
    }
}
