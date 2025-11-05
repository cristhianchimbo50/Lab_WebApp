using Lab_Contracts.Medicos;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Medicos
{
    public class MedicoService : IMedicoService
    {
        private readonly LabDbContext Contexto;

        public MedicoService(LabDbContext Contexto)
        {
            this.Contexto = Contexto;
        }

        public async Task<List<MedicoDto>> ObtenerMedicosAsync()
        {
            return await Contexto.medicos
                .Select(m => new MedicoDto
                {
                    IdMedico = m.id_medico,
                    NombreMedico = m.nombre_medico,
                    Especialidad = m.especialidad,
                    Telefono = m.telefono,
                    Correo = m.correo,
                    Anulado = m.anulado ?? false
                })
                .ToListAsync();
        }

        public async Task<MedicoDto?> ObtenerMedicoPorIdAsync(int IdMedico)
        {
            var m = await Contexto.medicos.FindAsync(IdMedico);
            if (m == null) return null;
            return new MedicoDto
            {
                IdMedico = m.id_medico,
                NombreMedico = m.nombre_medico,
                Especialidad = m.especialidad,
                Telefono = m.telefono,
                Correo = m.correo,
                Anulado = m.anulado ?? false
            };
        }

        public async Task<List<MedicoDto>> ObtenerMedicosPorNombreAsync(string Nombre)
        {
            return await Contexto.medicos
                .Where(m => m.nombre_medico.Contains(Nombre))
                .Select(m => new MedicoDto
                {
                    IdMedico = m.id_medico,
                    NombreMedico = m.nombre_medico,
                    Especialidad = m.especialidad,
                    Telefono = m.telefono,
                    Correo = m.correo,
                    Anulado = m.anulado ?? false
                })
                .ToListAsync();
        }

        public async Task<List<MedicoDto>> ObtenerMedicosPorEspecialidadAsync(string Especialidad)
        {
            return await Contexto.medicos
                .Where(m => m.especialidad.Contains(Especialidad))
                .Select(m => new MedicoDto
                {
                    IdMedico = m.id_medico,
                    NombreMedico = m.nombre_medico,
                    Especialidad = m.especialidad,
                    Telefono = m.telefono,
                    Correo = m.correo,
                    Anulado = m.anulado ?? false
                })
                .ToListAsync();
        }

        public async Task<MedicoDto?> ObtenerMedicoPorCorreoAsync(string Correo)
        {
            var m = await Contexto.medicos.FirstOrDefaultAsync(x => x.correo == Correo);
            if (m == null) return null;
            return new MedicoDto
            {
                IdMedico = m.id_medico,
                NombreMedico = m.nombre_medico,
                Especialidad = m.especialidad,
                Telefono = m.telefono,
                Correo = m.correo,
                Anulado = m.anulado ?? false
            };
        }

        public async Task<MedicoDto> RegistrarMedicoAsync(MedicoDto DatosMedico)
        {
            var medico = new medico
            {
                nombre_medico = DatosMedico.NombreMedico,
                especialidad = DatosMedico.Especialidad,
                telefono = DatosMedico.Telefono,
                correo = DatosMedico.Correo,
                anulado = false
            };
            Contexto.medicos.Add(medico);
            await Contexto.SaveChangesAsync();

            DatosMedico.IdMedico = medico.id_medico;
            DatosMedico.Anulado = false;
            return DatosMedico;
        }

        public async Task<bool> EditarMedicoAsync(int IdMedico, MedicoDto DatosMedico)
        {
            var medico = await Contexto.medicos.FindAsync(IdMedico);
            if (medico == null) return false;

            medico.nombre_medico = DatosMedico.NombreMedico;
            medico.especialidad = DatosMedico.Especialidad;
            medico.telefono = DatosMedico.Telefono;
            medico.correo = DatosMedico.Correo;
            medico.anulado = DatosMedico.Anulado;

            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularMedicoAsync(int IdMedico)
        {
            var medico = await Contexto.medicos.FindAsync(IdMedico);
            if (medico == null) return false;

            medico.anulado = true;
            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<List<MedicoDto>> ObtenerMedicosPorCorreoAsync(string Correo)
        {
            return await Contexto.medicos
                .Where(m => m.correo.Contains(Correo))
                .Select(m => new MedicoDto
                {
                    IdMedico = m.id_medico,
                    NombreMedico = m.nombre_medico,
                    Especialidad = m.especialidad,
                    Correo = m.correo,
                    Telefono = m.telefono,
                    Anulado = m.anulado ?? false
                }).ToListAsync();
        }

        public async Task<List<MedicoDto>> ListarMedicosAsync()
        {
            return await Contexto.medicos
                .Where(x => x.anulado == false)
                .Select(m => new MedicoDto
                {
                    IdMedico = m.id_medico,
                    NombreMedico = m.nombre_medico,
                    Especialidad = m.especialidad,
                    Telefono = m.telefono,
                    Correo = m.correo
                })
                .ToListAsync();
        }
    }
}
