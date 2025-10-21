using Lab_Contracts.Reactivos;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Reactivos
{
    public class ReactivoService : IReactivoService
    {
        private readonly LabDbContext _context;

        public ReactivoService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<List<ReactivoDto>> GetReactivosAsync()
        {
            return await _context.reactivos
                .Where(r => !(r.anulado ?? false))
                .Select(r => new ReactivoDto
                {
                    IdReactivo = r.id_reactivo,
                    NombreReactivo = r.nombre_reactivo,
                    Fabricante = r.fabricante,
                    Unidad = r.unidad,
                    Anulado = r.anulado ?? false,
                    CantidadDisponible = (decimal)r.cantidad_disponible
                })
                .ToListAsync();
        }


        public async Task<ReactivoDto?> GetReactivoPorIdAsync(int id)
        {
            var r = await _context.reactivos.FindAsync(id);
            if (r == null) return null;
            return new ReactivoDto
            {
                IdReactivo = r.id_reactivo,
                NombreReactivo = r.nombre_reactivo,
                Fabricante = r.fabricante,
                Unidad = r.unidad,
                Anulado = r.anulado ?? false,
                CantidadDisponible = (decimal)r.cantidad_disponible
            };
        }

        public async Task<ReactivoDto> CrearReactivoAsync(ReactivoDto dto)
        {
            var reactivo = new reactivo
            {
                nombre_reactivo = dto.NombreReactivo,
                fabricante = dto.Fabricante,
                unidad = dto.Unidad,
                anulado = false,
                cantidad_disponible = dto.CantidadDisponible
            };
            _context.reactivos.Add(reactivo);
            await _context.SaveChangesAsync();
            dto.IdReactivo = reactivo.id_reactivo;
            return dto;
        }

        public async Task<bool> EditarReactivoAsync(int id, ReactivoDto dto)
        {
            var reactivo = await _context.reactivos.FindAsync(id);
            if (reactivo == null) return false;

            reactivo.nombre_reactivo = dto.NombreReactivo;
            reactivo.fabricante = dto.Fabricante;
            reactivo.unidad = dto.Unidad;
            reactivo.cantidad_disponible = dto.CantidadDisponible;
            reactivo.anulado = dto.Anulado;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularReactivoAsync(int id)
        {
            var reactivo = await _context.reactivos.FindAsync(id);
            if (reactivo == null) return false;

            reactivo.anulado = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegistrarIngresosAsync(IEnumerable<MovimientoReactivoIngresoDto> ingresos)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var ingreso in ingresos)
                {
                    var reactivo = await _context.reactivos
                        .FirstOrDefaultAsync(r => r.id_reactivo == ingreso.IdReactivo);

                    if (reactivo == null)
                        continue;

                    var movimiento = new Infrastructure.EF.Models.movimiento_reactivo
                    {
                        id_reactivo = ingreso.IdReactivo,
                        tipo_movimiento = "INGRESO",
                        cantidad = ingreso.Cantidad,
                        fecha_movimiento = ingreso.FechaMovimiento,
                        observacion = ingreso.Observacion
                    };
                    await _context.movimiento_reactivos.AddAsync(movimiento);

                    reactivo.cantidad_disponible += ingreso.Cantidad;
                    _context.reactivos.Update(reactivo);
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


        public async Task<bool> RegistrarEgresosAsync(IEnumerable<MovimientoReactivoEgresoDto> egresos)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var egreso in egresos)
                {
                    var reactivo = await _context.reactivos.FirstOrDefaultAsync(r => r.id_reactivo == egreso.IdReactivo);
                    if (reactivo == null) continue;

                    if (reactivo.cantidad_disponible < egreso.Cantidad)
                        throw new InvalidOperationException($"Stock insuficiente para {reactivo.nombre_reactivo}");

                    var movimiento = new movimiento_reactivo
                    {
                        id_reactivo = egreso.IdReactivo,
                        tipo_movimiento = "EGRESO",
                        cantidad = egreso.Cantidad,
                        fecha_movimiento = egreso.FechaMovimiento,
                        observacion = egreso.Observacion,
                        id_detalle_resultado = egreso.IdDetalleResultado
                    };
                    await _context.movimiento_reactivos.AddAsync(movimiento);
                    reactivo.cantidad_disponible -= egreso.Cantidad;
                    _context.reactivos.Update(reactivo);
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
    }
}
