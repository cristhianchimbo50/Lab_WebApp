using Lab_Contracts.Examenes;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Lab_APIRest.Services.Examenes
{
    public class ExamenService : IExamenService
    {
        private readonly LabDbContext _context;

        public ExamenService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<List<ExamenDto>> ListarExamenesAsync()
        {
            return await _context.examen.AsNoTracking().Select(e => Map(e)).ToListAsync();
        }

        public async Task<ExamenDto?> ObtenerDetalleExamenAsync(int idExamen)
        {
            var entidad = await _context.examen.AsNoTracking().FirstOrDefaultAsync(x => x.id_examen == idExamen);
            return entidad == null ? null : Map(entidad);
        }

        public async Task<List<ExamenDto>> ListarExamenesPorNombreAsync(string nombre)
        {
            return await _context.examen.AsNoTracking()
                .Where(e => e.nombre_examen!.Contains(nombre))
                .Select(e => Map(e))
                .ToListAsync();
        }

        public async Task<ExamenDto> GuardarExamenAsync(ExamenDto datosExamen)
        {
            var entidad = new examen
            {
                nombre_examen = datosExamen.NombreExamen,
                valor_referencia = datosExamen.ValorReferencia,
                unidad = datosExamen.Unidad,
                precio = datosExamen.Precio,
                anulado = false,
                estudio = datosExamen.Estudio,
                tipo_muestra = datosExamen.TipoMuestra,
                tiempo_entrega = datosExamen.TiempoEntrega,
                tipo_examen = datosExamen.TipoExamen,
                tecnica = datosExamen.Tecnica,
                titulo_examen = datosExamen.TituloExamen
            };
            _context.examen.Add(entidad);
            await _context.SaveChangesAsync();
            datosExamen.IdExamen = entidad.id_examen;
            datosExamen.Anulado = false;
            return datosExamen;
        }

        public async Task<bool> GuardarExamenAsync(int idExamen, ExamenDto datosExamen)
        {
            var entidad = await _context.examen.FindAsync(idExamen);
            if (entidad == null) return false;
            entidad.nombre_examen = datosExamen.NombreExamen;
            entidad.valor_referencia = datosExamen.ValorReferencia;
            entidad.unidad = datosExamen.Unidad;
            entidad.precio = datosExamen.Precio;
            entidad.estudio = datosExamen.Estudio;
            entidad.tipo_muestra = datosExamen.TipoMuestra;
            entidad.tiempo_entrega = datosExamen.TiempoEntrega;
            entidad.tipo_examen = datosExamen.TipoExamen;
            entidad.tecnica = datosExamen.Tecnica;
            entidad.titulo_examen = datosExamen.TituloExamen;
            entidad.anulado = datosExamen.Anulado;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularExamenAsync(int idExamen)
        {
            var entidad = await _context.examen.FindAsync(idExamen);
            if (entidad == null) return false;
            entidad.anulado = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ExamenDto>> ListarExamenesHijosAsync(int idExamenPadre)
        {
            var hijos = await _context.examen_composicion
                .Where(ec => ec.id_examen_padre == idExamenPadre)
                .Join(_context.examen,
                    ec => ec.id_examen_hijo,
                    e => e.id_examen,
                    (ec, e) => e)
                .ToListAsync();
            return hijos.Select(Map).ToList();
        }

        public async Task<bool> AsignarExamenHijoAsync(int idExamenPadre, int idExamenHijo)
        {
            var existe = await _context.examen_composicion.AnyAsync(x => x.id_examen_padre == idExamenPadre && x.id_examen_hijo == idExamenHijo);
            if (existe) return false;
            var composicion = new examen_composicion { id_examen_padre = idExamenPadre, id_examen_hijo = idExamenHijo };
            _context.examen_composicion.Add(composicion);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarExamenHijoAsync(int idExamenPadre, int idExamenHijo)
        {
            var composicion = await _context.examen_composicion.FirstOrDefaultAsync(x => x.id_examen_padre == idExamenPadre && x.id_examen_hijo == idExamenHijo);
            if (composicion == null) return false;
            _context.examen_composicion.Remove(composicion);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ExamenDto Map(examen e) => new()
        {
            IdExamen = e.id_examen,
            NombreExamen = e.nombre_examen ?? string.Empty,
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
}
