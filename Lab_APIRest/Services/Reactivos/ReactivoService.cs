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
    }
}
