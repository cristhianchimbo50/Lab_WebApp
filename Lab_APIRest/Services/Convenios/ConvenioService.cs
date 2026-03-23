using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using convenio = Lab_APIRest.Infrastructure.EF.Models.convenio;
using orden = Lab_APIRest.Infrastructure.EF.Models.orden;
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

        private static ConvenioDto MapConvenio(convenio entidad) => new()
        {
            IdConvenio = entidad.id_convenio,
            IdMedico = entidad.id_medico,
            NombreMedico = entidad.medico_navigation?.nombre_medico ?? string.Empty,
            FechaConvenio = entidad.fecha_convenio,
            PorcentajeComision = entidad.porcentaje_comision,
            MontoTotal = entidad.monto_total,
            Anulado = !entidad.activo
        };

        private static DetalleConvenioDto MapOrdenConvenio(orden o, int idConvenio) => new()
        {
            IdDetalleConvenio = o.id_orden,
            IdConvenio = idConvenio,
            IdOrden = o.id_orden,
            NumeroOrden = o.numero_orden ?? string.Empty,
            Paciente = $"{o.paciente_navigation?.persona_navigation?.nombres} {o.paciente_navigation?.persona_navigation?.apellidos}".Trim(),
            FechaOrden = o.fecha_orden,
            Subtotal = o.total
        };

        public async Task<IEnumerable<ConvenioDto>> ListarConveniosAsync()
        {
            var convenios = await _context.Convenio
                .Include(c => c.medico_navigation)
                .OrderByDescending(c => c.fecha_convenio)
                .ToListAsync();
            return convenios.Select(MapConvenio);
        }

        public async Task<ResultadoPaginadoDto<ConvenioDto>> ListarConveniosPaginadosAsync(ConvenioFiltroDto filtro)
        {
            var query = _context.Convenio.Include(c => c.medico_navigation).AsNoTracking().AsQueryable();
            if (filtro.FechaDesde.HasValue) query = query.Where(c => c.fecha_convenio >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue) query = query.Where(c => c.fecha_convenio <= filtro.FechaHasta.Value);
            if (!(filtro.IncluirAnuladas && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnuladas && !filtro.IncluirVigentes) query = query.Where(c => c.activo == false);
                else if (!filtro.IncluirAnuladas && filtro.IncluirVigentes) query = query.Where(c => c.activo == true);
            }
            if (!string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "numero": query = query.Where(c => EF.Functions.Like(c.id_convenio.ToString(), $"%{val}%")); break;
                    case "medico": query = query.Where(c => c.medico_navigation != null && c.medico_navigation.nombre_medico.ToLower().Contains(val)); break;
                }
            }
            var total = await query.CountAsync();
            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(ConvenioDto.IdConvenio) => asc ? query.OrderBy(c => c.id_convenio) : query.OrderByDescending(c => c.id_convenio),
                nameof(ConvenioDto.NombreMedico) => asc ? query.OrderBy(c => c.medico_navigation != null ? c.medico_navigation.nombre_medico : string.Empty) : query.OrderByDescending(c => c.medico_navigation != null ? c.medico_navigation.nombre_medico : string.Empty),
                nameof(ConvenioDto.FechaConvenio) => asc ? query.OrderBy(c => c.fecha_convenio) : query.OrderByDescending(c => c.fecha_convenio),
                nameof(ConvenioDto.MontoTotal) => asc ? query.OrderBy(c => c.monto_total) : query.OrderByDescending(c => c.monto_total),
                _ => query.OrderByDescending(c => c.fecha_convenio)
            };
            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);
            var entidades = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = entidades.Select(MapConvenio).ToList();
            return new ResultadoPaginadoDto<ConvenioDto> { TotalCount = total, PageNumber = pageNumber, PageSize = pageSize, Items = items };
        }

        public Task<ResultadoPaginadoDto<ConvenioDto>> ListarConveniosPaginadosAsync(string? criterio, string? valor, DateOnly? desde, DateOnly? hasta, int page, int pageSize)
        {
            var filtro = new ConvenioFiltroDto { CriterioBusqueda = criterio, ValorBusqueda = valor, FechaDesde = desde, FechaHasta = hasta, PageNumber = page, PageSize = pageSize };
            return ListarConveniosPaginadosAsync(filtro);
        }

        public async Task<ConvenioDetalleDto?> ObtenerDetalleConvenioAsync(int idConvenio)
        {
            var convenio = await _context.Convenio.Include(c => c.medico_navigation).FirstOrDefaultAsync(c => c.id_convenio == idConvenio);
            if (convenio == null) return null;
            var ordenes = await _context.Orden
                .Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Where(o => o.id_convenio == idConvenio)
                .ToListAsync();
            return new ConvenioDetalleDto
            {
                IdConvenio = convenio.id_convenio,
                IdMedico = convenio.id_medico,
                NombreMedico = convenio.medico_navigation?.nombre_medico,
                FechaConvenio = convenio.fecha_convenio,
                PorcentajeComision = convenio.porcentaje_comision,
                MontoTotal = convenio.monto_total,
                Anulado = !convenio.activo,
                Ordenes = ordenes.Select(o => MapOrdenConvenio(o, convenio.id_convenio)).ToList()
            };
        }

        public async Task<IEnumerable<OrdenDisponibleDto>> ListarOrdenesDisponiblesAsync(int idMedico)
        {
            return await _context.Orden
                .Include(o => o.paciente_navigation)!.ThenInclude(p => p.persona_navigation)
                .Where(o => o.id_medico == idMedico && o.id_convenio == null && o.activo)
                .Select(o => new OrdenDisponibleDto
                {
                    IdOrden = o.id_orden,
                    NumeroOrden = o.numero_orden,
                    Paciente = (o.paciente_navigation != null && o.paciente_navigation.persona_navigation != null
                        ? (o.paciente_navigation.persona_navigation.nombres + " " + o.paciente_navigation.persona_navigation.apellidos)
                        : string.Empty).Trim(),
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
                    activo = true,
                    fecha_fin = null
                };
                _context.Convenio.Add(entidad);
                await _context.SaveChangesAsync();
                foreach (var od in convenioRegistro.Ordenes)
                {
                    var orden = await _context.Orden.FindAsync(od.IdOrden);
                    if (orden != null)
                    {
                        orden.id_convenio = entidad.id_convenio;
                    }
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
                var entidad = await _context.Convenio.FirstOrDefaultAsync(c => c.id_convenio == idConvenio);
                if (entidad == null) return false;
                entidad.activo = false;
                entidad.fecha_fin = DateTime.UtcNow;
                var ordenes = await _context.Orden.Where(o => o.id_convenio == idConvenio).ToListAsync();
                foreach (var ord in ordenes)
                {
                    ord.id_convenio = null;
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
