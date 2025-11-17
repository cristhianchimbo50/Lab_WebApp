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

        private static PacienteDto MapPaciente(Paciente entidadPaciente) => new()
        {
            IdPaciente = entidadPaciente.IdPaciente,
            CedulaPaciente = entidadPaciente.CedulaPaciente,
            NombrePaciente = entidadPaciente.NombrePaciente,
            FechaNacPaciente = entidadPaciente.FechaNacPaciente.ToDateTime(TimeOnly.MinValue),
            EdadPaciente = CalcularEdad(entidadPaciente.FechaNacPaciente.ToDateTime(TimeOnly.MinValue)),
            DireccionPaciente = entidadPaciente.DireccionPaciente ?? string.Empty,
            CorreoElectronicoPaciente = entidadPaciente.CorreoElectronicoPaciente ?? string.Empty,
            TelefonoPaciente = entidadPaciente.TelefonoPaciente ?? string.Empty,
            FechaRegistro = entidadPaciente.FechaCreacion,
            Anulado = !entidadPaciente.Activo,
            IdUsuario = entidadPaciente.IdUsuario
        };

        public async Task<List<PacienteDto>> ListarPacientesAsync()
        {
            var lista = await _context.Paciente.Include(p => p.IdUsuarioNavigation).ToListAsync();
            return lista.Select(MapPaciente).ToList();
        }

        public async Task<PacienteDto?> ObtenerDetallePacienteAsync(int idPaciente)
        {
            var entidadPaciente = await _context.Paciente.Include(p => p.IdUsuarioNavigation).FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);
            return entidadPaciente == null ? null : MapPaciente(entidadPaciente);
        }

        public async Task<List<PacienteDto>?> ListarPacientesAsync(string criterio, string valor)
        {
            if (string.IsNullOrWhiteSpace(criterio) || string.IsNullOrWhiteSpace(valor))
                return new List<PacienteDto>();

            var campoLower = criterio.ToLower();
            IQueryable<Paciente> query = _context.Paciente;
            switch (campoLower)
            {
                case "cedula":
                    var porCedula = await query.FirstOrDefaultAsync(x => x.CedulaPaciente == valor);
                    return porCedula == null ? new List<PacienteDto>() : new List<PacienteDto> { MapPaciente(porCedula) };
                case "nombre":
                    return await query.Where(p => p.NombrePaciente.Contains(valor)).Select(p => MapPaciente(p)).ToListAsync();
                case "correo":
                    return await query.Where(p => p.CorreoElectronicoPaciente != null && p.CorreoElectronicoPaciente.Contains(valor)).Select(p => MapPaciente(p)).ToListAsync();
                default:
                    return new List<PacienteDto>();
            }
        }

        public async Task<ResultadoPaginadoDto<PacienteDto>> ListarPacientesPaginadosAsync(PacienteFiltroDto filtro)
        {
            var query = _context.Paciente.Include(p => p.IdUsuarioNavigation).AsNoTracking().AsQueryable();

            if (!(filtro.IncluirAnulados && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnulados && !filtro.IncluirVigentes)
                    query = query.Where(p => p.Activo == false);
                else if (!filtro.IncluirAnulados && filtro.IncluirVigentes)
                    query = query.Where(p => p.Activo == true);
            }

            if (!string.IsNullOrWhiteSpace(filtro.CriterioBusqueda) && !string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "cedula": query = query.Where(p => (p.CedulaPaciente ?? "").ToLower().Contains(val)); break;
                    case "nombre": query = query.Where(p => (p.NombrePaciente ?? "").ToLower().Contains(val)); break;
                    case "correo": query = query.Where(p => (p.CorreoElectronicoPaciente ?? "").ToLower().Contains(val)); break;
                }
            }

            var total = await query.CountAsync();

            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(PacienteDto.CedulaPaciente) => asc ? query.OrderBy(p => p.CedulaPaciente) : query.OrderByDescending(p => p.CedulaPaciente),
                nameof(PacienteDto.NombrePaciente) => asc ? query.OrderBy(p => p.NombrePaciente) : query.OrderByDescending(p => p.NombrePaciente),
                nameof(PacienteDto.EdadPaciente) => asc ? query.OrderBy(p => p.FechaNacPaciente) : query.OrderByDescending(p => p.FechaNacPaciente),
                nameof(PacienteDto.CorreoElectronicoPaciente) => asc ? query.OrderBy(p => p.CorreoElectronicoPaciente) : query.OrderByDescending(p => p.CorreoElectronicoPaciente),
                nameof(PacienteDto.TelefonoPaciente) => asc ? query.OrderBy(p => p.TelefonoPaciente) : query.OrderByDescending(p => p.TelefonoPaciente),
                _ => asc ? query.OrderBy(p => p.IdPaciente) : query.OrderByDescending(p => p.IdPaciente)
            };

            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);

            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(p => MapPaciente(p))
                .ToListAsync();

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

            var usuarioExistente = await _context.Usuario
                .FirstOrDefaultAsync(u => u.CorreoUsuario == dto.CorreoElectronicoPaciente);

            int idUsuario;

            if (usuarioExistente != null)
            {
                idUsuario = usuarioExistente.IdUsuario;
            }
            else
            {
                var usuario = new Usuario
                {
                    CorreoUsuario = dto.CorreoElectronicoPaciente,
                    ClaveUsuario = null,
                    Nombre = dto.NombrePaciente,
                    Rol = "paciente",
                    Activo = false,
                    FechaCreacion = DateTime.UtcNow
                };
                _context.Usuario.Add(usuario);
                await _context.SaveChangesAsync();

                idUsuario = usuario.IdUsuario;

                var randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
                var token = Convert.ToBase64String(randomBytes);
                var tokenHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));

                var tokenRegistro = new TokensUsuarios
                {
                    IdUsuario = usuario.IdUsuario,
                    TokenHash = tokenHash,
                    TipoToken = "activacion",
                    FechaSolicitud = DateTime.UtcNow,
                    FechaExpiracion = DateTime.UtcNow.AddHours(24),
                    Usado = false
                };
                _context.TokensUsuarios.Add(tokenRegistro);
                await _context.SaveChangesAsync();

                var dominio = "https://localhost:7283";
                var enlace = $"{dominio}/activar-cuenta?token={Uri.EscapeDataString(token)}";

                var asunto = "Activación de cuenta - Laboratorio Clínico La Inmaculada";
                var cuerpo = $@"\n            <p>Hola <b>{dto.NombrePaciente}</b>,</p>\n            <p>Se ha creado una cuenta para ti en el sistema del Laboratorio Clínico La Inmaculada.</p>\n            <p>Para activar tu cuenta y establecer tu contraseña, haz clic en el siguiente enlace:</p>\n            <p><a href='{enlace}' target='_blank'>Activar mi cuenta</a></p>\n            <p>Este enlace estará disponible durante 24 horas.</p>";

                await _emailService.EnviarCorreoAsync(dto.CorreoElectronicoPaciente, dto.NombrePaciente, asunto, cuerpo);
            }

            if (await _context.Paciente.AnyAsync(p => p.IdUsuario == idUsuario))
                return (false, "Este usuario ya está registrado como paciente.", null);

            var entidadPaciente = new Paciente
            {
                CedulaPaciente = dto.CedulaPaciente,
                NombrePaciente = dto.NombrePaciente,
                FechaNacPaciente = DateOnly.FromDateTime(dto.FechaNacPaciente),
                DireccionPaciente = dto.DireccionPaciente,
                CorreoElectronicoPaciente = dto.CorreoElectronicoPaciente,
                TelefonoPaciente = dto.TelefonoPaciente,
                FechaCreacion = DateTime.UtcNow,
                Activo = true,
                IdUsuario = idUsuario
            };

            _context.Paciente.Add(entidadPaciente);
            await _context.SaveChangesAsync();

            dto.IdPaciente = entidadPaciente.IdPaciente;
            dto.EdadPaciente = CalcularEdad(dto.FechaNacPaciente);
            dto.FechaRegistro = entidadPaciente.FechaCreacion;
            dto.Anulado = false;
            dto.IdUsuario = idUsuario;

            var mensaje = usuarioExistente != null
                ? "El paciente se vinculó correctamente con su cuenta existente."
                : "Paciente registrado correctamente. Se envió el enlace de activación.";

            return (true, mensaje, dto);
        }

        public async Task<bool> GuardarPacienteAsync(int idPaciente, PacienteDto dto)
        {
            var entidadPaciente = await _context.Paciente.FindAsync(idPaciente);
            if (entidadPaciente == null) return false;

            entidadPaciente.CedulaPaciente = dto.CedulaPaciente;
            entidadPaciente.NombrePaciente = dto.NombrePaciente;
            entidadPaciente.FechaNacPaciente = DateOnly.FromDateTime(dto.FechaNacPaciente);
            entidadPaciente.DireccionPaciente = dto.DireccionPaciente;
            entidadPaciente.TelefonoPaciente = dto.TelefonoPaciente;
            entidadPaciente.CorreoElectronicoPaciente = dto.CorreoElectronicoPaciente;
            entidadPaciente.IdUsuario = dto.IdUsuario;
            entidadPaciente.Activo = !dto.Anulado;
            entidadPaciente.FechaActualizacion = DateTime.UtcNow;
            if (!entidadPaciente.Activo)
            {
                entidadPaciente.FechaFin = entidadPaciente.FechaFin ?? DateTime.UtcNow;
            }
            else
            {
                entidadPaciente.FechaFin = null;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularPacienteAsync(int idPaciente)
        {
            var entidadPaciente = await _context.Paciente.FindAsync(idPaciente);
            if (entidadPaciente == null) return false;
            if (!entidadPaciente.Activo) return true;
            entidadPaciente.Activo = false;
            entidadPaciente.FechaFin = DateTime.UtcNow;
            entidadPaciente.FechaActualizacion = DateTime.UtcNow;
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
    }
}
