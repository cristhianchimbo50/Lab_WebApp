using Lab_Contracts.Examenes;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Examenes
{
    public class ExamenReactivoAsociacionesApiService : BaseApiService, IExamenReactivoAsociacionesApiService
    {
        public ExamenReactivoAsociacionesApiService(IHttpClientFactory Factory, ProtectedSessionStorage Session, IJSRuntime Js)
            : base(Factory, Session, Js) { }

        public async Task<List<AsociacionReactivoDto>> ObtenerTodasAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, "api/ExamenReactivoAsociaciones");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<List<AsociacionReactivoDto>> BuscarPorExamenAsync(string NombreExamen)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/buscar-examen/{NombreExamen}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<List<AsociacionReactivoDto>> BuscarPorReactivoAsync(string NombreReactivo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/buscar-reactivo/{NombreReactivo}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<AsociacionReactivoDto?> ObtenerPorIdAsync(int IdAsociacion)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/{IdAsociacion}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<AsociacionReactivoDto>();
        }

        public async Task<AsociacionReactivoDto?> CrearAsync(AsociacionReactivoDto AsociacionReactivo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Post, "api/ExamenReactivoAsociaciones")
            {
                Content = JsonContent.Create(AsociacionReactivo)
            };
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            return Response.IsSuccessStatusCode
                ? await Response.Content.ReadFromJsonAsync<AsociacionReactivoDto>()
                : null;
        }

        public async Task<bool> EditarAsync(int IdAsociacion, AsociacionReactivoDto AsociacionReactivo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Put, $"api/ExamenReactivoAsociaciones/{IdAsociacion}")
            {
                Content = JsonContent.Create(AsociacionReactivo)
            };
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            return Response.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarAsync(int IdAsociacion)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Delete, $"api/ExamenReactivoAsociaciones/{IdAsociacion}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            return Response.IsSuccessStatusCode;
        }

        public async Task<List<AsociacionReactivoDto>> ObtenerPorExamenIdAsync(int IdExamen)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivoAsociaciones/asociados/{IdExamen}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<bool> GuardarPorExamenAsync(int IdExamen, List<AsociacionReactivoDto> Asociaciones)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Post, $"api/ExamenReactivoAsociaciones/asociados/{IdExamen}")
            {
                Content = JsonContent.Create(Asociaciones)
            };
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            return Response.IsSuccessStatusCode;
        }
    }
}
