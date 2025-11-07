using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.Services;
using Lab_Contracts.Pacientes;
using Lab_Contracts.Common;
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

        public async Task<List<PacienteDto>> ListarPacientesAsync()
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
                EdadPaciente = PacienteService.CalcularEdad(p.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue)),
                DireccionPaciente = p.direccion_paciente,
                CorreoElectronicoPaciente = p.correo_electronico_paciente,
                TelefonoPaciente = p.telefono_paciente,
                FechaRegistro = p.fecha_registro,
                Anulado = p.anulado ?? false,
                IdUsuario = p.id_usuario
            }).ToList();
        }

        public async Task<PacienteDto?> ObtenerDetallePacienteAsync(int idPaciente)
        {
            var entidad = await _context.pacientes
                .Include(p => p.id_usuarioNavigation)
                .FirstOrDefaultAsync(p => p.id_paciente == idPaciente);
            if (entidad == null) return null;
            return new PacienteDto
            {
                IdPaciente = entidad.id_paciente,
                CedulaPaciente = entidad.cedula_paciente,
                NombrePaciente = entidad.nombre_paciente,
                FechaNacPaciente = entidad.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue),
                EdadPaciente = PacienteService.CalcularEdad(entidad.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue)),
                DireccionPaciente = entidad.direccion_paciente,
                CorreoElectronicoPaciente = entidad.correo_electronico_paciente,
                TelefonoPaciente = entidad.telefono_paciente,
                FechaRegistro = entidad.fecha_registro,
                Anulado = entidad.anulado ?? false,
                IdUsuario = entidad.id_usuario
            };
        }

        public async Task<List<PacienteDto>?> ListarPacientesAsync(string criterio, string valor)
        {
            if (string.IsNullOrWhiteSpace(criterio) || string.IsNullOrWhiteSpace(valor))
                return new List<PacienteDto>();

            var campoLower = criterio.ToLower();
            if (campoLower == "cedula")
            {
                var entidad = await _context.pacientes.FirstOrDefaultAsync(x => x.cedula_paciente == valor);
                if (entidad == null) return new List<PacienteDto>();
                return new List<PacienteDto> { MapPaciente(entidad) };
            }
            else if (campoLower == "nombre")
            {
                return _context.pacientes.Where(p => p.nombre_paciente.Contains(valor)).Select(MapPaciente).ToList();
            }
            else if (campoLower == "correo")
            {
                return _context.pacientes.Where(p => p.correo_electronico_paciente.Contains(valor)).Select(MapPaciente).ToList();
            }

            return null;
        }

        public async Task<ResultadoPaginadoDto<PacienteDto>> ListarPacientesPaginadosAsync(PacienteFiltroDto filtro)
        {
            var query = _context.pacientes.Include(p => p.id_usuarioNavigation).AsNoTracking().AsQueryable();

            if (!(filtro.IncluirAnulados && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnulados && !filtro.IncluirVigentes)
                    query = query.Where(p => p.anulado == true);
                else if (!filtro.IncluirAnulados && filtro.IncluirVigentes)
                    query = query.Where(p => p.anulado == false || p.anulado == null);
            }

            if (!string.IsNullOrWhiteSpace(filtro.CriterioBusqueda) && !string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "cedula": query = query.Where(p => (p.cedula_paciente ?? "").ToLower().Contains(val)); break;
                    case "nombre": query = query.Where(p => (p.nombre_paciente ?? "").ToLower().Contains(val)); break;
                    case "correo": query = query.Where(p => (p.correo_electronico_paciente ?? "").ToLower().Contains(val)); break;
                }
            }

            var total = await query.CountAsync();

            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(PacienteDto.CedulaPaciente) => asc ? query.OrderBy(p => p.cedula_paciente) : query.OrderByDescending(p => p.cedula_paciente),
                nameof(PacienteDto.NombrePaciente) => asc ? query.OrderBy(p => p.nombre_paciente) : query.OrderByDescending(p => p.nombre_paciente),
                nameof(PacienteDto.EdadPaciente) => asc ? query.OrderBy(p => p.fecha_nac_paciente) : query.OrderByDescending(p => p.fecha_nac_paciente),
                nameof(PacienteDto.CorreoElectronicoPaciente) => asc ? query.OrderBy(p => p.correo_electronico_paciente) : query.OrderByDescending(p => p.correo_electronico_paciente),
                nameof(PacienteDto.TelefonoPaciente) => asc ? query.OrderBy(p => p.telefono_paciente) : query.OrderByDescending(p => p.telefono_paciente),
                _ => asc ? query.OrderBy(p => p.id_paciente) : query.OrderByDescending(p => p.id_paciente)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(p => new PacienteDto
                {
                    IdPaciente = p.id_paciente,
                    CedulaPaciente = p.cedula_paciente,
                    NombrePaciente = p.nombre_paciente,
                    FechaNacPaciente = p.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue),
                    EdadPaciente = PacienteService.CalcularEdad(p.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue)),
                    DireccionPaciente = p.direccion_paciente,
                    CorreoElectronicoPaciente = p.correo_electronico_paciente,
                    TelefonoPaciente = p.telefono_paciente,
                    FechaRegistro = p.fecha_registro,
                    Anulado = p.anulado ?? false,
                    IdUsuario = p.id_usuario
                }).ToListAsync();

            return new ResultadoPaginadoDto<PacienteDto>
            {
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }

        public async Task<(bool Exito, string Mensaje, PacienteDto? Paciente)> GuardarPacienteAsync(PacienteDto dto)
        {
            if (!ValidarCedula(dto.CedulaPaciente))
                return (false, "La cédula ingresada no es válida.", null);

            var usuarioExistente = await _context.usuarios
                .FirstOrDefaultAsync(u => u.correo_usuario == dto.CorreoElectronicoPaciente);

            int idUsuario;

            if (usuarioExistente != null)
            {
                idUsuario = usuarioExistente.id_usuario;
            }
            else
            {
                var usuario = new usuario
                {
                    correo_usuario = dto.CorreoElectronicoPaciente,
                    clave_usuario = null,
                    nombre = dto.NombrePaciente,
                    rol = "paciente",
                    activo = false,
                    fecha_creacion = DateTime.UtcNow
                };
                _context.usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                idUsuario = usuario.id_usuario;

                var randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
                var token = Convert.ToBase64String(randomBytes);
                var tokenHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));

                var tokenRegistro = new tokens_usuarios
                {
                    id_usuario = usuario.id_usuario,
                    token_hash = tokenHash,
                    tipo_token = "activacion",
                    fecha_solicitud = DateTime.UtcNow,
                    fecha_expiracion = DateTime.UtcNow.AddHours(24),
                    usado = false
                };
                _context.tokens_usuarios.Add(tokenRegistro);
                await _context.SaveChangesAsync();

                var dominio = "https://localhost:7283";
                var enlace = $"{dominio}/activar-cuenta?token={Uri.EscapeDataString(token)}";

                var asunto = "Activación de cuenta - Laboratorio Clínico La Inmaculada";
                var cuerpo = $@"
            <p>Hola <b>{dto.NombrePaciente}</b>,</p>
            <p>Se ha creado una cuenta para ti en el sistema del Laboratorio Clínico La Inmaculada.</p>
            <p>Para activar tu cuenta y establecer tu contraseña, haz clic en el siguiente enlace:</p>
            <p><a href='{enlace}' target='_blank'>Activar mi cuenta</a></p>
            <p>Este enlace estará disponible durante 24 horas.</p>";

                await _emailService.EnviarCorreoAsync(dto.CorreoElectronicoPaciente, dto.NombrePaciente, asunto, cuerpo);
            }

            if (await _context.pacientes.AnyAsync(p => p.id_usuario == idUsuario))
                return (false, "Este usuario ya está registrado como paciente.", null);

            var entidadPaciente = new paciente
            {
                cedula_paciente = dto.CedulaPaciente,
                nombre_paciente = dto.NombrePaciente,
                fecha_nac_paciente = DateOnly.FromDateTime(dto.FechaNacPaciente),
                direccion_paciente = dto.DireccionPaciente,
                correo_electronico_paciente = dto.CorreoElectronicoPaciente,
                telefono_paciente = dto.TelefonoPaciente,
                fecha_registro = DateTime.UtcNow,
                anulado = false,
                id_usuario = idUsuario
            };

            _context.pacientes.Add(entidadPaciente);
            await _context.SaveChangesAsync();

            dto.IdPaciente = entidadPaciente.id_paciente;
            dto.EdadPaciente = CalcularEdad(dto.FechaNacPaciente);

            var mensaje = usuarioExistente != null
                ? "El paciente se vinculó correctamente con su cuenta existente."
                : "Paciente registrado correctamente. Se envió el enlace de activación.";

            return (true, mensaje, dto);
        }

        public async Task<bool> GuardarPacienteAsync(int idPaciente, PacienteDto dto)
        {
            var entidad = await _context.pacientes.FindAsync(idPaciente);
            if (entidad == null) return false;

            entidad.cedula_paciente = dto.CedulaPaciente;
            entidad.nombre_paciente = dto.NombrePaciente;
            entidad.fecha_nac_paciente = DateOnly.FromDateTime(dto.FechaNacPaciente);
            entidad.direccion_paciente = dto.DireccionPaciente;
            entidad.telefono_paciente = dto.TelefonoPaciente;
            entidad.anulado = dto.Anulado;
            entidad.id_usuario = dto.IdUsuario;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularPacienteAsync(int idPaciente)
        {
            var entidad = await _context.pacientes.FindAsync(idPaciente);
            if (entidad == null) return false;

            entidad.anulado = true;
            await _context.SaveChangesAsync();
            return true;
        }

        private bool ValidarCedula(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula) || cedula.Length != 10 || !cedula.All(char.IsDigit))
                return false;

            int suma = 0;
            for (int i = 0; i < 9; i++)
            {
                int digito = int.Parse(cedula[i].ToString());
                int coef = (i % 2 == 0) ? 2 : 1;
                int producto = digito * coef;
                suma += (producto >= 10) ? (producto - 9) : producto;
            }

            int ultimoDigito = int.Parse(cedula[9].ToString());
            int digitoCalculado = (suma % 10 == 0) ? 0 : (10 - (suma % 10));
            return ultimoDigito == digitoCalculado;
        }

        private static int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        private PacienteDto MapPaciente(paciente entidadPaciente) =>
            new()
            {
                IdPaciente = entidadPaciente.id_paciente,
                CedulaPaciente = entidadPaciente.cedula_paciente,
                NombrePaciente = entidadPaciente.nombre_paciente,
                FechaNacPaciente = entidadPaciente.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue),
                EdadPaciente = PacienteService.CalcularEdad(entidadPaciente.fecha_nac_paciente.ToDateTime(TimeOnly.MinValue)),
                DireccionPaciente = entidadPaciente.direccion_paciente,
                CorreoElectronicoPaciente = entidadPaciente.correo_electronico_paciente,
                TelefonoPaciente = entidadPaciente.telefono_paciente,
                FechaRegistro = entidadPaciente.fecha_registro,
                Anulado = entidadPaciente.anulado ?? false,
                IdUsuario = entidadPaciente.id_usuario
            };
    }
}
