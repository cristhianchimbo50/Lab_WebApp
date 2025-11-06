using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_APIRest.Services.Pagos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,recepcionista")]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _pagoService;
        private readonly ILogger<PagosController> _logger;

        public PagosController(IPagoService pagoService, ILogger<PagosController> logger)
        {
            _pagoService = pagoService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<PagoDto>> GuardarPago([FromBody] PagoDto pagoDto)
        {
            try
            {
                var pagoRegistrado = await _pagoService.GuardarPagoAsync(pagoDto);
                if (pagoRegistrado == null)
                    return BadRequest("No se pudo registrar el pago. Verifique los datos ingresados.");

                return Ok(pagoRegistrado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el pago.");
                return StatusCode(500, "Ocurrió un error interno al registrar el pago.");
            }
        }

        [HttpGet("orden/{idOrden:int}")]
        public async Task<ActionResult<List<PagoDto>>> ListarPagosPorOrden(int idOrden)
        {
            var pagos = await _pagoService.ListarPagosPorOrdenAsync(idOrden);
            return Ok(pagos);
        }

        [HttpPost("cuentasporcobrar/listar")]
        public async Task<ActionResult<List<OrdenDto>>> ListarCuentasPorCobrar([FromBody] Lab_Contracts.Pagos.PagoFiltroDto filtro)
        {
            try
            {
                var ordenes = await _pagoService.ListarCuentasPorCobrarAsync(filtro);
                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar cuentas por cobrar.");
                return StatusCode(500, "Ocurrió un error interno al listar las cuentas por cobrar.");
            }
        }

        [HttpPost("cuentasporcobrar/listar-paginado")]
        public async Task<ActionResult<Lab_Contracts.Common.ResultadoPaginadoDto<OrdenDto>>> ListarCuentasPorCobrarPaginados([FromBody] Lab_Contracts.Pagos.PagoFiltroDto filtro)
        {
            try
            {
                var resultado = await _pagoService.ListarCuentasPorCobrarPaginadosAsync(filtro);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar cuentas por cobrar paginado.");
                return StatusCode(500, "Ocurrió un error interno al listar las cuentas por cobrar.");
            }
        }

        [HttpPost("cuentasporcobrar/registrar")]
        public async Task<ActionResult<PagoDto>> ProcesarCobroCuentaPorCobrar([FromBody] PagoDto pagoDto)
        {
            try
            {
                var pagoRegistrado = await _pagoService.ProcesarCobroCuentaPorCobrarAsync(pagoDto);
                if (pagoRegistrado == null)
                    return BadRequest("No se pudo registrar el cobro. Verifique la orden o los valores ingresados.");

                return Ok(pagoRegistrado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar cobro de cuenta por cobrar.");
                return StatusCode(500, "Ocurrió un error interno al registrar el cobro de la cuenta por cobrar.");
            }
        }
    }
}
