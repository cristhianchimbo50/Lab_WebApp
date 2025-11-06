using Lab_Contracts.Common;
using Lab_Contracts.Medicos;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Medicos
{
    public class MedicosApiService : BaseApiService, IMedicosApiService
    {
        public MedicosApiService(IHttpClientFactory Factory, ProtectedSessionStorage Session, IJSRuntime Js)
            : base(Factory, Session, Js) { }

        public async Task<List<MedicoDto>> ObtenerMedicosAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, "api/medicos");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();

            return await Response.Content.ReadFromJsonAsync<List<MedicoDto>>() ?? new();
        }

        public async Task<MedicoDto?> ObtenerMedicoPorIdAsync(int Id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/medicos/{Id}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();

            return await Response.Content.ReadFromJsonAsync<MedicoDto>();
        }

        public async Task<List<MedicoDto>> BuscarMedicosAsync(string Campo, string Valor)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, $"api/medicos/buscar?campo={Campo}&valor={Valor}");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();

            return await Response.Content.ReadFromJsonAsync<List<MedicoDto>>() ?? new();
        }

        public async Task<ResultadoPaginadoDto<MedicoDto>> BuscarMedicosAsync(MedicoFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Post, "api/medicos/buscar")
            {
                Content = JsonContent.Create(filtro)
            };
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();

            return await Response.Content.ReadFromJsonAsync<ResultadoPaginadoDto<MedicoDto>>()
                ?? new ResultadoPaginadoDto<MedicoDto> { Items = new List<MedicoDto>(), PageNumber = filtro.PageNumber, PageSize = filtro.PageSize };
        }

        public async Task<HttpResponseMessage> CrearMedicoAsync(MedicoDto Dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Post, "api/medicos")
            {
                Content = JsonContent.Create(Dto)
            };
            AddTokenHeader(Request);

            return await _http.SendAsync(Request);
        }

        public async Task<HttpResponseMessage> EditarMedicoAsync(int Id, MedicoDto Medico)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Put, $"api/medicos/{Id}")
            {
                Content = JsonContent.Create(Medico)
            };
            AddTokenHeader(Request);

            return await _http.SendAsync(Request);
        }

        public async Task<HttpResponseMessage> AnularMedicoAsync(int Id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Put, $"api/medicos/anular/{Id}");
            AddTokenHeader(Request);

            return await _http.SendAsync(Request);
        }

        public async Task<List<MedicoDto>> ListarMedicosAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var Request = new HttpRequestMessage(HttpMethod.Get, "api/medicos");
            AddTokenHeader(Request);

            var Response = await _http.SendAsync(Request);
            Response.EnsureSuccessStatusCode();

            return await Response.Content.ReadFromJsonAsync<List<MedicoDto>>() ?? new();
        }
    }
}
