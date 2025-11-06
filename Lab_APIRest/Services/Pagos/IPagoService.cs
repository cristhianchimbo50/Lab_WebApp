using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_Contracts.Common;

namespace Lab_APIRest.Services.Pagos
{
    public interface IPagoService
    {
        Task<PagoDto?> GuardarPagoAsync(PagoDto pagoDto);
        Task<List<PagoDto>> ListarPagosPorOrdenAsync(int idOrden);
        Task<PagoDto?> ProcesarCobroCuentaPorCobrarAsync(PagoDto pagoDto);
        Task<List<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro);
        Task<ResultadoPaginadoDto<OrdenDto>> ListarCuentasPorCobrarPaginadosAsync(PagoFiltroDto filtro);
    }
}
