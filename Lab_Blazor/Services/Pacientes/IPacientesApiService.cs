using Lab_Contracts.Pacientes;
using System.Net.Http;

namespace Lab_Blazor.Services.Pacientes
{
    public interface IPacientesApiService
    {
        Task<List<PacienteDto>> GetPacientesAsync();
        Task<PacienteDto?> GetPacientePorIdAsync(int id);
        Task<List<PacienteDto>> BuscarPacientesAsync(string campo, string valor);
        Task<HttpResponseMessage> CrearPacienteAsync(PacienteDto paciente);
        Task<HttpResponseMessage> EditarPacienteAsync(int id, PacienteDto paciente);
        Task<HttpResponseMessage> AnularPacienteAsync(int id);
        Task<PacienteDto?> ObtenerPacientePorCedulaAsync(string cedula);
    }
}
