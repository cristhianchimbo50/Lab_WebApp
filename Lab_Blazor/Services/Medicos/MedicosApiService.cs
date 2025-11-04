using Lab_Contracts.Medicos;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Medicos
{
    public class MedicosApiService : BaseApiService, IMedicosApiService
    {
        public MedicosApiService(IHttpClientFactory factory, ProtectedSessionStorage session, IJSRuntime js)
            : base(factory, session, js) { }

        public async Task<List<MedicoDto>> GetMedicosAsync()
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, "api/medicos");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<MedicoDto>>() ?? new();
        }

        public async Task<MedicoDto?> GetMedicoPorIdAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/medicos/{id}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<MedicoDto>();
        }

        public async Task<List<MedicoDto>> BuscarMedicosAsync(string campo, string valor)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Get, $"api/medicos/buscar?campo={campo}&valor={valor}");
            AddTokenHeader(request);

            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<MedicoDto>>() ?? new();
        }

        public async Task<HttpResponseMessage> CrearMedicoAsync(MedicoDto dto)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/medicos")
            {
                Content = JsonContent.Create(dto)
            };
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> EditarMedicoAsync(int id, MedicoDto medico)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/medicos/{id}")
            {
                Content = JsonContent.Create(medico)
            };
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

        public async Task<HttpResponseMessage> AnularMedicoAsync(int id)
        {
            if (!await SetAuthHeaderAsync())
                throw new HttpRequestException("Token no disponible o sesión expirada.");

            var request = new HttpRequestMessage(HttpMethod.Put, $"api/medicos/anular/{id}");
            AddTokenHeader(request);

            return await _http.SendAsync(request);
        }

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
    }
}
