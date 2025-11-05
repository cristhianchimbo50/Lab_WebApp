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
        private readonly LabDbContext Contexto;
        private readonly ILogger<ConvenioService> Logger;

        /// <summary>
        /// Constructor del servicio de convenios.
        /// </summary>
        /// <param name="Contexto">Contexto de base de datos.</param>
        /// <param name="Logger">Logger para registrar eventos y errores.</param>
        public ConvenioService(LabDbContext Contexto, ILogger<ConvenioService> Logger)
        {
            this.Contexto = Contexto;
            this.Logger = Logger;
        }

        /// <summary>
        /// Obtiene todos los convenios registrados.
        /// </summary>
        public async Task<IEnumerable<ConvenioDto>> ObtenerConveniosAsync()
        {
            return await Contexto.convenios
                .Include(Convenio => Convenio.id_medicoNavigation)
                .Select(Convenio => new ConvenioDto
                {
                    IdConvenio = Convenio.id_convenio,
                    IdMedico = Convenio.id_medico,
                    NombreMedico = Convenio.id_medicoNavigation!.nombre_medico,
                    FechaConvenio = Convenio.fecha_convenio,
                    PorcentajeComision = Convenio.porcentaje_comision,
                    MontoTotal = Convenio.monto_total,
                    Anulado = Convenio.anulado
                })
                .OrderByDescending(Convenio => Convenio.FechaConvenio)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene el detalle completo de un convenio, incluyendo órdenes asociadas.
        /// </summary>
        public async Task<ConvenioDetalleDto?> ObtenerDetalleConvenioAsync(int IdConvenio)
        {
            var ConvenioEntidad = await Contexto.convenios
                .Include(Convenio => Convenio.id_medicoNavigation)
                .Include(Convenio => Convenio.detalle_convenios)
                    .ThenInclude(Detalle => Detalle.id_ordenNavigation)
                        .ThenInclude(Orden => Orden.id_pacienteNavigation)
                .FirstOrDefaultAsync(Convenio => Convenio.id_convenio == IdConvenio);

            if (ConvenioEntidad == null)
                return null;

            return new ConvenioDetalleDto
            {
                IdConvenio = ConvenioEntidad.id_convenio,
                IdMedico = ConvenioEntidad.id_medico,
                NombreMedico = ConvenioEntidad.id_medicoNavigation?.nombre_medico,
                FechaConvenio = ConvenioEntidad.fecha_convenio,
                PorcentajeComision = ConvenioEntidad.porcentaje_comision,
                MontoTotal = ConvenioEntidad.monto_total,
                Anulado = ConvenioEntidad.anulado,
                Ordenes = ConvenioEntidad.detalle_convenios.Select(Detalle => new DetalleConvenioDto
                {
                    IdDetalleConvenio = Detalle.id_detalle_convenio,
                    IdOrden = Detalle.id_orden,
                    NumeroOrden = Detalle.id_ordenNavigation.numero_orden,
                    Paciente = Detalle.id_ordenNavigation.id_pacienteNavigation!.nombre_paciente,
                    FechaOrden = Detalle.id_ordenNavigation.fecha_orden,
                    Subtotal = Detalle.subtotal
                }).ToList()
            };
        }

        /// <summary>
        /// Obtiene las órdenes disponibles de un médico que aún no han sido liquidadas en convenios.
        /// </summary>
        public async Task<IEnumerable<OrdenDisponibleDto>> ObtenerOrdenesDisponiblesAsync(int IdMedico)
        {
            return await Contexto.ordens
                .Include(Orden => Orden.id_pacienteNavigation)
                .Where(Orden => Orden.id_medico == IdMedico 
                    && (Orden.liquidado_convenio == false || Orden.liquidado_convenio == null)
                    && (Orden.anulado == false || Orden.anulado == null))
                .Select(Orden => new OrdenDisponibleDto
                {
                    IdOrden = Orden.id_orden,
                    NumeroOrden = Orden.numero_orden,
                    Paciente = Orden.id_pacienteNavigation!.nombre_paciente,
                    FechaOrden = Orden.fecha_orden,
                    Total = Orden.total
                })
                .OrderByDescending(Orden => Orden.FechaOrden)
                .ToListAsync();
        }

        /// <summary>
        /// Registra un nuevo convenio y marca las órdenes asociadas como liquidadas.
        /// </summary>
        public async Task<bool> RegistrarConvenioAsync(ConvenioRegistroDto ConvenioRegistro)
        {
            await using var Transaccion = await Contexto.Database.BeginTransactionAsync();

            try
            {
                var ConvenioEntidad = new convenio
                {
                    id_medico = ConvenioRegistro.IdMedico,
                    fecha_convenio = ConvenioRegistro.FechaConvenio,
                    porcentaje_comision = ConvenioRegistro.PorcentajeComision,
                    monto_total = ConvenioRegistro.MontoTotal,
                    anulado = false
                };

                Contexto.convenios.Add(ConvenioEntidad);
                await Contexto.SaveChangesAsync();

                foreach (var OrdenRegistro in ConvenioRegistro.Ordenes)
                {
                    Contexto.detalle_convenios.Add(new detalle_convenio
                    {
                        id_convenio = ConvenioEntidad.id_convenio,
                        id_orden = OrdenRegistro.IdOrden,
                        subtotal = OrdenRegistro.Subtotal
                    });

                    var OrdenEntidad = await Contexto.ordens.FindAsync(OrdenRegistro.IdOrden);
                    if (OrdenEntidad != null)
                        OrdenEntidad.liquidado_convenio = true;
                }

                await Contexto.SaveChangesAsync();
                await Transaccion.CommitAsync();

                return true;
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, "Error al registrar el convenio.");
                await Transaccion.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Anula un convenio y revierte el estado de las órdenes asociadas.
        /// </summary>
        public async Task<bool> AnularConvenioAsync(int IdConvenio)
        {
            try
            {
                var ConvenioEntidad = await Contexto.convenios
                    .Include(Convenio => Convenio.detalle_convenios)
                    .FirstOrDefaultAsync(Convenio => Convenio.id_convenio == IdConvenio);

                if (ConvenioEntidad == null)
                    return false;

                ConvenioEntidad.anulado = true;

                foreach (var Detalle in ConvenioEntidad.detalle_convenios)
                {
                    var OrdenEntidad = await Contexto.ordens.FindAsync(Detalle.id_orden);
                    if (OrdenEntidad != null)
                        OrdenEntidad.liquidado_convenio = false;
                }

                await Contexto.SaveChangesAsync();
                return true;
            }
            catch (Exception Ex)
            {
                Logger.LogError(Ex, $"Error al anular el convenio con ID {IdConvenio}.");
                return false;
            }
        }
    }
}
