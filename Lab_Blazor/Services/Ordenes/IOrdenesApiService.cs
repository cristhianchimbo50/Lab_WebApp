using Lab_Contracts.Examenes;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Resultados;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Ordenes
{
    public interface IOrdenesApiService
    {
        Task<List<OrdenDto>> ListarOrdenesAsync();
        Task<ResultadoPaginadoDto<OrdenDto>> ListarOrdenesPaginadosAsync(OrdenFiltroDto filtro);
        Task<List<ExamenDto>> ListarExamenesPorOrdenAsync(int idOrden);
        Task<List<OrdenDto>> ListarOrdenesPorPacienteAsync(int idPaciente);
        Task<OrdenDto?> ObtenerOrdenAsync(int idOrden);
        Task<OrdenDetalleDto?> ObtenerDetalleOrdenAsync(int idOrden);
        Task<(OrdenDetalleDto? Detalle, bool TieneSaldoPendiente)> ObtenerDetalleOrdenPorPacienteAsync(int idPaciente, int idOrden);
        Task<OrdenRespuestaDto?> GuardarOrdenAsync(OrdenCompletaDto orden);
        Task<HttpResponseMessage> GuardarOrdenHttpAsync(OrdenCompletaDto orden);
        Task<HttpResponseMessage> GuardarResultadosAsync(ResultadoGuardarDto dto);
        Task<HttpResponseMessage> VerificarNotificacionResultadosOrdenAsync(int idOrden);
        Task<byte[]> GenerarOrdenTicketPdfAsync(int idOrden);
        Task<HttpResponseMessage> AnularOrdenAsync(int idOrden);
        Task<HttpResponseMessage> AnularOrdenCompletaAsync(int idOrden);
    }

}