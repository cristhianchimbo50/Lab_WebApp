using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.Services;
using Lab_Contracts.Pacientes;
using Microsoft.EntityFrameworkCore;

namespace Lab_APIRest.Services.Pacientes
{
    public class PacienteService : IPacienteService
    {
        private readonly LabDbContext Contexto;
        private readonly EmailService ServicioCorreo;

        public PacienteService(LabDbContext Contexto, EmailService ServicioCorreo)
        {
            this.Contexto = Contexto;
            this.ServicioCorreo = ServicioCorreo;
        }

        public async Task<List<PacienteDto>> ObtenerPacientesAsync()
        {
            var Lista = await Contexto.pacientes
                .Include(p => p.id_usuarioNavigation)
                .ToListAsync();

            return Lista.Select(p => new PacienteDto
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
                EsContraseniaTemporal = p.id_usuarioNavigation?.es_contrasenia_temporal
            }).ToList();
        }

        public async Task<PacienteDto?> ObtenerPacientePorIdAsync(int IdPaciente)
        {
            var entidad = await Contexto.pacientes
                .Include(p => p.id_usuarioNavigation)
                .FirstOrDefaultAsync(p => p.id_paciente == IdPaciente);
            if (entidad == null) return null;
            return new PacienteDto
            {
                IdPaciente = entidad.id_paciente,
                CedulaPaciente = entidad.cedula_paciente,
                NombrePaciente = entidad.nombre_paciente,
                FechaNacPaciente = entidad.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue),
                EdadPaciente = CalcularEdad(entidad.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue)),
                DireccionPaciente = entidad.direccion_paciente,
                CorreoElectronicoPaciente = entidad.correo_electronico_paciente,
                TelefonoPaciente = entidad.telefono_paciente,
                FechaRegistro = entidad.fecha_registro,
                Anulado = entidad.anulado ?? false,
                IdUsuario = entidad.id_usuario,
                EsContraseniaTemporal = entidad.id_usuarioNavigation?.es_contrasenia_temporal
            };
        }

        public async Task<List<PacienteDto>?> BuscarPacientesAsync(string Campo, string Valor)
        {
            if (string.IsNullOrWhiteSpace(Campo) || string.IsNullOrWhiteSpace(Valor))
                return new List<PacienteDto>();

            var campoLower = Campo.ToLower();

            if (campoLower == "cedula")
            {
                var entidad = await Contexto.pacientes.FirstOrDefaultAsync(x => x.cedula_paciente == Valor);
                if (entidad == null) return new List<PacienteDto>();
                return new List<PacienteDto> { MapPaciente(entidad) };
            }
            else if (campoLower == "nombre")
            {
                return Contexto.pacientes
                    .Where(p => p.nombre_paciente.Contains(Valor))
                    .Select(MapPaciente)
                    .ToList();
            }
            else if (campoLower == "correo")
            {
                return Contexto.pacientes
                    .Where(p => p.correo_electronico_paciente.Contains(Valor))
                    .Select(MapPaciente)
                    .ToList();
            }

            return null;
        }

        public async Task<(bool Exito, string Mensaje, PacienteDto? Paciente)> RegistrarPacienteAsync(PacienteDto dto)
        {
            if (!ValidarCedula(dto.CedulaPaciente))
                return (false, "La cédula ingresada no es válida.", null);

            var existePaciente = await Contexto.pacientes
                .AnyAsync(p => p.cedula_paciente == dto.CedulaPaciente ||
                               p.correo_electronico_paciente == dto.CorreoElectronicoPaciente);
            if (existePaciente)
                return (false, "Ya existe un paciente con la misma cédula o correo.", null);

            string ContraseniaTemporal = GenerarContraseniaTemporal();
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<object>();
            string hashClave = hasher.HashPassword(null!, ContraseniaTemporal);

            var usuario = new usuario
            {
                correo_usuario = dto.CorreoElectronicoPaciente,
                clave_usuario = hashClave,
                nombre = dto.NombrePaciente,
                rol = "paciente",
                es_contrasenia_temporal = true,
                fecha_expira_temporal = DateTime.UtcNow.AddHours(48),
                activo = true,
            };

            Contexto.usuarios.Add(usuario);
            await Contexto.SaveChangesAsync();

            var entidadPaciente = new paciente
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

            Contexto.pacientes.Add(entidadPaciente);
            await Contexto.SaveChangesAsync();

            var cuerpoCorreo = $@"
        <h2>Bienvenido al Laboratorio Clínico <strong>'La Inmaculada'</strong></h2>
        <p>Estimado(a) <b>{dto.NombrePaciente}</b>, su cuenta ha sido creada exitosamente.</p>
        <p><b>Usuario:</b> {dto.CorreoElectronicoPaciente}</p>
        <p><b>Contraseña temporal:</b> {ContraseniaTemporal}</p>
        <p>Por motivos de seguridad, cambie su contraseña al iniciar sesión.</p>";

            await ServicioCorreo.EnviarCorreoAsync(
                dto.CorreoElectronicoPaciente,
                dto.NombrePaciente,
                "Credenciales de acceso - Laboratorio Clínico <strong>'La Inmaculada'</strong>",
                cuerpoCorreo
            );
            
            dto.IdPaciente = entidadPaciente.id_paciente;
            dto.EdadPaciente = CalcularEdad(dto.FechaNacPaciente);
            dto.ContraseniaTemporal = ContraseniaTemporal;

            return (true, "Paciente registrado correctamente.", dto);
        }

        public async Task<bool> EditarPacienteAsync(int IdPaciente, PacienteDto dto)
        {
            var entidad = await Contexto.pacientes.FindAsync(IdPaciente);
            if (entidad == null) return false;

            entidad.cedula_paciente = dto.CedulaPaciente;
            entidad.nombre_paciente = dto.NombrePaciente;
            entidad.fecha_nac_paciente = DateOnly.FromDateTime(dto.FechaNacPaciente);
            entidad.direccion_paciente = dto.DireccionPaciente;
            entidad.telefono_paciente = dto.TelefonoPaciente;
            entidad.anulado = dto.Anulado;
            entidad.id_usuario = dto.IdUsuario;

            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularPacienteAsync(int IdPaciente)
        {
            var entidad = await Contexto.pacientes.FindAsync(IdPaciente);
            if (entidad == null) return false;

            entidad.anulado = true;
            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Exito, string Mensaje, string? NuevaTemporal)> ReenviarCredencialesTemporalesAsync(int IdPaciente)
        {
            var entidad = await Contexto.pacientes.FirstOrDefaultAsync(p => p.id_paciente == IdPaciente);
            if (entidad == null)
                return (false, "Paciente no encontrado.", null);

            var usuario = await Contexto.usuarios.FirstOrDefaultAsync(u => u.id_usuario == entidad.id_usuario);
            if (usuario == null)
                return (false, "Usuario asociado no encontrado.", null);

            var NuevaTemp = GenerarContraseniaTemporal();
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<object>();

            usuario.clave_usuario = hasher.HashPassword(null!, NuevaTemp);
            usuario.es_contrasenia_temporal = true;
            usuario.fecha_expira_temporal = DateTime.UtcNow.AddHours(48);

            await Contexto.SaveChangesAsync();

            var cuerpo = $@"
                <h2>Reinicio de contraseña temporal</h2>
                <p>Estimado(a) <b>{entidad.nombre_paciente}</b>, se ha generado una nueva contraseña temporal.</p>
                <p><b>Usuario:</b> {entidad.correo_electronico_paciente}</p>
                <p><b>Nueva contraseña temporal:</b> <span style='color:#0d6efd'>{NuevaTemp}</span></p>
                <p>Esta contraseña expirará en <b>48 horas</b>.</p>";

            await ServicioCorreo.EnviarCorreoAsync(
                entidad.correo_electronico_paciente,
                entidad.nombre_paciente,
                "Nueva contraseña temporal - Laboratorio Clínico <strong>'La Inmaculada'</strong>",
                cuerpo
            );

            return (true, "Se envió una nueva contraseña temporal al correo del paciente.", NuevaTemp);
        }

        private bool ValidarCedula(string Cedula)
        {
            if (string.IsNullOrWhiteSpace(Cedula) || Cedula.Length != 10 || !Cedula.All(char.IsDigit))
                return false;

            int suma = 0;
            for (int i = 0; i < 9; i++)
            {
                int digito = int.Parse(Cedula[i].ToString());
                int coef = (i % 2 == 0) ? 2 : 1;
                int producto = digito * coef;
                suma += (producto >= 10) ? (producto - 9) : producto;
            }

            int ultimoDigito = int.Parse(Cedula[9].ToString());
            int digitoCalculado = (suma % 10 == 0) ? 0 : (10 - (suma % 10));
            return ultimoDigito == digitoCalculado;
        }

        private string GenerarContraseniaTemporal()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private int CalcularEdad(DateTime FechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - FechaNacimiento.Year;
            if (FechaNacimiento > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        private PacienteDto MapPaciente(paciente entidadPaciente) =>
        new()
        {
            IdPaciente = entidadPaciente.id_paciente,
            CedulaPaciente = entidadPaciente.cedula_paciente,
            NombrePaciente = entidadPaciente.nombre_paciente,
            FechaNacPaciente = entidadPaciente.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue),
            EdadPaciente = CalcularEdad(entidadPaciente.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue)), 
            DireccionPaciente = entidadPaciente.direccion_paciente,
            CorreoElectronicoPaciente = entidadPaciente.correo_electronico_paciente,
            TelefonoPaciente = entidadPaciente.telefono_paciente,
            FechaRegistro = entidadPaciente.fecha_registro,
            Anulado = entidadPaciente.anulado ?? false,
            IdUsuario = entidadPaciente.id_usuario
        };

    }
}
