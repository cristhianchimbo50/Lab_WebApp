using Lab_Contracts.Medicos;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Medicos
{
    public interface IMedicosApiService
    {
        Task<List<MedicoDto>> ListarMedicosAsync();
        Task<List<MedicoDto>> ListarMedicosAsync(string campo, string valor);
        Task<MedicoDto?> ObtenerDetalleMedicoAsync(int idMedico);
        Task<ResultadoPaginadoDto<MedicoDto>> ListarMedicosPaginadosAsync(MedicoFiltroDto filtro);
        Task<HttpResponseMessage> GuardarMedicoAsync(MedicoDto medico);
        Task<HttpResponseMessage> GuardarMedicoAsync(int idMedico, MedicoDto medico);
        Task<HttpResponseMessage> AnularMedicoAsync(int idMedico);
    }
}
