using Lab_APIRest.Infrastructure.EF;
using Microsoft.Extensions.Logging;
using Lab_APIRest.Infrastructure.EF.Models;
using paciente = Lab_APIRest.Infrastructure.EF.Models.paciente;
using persona = Lab_APIRest.Infrastructure.EF.Models.persona;
using Lab_APIRest.Services.Email;
using Lab_Contracts.Pacientes;
using Lab_Contracts.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Lab_APIRest.Services.Pacientes
{
    public class PacienteService : IPacienteService
    {
        private readonly LabDbContext _context;
        private readonly IEmailService _emailService;
        private readonly string _frontendBaseUrl;
        private readonly ILogger<PacienteService> _logger;

        public PacienteService(LabDbContext context, IEmailService emailService, IConfiguration configuration, ILogger<PacienteService> logger)
        {
            _context = context;
            _emailService = emailService;
            _frontendBaseUrl = configuration["FrontendBaseUrl"] ?? "http://laboratorioinmaculada.site";
            _logger = logger;
        }

        private static PacienteDto MapPaciente(paciente entidadPaciente) => new()
        {
            IdPaciente = entidadPaciente.id_paciente,
            IdPersona = entidadPaciente.id_persona,
            Cedula = entidadPaciente.persona_navigation?.cedula ?? string.Empty,
            Nombres = entidadPaciente.persona_navigation?.nombres ?? string.Empty,
            Apellidos = entidadPaciente.persona_navigation?.apellidos ?? string.Empty,
            Correo = entidadPaciente.persona_navigation?.usuario.FirstOrDefault(u => u.activo == true)?.correo
                ?? entidadPaciente.persona_navigation?.usuario.FirstOrDefault()?.correo
                ?? string.Empty,
            Telefono = entidadPaciente.persona_navigation?.telefono ?? string.Empty,
            Direccion = entidadPaciente.persona_navigation?.direccion ?? string.Empty,
            FechaNacimiento = (entidadPaciente.persona_navigation?.fecha_nacimiento ?? DateOnly.MinValue).ToDateTime(TimeOnly.MinValue),
            IdGenero = entidadPaciente.persona_navigation?.id_genero,
            NombreGenero = entidadPaciente.persona_navigation?.genero_navigation?.nombre,
            Activo = entidadPaciente.activo
        };

        private static PacienteDto MapPersona(persona entidadPersona) => new()
        {
            IdPaciente = 0,
            IdPersona = entidadPersona.id_persona,
            Cedula = entidadPersona.cedula ?? string.Empty,
            Nombres = entidadPersona.nombres ?? string.Empty,
            Apellidos = entidadPersona.apellidos ?? string.Empty,
            Correo = entidadPersona.usuario.FirstOrDefault(u => u.activo == true)?.correo
                ?? entidadPersona.usuario.FirstOrDefault()?.correo
                ?? string.Empty,
            Telefono = entidadPersona.telefono ?? string.Empty,
            Direccion = entidadPersona.direccion ?? string.Empty,
            FechaNacimiento = (entidadPersona.fecha_nacimiento ?? DateOnly.MinValue).ToDateTime(TimeOnly.MinValue),
            IdGenero = entidadPersona.id_genero,
            NombreGenero = entidadPersona.genero_navigation?.nombre,
            Activo = entidadPersona.activo
        };

        public async Task<List<PacienteDto>> ListarPacientesAsync()
        {
            var lista = await _context.Paciente
                .Include(p => p.persona_navigation)!.ThenInclude(p => p.genero_navigation)
                .Include(p => p.persona_navigation)!.ThenInclude(p => p.usuario)
                .ToListAsync();
            return lista.Select(MapPaciente).ToList();
        }

        public async Task<PacienteDto?> ObtenerDetallePacienteAsync(int idPaciente)
        {
            var entidadPaciente = await _context.Paciente
                .Include(p => p.persona_navigation)!.ThenInclude(p => p.genero_navigation)
                .Include(p => p.persona_navigation)!.ThenInclude(p => p.usuario)
                .FirstOrDefaultAsync(p => p.id_paciente == idPaciente);
            return entidadPaciente == null ? null : MapPaciente(entidadPaciente);
        }

        public async Task<PacienteDto?> ObtenerPersonaPorCedulaAsync(string cedula)
        {
            if (string.IsNullOrWhiteSpace(cedula)) return null;

            var entidadPersona = await _context.Persona
                .Include(p => p.genero_navigation)
                .Include(p => p.usuario)
                .FirstOrDefaultAsync(p => p.cedula == cedula);

            return entidadPersona == null ? null : MapPersona(entidadPersona);
        }

        public async Task<List<PacienteDto>?> ListarPacientesAsync(string criterio, string valor)
        {
            if (string.IsNullOrWhiteSpace(criterio) || string.IsNullOrWhiteSpace(valor))
                return new List<PacienteDto>();

            var campoLower = criterio.ToLower();
            IQueryable<paciente> query = _context.Paciente
                .Include(p => p.persona_navigation)!.ThenInclude(p => p.genero_navigation)
                .Include(p => p.persona_navigation)!.ThenInclude(p => p.usuario);
            switch (campoLower)
            {
                case "cedula":
                    var porCedula = await query.FirstOrDefaultAsync(x => x.persona_navigation!.cedula == valor);
                    return porCedula == null ? new List<PacienteDto>() : new List<PacienteDto> { MapPaciente(porCedula) };
                case "nombre":
                    return await query.Where(p => (p.persona_navigation!.nombres + " " + p.persona_navigation!.apellidos).Contains(valor)).Select(p => MapPaciente(p)).ToListAsync();
                default:
                    return null;
            }
        }

        public async Task<ResultadoPaginadoDto<PacienteDto>> ListarPacientesPaginadosAsync(PacienteFiltroDto filtro)
        {
            var query = _context.Paciente
                .Include(p => p.persona_navigation)!.ThenInclude(p => p.genero_navigation)
                .Include(p => p.persona_navigation)!.ThenInclude(p => p.usuario)
                .AsNoTracking()
                .AsQueryable();

            if (!(filtro.IncluirAnulados && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnulados && !filtro.IncluirVigentes)
                    query = query.Where(p => p.activo == false);
                else if (!filtro.IncluirAnulados && filtro.IncluirVigentes)
                    query = query.Where(p => p.activo == true);
            }

            if (!string.IsNullOrWhiteSpace(filtro.CriterioBusqueda) && !string.IsNullOrWhiteSpace(filtro.ValorBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "cedula": query = query.Where(p => (p.persona_navigation!.cedula ?? "").ToLower().Contains(val)); break;
                    case "nombre": query = query.Where(p => ((p.persona_navigation!.nombres + " " + p.persona_navigation!.apellidos) ?? "").ToLower().Contains(val)); break;
                }
            }

            var total = await query.CountAsync();

            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(PacienteDto.Cedula) => asc ? query.OrderBy(p => p.persona_navigation!.cedula) : query.OrderByDescending(p => p.persona_navigation!.cedula),
                nameof(PacienteDto.Nombres) => asc ? query.OrderBy(p => p.persona_navigation!.nombres) : query.OrderByDescending(p => p.persona_navigation!.nombres),
                nameof(PacienteDto.Apellidos) => asc ? query.OrderBy(p => p.persona_navigation!.apellidos) : query.OrderByDescending(p => p.persona_navigation!.apellidos),
                nameof(PacienteDto.FechaNacimiento) => asc ? query.OrderBy(p => p.persona_navigation!.fecha_nacimiento) : query.OrderByDescending(p => p.persona_navigation!.fecha_nacimiento),
                nameof(PacienteDto.Telefono) => asc ? query.OrderBy(p => p.persona_navigation!.telefono) : query.OrderByDescending(p => p.persona_navigation!.telefono),
                _ => asc ? query.OrderBy(p => p.id_paciente) : query.OrderByDescending(p => p.id_paciente)
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
            try
            {
                if (!ValidarCedula(dto.Cedula))
                {
                    _logger.LogWarning("Intento de guardar paciente con cédula inválida: {Cedula}", dto.Cedula);
                    return (false, "La cédula ingresada no es válida.", null);
                }

                if (!dto.IdGenero.HasValue)
                {
                    _logger.LogWarning("Intento de guardar paciente sin género especificado. Cédula: {Cedula}", dto.Cedula);
                    return (false, "El género es obligatorio.", null);
                }

                await using var transaccion = await _context.Database.BeginTransactionAsync();

                var persona = await _context.Persona
                    .Include(p => p.usuario)
                    .FirstOrDefaultAsync(p => p.cedula == dto.Cedula);

                if (persona == null)
                {
                    persona = new persona
                    {
                        cedula = dto.Cedula,
                        nombres = dto.Nombres,
                        apellidos = dto.Apellidos,
                        telefono = dto.Telefono,
                        direccion = dto.Direccion,
                        fecha_nacimiento = dto.FechaNacimiento.HasValue ? DateOnly.FromDateTime(dto.FechaNacimiento.Value) : null,
                        id_genero = dto.IdGenero.Value,
                        activo = true,
                        fecha_creacion = DateTime.UtcNow
                    };
                    _context.Persona.Add(persona);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Nueva persona creada con cédula: {Cedula}", dto.Cedula);
                }

                var pacienteExistente = await _context.Paciente
                    .Include(p => p.persona_navigation)!.ThenInclude(p => p.genero_navigation)
                    .Include(p => p.persona_navigation)!.ThenInclude(p => p.usuario)
                    .FirstOrDefaultAsync(p => p.id_persona == persona.id_persona);

                if (pacienteExistente != null)
                {
                    await transaccion.RollbackAsync();
                    _logger.LogInformation("La persona con cédula {Cedula} ya está registrada como paciente.", dto.Cedula);
                    return (true, "La persona ya está registrada como paciente.", MapPaciente(pacienteExistente));
                }

                var entidadPaciente = new paciente
                {
                    id_persona = persona.id_persona,
                    activo = true,
                    fecha_creacion = DateTime.UtcNow
                };

                _context.Paciente.Add(entidadPaciente);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Paciente registrado correctamente para persona con cédula: {Cedula}", dto.Cedula);

                usuario? usuarioPaciente = null;

                var tieneUsuario = persona.usuario.Any();

                if (!tieneUsuario && !string.IsNullOrWhiteSpace(dto.Correo))
                {
                    var crearUsuario = await CrearUsuarioPacienteAsync(persona, dto.Correo);
                    if (!crearUsuario.Exito)
                    {
                        await transaccion.RollbackAsync();
                        _logger.LogError("Error al crear usuario para paciente con cédula {Cedula}: {Mensaje}", dto.Cedula, crearUsuario.Mensaje);
                        return (false, crearUsuario.Mensaje, null);
                    }
                    usuarioPaciente = crearUsuario.UsuarioCreado;
                }

                await transaccion.CommitAsync();

                var dtoRespuesta = MapPaciente(entidadPaciente);
                dtoRespuesta.Correo = usuarioPaciente?.correo ?? dtoRespuesta.Correo;

                return (true, "Paciente registrado correctamente", dtoRespuesta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al guardar paciente con cédula {Cedula}", dto.Cedula);
                return (false, "Ocurrió un error al registrar el paciente.", null);
            }
        }

        public async Task<bool> GuardarPacienteAsync(int idPaciente, PacienteDto dto)
        {
            var entidadPaciente = await _context.Paciente
                .Include(p => p.persona_navigation)!.ThenInclude(per => per.usuario)
                .FirstOrDefaultAsync(p => p.id_paciente == idPaciente);
            if (entidadPaciente == null) return false;

            var persona = entidadPaciente.persona_navigation!;
            persona.cedula = dto.Cedula;
            persona.nombres = dto.Nombres;
            persona.apellidos = dto.Apellidos;
            persona.telefono = dto.Telefono;
            persona.direccion = dto.Direccion;
            if (!dto.IdGenero.HasValue)
                return false;
            persona.id_genero = dto.IdGenero.Value;
            persona.fecha_nacimiento = dto.FechaNacimiento.HasValue ? DateOnly.FromDateTime(dto.FechaNacimiento.Value) : null;
            persona.fecha_actualizacion = DateTime.UtcNow;

            entidadPaciente.activo = dto.Activo;
            entidadPaciente.fecha_actualizacion = DateTime.UtcNow;
            if (!entidadPaciente.activo)
            {
                entidadPaciente.fecha_fin = entidadPaciente.fecha_fin ?? DateTime.UtcNow;
            }
            else
            {
                entidadPaciente.fecha_fin = null;
            }

            var usuarioExistente = persona.usuario.FirstOrDefault();
            var tieneCorreo = !string.IsNullOrWhiteSpace(usuarioExistente?.correo);

            if (!tieneCorreo && !string.IsNullOrWhiteSpace(dto.Correo))
            {
                var crearUsuario = await CrearUsuarioPacienteAsync(persona, dto.Correo);
                if (!crearUsuario.Exito) return false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularPacienteAsync(int idPaciente)
        {
            var entidadPaciente = await _context.Paciente.FindAsync(idPaciente);
            if (entidadPaciente == null) return false;
            if (!entidadPaciente.activo) return true;
            entidadPaciente.activo = false;
            entidadPaciente.fecha_fin = DateTime.UtcNow;
            entidadPaciente.fecha_actualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<GeneroDto>> ListarGenerosAsync()
        {
            return await _context.Genero
                .AsNoTracking()
                .Select(g => new GeneroDto
                {
                    IdGenero = g.id_genero,
                    Nombre = g.nombre,
                    Descripcion = g.descripcion,
                    Activo = g.activo
                })
                .ToListAsync();
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

        private async Task<(bool Exito, string Mensaje, usuario? UsuarioCreado)> CrearUsuarioPacienteAsync(persona persona, string correo)
        {
            var rolPacienteId = await _context.Rol
                .Where(r => r.nombre.ToLower() == "paciente")
                .Select(r => (int?)r.id_rol)
                .FirstOrDefaultAsync();

            if (!rolPacienteId.HasValue)
                return (false, "No se encontró el rol de paciente.", null);

            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToBase64String(tokenBytes);
            var tokenHash = SHA256.HashData(Encoding.UTF8.GetBytes(token));

            var usuarioPaciente = new usuario
            {
                id_persona = persona.id_persona,
                id_rol = rolPacienteId.Value,
                correo = correo,
                activo = false,
                password_hash = Convert.ToHexString(tokenHash),
                fecha_creacion = DateTime.UtcNow
            };
            _context.Usuario.Add(usuarioPaciente);
            await _context.SaveChangesAsync();

            var tokenRegistro = new tokens_usuarios
            {
                id_usuario = usuarioPaciente.id_usuario,
                token_hash = tokenHash,
                tipo_token = "activacion",
                fecha_solicitud = DateTime.UtcNow,
                fecha_expiracion = DateTime.UtcNow.AddHours(24),
                usado = false
            };
            _context.TokensUsuarios.Add(tokenRegistro);
            await _context.SaveChangesAsync();

            var tokenUrl = Uri.EscapeDataString(token);
            var enlace = $"{_frontendBaseUrl.TrimEnd('/')}/activar-cuenta?token={tokenUrl}";

            var asunto = "Activación de cuenta - Laboratorio Clínico La Inmaculada";
            var cuerpo = $@"<p>Hola <strong>{persona.nombres} {persona.apellidos}</strong>,</p>

<p>Se ha creado una cuenta para ti en la página del Laboratorio Clínico La Inmaculada.</p>

<p>Para activar tu cuenta y establecer tu contraseña, haz clic en el siguiente enlace:</p>

<p>
    <a href=""{enlace}"" target=""_blank"">Activar mi cuenta</a>
</p>

<p>Este enlace estará disponible durante 24 horas.</p>

<p>Si no solicitaste este registro, puedes ignorar este mensaje.</p>";

            await _emailService.EnviarCorreoAsync(usuarioPaciente.correo, $"{persona.nombres} {persona.apellidos}", asunto, cuerpo);

            return (true, string.Empty, usuarioPaciente);
        }
    }
}
