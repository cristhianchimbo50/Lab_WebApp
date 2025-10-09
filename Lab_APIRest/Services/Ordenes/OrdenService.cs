using Lab_APIRest.Infrastructure.EF.Models;
using Lab_Contracts.Ordenes;
using System;
using Lab_APIRest.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Ordenes
{
public class OrdenService : IOrdenService
{
    private readonly LabDbContext _context;

    public OrdenService(LabDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrdenDto>> ListarOrdenesAsync()
    {
        var ordenes = await _context.ordens
            .Include(o => o.detalle_ordens)
            .OrderByDescending(o => o.id_orden)
            .Select(o => new OrdenDto
            {
                IdOrden = o.id_orden,
                NumeroOrden = o.numero_orden,
                IdPaciente = (int)o.id_paciente,
                FechaOrden = o.fecha_orden,
                Total = o.total,
                SaldoPendiente = (decimal)o.saldo_pendiente,
                TotalPagado = (decimal)o.total_pagado,
                EstadoPago = o.estado_pago,
                Anulado = (bool)o.anulado,
                LiquidadoConvenio = (bool)o.liquidado_convenio,
                IdMedico = o.id_medico,
                Observacion = o.observacion,
                Detalles = o.detalle_ordens.Select(d => new DetalleOrdenDto
                {
                    IdDetalleOrden = d.id_detalle_orden,
                    IdOrden = (int)d.id_orden,
                    IdExamen = (int)d.id_examen,
                    Precio = (decimal)d.precio,
                    IdResultado = d.id_resultado
                }).ToList()
            })
            .ToListAsync();

        return ordenes;
    }

    public async Task<OrdenDto?> ObtenerOrdenPorIdAsync(int idOrden)
    {
        var o = await _context.ordens
            .Include(x => x.detalle_ordens)
            .FirstOrDefaultAsync(x => x.id_orden == idOrden);

        if (o == null) return null;

        return new OrdenDto
        {
            IdOrden = o.id_orden,
            NumeroOrden = o.numero_orden,
            IdPaciente = (int)o.id_paciente,
            FechaOrden = o.fecha_orden,
            Total = o.total,
            SaldoPendiente = (decimal)o.saldo_pendiente,
            TotalPagado = (decimal)o.total_pagado,
            EstadoPago = o.estado_pago,
            Anulado = (bool)o.anulado,
            LiquidadoConvenio = (bool)o.liquidado_convenio,
            IdMedico = o.id_medico,
            Observacion = o.observacion,
            Detalles = o.detalle_ordens.Select(d => new DetalleOrdenDto
            {
                IdDetalleOrden = d.id_detalle_orden,
                IdOrden = (int)d.id_orden,
                IdExamen = (int)d.id_examen,
                Precio = (decimal)d.precio,
                IdResultado = d.id_resultado
            }).ToList()
        };
    }

    public async Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto dto)
    {
        var ordenDto = dto.Orden;

        var entidad = new orden
        {
            id_paciente = ordenDto.IdPaciente,
            fecha_orden = ordenDto.FechaOrden,
            id_medico = ordenDto.IdMedico,
            observacion = ordenDto.Observacion,
            estado_pago = "PENDIENTE",
            anulado = false,
            liquidado_convenio = false,
            numero_orden = Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
        };

        entidad.detalle_ordens = dto.IdsExamenes.Select(idExamen => new detalle_orden
        {
            id_examen = idExamen,
            precio = 0
        }).ToList();

        entidad.total = (decimal)entidad.detalle_ordens.Sum(d => d.precio);
        entidad.total_pagado = 0;
        entidad.saldo_pendiente = entidad.total;

        _context.ordens.Add(entidad);
        await _context.SaveChangesAsync();

        return new OrdenRespuestaDto
        {
            IdOrden = entidad.id_orden,
            NumeroOrden = entidad.numero_orden
        };
    }
}

}
