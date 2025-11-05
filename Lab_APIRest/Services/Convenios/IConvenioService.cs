using Lab_Contracts.Convenios;

namespace Lab_APIRest.Services.Convenios
{
    public interface IConvenioService
    {
        Task<IEnumerable<ConvenioDto>> ObtenerConveniosAsync();
        Task<ConvenioDetalleDto?> ObtenerDetalleConvenioAsync(int IdConvenio);
        Task<IEnumerable<OrdenDisponibleDto>> ObtenerOrdenesDisponiblesAsync(int IdMedico);
        Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto ConvenioRegistro);
        Task<bool> AnularConvenioAsync(int IdConvenio);
    }
}
