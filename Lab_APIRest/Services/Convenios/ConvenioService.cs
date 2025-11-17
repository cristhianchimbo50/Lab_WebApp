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

        private static ConvenioDto MapConvenio(Convenio entidad) => new()
        {
            IdConvenio = entidad.IdConvenio,
            IdMedico = entidad.IdMedico,
            NombreMedico = entidad.IdMedicoNavigation?.NombreMedico,
            FechaConvenio = entidad.FechaConvenio,
            PorcentajeComision = entidad.PorcentajeComision,
            MontoTotal = entidad.MontoTotal,
            Anulado = !entidad.Activo
        };

        private static DetalleConvenioDto MapOrdenConvenio(Orden o, int idConvenio) => new()
        {
            IdDetalleConvenio = o.IdOrden,
            IdConvenio = idConvenio,
            IdOrden = o.IdOrden,
            NumeroOrden = o.NumeroOrden,
            Paciente = o.IdPacienteNavigation?.NombrePaciente,
            FechaOrden = o.FechaOrden,
            Subtotal = o.Total
        };

        public async Task<IEnumerable<ConvenioDto>> ListarConveniosAsync()
        {
            var convenios = await _context.Convenio
                .Include(c => c.IdMedicoNavigation)
                .OrderByDescending(c => c.FechaConvenio)
                .ToListAsync();
            return convenios.Select(MapConvenio);
        }

        public async Task<ResultadoPaginadoDto<ConvenioDto>> ListarConveniosPaginadosAsync(ConvenioFiltroDto filtro)
        {
            var query = _context.Convenio.Include(c => c.IdMedicoNavigation).AsNoTracking().AsQueryable();
            if (filtro.FechaDesde.HasValue) query = query.Where(c => c.FechaConvenio >= filtro.FechaDesde.Value);
            if (filtro.FechaHasta.HasValue) query = query.Where(c => c.FechaConvenio <= filtro.FechaHasta.Value);
            if (!(filtro.IncluirAnuladas && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnuladas && !filtro.IncluirVigentes) query = query.Where(c => c.Activo == false);
                else if (!filtro.IncluirAnuladas && filtro.IncluirVigentes) query = query.Where(c => c.Activo == true);
            }
            if (!string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "numero": query = query.Where(c => EF.Functions.Like(c.IdConvenio.ToString(), $"%{val}%")); break;
                    case "medico": query = query.Where(c => c.IdMedicoNavigation != null && c.IdMedicoNavigation.NombreMedico.ToLower().Contains(val)); break;
                }
            }
            var total = await query.CountAsync();
            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(ConvenioDto.IdConvenio) => asc ? query.OrderBy(c => c.IdConvenio) : query.OrderByDescending(c => c.IdConvenio),
                nameof(ConvenioDto.NombreMedico) => asc ? query.OrderBy(c => c.IdMedicoNavigation!.NombreMedico) : query.OrderByDescending(c => c.IdMedicoNavigation!.NombreMedico),
                nameof(ConvenioDto.FechaConvenio) => asc ? query.OrderBy(c => c.FechaConvenio) : query.OrderByDescending(c => c.FechaConvenio),
                nameof(ConvenioDto.MontoTotal) => asc ? query.OrderBy(c => c.MontoTotal) : query.OrderByDescending(c => c.MontoTotal),
                _ => query.OrderByDescending(c => c.FechaConvenio)
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
            var convenio = await _context.Convenio.Include(c => c.IdMedicoNavigation).FirstOrDefaultAsync(c => c.IdConvenio == idConvenio);
            if (convenio == null) return null;
            var ordenes = await _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .Where(o => o.IdConvenio == idConvenio)
                .ToListAsync();
            return new ConvenioDetalleDto
            {
                IdConvenio = convenio.IdConvenio,
                IdMedico = convenio.IdMedico,
                NombreMedico = convenio.IdMedicoNavigation?.NombreMedico,
                FechaConvenio = convenio.FechaConvenio,
                PorcentajeComision = convenio.PorcentajeComision,
                MontoTotal = convenio.MontoTotal,
                Anulado = !convenio.Activo,
                Ordenes = ordenes.Select(o => MapOrdenConvenio(o, convenio.IdConvenio)).ToList()
            };
        }

        public async Task<IEnumerable<OrdenDisponibleDto>> ListarOrdenesDisponiblesAsync(int idMedico)
        {
            return await _context.Orden
                .Include(o => o.IdPacienteNavigation)
                .Where(o => o.IdMedico == idMedico && o.IdConvenio == null && o.Activo)
                .Select(o => new OrdenDisponibleDto
                {
                    IdOrden = o.IdOrden,
                    NumeroOrden = o.NumeroOrden,
                    Paciente = o.IdPacienteNavigation!.NombrePaciente,
                    FechaOrden = o.FechaOrden,
                    Total = o.Total
                })
                .OrderByDescending(o => o.FechaOrden)
                .ToListAsync();
        }

        public async Task<bool> GuardarConvenioAsync(ConvenioRegistroDto convenioRegistro)
        {
            await using var transaccion = await _context.Database.BeginTransactionAsync();
            try
            {
                var entidad = new Convenio
                {
                    IdMedico = convenioRegistro.IdMedico,
                    FechaConvenio = convenioRegistro.FechaConvenio,
                    PorcentajeComision = convenioRegistro.PorcentajeComision,
                    MontoTotal = convenioRegistro.MontoTotal,
                    Activo = true,
                    FechaFin = null
                };
                _context.Convenio.Add(entidad);
                await _context.SaveChangesAsync();
                foreach (var od in convenioRegistro.Ordenes)
                {
                    var orden = await _context.Orden.FindAsync(od.IdOrden);
                    if (orden != null)
                    {
                        orden.IdConvenio = entidad.IdConvenio;
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
                var entidad = await _context.Convenio.FirstOrDefaultAsync(c => c.IdConvenio == idConvenio);
                if (entidad == null) return false;
                entidad.Activo = false;
                entidad.FechaFin = DateTime.UtcNow;
                var ordenes = await _context.Orden.Where(o => o.IdConvenio == idConvenio).ToListAsync();
                foreach (var ord in ordenes)
                {
                    ord.IdConvenio = null;
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
