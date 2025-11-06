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

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/ExamenReactivoAsociaciones");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenAsync(string nombreExamen)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/buscar-examen/{nombreExamen}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorReactivoAsync(string nombreReactivo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/buscar-reactivo/{nombreReactivo}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<AsociacionReactivoDto?> ObtenerDetalleAsociacionAsync(int idAsociacion)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/{idAsociacion}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AsociacionReactivoDto>();
        }

        public async Task<AsociacionReactivoDto?> CrearAsociacionAsync(AsociacionReactivoDto asociacionReactivo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/ExamenReactivoAsociaciones")
            {
                Content = JsonContent.Create(asociacionReactivo)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode
                ? await response.Content.ReadFromJsonAsync<AsociacionReactivoDto>()
                : null;
        }

        public async Task<bool> EditarAsociacionAsync(int idAsociacion, AsociacionReactivoDto asociacionReactivo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/ExamenReactivoAsociaciones/{idAsociacion}")
            {
                Content = JsonContent.Create(asociacionReactivo)
            };
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarAsociacionAsync(int idAsociacion)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"api/ExamenReactivoAsociaciones/{idAsociacion}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<AsociacionReactivoDto>> ListarAsociacionesPorExamenIdAsync(int idExamen)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/asociados/{idExamen}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<bool> GuardarAsociacionesPorExamenAsync(int idExamen, List<AsociacionReactivoDto> asociaciones)
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
