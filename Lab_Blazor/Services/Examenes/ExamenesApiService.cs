using Lab_Contracts.Examenes;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Examenes
{
    public class ExamenesApiService : IExamenesApiService
    {
        private readonly HttpClient _http;

        public ExamenesApiService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("Api");
        }

        public async Task<List<ExamenDto>> GetExamenesAsync()
        {
            return await _http.GetFromJsonAsync<List<ExamenDto>>("api/examenes") ?? new();
        }

        public async Task<ExamenDto?> GetExamenPorIdAsync(int id)
        {
            return await _http.GetFromJsonAsync<ExamenDto>($"api/examenes/{id}");
        }

        public async Task<List<ExamenDto>> BuscarExamenesPorNombreAsync(string nombre)
        {
            return await _http.GetFromJsonAsync<List<ExamenDto>>($"api/examenes/buscar?nombre={nombre}") ?? new();
        }

        public async Task<HttpResponseMessage> CrearExamenAsync(ExamenDto examen)
        {
            return await _http.PostAsJsonAsync("api/examenes", examen);
        }

        public async Task<HttpResponseMessage> EditarExamenAsync(int id, ExamenDto examen)
        {
            return await _http.PutAsJsonAsync($"api/examenes/{id}", examen);
        }

        public async Task<HttpResponseMessage> AnularExamenAsync(int id)
        {
            return await _http.PutAsync($"api/examenes/anular/{id}", null);
        }

        //Para composicion
        public async Task<List<ExamenDto>> ObtenerHijosDeExamenAsync(int idExamenPadre)
        {
            return await _http.GetFromJsonAsync<List<ExamenDto>>($"api/examenes/{idExamenPadre}/hijos") ?? new();
        }

        public async Task<HttpResponseMessage> AgregarExamenHijoAsync(int idPadre, int idHijo)
        {
            return await _http.PostAsync($"api/examenes/{idPadre}/hijos/{idHijo}", null);
        }

        public async Task<HttpResponseMessage> EliminarExamenHijoAsync(int idPadre, int idHijo)
        {
            return await _http.DeleteAsync($"api/examenes/{idPadre}/hijos/{idHijo}");
        }

        //Para examen con reactivos

        public async Task<List<AsociacionReactivoDto>> GetReactivosAsociadosPorExamenAsync(int idExamen)
        {
            return await _http.GetFromJsonAsync<List<AsociacionReactivoDto>>($"api/ExamenReactivos/por-examen/{idExamen}") ?? new();
        }

        public async Task<HttpResponseMessage> GuardarReactivosPorExamenAsync(int idExamen, List<AsociacionReactivoDto> reactivos)
        {
            return await _http.PostAsJsonAsync($"api/ExamenReactivos/guardar-masivo/{idExamen}", reactivos);
        }

        //Para orden

        public async Task<List<ExamenDto>> ListarExamenesAsync(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return await _http.GetFromJsonAsync<List<ExamenDto>>("api/examenes");

            return await _http.GetFromJsonAsync<List<ExamenDto>>($"api/examenes/buscar?nombre={filtro}");
        }
    }
}
