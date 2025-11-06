using Lab_Contracts.Pacientes;
using System.Net.Http;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Pacientes
{
    public interface IPacientesApiService
    {
        Task<List<PacienteDto>> ObtenerPacientesAsync();
        Task<PacienteDto?> ObtenerPacientePorIdAsync(int Id);
        Task<List<PacienteDto>> BuscarPacientesAsync(string Campo, string Valor);
        Task<ResultadoPaginadoDto<PacienteDto>> BuscarPacientesAsync(PacienteFiltroDto filtro);

        Task<(bool Exito, string Mensaje, PacienteDto? Paciente)> CrearPacienteAsync(PacienteDto Paciente);

        Task<HttpResponseMessage> EditarPacienteAsync(int Id, PacienteDto Paciente);
        Task<HttpResponseMessage> AnularPacienteAsync(int Id);
        Task<PacienteDto?> ObtenerPacientePorCedulaAsync(string Cedula);

        Task<(bool Exito, string Mensaje)> ReenviarTemporalAsync(int IdPaciente);

    }
}
