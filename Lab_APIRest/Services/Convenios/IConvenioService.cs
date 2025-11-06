using Lab_Contracts.Convenios;
using Lab_Contracts.Common;
using System.Threading.Tasks;

namespace Lab_APIRest.Services.Convenios
{
    public interface IConvenioService
    {
        Task<IEnumerable<ConvenioDto>> ListarConveniosAsync();
        Task<ResultadoPaginadoDto<ConvenioDto>> ListarConveniosPaginadosAsync(ConvenioFiltroDto filtro);
        Task<ResultadoPaginadoDto<ConvenioDto>> ListarConveniosPaginadosAsync(string? criterio, string? valor, DateOnly? desde, DateOnly? hasta, int page, int pageSize);
        Task<ConvenioDetalleDto?> ObtenerDetalleConvenioAsync(int idConvenio);
        Task<IEnumerable<OrdenDisponibleDto>> ListarOrdenesDisponiblesAsync(int idMedico);
        Task<bool> GuardarConvenioAsync(ConvenioRegistroDto convenioRegistro);
        Task<bool> AnularConvenioAsync(int idConvenio);
    }
}
