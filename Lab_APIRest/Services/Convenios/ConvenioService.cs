using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_Contracts.Convenios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lab_APIRest.Services.Convenios
{
    /// <summary>
    /// Servicio encargado de la lógica de negocio relacionada con los convenios médicos.
    /// </summary>
    public class ConvenioService : IConvenioService
    {
        private readonly LabDbContext _context;
        private readonly ILogger<ConvenioService> _logger;

        /// <summary>
        /// Constructor del servicio de convenios.
        /// </summary>
        /// <param name="context">Contexto de base de datos.</param>
        /// <param name="logger">Logger para registrar eventos y errores.</param>
        public ConvenioService(LabDbContext context, ILogger<ConvenioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los convenios registrados.
        /// </summary>
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

        /// <summary>
        /// Obtiene el detalle completo de un convenio, incluyendo órdenes asociadas.
        /// </summary>
        public async Task<ConvenioDetalleDto?> ObtenerDetalleConvenioAsync(int id)
        {
            var convenio = await _context.convenios
                .Include(c => c.id_medicoNavigation)
                .Include(c => c.detalle_convenios)
                    .ThenInclude(d => d.id_ordenNavigation)
                        .ThenInclude(o => o.id_pacienteNavigation)
                .FirstOrDefaultAsync(c => c.id_convenio == id);

            if (convenio == null)
                return null;

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

        /// <summary>
        /// Obtiene las órdenes disponibles de un médico que aún no han sido liquidadas en convenios.
        /// </summary>
        public async Task<IEnumerable<OrdenDisponibleDto>> ObtenerOrdenesDisponiblesAsync(int idMedico)
        {
            return await _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .Where(o => o.id_medico == idMedico 
                    && (o.liquidado_convenio == false || o.liquidado_convenio == null)
                    && (o.anulado == false || o.anulado == null))
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

        /// <summary>
        /// Registra un nuevo convenio y marca las órdenes asociadas como liquidadas.
        /// </summary>
        public async Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el convenio.");
                await transaction.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Anula un convenio y revierte el estado de las órdenes asociadas.
        /// </summary>
        public async Task<bool> AnularConvenioAsync(int id)
        {
            try
            {
                var convenio = await _context.convenios
                    .Include(c => c.detalle_convenios)
                    .FirstOrDefaultAsync(c => c.id_convenio == id);

                if (convenio == null)
                    return false;

                convenio.anulado = true;

                foreach (var detalle in convenio.detalle_convenios)
                {
                    var orden = await _context.ordens.FindAsync(detalle.id_orden);
                    if (orden != null)
                        orden.liquidado_convenio = false;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular el convenio con ID {id}.");
                return false;
            }
        }
    }
}
