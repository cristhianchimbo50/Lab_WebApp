using Lab_Contracts.Examenes;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Examenes
{
    public class ExamenService : IExamenService
    {
        private readonly LabDbContext Contexto;

        public ExamenService(LabDbContext Contexto)
        {
            this.Contexto = Contexto;
        }

        public async Task<List<ExamenDto>> ObtenerExamenesAsync()
        {
            return await Contexto.examen
                .AsNoTracking()
                .Select(entidadExamen => new ExamenDto
                {
                    IdExamen = entidadExamen.id_examen,
                    NombreExamen = entidadExamen.nombre_examen,
                    ValorReferencia = entidadExamen.valor_referencia,
                    Unidad = entidadExamen.unidad,
                    Precio = entidadExamen.precio,
                    Anulado = entidadExamen.anulado ?? false,
                    Estudio = entidadExamen.estudio,
                    TipoMuestra = entidadExamen.tipo_muestra,
                    TiempoEntrega = entidadExamen.tiempo_entrega,
                    TipoExamen = entidadExamen.tipo_examen,
                    Tecnica = entidadExamen.tecnica,
                    TituloExamen = entidadExamen.titulo_examen
                })
                .ToListAsync();
        }

        public async Task<ExamenDto?> ObtenerExamenPorIdAsync(int IdExamen)
        {
            var entidadExamen = await Contexto.examen.AsNoTracking().FirstOrDefaultAsync(x => x.id_examen == IdExamen);
            if (entidadExamen == null) return null;
            return new ExamenDto
            {
                IdExamen = entidadExamen.id_examen,
                NombreExamen = entidadExamen.nombre_examen,
                ValorReferencia = entidadExamen.valor_referencia,
                Unidad = entidadExamen.unidad,
                Precio = entidadExamen.precio,
                Anulado = entidadExamen.anulado ?? false,
                Estudio = entidadExamen.estudio,
                TipoMuestra = entidadExamen.tipo_muestra,
                TiempoEntrega = entidadExamen.tiempo_entrega,
                TipoExamen = entidadExamen.tipo_examen,
                Tecnica = entidadExamen.tecnica,
                TituloExamen = entidadExamen.titulo_examen
            };
        }

        public async Task<List<ExamenDto>> BuscarExamenesPorNombreAsync(string Nombre)
        {
            return await Contexto.examen
                .AsNoTracking()
                .Where(entidadExamen => entidadExamen.nombre_examen.Contains(Nombre))
                .Select(entidadExamen => new ExamenDto
                {
                    IdExamen = entidadExamen.id_examen,
                    NombreExamen = entidadExamen.nombre_examen,
                    ValorReferencia = entidadExamen.valor_referencia,
                    Unidad = entidadExamen.unidad,
                    Precio = entidadExamen.precio,
                    Anulado = entidadExamen.anulado ?? false,
                    Estudio = entidadExamen.estudio,
                    TipoMuestra = entidadExamen.tipo_muestra,
                    TiempoEntrega = entidadExamen.tiempo_entrega,
                    TipoExamen = entidadExamen.tipo_examen,
                    Tecnica = entidadExamen.tecnica,
                    TituloExamen = entidadExamen.titulo_examen
                })
                .ToListAsync();
        }

        public async Task<ExamenDto> RegistrarExamenAsync(ExamenDto DatosExamen)
        {
            var entidad = new examen
            {
                nombre_examen = DatosExamen.NombreExamen,
                valor_referencia = DatosExamen.ValorReferencia,
                unidad = DatosExamen.Unidad,
                precio = DatosExamen.Precio,
                anulado = false,
                estudio = DatosExamen.Estudio,
                tipo_muestra = DatosExamen.TipoMuestra,
                tiempo_entrega = DatosExamen.TiempoEntrega,
                tipo_examen = DatosExamen.TipoExamen,
                tecnica = DatosExamen.Tecnica,
                titulo_examen = DatosExamen.TituloExamen
            };
            Contexto.examen.Add(entidad);
            await Contexto.SaveChangesAsync();

            DatosExamen.IdExamen = entidad.id_examen;
            DatosExamen.Anulado = false;
            return DatosExamen;
        }

        public async Task<bool> EditarExamenAsync(int IdExamen, ExamenDto DatosExamen)
        {
            var entidad = await Contexto.examen.FindAsync(IdExamen);
            if (entidad == null) return false;

            entidad.nombre_examen = DatosExamen.NombreExamen;
            entidad.valor_referencia = DatosExamen.ValorReferencia;
            entidad.unidad = DatosExamen.Unidad;
            entidad.precio = DatosExamen.Precio;
            entidad.estudio = DatosExamen.Estudio;
            entidad.tipo_muestra = DatosExamen.TipoMuestra;
            entidad.tiempo_entrega = DatosExamen.TiempoEntrega;
            entidad.tipo_examen = DatosExamen.TipoExamen;
            entidad.tecnica = DatosExamen.Tecnica;
            entidad.titulo_examen = DatosExamen.TituloExamen;
            entidad.anulado = DatosExamen.Anulado;

            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularExamenAsync(int IdExamen)
        {
            var entidad = await Contexto.examen.FindAsync(IdExamen);
            if (entidad == null) return false;

            entidad.anulado = true;
            await Contexto.SaveChangesAsync();
            return true;
        }

        //Para examenes compuestos

        public async Task<List<ExamenDto>> ObtenerHijosDeExamenAsync(int IdExamenPadre)
        {
            var hijos = await Contexto.examen_composicion
                .Where(ec => ec.id_examen_padre == IdExamenPadre)
                .Join(Contexto.examen,
                    ec => ec.id_examen_hijo,
                    e => e.id_examen,
                    (ec, entidadExamen) => new ExamenDto
                    {
                        IdExamen = entidadExamen.id_examen,
                        NombreExamen = entidadExamen.nombre_examen,
                        ValorReferencia = entidadExamen.valor_referencia,
                        Unidad = entidadExamen.unidad,
                        Precio = entidadExamen.precio,
                        Anulado = entidadExamen.anulado ?? false,
                        Estudio = entidadExamen.estudio,
                        TipoMuestra = entidadExamen.tipo_muestra,
                        TiempoEntrega = entidadExamen.tiempo_entrega,
                        TipoExamen = entidadExamen.tipo_examen,
                        Tecnica = entidadExamen.tecnica,
                        TituloExamen = entidadExamen.titulo_examen
                    })
                .ToListAsync();

            return hijos;
        }

        public async Task<bool> AgregarExamenHijoAsync(int IdExamenPadre, int IdExamenHijo)
        {
            var existe = await Contexto.examen_composicion
                .AnyAsync(x => x.id_examen_padre == IdExamenPadre && x.id_examen_hijo == IdExamenHijo);

            if (existe) return false;

            var composicion = new examen_composicion
            {
                id_examen_padre = IdExamenPadre,
                id_examen_hijo = IdExamenHijo
            };
            Contexto.examen_composicion.Add(composicion);
            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarExamenHijoAsync(int IdExamenPadre, int IdExamenHijo)
        {
            var composicion = await Contexto.examen_composicion
                .FirstOrDefaultAsync(x => x.id_examen_padre == IdExamenPadre && x.id_examen_hijo == IdExamenHijo);

            if (composicion == null) return false;

            Contexto.examen_composicion.Remove(composicion);
            await Contexto.SaveChangesAsync();
            return true;
        }

    }
}
