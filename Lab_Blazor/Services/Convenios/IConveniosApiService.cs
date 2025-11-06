using Lab_Contracts.Convenios;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Convenios
{
    public interface IConveniosApiService
    {
        Task<List<ConvenioDto>> ListarConveniosAsync();
        Task<ResultadoPaginadoDto<ConvenioDto>> ListarConveniosPaginadosAsync(ConvenioFiltroDto filtro);
        Task<ResultadoPaginadoDto<ConvenioDto>> ListarConveniosPaginadosAsync(string? criterio, string? valor, DateOnly? desde, DateOnly? hasta, int page, int pageSize);
        Task<ConvenioDetalleDto?> ObtenerDetalleConvenioAsync(int idConvenio);
        Task<List<OrdenDisponibleDto>> ListarOrdenesDisponiblesPorMedicoAsync(int idMedico);
        Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto registroConvenio);
        Task<bool> AnularConvenioAsync(int idConvenio);
    }
}
