using Lab_Contracts.Convenios;

namespace Lab_Blazor.Services.Convenios
{
    public interface IConveniosApiService
    {
        Task<List<ConvenioDto>> ObtenerConveniosAsync();
        Task<ConvenioDetalleDto?> ObtenerDetalleAsync(int id);
        Task<List<OrdenDisponibleDto>> ObtenerOrdenesDisponiblesAsync(int idMedico);
        Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto dto);
        Task<bool> AnularConvenioAsync(int id);
    }
}
