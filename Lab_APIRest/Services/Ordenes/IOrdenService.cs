using Lab_Contracts.Ordenes;

namespace Lab_APIRest.Services.Ordenes
{
    public interface IOrdenService
    {
        Task<List<OrdenDto>> ListarOrdenesAsync(); // Lista todas las órdenes
        Task<OrdenDto?> ObtenerOrdenPorIdAsync(int idOrden); // Detalle por ID
        Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto dto); // Crea nueva orden
        Task<bool> AnularOrdenCompletaAsync(int idOrden); // Anula completa
        Task<byte[]?> GenerarTicketOrdenAsync(int idOrden); // Genera PDF
    }
}
