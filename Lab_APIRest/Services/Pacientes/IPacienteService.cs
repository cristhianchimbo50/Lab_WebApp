using Lab_Contracts.Pacientes;

namespace Lab_APIRest.Services.Pacientes
{
    public interface IPacienteService
    {
        Task<List<PacienteDto>> ObtenerPacientesAsync();
        Task<PacienteDto?> ObtenerPacientePorIdAsync(int IdPaciente);
        Task<List<PacienteDto>?> BuscarPacientesAsync(string Campo, string Valor);
        Task<(bool Exito, string Mensaje, PacienteDto? Paciente)> RegistrarPacienteAsync(PacienteDto dto);
        Task<bool> EditarPacienteAsync(int IdPaciente, PacienteDto dto);
        Task<bool> AnularPacienteAsync(int IdPaciente);
        Task<(bool Exito, string Mensaje, string? NuevaTemporal)> ReenviarCredencialesTemporalesAsync(int IdPaciente);
    }
}
