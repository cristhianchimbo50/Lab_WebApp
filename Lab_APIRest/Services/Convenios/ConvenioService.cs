using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_Contracts.Convenios;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Convenios
{
    public class ConvenioService : IConvenioService
    {
        private readonly LabDbContext _context;

        public ConvenioService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ConvenioDto>> ObtenerConveniosAsync()
        {
            return await _context.convenios
                .Include(c => c.id_medicoNavigation)
                .Select(c => new ConvenioDto
                {
                    IdConvenio = c.id_convenio,
                    IdMedico = c.id_medico,
                    NombreMedico = c.id_medicoNavigation!.nombre_medico,
                    FechaConvenio = c.fecha_convenio,
                    PorcentajeComision = c.porcentaje_comision,
                    MontoTotal = c.monto_total,
                    Anulado = c.anulado
                })
                .OrderByDescending(c => c.FechaConvenio)
                .ToListAsync();
        }

        public async Task<ConvenioDetalleDto?> ObtenerDetalleConvenioAsync(int id)
        {
            var convenio = await _context.convenios
                .Include(c => c.id_medicoNavigation)
                .Include(c => c.detalle_convenios)
                    .ThenInclude(d => d.id_ordenNavigation)
                        .ThenInclude(o => o.id_pacienteNavigation)
                .FirstOrDefaultAsync(c => c.id_convenio == id);

            if (convenio == null) return null;

            return new ConvenioDetalleDto
            {
                IdConvenio = convenio.id_convenio,
                IdMedico = convenio.id_medico,
                NombreMedico = convenio.id_medicoNavigation?.nombre_medico,
                FechaConvenio = convenio.fecha_convenio,
                PorcentajeComision = convenio.porcentaje_comision,
                MontoTotal = convenio.monto_total,
                Anulado = convenio.anulado,
                Ordenes = convenio.detalle_convenios.Select(d => new DetalleConvenioDto
                {
                    IdDetalleConvenio = d.id_detalle_convenio,
                    IdOrden = d.id_orden,
                    NumeroOrden = d.id_ordenNavigation.numero_orden,
                    Paciente = d.id_ordenNavigation.id_pacienteNavigation!.nombre_paciente,
                    FechaOrden = d.id_ordenNavigation.fecha_orden,
                    Subtotal = d.subtotal
                }).ToList()
            };
        }

        public async Task<IEnumerable<OrdenDisponibleDto>> ObtenerOrdenesDisponiblesAsync(int idMedico)
        {
            return await _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .Where(o => o.id_medico == idMedico && o.liquidado_convenio == false && o.anulado == false)
                .Select(o => new OrdenDisponibleDto
                {
                    IdOrden = o.id_orden,
                    NumeroOrden = o.numero_orden,
                    Paciente = o.id_pacienteNavigation!.nombre_paciente,
                    FechaOrden = o.fecha_orden,
                    Total = o.total
                })
                .OrderByDescending(o => o.FechaOrden)
                .ToListAsync();
        }

        public async Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var nuevoConvenio = new convenio
                {
                    id_medico = dto.IdMedico,
                    fecha_convenio = dto.FechaConvenio,
                    porcentaje_comision = dto.PorcentajeComision,
                    monto_total = dto.MontoTotal,
                    anulado = false
                };

                _context.convenios.Add(nuevoConvenio);
                await _context.SaveChangesAsync();

                foreach (var orden in dto.Ordenes)
                {
                    _context.detalle_convenios.Add(new detalle_convenio
                    {
                        id_convenio = nuevoConvenio.id_convenio,
                        id_orden = orden.IdOrden,
                        subtotal = orden.Subtotal
                    });

                    var ordenDb = await _context.ordens.FindAsync(orden.IdOrden);
                    if (ordenDb != null)
                        ordenDb.liquidado_convenio = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> AnularConvenioAsync(int id)
        {
            var convenio = await _context.convenios
                .Include(c => c.detalle_convenios)
                .FirstOrDefaultAsync(c => c.id_convenio == id);

            if (convenio == null) return false;

            convenio.anulado = true;

            // Revertir órdenes asociadas
            foreach (var detalle in convenio.detalle_convenios)
            {
                var orden = await _context.ordens.FindAsync(detalle.id_orden);
                if (orden != null)
                    orden.liquidado_convenio = false;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
