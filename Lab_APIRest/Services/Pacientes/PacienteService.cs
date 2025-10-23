using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.Services;
using Lab_Contracts.Pacientes;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Pacientes
{
    public class PacienteService : IPacienteService
    {
        private readonly LabDbContext _context;
        private readonly EmailService _emailService;

        public PacienteService(LabDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<List<PacienteDto>> GetPacientesAsync()
        {
            var lista = await _context.pacientes
                .Include(p => p.id_usuarioNavigation)
                .ToListAsync();

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
                IdUsuario = p.id_usuario,
                EsContraseñaTemporal = p.id_usuarioNavigation?.es_contraseña_temporal
            }).ToList();
        }


        public async Task<PacienteDto?> GetPacienteByIdAsync(int id)
        {
            var p = await _context.pacientes
                .Include(p => p.id_usuarioNavigation)
                .FirstOrDefaultAsync(p => p.id_paciente == id);
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
                IdUsuario = p.id_usuario,
                EsContraseñaTemporal = p.id_usuarioNavigation?.es_contraseña_temporal
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
                IdUsuario = p.id_usuario,
                EsContraseñaTemporal = p.id_usuarioNavigation?.es_contraseña_temporal
            };
        }

        public async Task<PacienteDto> CrearPacienteAsync(PacienteDto dto, int usuarioId)
        {
            var existePaciente = await _context.pacientes
                .AnyAsync(p => p.cedula_paciente == dto.CedulaPaciente ||
                               p.correo_electronico_paciente == dto.CorreoElectronicoPaciente);
            if (existePaciente)
                throw new InvalidOperationException("Ya existe un paciente con la misma cédula o correo.");

            string contraseñaTemporal = GenerarContraseñaTemporal();

            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<object>();
            string hashClave = passwordHasher.HashPassword(null!, contraseñaTemporal);

            var usuario = new usuario
            {
                correo_usuario = dto.CorreoElectronicoPaciente,
                clave_usuario = hashClave,
                nombre = dto.NombrePaciente,
                rol = "paciente",
                es_contraseña_temporal = true,
                fecha_expira_temporal = DateTime.UtcNow.AddHours(48),
                estado_registro = true,
                estado = "ACTIVO"
            };


            _context.usuarios.Add(usuario);
            await _context.SaveChangesAsync();

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
                id_usuario = usuario.id_usuario
            };

            _context.pacientes.Add(paciente);
            await _context.SaveChangesAsync();

            string cuerpoCorreo = $@"
        <h2>Bienvenido al Laboratorio Clínico</h2>
        <p>Estimado(a) <b>{dto.NombrePaciente}</b>, su cuenta ha sido creada exitosamente.</p>
        <p><b>Usuario:</b> {dto.CorreoElectronicoPaciente}</p>
        <p><b>Contraseña temporal:</b> {contraseñaTemporal}</p>
        <p>Por motivos de seguridad, cambie su contraseña al iniciar sesión.</p>
        <br/>
        <small>No responda a este correo. Mensaje generado automáticamente por el sistema.</small>";

            await _emailService.EnviarCorreoAsync(
                dto.CorreoElectronicoPaciente,
                dto.NombrePaciente,
                "Credenciales de acceso - Laboratorio Clínico",
                cuerpoCorreo
            );

            dto.IdPaciente = paciente.id_paciente;
            dto.FechaRegistro = paciente.fecha_registro;
            dto.Anulado = false;
            dto.IdUsuario = usuario.id_usuario;
            dto.EdadPaciente = CalcularEdad(dto.FechaNacPaciente);
            dto.ContraseñaTemporal = contraseñaTemporal;

            return dto;
        }

        private string GenerarContraseñaTemporal()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<bool> EditarPacienteAsync(int id, PacienteDto dto)
        {
            var paciente = await _context.pacientes.FindAsync(id);
            if (paciente == null) return false;

            paciente.cedula_paciente = dto.CedulaPaciente;
            paciente.nombre_paciente = dto.NombrePaciente;
            paciente.fecha_nac_paciente = DateOnly.FromDateTime(dto.FechaNacPaciente);
            paciente.direccion_paciente = dto.DireccionPaciente;
            paciente.telefono_paciente = dto.TelefonoPaciente;
            paciente.anulado = dto.Anulado;
            paciente.id_usuario = dto.IdUsuario;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Exito, string Mensaje, string? NuevaTemporal)> ReenviarCredencialesTemporalesAsync(int idPaciente)
        {
            var paciente = await _context.pacientes.FirstOrDefaultAsync(p => p.id_paciente == idPaciente);
            if (paciente == null)
                return (false, "Paciente no encontrado.", null);

            var usuario = await _context.usuarios.FirstOrDefaultAsync(u => u.id_usuario == paciente.id_usuario);
            if (usuario == null)
                return (false, "Usuario asociado no encontrado.", null);

            var nuevaTemp = GenerarContraseñaTemporal();
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<object>();

            usuario.clave_usuario = hasher.HashPassword(null!, nuevaTemp);
            usuario.es_contraseña_temporal = true;

            usuario.fecha_expira_temporal = DateTime.UtcNow.AddHours(48);

            await _context.SaveChangesAsync();

            // ✉️ Enviar correo al paciente
            var cuerpo = $@"
        <h2>Reinicio de contraseña temporal</h2>
        <p>Estimado(a) <b>{paciente.nombre_paciente}</b>, se ha generado una nueva contraseña temporal.</p>
        <p><b>Usuario:</b> {paciente.correo_electronico_paciente}</p>
        <p><b>Nueva contraseña temporal:</b> <span style='font-size:1.1em;color:#0d6efd'>{nuevaTemp}</span></p>
        <p>Esta contraseña expirará en <b>48 horas</b>.</p>
        <p>Por seguridad, deberá cambiarla al iniciar sesión.</p>
        <br/>
        <p>Saludos,<br/><b>Laboratorio Clínico La Inmaculada</b></p>";

            await _emailService.EnviarCorreoAsync(
                paciente.correo_electronico_paciente,
                paciente.nombre_paciente,
                "Nueva contraseña temporal - Laboratorio Clínico",
                cuerpo
            );

            return (true, "Se envió una nueva contraseña temporal al correo del paciente.", nuevaTemp);
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
                .Include(p => p.id_usuarioNavigation)
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
                    IdUsuario = p.id_usuario,
                    EsContraseñaTemporal = p.id_usuarioNavigation.es_contraseña_temporal
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
                    IdUsuario = p.id_usuario,
                    EsContraseñaTemporal = p.id_usuarioNavigation.es_contraseña_temporal
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
