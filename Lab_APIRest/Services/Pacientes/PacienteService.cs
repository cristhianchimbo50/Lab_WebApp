using Lab_Contracts.Pacientes;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Pacientes
{
    public class PacienteService : IPacienteService
    {
        private readonly LabDbContext _context;

        public PacienteService(LabDbContext context)
        {
            _context = context;
        }

        public async Task<List<PacienteDto>> GetPacientesAsync()
        {
            var lista = await _context.pacientes.ToListAsync();
            return lista.Select(p => new PacienteDto
            {
                IdPaciente = p.id_paciente,
                CedulaPaciente = p.cedula_paciente,
                NombrePaciente = p.nombre_paciente,
                FechaNacPaciente = p.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue),
                EdadPaciente = CalcularEdad(p.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue)),
                DireccionPaciente = p.direccion_paciente,
                CorreoElectronicoPaciente = p.correo_electronico_paciente,
                TelefonoPaciente = p.telefono_paciente,
                FechaRegistro = p.fecha_registro,
                Anulado = p.anulado ?? false,
                IdUsuario = p.id_usuario
            }).ToList();
        }

        public async Task<PacienteDto?> GetPacienteByIdAsync(int id)
        {
            var p = await _context.pacientes.FindAsync(id);
            if (p == null) return null;
            return new PacienteDto
            {
                IdPaciente = p.id_paciente,
                CedulaPaciente = p.cedula_paciente,
                NombrePaciente = p.nombre_paciente,
                FechaNacPaciente = p.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue),
                EdadPaciente = CalcularEdad(p.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue)),
                DireccionPaciente = p.direccion_paciente,
                CorreoElectronicoPaciente = p.correo_electronico_paciente,
                TelefonoPaciente = p.telefono_paciente,
                FechaRegistro = p.fecha_registro,
                Anulado = p.anulado ?? false,
                IdUsuario = p.id_usuario
            };
        }

        public async Task<PacienteDto?> GetPacienteByCedulaAsync(string cedula)
        {
            var p = await _context.pacientes.FirstOrDefaultAsync(x => x.cedula_paciente == cedula);
            if (p == null) return null;
            return new PacienteDto
            {
                IdPaciente = p.id_paciente,
                CedulaPaciente = p.cedula_paciente,
                NombrePaciente = p.nombre_paciente,
                FechaNacPaciente = p.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue),
                EdadPaciente = CalcularEdad(p.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue)),
                DireccionPaciente = p.direccion_paciente,
                CorreoElectronicoPaciente = p.correo_electronico_paciente,
                TelefonoPaciente = p.telefono_paciente,
                FechaRegistro = p.fecha_registro,
                Anulado = p.anulado ?? false,
                IdUsuario = p.id_usuario
            };
        }

        public async Task<PacienteDto> CrearPacienteAsync(PacienteDto dto, int usuarioId)
        {
            var paciente = new paciente
            {
                cedula_paciente = dto.CedulaPaciente,
                nombre_paciente = dto.NombrePaciente,
                fecha_nac_paciente = DateOnly.FromDateTime(dto.FechaNacPaciente),
                direccion_paciente = dto.DireccionPaciente,
                correo_electronico_paciente = dto.CorreoElectronicoPaciente,
                telefono_paciente = dto.TelefonoPaciente,
                fecha_registro = DateTime.Now,
                anulado = false,
                id_usuario = usuarioId
            };

            _context.pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            dto.IdPaciente = paciente.id_paciente;
            dto.FechaRegistro = paciente.fecha_registro;
            dto.Anulado = false;
            dto.IdUsuario = usuarioId;
            dto.EdadPaciente = CalcularEdad(dto.FechaNacPaciente);
            return dto;
        }

        public async Task<bool> EditarPacienteAsync(int id, PacienteDto dto)
        {
            var paciente = await _context.pacientes.FindAsync(id);
            if (paciente == null) return false;

            paciente.cedula_paciente = dto.CedulaPaciente;
            paciente.nombre_paciente = dto.NombrePaciente;
            paciente.fecha_nac_paciente = DateOnly.FromDateTime(dto.FechaNacPaciente);
            paciente.direccion_paciente = dto.DireccionPaciente;
            paciente.correo_electronico_paciente = dto.CorreoElectronicoPaciente;
            paciente.telefono_paciente = dto.TelefonoPaciente;
            paciente.anulado = dto.Anulado;
            paciente.id_usuario = dto.IdUsuario;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularPacienteAsync(int id)
        {
            var paciente = await _context.pacientes.FindAsync(id);
            if (paciente == null) return false;

            paciente.anulado = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<PacienteDto>> GetPacientesPorNombreAsync(string nombre)
        {
            return await _context.pacientes
                .Where(p => p.nombre_paciente.Contains(nombre))
                .Select(p => new PacienteDto
                {
                    IdPaciente = p.id_paciente,
                    CedulaPaciente = p.cedula_paciente,
                    NombrePaciente = p.nombre_paciente,
                    FechaNacPaciente = p.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue),
                    DireccionPaciente = p.direccion_paciente,
                    CorreoElectronicoPaciente = p.correo_electronico_paciente,
                    TelefonoPaciente = p.telefono_paciente,
                    FechaRegistro = p.fecha_registro,
                    Anulado = p.anulado ?? false,
                    IdUsuario = p.id_usuario
                }).ToListAsync();
        }

        public async Task<List<PacienteDto>> GetPacientesPorCorreoAsync(string correo)
        {
            return await _context.pacientes
                .Where(p => p.correo_electronico_paciente.Contains(correo))
                .Select(p => new PacienteDto
                {
                    IdPaciente = p.id_paciente,
                    CedulaPaciente = p.cedula_paciente,
                    NombrePaciente = p.nombre_paciente,
                    FechaNacPaciente = p.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue),
                    DireccionPaciente = p.direccion_paciente,
                    CorreoElectronicoPaciente = p.correo_electronico_paciente,
                    TelefonoPaciente = p.telefono_paciente,
                    FechaRegistro = p.fecha_registro,
                    Anulado = p.anulado ?? false,
                    IdUsuario = p.id_usuario
                }).ToListAsync();
        }


        private int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento > hoy.AddYears(-edad)) edad--;
            return edad;
        }
    }
}
