using Lab_Contracts.Examenes;
using Lab_APIRest.Services.Examenes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Lab_Contracts.Common;

namespace Lab_APIRest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "administrador,laboratorista")]
    public class ExamenesController : ControllerBase
    {
        private readonly IExamenService _examenService;

        public ExamenesController(IExamenService examenService)
        {
            _examenService = examenService;
        }

        [HttpPost("buscar")]
        public async Task<ActionResult<ResultadoPaginadoDto<ExamenDto>>> ListarExamenesPaginados([FromBody] ExamenFiltroDto filtro)
        {
            var result = await _examenService.ListarExamenesPaginadosAsync(filtro);
            return Ok(result);
        }

        // ...existing endpoints...
    }
}
