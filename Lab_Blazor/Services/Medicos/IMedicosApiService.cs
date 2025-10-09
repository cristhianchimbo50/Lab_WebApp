using Lab_Contracts.Medicos;

namespace Lab_Blazor.Services.Medicos
{
    public interface IMedicosApiService
    {
        Task<List<MedicoDto>> GetMedicosAsync();
        Task<MedicoDto?> GetMedicoPorIdAsync(int id);
        Task<List<MedicoDto>> BuscarMedicosAsync(string campo, string valor);
        Task<HttpResponseMessage> CrearMedicoAsync(MedicoDto medico);
        Task<HttpResponseMessage> EditarMedicoAsync(int id, MedicoDto medico);
        Task<HttpResponseMessage> AnularMedicoAsync(int id);
    }
}
