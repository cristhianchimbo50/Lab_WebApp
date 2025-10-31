using Lab_Contracts.Examenes;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Resultados;

namespace Lab_Blazor.Services.Ordenes
{
    public interface IOrdenesApiService
    {
        Task<List<OrdenDto>> GetOrdenesAsync();
        Task<OrdenDto?> GetOrdenPorIdAsync(int id);
        Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto orden);
        Task<HttpResponseMessage> CrearOrdenHttpResponseAsync(OrdenCompletaDto orden);

        Task<OrdenDetalleDto?> ObtenerDetalleOrdenAsync(int idOrden);
        Task<HttpResponseMessage> AnularOrdenAsync(int idOrden);
        Task<byte[]> ObtenerTicketOrdenPdfAsync(int idOrden);

        Task<List<ExamenDto>> ObtenerExamenesPorOrdenAsync(int idOrden);
        Task<HttpResponseMessage> GuardarResultadosAsync(ResultadoGuardarDto dto);

        Task<HttpResponseMessage> AnularOrdenCompletaAsync(int idOrden);

        Task<List<OrdenDto>> GetOrdenesPacienteAsync(int idPaciente);

        Task<(OrdenDetalleDto? Detalle, bool TieneSaldoPendiente)> ObtenerDetalleOrdenPacienteAsync(int idPaciente, int idOrden);

        Task<HttpResponseMessage> VerificarNotificacionResultadosAsync(int idOrden);

    }

}