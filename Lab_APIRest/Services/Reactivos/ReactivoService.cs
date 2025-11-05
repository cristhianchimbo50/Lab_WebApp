using Lab_Contracts.Reactivos;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Reactivos
{
    public class ReactivoService : IReactivoService
    {
        private readonly LabDbContext Contexto;

        public ReactivoService(LabDbContext contexto)
        {
            this.Contexto = contexto;
        }

        public async Task<List<ReactivoDto>> ObtenerReactivosAsync()
        {
            return await Contexto.reactivos
                .Where(ReactivoEntidad => !(ReactivoEntidad.anulado ?? false))
                .Select(ReactivoEntidad => new ReactivoDto
                {
                    IdReactivo = ReactivoEntidad.id_reactivo,
                    NombreReactivo = ReactivoEntidad.nombre_reactivo,
                    Fabricante = ReactivoEntidad.fabricante,
                    Unidad = ReactivoEntidad.unidad,
                    Anulado = ReactivoEntidad.anulado ?? false,
                    CantidadDisponible = (decimal)ReactivoEntidad.cantidad_disponible
                })
                .ToListAsync();
        }


        public async Task<ReactivoDto?> ObtenerReactivoPorIdAsync(int IdReactivo)
        {
            var ReactivoEntidad = await Contexto.reactivos.FindAsync(IdReactivo);
            if (ReactivoEntidad == null) return null;
            return new ReactivoDto
            {
                IdReactivo = ReactivoEntidad.id_reactivo,
                NombreReactivo = ReactivoEntidad.nombre_reactivo,
                Fabricante = ReactivoEntidad.fabricante,
                Unidad = ReactivoEntidad.unidad,
                Anulado = ReactivoEntidad.anulado ?? false,
                CantidadDisponible = (decimal)ReactivoEntidad.cantidad_disponible
            };
        }

        public async Task<ReactivoDto> CrearReactivoAsync(ReactivoDto Reactivo)
        {
            var ReactivoEntidad = new reactivo
            {
                nombre_reactivo = Reactivo.NombreReactivo,
                fabricante = Reactivo.Fabricante,
                unidad = Reactivo.Unidad,
                anulado = false,
                cantidad_disponible = Reactivo.CantidadDisponible
            };
            Contexto.reactivos.Add(ReactivoEntidad);
            await Contexto.SaveChangesAsync();
            Reactivo.IdReactivo = ReactivoEntidad.id_reactivo;
            return Reactivo;
        }

        public async Task<bool> EditarReactivoAsync(int IdReactivo, ReactivoDto Reactivo)
        {
            var ReactivoEntidad = await Contexto.reactivos.FindAsync(IdReactivo);
            if (ReactivoEntidad == null) return false;

            ReactivoEntidad.nombre_reactivo = Reactivo.NombreReactivo;
            ReactivoEntidad.fabricante = Reactivo.Fabricante;
            ReactivoEntidad.unidad = Reactivo.Unidad;
            ReactivoEntidad.cantidad_disponible = Reactivo.CantidadDisponible;
            ReactivoEntidad.anulado = Reactivo.Anulado;

            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularReactivoAsync(int IdReactivo)
        {
            var ReactivoEntidad = await Contexto.reactivos.FindAsync(IdReactivo);
            if (ReactivoEntidad == null) return false;

            ReactivoEntidad.anulado = true;
            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RegistrarIngresosAsync(IEnumerable<MovimientoReactivoIngresoDto> Ingresos)
        {
            using var Transaccion = await Contexto.Database.BeginTransactionAsync();
            try
            {
                foreach (var Ingreso in Ingresos)
                {
                    var ReactivoEntidad = await Contexto.reactivos
                        .FirstOrDefaultAsync(r => r.id_reactivo == Ingreso.IdReactivo);

                    if (ReactivoEntidad == null)
                        continue;

                    var MovimientoEntidad = new Infrastructure.EF.Models.movimiento_reactivo
                    {
                        id_reactivo = Ingreso.IdReactivo,
                        tipo_movimiento = "INGRESO",
                        cantidad = Ingreso.Cantidad,
                        fecha_movimiento = Ingreso.FechaMovimiento,
                        observacion = Ingreso.Observacion
                    };
                    await Contexto.movimiento_reactivos.AddAsync(MovimientoEntidad);

                    ReactivoEntidad.cantidad_disponible += Ingreso.Cantidad;
                    Contexto.reactivos.Update(ReactivoEntidad);
                }

                await Contexto.SaveChangesAsync();
                await Transaccion.CommitAsync();
                return true;
            }
            catch
            {
                await Transaccion.RollbackAsync();
                return false;
            }
        }


        public async Task<bool> RegistrarEgresosAsync(IEnumerable<MovimientoReactivoEgresoDto> Egresos)
        {
            using var Transaccion = await Contexto.Database.BeginTransactionAsync();
            try
            {
                foreach (var Egreso in Egresos)
                {
                    var ReactivoEntidad = await Contexto.reactivos.FirstOrDefaultAsync(r => r.id_reactivo == Egreso.IdReactivo);
                    if (ReactivoEntidad == null) continue;

                    if (ReactivoEntidad.cantidad_disponible < Egreso.Cantidad)
                        throw new InvalidOperationException($"Stock insuficiente para {ReactivoEntidad.nombre_reactivo}");

                    var MovimientoEntidad = new movimiento_reactivo
                    {
                        id_reactivo = Egreso.IdReactivo,
                        tipo_movimiento = "EGRESO",
                        cantidad = Egreso.Cantidad,
                        fecha_movimiento = Egreso.FechaMovimiento,
                        observacion = Egreso.Observacion,
                        id_detalle_resultado = Egreso.IdDetalleResultado
                    };
                    await Contexto.movimiento_reactivos.AddAsync(MovimientoEntidad);
                    ReactivoEntidad.cantidad_disponible -= Egreso.Cantidad;
                    Contexto.reactivos.Update(ReactivoEntidad);
                }

                await Contexto.SaveChangesAsync();
                await Transaccion.CommitAsync();
                return true;
            }
            catch
            {
                await Transaccion.RollbackAsync();
                return false;
            }
        }
    }
}
