using Lab_Contracts.Examenes;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Examenes
{
    public class ExamenService : IExamenService
    {
        private readonly LabDbContext _context;

        public ExamenService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<List<ExamenDto>> GetExamenesAsync()
        {
            return await _context.examen
                .AsNoTracking()
                .Select(e => new ExamenDto
                {
                    IdExamen = e.id_examen,
                    NombreExamen = e.nombre_examen,
                    ValorReferencia = e.valor_referencia,
                    Unidad = e.unidad,
                    Precio = e.precio,
                    Anulado = e.anulado ?? false,
                    Estudio = e.estudio,
                    TipoMuestra = e.tipo_muestra,
                    TiempoEntrega = e.tiempo_entrega,
                    TipoExamen = e.tipo_examen,
                    Tecnica = e.tecnica,
                    TituloExamen = e.titulo_examen
                })
                .ToListAsync();
        }

        public async Task<ExamenDto?> GetExamenByIdAsync(int id)
        {
            var e = await _context.examen.AsNoTracking().FirstOrDefaultAsync(x => x.id_examen == id);
            if (e == null) return null;
            return new ExamenDto
            {
                IdExamen = e.id_examen,
                NombreExamen = e.nombre_examen,
                ValorReferencia = e.valor_referencia,
                Unidad = e.unidad,
                Precio = e.precio,
                Anulado = e.anulado ?? false,
                Estudio = e.estudio,
                TipoMuestra = e.tipo_muestra,
                TiempoEntrega = e.tiempo_entrega,
                TipoExamen = e.tipo_examen,
                Tecnica = e.tecnica,
                TituloExamen = e.titulo_examen
            };
        }

        public async Task<List<ExamenDto>> BuscarExamenesPorNombreAsync(string nombre)
        {
            return await _context.examen
                .AsNoTracking()
                .Where(e => e.nombre_examen.Contains(nombre))
                .Select(e => new ExamenDto
                {
                    IdExamen = e.id_examen,
                    NombreExamen = e.nombre_examen,
                    ValorReferencia = e.valor_referencia,
                    Unidad = e.unidad,
                    Precio = e.precio,
                    Anulado = e.anulado ?? false,
                    Estudio = e.estudio,
                    TipoMuestra = e.tipo_muestra,
                    TiempoEntrega = e.tiempo_entrega,
                    TipoExamen = e.tipo_examen,
                    Tecnica = e.tecnica,
                    TituloExamen = e.titulo_examen
                })
                .ToListAsync();
        }

        public async Task<ExamenDto> CrearExamenAsync(ExamenDto dto)
        {
            var examen = new examen
            {
                nombre_examen = dto.NombreExamen,
                valor_referencia = dto.ValorReferencia,
                unidad = dto.Unidad,
                precio = dto.Precio,
                anulado = false,
                estudio = dto.Estudio,
                tipo_muestra = dto.TipoMuestra,
                tiempo_entrega = dto.TiempoEntrega,
                tipo_examen = dto.TipoExamen,
                tecnica = dto.Tecnica,
                titulo_examen = dto.TituloExamen
            };
            _context.examen.Add(examen);
            await _context.SaveChangesAsync();

            dto.IdExamen = examen.id_examen;
            dto.Anulado = false;
            return dto;
        }

        public async Task<bool> EditarExamenAsync(int id, ExamenDto dto)
        {
            var examen = await _context.examen.FindAsync(id);
            if (examen == null) return false;

            examen.nombre_examen = dto.NombreExamen;
            examen.valor_referencia = dto.ValorReferencia;
            examen.unidad = dto.Unidad;
            examen.precio = dto.Precio;
            examen.estudio = dto.Estudio;
            examen.tipo_muestra = dto.TipoMuestra;
            examen.tiempo_entrega = dto.TiempoEntrega;
            examen.tipo_examen = dto.TipoExamen;
            examen.tecnica = dto.Tecnica;
            examen.titulo_examen = dto.TituloExamen;
            examen.anulado = dto.Anulado;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularExamenAsync(int id)
        {
            var examen = await _context.examen.FindAsync(id);
            if (examen == null) return false;

            examen.anulado = true;
            await _context.SaveChangesAsync();
            return true;
        }

        //Para examenes compuestos

        public async Task<List<ExamenDto>> ObtenerHijosDeExamenAsync(int idExamenPadre)
        {
            var hijos = await _context.examen_composicion
                .Where(ec => ec.id_examen_padre == idExamenPadre)
                .Join(_context.examen,
                    ec => ec.id_examen_hijo,
                    e => e.id_examen,
                    (ec, e) => new ExamenDto
                    {
                        IdExamen = e.id_examen,
                        NombreExamen = e.nombre_examen,
                        ValorReferencia = e.valor_referencia,
                        Unidad = e.unidad,
                        Precio = e.precio,
                        Anulado = e.anulado ?? false,
                        Estudio = e.estudio,
                        TipoMuestra = e.tipo_muestra,
                        TiempoEntrega = e.tiempo_entrega,
                        TipoExamen = e.tipo_examen,
                        Tecnica = e.tecnica,
                        TituloExamen = e.titulo_examen
                    })
                .ToListAsync();

            return hijos;
        }

        public async Task<bool> AgregarExamenHijoAsync(int idExamenPadre, int idExamenHijo)
        {
            var existe = await _context.examen_composicion
                .AnyAsync(x => x.id_examen_padre == idExamenPadre && x.id_examen_hijo == idExamenHijo);

            if (existe) return false;

            var composicion = new examen_composicion
            {
                id_examen_padre = idExamenPadre,
                id_examen_hijo = idExamenHijo
            };
            _context.examen_composicion.Add(composicion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarExamenHijoAsync(int idExamenPadre, int idExamenHijo)
        {
            var composicion = await _context.examen_composicion
                .FirstOrDefaultAsync(x => x.id_examen_padre == idExamenPadre && x.id_examen_hijo == idExamenHijo);

            if (composicion == null) return false;

            _context.examen_composicion.Remove(composicion);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
