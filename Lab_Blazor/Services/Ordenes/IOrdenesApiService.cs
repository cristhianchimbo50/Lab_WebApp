using Lab_Contracts.Examenes;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Resultados;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Ordenes
{
    public interface IOrdenesApiService
    {
        Task<List<OrdenDto>> ObtenerOrdenesAsync();
        Task<OrdenDto?> ObtenerOrdenPorIdAsync(int Id);
        Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto Orden);
        Task<HttpResponseMessage> CrearOrdenHttpResponseAsync(OrdenCompletaDto Orden);

        Task<OrdenDetalleDto?> ObtenerDetalleOrdenAsync(int IdOrden);
        Task<HttpResponseMessage> AnularOrdenAsync(int IdOrden);
        Task<byte[]> ObtenerTicketOrdenPdfAsync(int IdOrden);

        Task<List<ExamenDto>> ObtenerExamenesPorOrdenAsync(int IdOrden);
        Task<HttpResponseMessage> GuardarResultadosAsync(ResultadoGuardarDto Dto);

        Task<HttpResponseMessage> AnularOrdenCompletaAsync(int IdOrden);

        Task<List<OrdenDto>> ObtenerOrdenesPacienteAsync(int IdPaciente);

        Task<(OrdenDetalleDto? Detalle, bool TieneSaldoPendiente)> ObtenerDetalleOrdenPacienteAsync(int IdPaciente, int IdOrden);

        Task<HttpResponseMessage> VerificarNotificacionResultadosAsync(int IdOrden);

        Task<ResultadoPaginadoDto<OrdenDto>> BuscarOrdenesAsync(OrdenFiltroDto Filtro);

    }

}