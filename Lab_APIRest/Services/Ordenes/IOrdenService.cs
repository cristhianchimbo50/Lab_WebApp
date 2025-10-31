using Lab_Contracts.Ordenes;

namespace Lab_APIRest.Services.Ordenes
{
    public interface IOrdenService
    {
        Task<List<object>> GetOrdenesAsync();
        Task<OrdenDetalleDto?> ObtenerDetalleOrdenOriginalAsync(int id);
        Task<bool> AnularOrdenAsync(int id);
        Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto dto);
        Task<byte[]?> ObtenerTicketPdfAsync(int id);
        Task<bool> AnularOrdenCompletaAsync(int idOrden);

        Task<List<object>> GetOrdenesPorPacienteAsync(int idPaciente);
        Task<OrdenDetalleDto?> ObtenerDetalleOrdenPacienteAsync(int idOrden);

        Task VerificarYNotificarResultadosCompletosAsync(int idOrden);


    }
}
