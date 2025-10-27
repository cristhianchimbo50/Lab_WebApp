using Lab_Contracts.Pacientes;

namespace Lab_APIRest.Services.Pacientes
{
    public interface IPacienteService
    {
        Task<List<PacienteDto>> GetPacientesAsync();
        Task<PacienteDto?> GetPacienteByIdAsync(int id);
        Task<List<PacienteDto>?> BuscarPacientesAsync(string campo, string valor);
        Task<(bool Exito, string Mensaje, PacienteDto? Paciente)> RegistrarPacienteAsync(PacienteDto dto);
        Task<bool> EditarPacienteAsync(int id, PacienteDto dto);
        Task<bool> AnularPacienteAsync(int id);
        Task<(bool Exito, string Mensaje, string? NuevaTemporal)> ReenviarCredencialesTemporalesAsync(int idPaciente);
    }
}
