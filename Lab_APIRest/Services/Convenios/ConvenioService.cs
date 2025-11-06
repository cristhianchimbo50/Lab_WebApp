using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_Contracts.Common;
using Lab_Contracts.Convenios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Lab_APIRest.Services.Convenios
{
    public class ConvenioService : IConvenioService
    {
        private readonly LabDbContext _context;
        private readonly ILogger<ConvenioService> _logger;

        public ConvenioService(LabDbContext context, ILogger<ConvenioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ConvenioDto>> ListarConveniosAsync()
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

        public async Task<ResultadoPaginadoDto<ConvenioDto>> ListarConveniosPaginadosAsync(ConvenioFiltroDto filtro)
        {
            var query = _context.convenios.Include(c => c.id_medicoNavigation).AsNoTracking().AsQueryable();
            if (filtro.FechaDesde.HasValue) query = query.Where(c => c.fecha_convenio >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue) query = query.Where(c => c.fecha_convenio <= filtro.FechaHasta.Value);
            if (!(filtro.IncluirAnuladas && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnuladas && !filtro.IncluirVigentes) query = query.Where(c => c.anulado == true);
                else if (!filtro.IncluirAnuladas && filtro.IncluirVigentes) query = query.Where(c => c.anulado == false || c.anulado == null);
            }
            if (!string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "numero": query = query.Where(c => EF.Functions.Like(c.id_convenio.ToString(), $"%{val}%")); break;
                    case "medico": query = query.Where(c => c.id_medicoNavigation != null && c.id_medicoNavigation.nombre_medico.ToLower().Contains(val)); break;
                }
            }
            var total = await query.CountAsync();
            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(ConvenioDto.IdConvenio) => asc ? query.OrderBy(c => c.id_convenio) : query.OrderByDescending(c => c.id_convenio),
                nameof(ConvenioDto.NombreMedico) => asc ? query.OrderBy(c => c.id_medicoNavigation!.nombre_medico) : query.OrderByDescending(c => c.id_medicoNavigation!.nombre_medico),
                nameof(ConvenioDto.FechaConvenio) => asc ? query.OrderBy(c => c.fecha_convenio) : query.OrderByDescending(c => c.fecha_convenio),
                nameof(ConvenioDto.MontoTotal) => asc ? query.OrderBy(c => c.monto_total) : query.OrderByDescending(c => c.monto_total),
                _ => query.OrderByDescending(c => c.fecha_convenio)
            };
            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
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
                .ToListAsync();
            return new ResultadoPaginadoDto<ConvenioDto> { TotalCount = total, PageNumber = pageNumber, PageSize = pageSize, Items = items };
        }

        public Task<ResultadoPaginadoDto<ConvenioDto>> ListarConveniosPaginadosAsync(string? criterio, string? valor, DateOnly? desde, DateOnly? hasta, int page, int pageSize)
        {
            var filtro = new ConvenioFiltroDto { CriterioBusqueda = criterio, ValorBusqueda = valor, FechaDesde = desde, FechaHasta = hasta, PageNumber = page, PageSize = pageSize };
            return ListarConveniosPaginadosAsync(filtro);
        }

        public async Task<ConvenioDetalleDto?> ObtenerDetalleConvenioAsync(int idConvenio)
        {
            var entidad = await _context.convenios
                .Include(c => c.id_medicoNavigation)
                .Include(c => c.detalle_convenios).ThenInclude(d => d.id_ordenNavigation).ThenInclude(o => o.id_pacienteNavigation)
                .FirstOrDefaultAsync(c => c.id_convenio == idConvenio);
            if (entidad == null) return null;
            return new ConvenioDetalleDto
            {
                IdConvenio = entidad.id_convenio,
                IdMedico = entidad.id_medico,
                NombreMedico = entidad.id_medicoNavigation?.nombre_medico,
                FechaConvenio = entidad.fecha_convenio,
                PorcentajeComision = entidad.porcentaje_comision,
                MontoTotal = entidad.monto_total,
                Anulado = entidad.anulado,
                Ordenes = entidad.detalle_convenios.Select(d => new DetalleConvenioDto
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

        public async Task<IEnumerable<OrdenDisponibleDto>> ListarOrdenesDisponiblesAsync(int idMedico)
        {
            return await _context.ordens
                .Include(o => o.id_pacienteNavigation)
                .Where(o => o.id_medico == idMedico && (o.liquidado_convenio == false || o.liquidado_convenio == null) && (o.anulado == false || o.anulado == null))
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

        public async Task<bool> GuardarConvenioAsync(ConvenioRegistroDto convenioRegistro)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var entidad = new convenio
                {
                    id_medico = convenioRegistro.IdMedico,
                    fecha_convenio = convenioRegistro.FechaConvenio,
                    porcentaje_comision = convenioRegistro.PorcentajeComision,
                    monto_total = convenioRegistro.MontoTotal,
                    anulado = false
                };
                _context.convenios.Add(entidad);
                await _context.SaveChangesAsync();
                foreach (var od in convenioRegistro.Ordenes)
                {
                    _context.detalle_convenios.Add(new detalle_convenio { id_convenio = entidad.id_convenio, id_orden = od.IdOrden, subtotal = od.Subtotal });
                    var orden = await _context.ordens.FindAsync(od.IdOrden);
                    if (orden != null) orden.liquidado_convenio = true;
                }
                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar el convenio.");
                await transaccion.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> AnularConvenioAsync(int idConvenio)
        {
            try
            {
                var entidad = await _context.convenios.Include(c => c.detalle_convenios).FirstOrDefaultAsync(c => c.id_convenio == idConvenio);
                if (entidad == null) return false;
                entidad.anulado = true;
                foreach (var det in entidad.detalle_convenios)
                {
                    var orden = await _context.ordens.FindAsync(det.id_orden);
                    if (orden != null) orden.liquidado_convenio = false;
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al anular el convenio con ID {idConvenio}.");
                return false;
            }
        }
    }
}
