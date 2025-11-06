using Lab_Contracts.Ordenes;
using Lab_Contracts.Common;

namespace Lab_APIRest.Services.Ordenes
{
    public interface IOrdenService
    {
        Task<List<object>> ListarOrdenesAsync();
        Task<ResultadoPaginadoDto<OrdenDto>> ListarOrdenesPaginadosAsync(OrdenFiltroDto filtro);
        Task<OrdenDetalleDto?> ObtenerDetalleOrdenAsync(int idOrden);
        Task<bool> AnularOrdenAsync(int idOrden);
        Task<OrdenRespuestaDto?> GuardarOrdenAsync(OrdenCompletaDto datosOrden);
        Task<byte[]?> GenerarOrdenTicketPdfAsync(int idOrden);
        Task<bool> AnularOrdenCompletaAsync(int idOrden);
        Task<List<object>> ListarOrdenesPorPacienteAsync(int idPaciente);
        Task<OrdenDetalleDto?> ObtenerDetalleOrdenPacienteAsync(int idOrden);
        Task VerificarYNotificarResultadosCompletosAsync(int idOrden);
    }
}
