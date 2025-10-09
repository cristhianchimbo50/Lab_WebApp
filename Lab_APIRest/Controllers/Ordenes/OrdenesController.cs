using Lab_APIRest.Services.Ordenes;
using Lab_Contracts.Ordenes;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class OrdenesController : ControllerBase
{
    private readonly IOrdenService _ordenService;

    public OrdenesController(IOrdenService ordenService)
    {
        _ordenService = ordenService;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrdenDto>>> GetOrdenes()
    {
        var ordenes = await _ordenService.ListarOrdenesAsync();
        return Ok(ordenes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrdenDto>> GetOrden(int id)
    {
        var orden = await _ordenService.ObtenerOrdenPorIdAsync(id);
        if (orden == null) return NotFound();
        return Ok(orden);
    }

    [HttpPost]
    public async Task<ActionResult<OrdenRespuestaDto>> CrearOrden([FromBody] OrdenCompletaDto dto)
    {
        var result = await _ordenService.CrearOrdenAsync(dto);
        if (result == null) return BadRequest();
        return Ok(result);
    }
}
