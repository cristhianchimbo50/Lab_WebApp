using Lab_Contracts.Ordenes;
using Lab_Contracts.Pagos;
using Lab_APIRest.Services.Pagos;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers
{
    /// <summary>
    /// Controlador responsable de gestionar los pagos y cuentas por cobrar de las órdenes de laboratorio.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly IPagoService _service;
        private readonly ILogger<PagosController> _logger;

        /// <summary>
        /// Constructor del controlador de pagos.
        /// </summary>
        /// <param name="service">Servicio de pagos inyectado.</param>
        /// <param name="logger">Logger para registrar errores y eventos.</param>
        public PagosController(IPagoService service, ILogger<PagosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Registra un nuevo pago para una orden.
        /// </summary>
        /// <param name="dto">Datos del pago.</param>
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

        /// <summary>
        /// Obtiene la lista de pagos registrados para una orden específica.
        /// </summary>
        /// <param name="idOrden">Identificador de la orden.</param>
        [HttpGet("orden/{idOrden:int}")]
        public async Task<ActionResult<List<PagoDto>>> ListarPagosPorOrden(int idOrden)
        {
            var pagos = await _service.ListarPagosPorOrdenAsync(idOrden);
            return Ok(pagos);
        }

        /// <summary>
        /// Lista las cuentas por cobrar aplicando filtros de búsqueda (fecha, paciente, estado).
        /// </summary>
        /// <param name="filtro">Filtro de búsqueda.</param>
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

        /// <summary>
        /// Registra el cobro de una cuenta por cobrar.
        /// </summary>
        /// <param name="dto">Datos del pago aplicado a la cuenta por cobrar.</param>
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
