using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_APIRest.Services.Pagos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers
{
    /// <summary>
    /// Controlador responsable de gestionar los pagos y cuentas por cobrar de las órdenes de laboratorio.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,recepcionista")]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _service;
        private readonly ILogger<PagosController> _logger;

        public PagosController(IPagoService service, ILogger<PagosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<PagoDto>> RegistrarPago([FromBody] PagoDto dto)
        {
            try
            {
                var pago = await _service.RegistrarPagoAsync(dto);
                if (pago == null)
                    return BadRequest("No se pudo registrar el pago. Verifique los datos ingresados.");

                return Ok(pago);
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
            var pagos = await _service.ListarPagosPorOrdenAsync(idOrden);
            return Ok(pagos);
        }

        [HttpPost("cuentasporcobrar/listar")]
        public async Task<ActionResult<List<OrdenDto>>> ListarCuentasPorCobrar([FromBody] PagoFiltroDto filtro)
        {
            try
            {
                var ordenes = await _service.ListarCuentasPorCobrarAsync(filtro);
                return Ok(ordenes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar cuentas por cobrar.");
                return StatusCode(500, "Ocurrió un error interno al listar las cuentas por cobrar.");
            }
        }

        [HttpPost("cuentasporcobrar/registrar")]
        public async Task<ActionResult<PagoDto>> RegistrarCobroCuentaPorCobrar([FromBody] PagoDto dto)
        {
            try
            {
                var pago = await _service.RegistrarCobroCuentaPorCobrarAsync(dto);
                if (pago == null)
                    return BadRequest("No se pudo registrar el cobro. Verifique la orden o los valores ingresados.");

                return Ok(pago);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar cobro de cuenta por cobrar.");
                return StatusCode(500, "Ocurrió un error interno al registrar el cobro de la cuenta por cobrar.");
            }
        }
    }
}
