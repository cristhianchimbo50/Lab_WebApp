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

        public async Task<List<MedicoDto>> ListarMedicosAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/medicos");
            AddTokenHeader(request);
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<MedicoDto>>() ?? new();
        }

        public async Task<List<MedicoDto>> ListarMedicosAsync(string campo, string valor)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/medicos/buscar?campo={campo}&valor={valor}");
            AddTokenHeader(request);
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<MedicoDto>>() ?? new();
        }

        public async Task<MedicoDto?> ObtenerDetalleMedicoAsync(int idMedico)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/medicos/{idMedico}");
            AddTokenHeader(request);
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MedicoDto>();
        }

        public async Task<ResultadoPaginadoDto<MedicoDto>> ListarMedicosPaginadosAsync(MedicoFiltroDto filtro)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/medicos/buscar")
            {
                Content = JsonContent.Create(filtro)
            };
            AddTokenHeader(request);
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ResultadoPaginadoDto<MedicoDto>>()
                ?? new ResultadoPaginadoDto<MedicoDto> { Items = new List<MedicoDto>(), PageNumber = filtro.PageNumber, PageSize = filtro.PageSize };
        }

        public async Task<HttpResponseMessage> GuardarMedicoAsync(MedicoDto medico)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/medicos")
            {
                Content = JsonContent.Create(medico)
            };
            AddTokenHeader(request);
            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> GuardarMedicoAsync(int idMedico, MedicoDto medico)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/medicos/{idMedico}")
            {
                Content = JsonContent.Create(medico)
            };
            AddTokenHeader(request);
            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> AnularMedicoAsync(int idMedico)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/medicos/anular/{idMedico}");
            AddTokenHeader(request);
            return await _http.SendAsync(request);
        }
    }
}
