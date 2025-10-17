using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Services;
using Lab_APIRest.Services.Resultados;
using Lab_Contracts.Ordenes;
using Lab_Contracts.Resultados;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdenesController : ControllerBase
{
    private readonly LabDbContext _context;
    private readonly PdfTicketService _pdfTicketService;
    private readonly IResultadoService _resultadoService;

    public OrdenesController(
        LabDbContext context,
        PdfTicketService pdfTicketService,
        IResultadoService resultadoService)
    {
        _context = context;
        _pdfTicketService = pdfTicketService;
        _resultadoService = resultadoService;
    }

    // GET: api/ordenes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetOrdenes()
    {
        var data = await _context.ordens
            .Include(o => o.id_pacienteNavigation)
            .Select(o => new
            {
                IdOrden = o.id_orden,
                NumeroOrden = o.numero_orden,
                CedulaPaciente = o.id_pacienteNavigation!.cedula_paciente,
                NombrePaciente = o.id_pacienteNavigation!.nombre_paciente,
                FechaOrden = o.fecha_orden,
                Total = o.total,
                TotalPagado = o.total_pagado ?? 0,
                SaldoPendiente = o.saldo_pendiente ?? 0,
                EstadoPago = o.estado_pago,
                Anulado = o.anulado ?? false
            })
            .OrderByDescending(x => x.IdOrden)
            .ToListAsync();

        return Ok(data);
    }

    // GET: api/ordenes/detalle/5
    [HttpGet("detalle/{id}")]
    public async Task<ActionResult<object>> ObtenerDetalleOrden(int id)
    {
        var orden = await _context.ordens
            .Include(o => o.id_pacienteNavigation)
            .Include(o => o.id_medicoNavigation)
            .Include(o => o.detalle_ordens)
                .ThenInclude(d => d.id_examenNavigation)
            .Include(o => o.detalle_ordens)
                .ThenInclude(d => d.id_resultadoNavigation)
            .FirstOrDefaultAsync(o => o.id_orden == id);

        if (orden == null)
            return NotFound();

        var dto = new
        {
            IdOrden = orden.id_orden,
            IdPaciente = orden.id_paciente,
            NumeroOrden = orden.numero_orden,
            FechaOrden = orden.fecha_orden,
            EstadoPago = orden.estado_pago,
            CedulaPaciente = orden.id_pacienteNavigation?.cedula_paciente,
            NombrePaciente = orden.id_pacienteNavigation?.nombre_paciente,
            DireccionPaciente = orden.id_pacienteNavigation?.direccion_paciente,
            CorreoPaciente = orden.id_pacienteNavigation?.correo_electronico_paciente,
            TelefonoPaciente = orden.id_pacienteNavigation?.telefono_paciente,
            Anulado = orden.anulado ?? false,
            TotalOrden = orden.total,
            TotalPagado = orden.total_pagado ?? 0,
            SaldoPendiente = orden.saldo_pendiente ?? 0,
            NombreMedico = orden.id_medicoNavigation?.nombre_medico,
            IdMedico = orden.id_medico,
            Examenes = orden.detalle_ordens.Select(d => new
            {
                IdExamen = d.id_examen ?? 0,
                NombreExamen = d.id_examenNavigation!.nombre_examen,
                NombreEstudio = d.id_examenNavigation!.estudio,
                IdResultado = d.id_resultado,
                NumeroResultado = d.id_resultadoNavigation != null ? d.id_resultadoNavigation.numero_resultado : null
            }).ToList()
        };

        return Ok(dto);
    }

    // PUT: api/ordenes/anular/5
    [HttpPut("anular/{id}")]
    public async Task<IActionResult> AnularOrden(int id)
    {
        var orden = await _context.ordens
            .Include(o => o.detalle_ordens)
                .ThenInclude(d => d.id_resultadoNavigation)
                    .ThenInclude(r => r.detalle_resultados)
            .FirstOrDefaultAsync(o => o.id_orden == id);

        if (orden == null)
            return NotFound();

        orden.anulado = true;

        var resultados = orden.detalle_ordens
            .Where(d => d.id_resultadoNavigation != null)
            .Select(d => d.id_resultadoNavigation!)
            .Distinct()
            .ToList();

        foreach (var r in resultados)
        {
            r.anulado = true;
            foreach (var det in r.detalle_resultados)
                det.anulado = true;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/ticket-pdf")]
    public async Task<IActionResult> ObtenerTicketPdf(int id)
    {
        var orden = await _context.ordens
            .Include(o => o.id_pacienteNavigation)
            .Include(o => o.id_medicoNavigation)
            .Include(o => o.detalle_ordens!)
                .ThenInclude(d => d.id_examenNavigation)
            .FirstOrDefaultAsync(o => o.id_orden == id);

        if (orden == null)
            return NotFound("Orden no encontrada.");

        int edadPaciente = 0;

        if (orden.id_pacienteNavigation?.fecha_nac_paciente is DateOnly fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var fechaNac = fechaNacimiento.ToDateTime(TimeOnly.MinValue);

            edadPaciente = hoy.Year - fechaNac.Year;
            if (fechaNac > hoy.AddYears(-edadPaciente))
                edadPaciente--;
        }

        var ordenDto = new OrdenTicketDto
        {
            NumeroOrden = orden.numero_orden,
            FechaOrden = orden.fecha_orden.ToDateTime(TimeOnly.MinValue),
            NombrePaciente = orden.id_pacienteNavigation?.nombre_paciente ?? "(Sin nombre)",
            CedulaPaciente = orden.id_pacienteNavigation?.cedula_paciente ?? "(Sin cédula)",
            EdadPaciente = edadPaciente,
            NombreMedico = orden.id_medicoNavigation?.nombre_medico ?? "(Sin médico)",
            Total = orden.total,
            TotalPagado = orden.total_pagado ?? 0,
            SaldoPendiente = orden.saldo_pendiente ?? 0,
            TipoPago = orden.estado_pago ?? "Desconocido",
            Examenes = orden.detalle_ordens.Select(d => new ExamenTicketDto
            {
                NombreExamen = d.id_examenNavigation?.nombre_examen ?? "(Sin examen)",
                Precio = d.precio ?? 0
            }).ToList()
        };
        var pdfBytes = _pdfTicketService.GenerarTicketOrden(ordenDto);
        return File(pdfBytes, "application/pdf", $"orden_{orden.numero_orden}_ticket.pdf");
    }

    // POST: api/ordenes/ingresar-resultado
    [HttpPost("ingresar-resultado")]
    public async Task<IActionResult> IngresarResultado([FromBody] ResultadoGuardarDto dto)
    {
        var ok = await _resultadoService.GuardarResultadosAsync(dto);
        if (ok)
            return Ok(new { mensaje = "Resultados guardados correctamente." });
        else
            return BadRequest(new { mensaje = "No se pudo guardar los resultados." });
    }


}
