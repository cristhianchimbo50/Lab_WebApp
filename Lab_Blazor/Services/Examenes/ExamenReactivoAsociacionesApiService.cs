using Lab_Contracts.Examenes;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Examenes
{
    public class ExamenReactivoAsociacionesApiService : BaseApiService, IExamenReactivoAsociacionesApiService
    {
        public ExamenReactivoAsociacionesApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

        public async Task<List<AsociacionReactivoDto>> ObtenerTodasAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/ExamenReactivoAsociaciones");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<List<AsociacionReactivoDto>> BuscarPorExamenAsync(string nombre)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/buscar-examen/{nombre}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<List<AsociacionReactivoDto>> BuscarPorReactivoAsync(string nombre)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/buscar-reactivo/{nombre}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<AsociacionReactivoDto?> ObtenerPorIdAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/{id}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AsociacionReactivoDto>();
        }

        public async Task<AsociacionReactivoDto?> CrearAsync(AsociacionReactivoDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/ExamenReactivoAsociaciones")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<AsociacionReactivoDto>()
                : null;
        }

        public async Task<bool> EditarAsync(int id, AsociacionReactivoDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/ExamenReactivoAsociaciones/{id}")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/ExamenReactivoAsociaciones/{id}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<AsociacionReactivoDto>> ObtenerPorExamenIdAsync(int idExamen)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/asociados/{idExamen}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<bool> GuardarPorExamenAsync(int idExamen, List<AsociacionReactivoDto> asociaciones)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, $"api/ExamenReactivoAsociaciones/asociados/{idExamen}")
            {
                Content = JsonContent.Create(asociaciones)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
