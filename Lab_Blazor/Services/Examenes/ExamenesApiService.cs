using Lab_Contracts.Examenes;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Examenes
{
    public class ExamenesApiService : BaseApiService, IExamenesApiService
    {
        public ExamenesApiService(IHttpClientFactory Factory, ProtectedSessionStorage Session, IJSRuntime Js)
            : base(Factory, Session, Js) { }

        public async Task<List<ExamenDto>> GetExamenesAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, "api/examenes");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<ExamenDto?> GetExamenPorIdAsync(int Id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/examenes/{Id}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<ExamenDto>();
        }

        public async Task<List<ExamenDto>> BuscarExamenesPorNombreAsync(string Nombre)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/examenes/buscar?nombre={Nombre}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> CrearExamenAsync(ExamenDto Examen)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Post, "api/examenes")
            {
                Content = JsonContent.Create(Examen)
            };
            AddTokenHeader(Request);

            return await _http.SendAsync(Request);
        }

        public async Task<HttpResponseMessage> EditarExamenAsync(int Id, ExamenDto Examen)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Put, $"api/examenes/{Id}")
            {
                Content = JsonContent.Create(Examen)
            };
            AddTokenHeader(Request);

            return await _http.SendAsync(Request);
        }

        public async Task<HttpResponseMessage> AnularExamenAsync(int Id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Put, $"api/examenes/anular/{Id}");
            AddTokenHeader(Request);

            return await _http.SendAsync(Request);
        }

        public async Task<List<ExamenDto>> ObtenerHijosDeExamenAsync(int IdExamenPadre)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/examenes/{IdExamenPadre}/hijos");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> AgregarExamenHijoAsync(int IdPadre, int IdHijo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Post, $"api/examenes/{IdPadre}/hijos/{IdHijo}");
            AddTokenHeader(Request);

            return await _http.SendAsync(Request);
        }

        public async Task<HttpResponseMessage> EliminarExamenHijoAsync(int IdPadre, int IdHijo)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Delete, $"api/examenes/{IdPadre}/hijos/{IdHijo}");
            AddTokenHeader(Request);

            return await _http.SendAsync(Request);
        }

        public async Task<List<AsociacionReactivoDto>> GetReactivosAsociadosPorExamenAsync(int IdExamen)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/ExamenReactivos/por-examen/{IdExamen}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<List<AsociacionReactivoDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> GuardarReactivosPorExamenAsync(int IdExamen, List<AsociacionReactivoDto> Reactivos)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Post, $"api/ExamenReactivos/guardar-masivo/{IdExamen}")
            {
                Content = JsonContent.Create(Reactivos)
            };
            AddTokenHeader(Request);

            return await _http.SendAsync(Request);
        }

        public async Task<List<ExamenDto>> ListarExamenesAsync(string Filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Url = string.IsNullOrWhiteSpace(Filtro)
                ? "api/examenes"
                : $"api/examenes/buscar?nombre={Filtro}";

            var Request = new HttpRequestMessage(HttpMethod.Get, Url);
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();
            return await Response.Content.ReadFromJsonAsync<List<ExamenDto>>() ?? new();
        }
    }
}
