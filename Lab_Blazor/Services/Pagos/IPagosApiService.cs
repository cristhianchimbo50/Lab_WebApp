using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_Contracts.Common;

namespace Lab_Blazor.Services.Pagos
{
    public interface IPagosApiService
    {
        Task<PagoDto?> RegistrarPagoAsync(PagoDto Dto);
        Task<List<PagoDto>> ListarPagosPorOrdenAsync(int IdOrden);
        Task<IEnumerable<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto Filtro);
        Task<ResultadoPaginadoDto<OrdenDto>> ListarCuentasPorCobrarAsync(PagoFiltroDto filtro, int pagina, int tamano);
        Task<PagoDto?> RegistrarCobroCuentaPorCobrarAsync(PagoDto Pago);
    }
}
