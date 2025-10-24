using Lab_Contracts.Ajustes.Perfil;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Lab_Blazor.Services.Perfil
{
    public class PerfilApiService : IPerfilApiService
    {
        private readonly HttpClient _http;
        private readonly ProtectedSessionStorage _sessionStorage;

        public PerfilApiService(IHttpClientFactory factory, ProtectedSessionStorage sessionStorage)
        {
            _http = factory.CreateClient("Api");
            _sessionStorage = sessionStorage;
        }

        public async Task<PerfilResponseDto?> ObtenerPerfilAsync()
        {
            try
            {
                var tokenResult = await _sessionStorage.GetAsync<string>("jwt");
                var token = tokenResult.Success ? tokenResult.Value : null;

                if (string.IsNullOrWhiteSpace(token))
                {
                    Console.WriteLine("No se encontró token en sesión.");
                    return null;
                }

                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _http.GetAsync("api/ajustes/perfil");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<PerfilResponseDto>();
                    return result;
                }

                var msg = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => "Sesión expirada o no autorizada.",
                    System.Net.HttpStatusCode.NotFound => "No se encontró información del perfil.",
                    _ => $"Error {response.StatusCode}: {await response.Content.ReadAsStringAsync()}"
                };

                Console.WriteLine($"Error al obtener perfil: {msg}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al consumir perfil: {ex.Message}");
                return null;
            }
        }
    }
}
