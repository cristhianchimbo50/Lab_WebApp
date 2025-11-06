using Lab_Contracts.Examenes;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Examenes
{
    public class ExamenComposicionApiService : BaseApiService, IExamenComposicionApiService
    {
        public ExamenComposicionApiService(IHttpClientFactory Factory, ProtectedSessionStorage Session, IJSRuntime Js)
            : base(Factory, Session, Js) { }

        public async Task<List<ExamenComposicionDto>> ObtenerComposicionesPorExamenPadreAsync(int IdExamenPadre)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/examencomposicion/padre/{IdExamenPadre}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<List<ExamenComposicionDto>>() ?? new();
        }

        public async Task<List<ExamenComposicionDto>> ObtenerComposicionesPorExamenHijoAsync(int IdExamenHijo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/examencomposicion/hijo/{IdExamenHijo}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<List<ExamenComposicionDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> CrearComposicionAsync(ExamenComposicionDto Composicion)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Post, "api/examencomposicion")
            {
                Content = JsonContent.Create(Composicion)
            };
            AddTokenHeader(Request);

            return await _http.SendAsync(Request);
        }

        public async Task<HttpResponseMessage> EliminarComposicionAsync(int IdExamenPadre, int IdExamenHijo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Delete, $"api/examencomposicion?idExamenPadre={IdExamenPadre}&idExamenHijo={IdExamenHijo}");
            AddTokenHeader(Request);

            return await _http.SendAsync(Request);
        }
    }
}
