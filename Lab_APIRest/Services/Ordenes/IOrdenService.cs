using Lab_Contracts.Ordenes;

namespace Lab_APIRest.Services.Ordenes
{
    public interface IOrdenService
    {
        Task<List<object>> GetOrdenesAsync();
        Task<object?> ObtenerDetalleOrdenOriginalAsync(int id);
        Task<bool> AnularOrdenAsync(int id);
        Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto dto);
        Task<byte[]?> ObtenerTicketPdfAsync(int id);
        Task<bool> AnularOrdenCompletaAsync(int idOrden);
    }
}
