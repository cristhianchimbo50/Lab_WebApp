using Lab_Contracts.Medicos;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Medicos
{
    public class MedicoService : IMedicoService
    {
        private readonly LabDbContext _context;

        public MedicoService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<List<MedicoDto>> GetMedicosAsync()
        {
            return await _context.medicos
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

        public async Task<MedicoDto?> GetMedicoByIdAsync(int id)
        {
            var m = await _context.medicos.FindAsync(id);
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

        public async Task<List<MedicoDto>> GetMedicosPorNombreAsync(string nombre)
        {
            return await _context.medicos
                .Where(m => m.nombre_medico.Contains(nombre))
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

        public async Task<List<MedicoDto>> GetMedicosPorEspecialidadAsync(string especialidad)
        {
            return await _context.medicos
                .Where(m => m.especialidad.Contains(especialidad))
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

        public async Task<MedicoDto?> GetMedicoPorCorreoAsync(string correo)
        {
            var m = await _context.medicos.FirstOrDefaultAsync(x => x.correo == correo);
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

        public async Task<MedicoDto> CrearMedicoAsync(MedicoDto dto)
        {
            var medico = new medico
            {
                nombre_medico = dto.NombreMedico,
                especialidad = dto.Especialidad,
                telefono = dto.Telefono,
                correo = dto.Correo,
                anulado = false
            };
            _context.medicos.Add(medico);
            await _context.SaveChangesAsync();

            dto.IdMedico = medico.id_medico;
            dto.Anulado = false;
            return dto;
        }

        public async Task<bool> EditarMedicoAsync(int id, MedicoDto dto)
        {
            var medico = await _context.medicos.FindAsync(id);
            if (medico == null) return false;

            medico.nombre_medico = dto.NombreMedico;
            medico.especialidad = dto.Especialidad;
            medico.telefono = dto.Telefono;
            medico.correo = dto.Correo;
            medico.anulado = dto.Anulado;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularMedicoAsync(int id)
        {
            var medico = await _context.medicos.FindAsync(id);
            if (medico == null) return false;

            medico.anulado = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MedicoDto>> GetMedicosPorCorreoAsync(string correo)
        {
            return await _context.medicos
                .Where(m => m.correo.Contains(correo))
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

    }
}
