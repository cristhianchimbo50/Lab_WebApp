using Lab_Contracts.Medicos;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Medicos
{
    public interface IMedicosApiService
    {
        Task<List<MedicoDto>> ObtenerMedicosAsync();
        Task<MedicoDto?> ObtenerMedicoPorIdAsync(int Id);
        Task<List<MedicoDto>> BuscarMedicosAsync(string Campo, string Valor);
        Task<ResultadoPaginadoDto<MedicoDto>> BuscarMedicosAsync(MedicoFiltroDto filtro);
        Task<HttpResponseMessage> CrearMedicoAsync(MedicoDto Dto);
        Task<HttpResponseMessage> EditarMedicoAsync(int Id, MedicoDto Medico);
        Task<HttpResponseMessage> AnularMedicoAsync(int Id);
        Task<List<MedicoDto>> ListarMedicosAsync();
    }
}
