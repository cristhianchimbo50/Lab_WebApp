using Lab_Contracts.Ordenes;

namespace Lab_APIRest.Services.Ordenes
{
    public interface IOrdenService
    {
        Task<List<object>> ObtenerOrdenesAsync();
        Task<OrdenDetalleDto?> ObtenerDetalleOrdenOriginalAsync(int IdOrden);
        Task<bool> AnularOrdenAsync(int IdOrden);
        Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto DatosOrden);
        Task<byte[]?> ObtenerTicketPdfAsync(int IdOrden);
        Task<bool> AnularOrdenCompletaAsync(int IdOrden);

        Task<List<object>> ObtenerOrdenesPorPacienteAsync(int IdPaciente);
        Task<OrdenDetalleDto?> ObtenerDetalleOrdenPacienteAsync(int IdOrden);

        Task VerificarYNotificarResultadosCompletosAsync(int IdOrden);


    }
}
