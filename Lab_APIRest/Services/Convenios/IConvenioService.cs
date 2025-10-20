using Lab_Contracts.Convenios;

namespace Lab_APIRest.Services.Convenios
{
    public interface IConvenioService
    {
        Task<IEnumerable<ConvenioDto>> ObtenerConveniosAsync();
        Task<ConvenioDetalleDto?> ObtenerDetalleConvenioAsync(int id);
        Task<IEnumerable<OrdenDisponibleDto>> ObtenerOrdenesDisponiblesAsync(int idMedico);
        Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto dto);
        Task<bool> AnularConvenioAsync(int id);
    }
}
