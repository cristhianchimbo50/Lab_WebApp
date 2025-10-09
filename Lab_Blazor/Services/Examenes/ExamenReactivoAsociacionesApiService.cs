using Lab_Contracts.Examenes;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Examenes
{
    public class ExamenReactivoAsociacionesApiService : IExamenReactivoAsociacionesApiService
    {
        private readonly HttpClient _http;
        public ExamenReactivoAsociacionesApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<List<AsociacionReactivoDto>> ObtenerTodasAsync()
        {
            return await _http.GetFromJsonAsync<List<AsociacionReactivoDto>>("api/ExamenReactivoAsociaciones")
                ?? new List<AsociacionReactivoDto>();
        }

        public async Task<List<AsociacionReactivoDto>> BuscarPorExamenAsync(string nombre)
        {
            return await _http.GetFromJsonAsync<List<AsociacionReactivoDto>>($"api/ExamenReactivoAsociaciones/buscar-examen/{nombre}")
                ?? new List<AsociacionReactivoDto>();
        }

        public async Task<List<AsociacionReactivoDto>> BuscarPorReactivoAsync(string nombre)
        {
            return await _http.GetFromJsonAsync<List<AsociacionReactivoDto>>($"api/ExamenReactivoAsociaciones/buscar-reactivo/{nombre}")
                ?? new List<AsociacionReactivoDto>();
        }

        public async Task<AsociacionReactivoDto?> ObtenerPorIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<AsociacionReactivoDto>($"api/ExamenReactivoAsociaciones/{id}");
        }

        public async Task<AsociacionReactivoDto?> CrearAsync(AsociacionReactivoDto dto)
        {
            var resp = await _http.PostAsJsonAsync("api/ExamenReactivoAsociaciones", dto);
            if (!resp.IsSuccessStatusCode) return null;
            return await resp.Content.ReadFromJsonAsync<AsociacionReactivoDto>();
        }

        public async Task<bool> EditarAsync(int id, AsociacionReactivoDto dto)
        {
            var resp = await _http.PutAsJsonAsync($"api/ExamenReactivoAsociaciones/{id}", dto);
            return resp.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var resp = await _http.DeleteAsync($"api/ExamenReactivoAsociaciones/{id}");
            return resp.IsSuccessStatusCode;
        }

        public async Task<List<AsociacionReactivoDto>> ObtenerPorExamenIdAsync(int idExamen)
        {
            return await _http.GetFromJsonAsync<List<AsociacionReactivoDto>>(
                $"api/ExamenReactivoAsociaciones/asociados/{idExamen}"
            ) ?? new();
        }

        public async Task<bool> GuardarPorExamenAsync(int idExamen, List<AsociacionReactivoDto> asociaciones)
        {
            var resp = await _http.PostAsJsonAsync(
                $"api/ExamenReactivoAsociaciones/asociados/{idExamen}", asociaciones
            );
            return resp.IsSuccessStatusCode;
        }

    }
}
