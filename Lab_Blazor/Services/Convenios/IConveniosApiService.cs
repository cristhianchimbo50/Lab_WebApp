using Lab_Contracts.Convenios;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Convenios
{
    public interface IConveniosApiService
    {
        Task<List<ConvenioDto>> ObtenerConveniosAsync();
        Task<ResultadoPaginadoDto<ConvenioDto>> BuscarConveniosAsync(ConvenioFiltroDto filtro);
        Task<ResultadoPaginadoDto<ConvenioDto>> BuscarConveniosAsync(string? criterio, string? valor, DateOnly? desde, DateOnly? hasta, int page, int pageSize);
        Task<ConvenioDetalleDto?> ObtenerDetalleAsync(int IdConvenio);
        Task<List<OrdenDisponibleDto>> ObtenerOrdenesDisponiblesAsync(int IdMedico);
        Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto RegistroConvenio);
        Task<bool> AnularConvenioAsync(int IdConvenio);
    }
}
