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
        private readonly IPagoService PagoService;
        private readonly ILogger<PagosController> Logger;

        public PagosController(IPagoService pagoService, ILogger<PagosController> logger)
        {
            PagoService = pagoService;
            Logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<PagoDto>> RegistrarPago([FromBody] PagoDto PagoDto)
        {
            try
            {
                var PagoRegistrado = await PagoService.RegistrarPago(PagoDto);
                if (PagoRegistrado == null)
                    return BadRequest("No se pudo registrar el pago. Verifique los datos ingresados.");

                return Ok(PagoRegistrado);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al registrar el pago.");
                return StatusCode(500, "Ocurrió un error interno al registrar el pago.");
            }
        }

        [HttpGet("orden/{IdOrden:int}")]
        public async Task<ActionResult<List<PagoDto>>> ListarPagosPorOrden(int IdOrden)
        {
            var Pagos = await PagoService.ListarPagosPorOrden(IdOrden);
            return Ok(Pagos);
        }

        [HttpPost("cuentasporcobrar/listar")]
        public async Task<ActionResult<List<OrdenDto>>> ListarCuentasPorCobrar([FromBody] PagoFiltroDto Filtro)
        {
            try
            {
                var Ordenes = await PagoService.ListarCuentasPorCobrar(Filtro);
                return Ok(Ordenes);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al listar cuentas por cobrar.");
                return StatusCode(500, "Ocurrió un error interno al listar las cuentas por cobrar.");
            }
        }

        [HttpPost("cuentasporcobrar/registrar")]
        public async Task<ActionResult<PagoDto>> RegistrarCobroCuentaPorCobrar([FromBody] PagoDto PagoDto)
        {
            try
            {
                var PagoRegistrado = await PagoService.RegistrarCobroCuentaPorCobrar(PagoDto);
                if (PagoRegistrado == null)
                    return BadRequest("No se pudo registrar el cobro. Verifique la orden o los valores ingresados.");

                return Ok(PagoRegistrado);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error al registrar cobro de cuenta por cobrar.");
                return StatusCode(500, "Ocurrió un error interno al registrar el cobro de la cuenta por cobrar.");
            }
        }
    }
}
