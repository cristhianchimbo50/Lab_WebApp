using Lab_Contracts.Usuarios;
using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using usuario = Lab_APIRest.Infrastructure.EF.Models.usuario;
using persona = Lab_APIRest.Infrastructure.EF.Models.persona;
using tokens_usuarios = Lab_APIRest.Infrastructure.EF.Models.tokens_usuarios;
using Microsoft.EntityFrameworkCore;
using Lab_APIRest.Services.Email;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Lab_APIRest.Services.Usuarios
{
    public class UsuariosService : IUsuariosService
    {
        private readonly LabDbContext _context;
        private readonly IEmailService _emailService;
        private readonly string _frontendBaseUrl;
        private readonly ILogger<UsuariosService> _logger;

        public UsuariosService(LabDbContext context, IEmailService emailService, IConfiguration configuration, ILogger<UsuariosService> logger)
        {
            _context = context;
            _emailService = emailService;
            _frontendBaseUrl = configuration["FrontendBaseUrl"] ?? "http://laboratorioinmaculada.site";
            _logger = logger;
        }

        private static UsuarioListadoDto MapUsuario(usuario entidad) => new()
        {
            IdUsuario = entidad.id_usuario,
            IdPersona = entidad.id_persona,
            Cedula = entidad.persona_navigation?.cedula ?? string.Empty,
            Nombres = entidad.persona_navigation?.nombres ?? string.Empty,
            Apellidos = entidad.persona_navigation?.apellidos ?? string.Empty,
            FechaNacimiento = (entidad.persona_navigation?.fecha_nacimiento ?? DateOnly.MinValue).ToDateTime(TimeOnly.MinValue),
            Correo = entidad.correo ?? string.Empty,
            Telefono = entidad.persona_navigation?.telefono ?? string.Empty,
            Direccion = entidad.persona_navigation?.direccion ?? string.Empty,
            IdGenero = entidad.persona_navigation?.id_genero ?? 0,
            NombreGenero = entidad.persona_navigation?.genero_navigation?.nombre ?? string.Empty,
            IdRol = entidad.id_rol,
            NombreRol = entidad.rol_navigation?.nombre ?? string.Empty,
            Activo = entidad.activo ?? false,
            FechaCreacion = entidad.fecha_creacion,
            UltimoAcceso = entidad.ultimo_acceso
        };

        public async Task<List<UsuarioListadoDto>> ListarUsuariosAsync(UsuarioFiltroDto filtro, CancellationToken ct = default)
        {
            try
            {
                var consulta = _context.Usuario
                    .Include(u => u.rol_navigation)
                    .Include(u => u.persona_navigation)!.ThenInclude(p => p.genero_navigation)
                    .Where(u => u.rol_navigation.nombre != "paciente");

                if (!string.IsNullOrWhiteSpace(filtro.Nombre)) consulta = consulta.Where(u => u.persona_navigation!.nombres.Contains(filtro.Nombre) || u.persona_navigation!.apellidos.Contains(filtro.Nombre));
                if (!string.IsNullOrWhiteSpace(filtro.Correo)) consulta = consulta.Where(u => (u.correo ?? string.Empty).Contains(filtro.Correo));
                if (filtro.IdRol.HasValue) consulta = consulta.Where(u => u.id_rol == filtro.IdRol.Value);
                if (filtro.Activo.HasValue) consulta = consulta.Where(u => (u.activo ?? false) == filtro.Activo.Value);

                return await consulta
                    .OrderBy(u => u.persona_navigation!.nombres)
                    .Select(u => MapUsuario(u))
                    .ToListAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al acceder a la base de datos al listar usuarios.");
                throw new Exception("Error al acceder a la base de datos al listar usuarios.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al listar usuarios.");
                throw new Exception("Error inesperado al listar usuarios.", ex);
            }
        }

        public async Task<UsuarioListadoDto?> ObtenerDetalleUsuarioAsync(int idUsuario, CancellationToken ct = default)
        {
            try
            {
                var entidad = await _context.Usuario
                    .Include(u => u.rol_navigation)
                    .Include(u => u.persona_navigation)!.ThenInclude(p => p.genero_navigation)
                    .FirstOrDefaultAsync(u => u.id_usuario == idUsuario && u.rol_navigation.nombre != "paciente", ct);
                return entidad == null ? null : MapUsuario(entidad);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al acceder a la base de datos al obtener el usuario con ID {IdUsuario}.", idUsuario);
                throw new Exception($"Error al acceder a la base de datos al obtener el usuario con ID {idUsuario}.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener el detalle del usuario con ID {IdUsuario}.", idUsuario);
                throw new Exception($"Error inesperado al obtener el detalle del usuario con ID {idUsuario}.", ex);
            }
        }

        public async Task<int> GuardarUsuarioAsync(UsuarioCrearDto usuario, CancellationToken ct = default)
        {
            try
            {
                if (!ValidarCedula(usuario.Cedula))
                    throw new ArgumentException("La cédula ingresada no es válida.");

                await using var transaccion = await _context.Database.BeginTransactionAsync(ct);

                if (usuario.IdGenero <= 0)
                    throw new ArgumentException("El género es obligatorio.");

                var persona = new persona
                {
                    cedula = usuario.Cedula,
                    nombres = (usuario.Nombres ?? string.Empty).ToUpperInvariant(),
                    apellidos = (usuario.Apellidos ?? string.Empty).ToUpperInvariant(),
                    fecha_nacimiento = usuario.FechaNacimiento == default ? null : DateOnly.FromDateTime(usuario.FechaNacimiento),
                    telefono = usuario.Telefono,
                    direccion = (usuario.Direccion ?? string.Empty).ToUpperInvariant(),
                    id_genero = usuario.IdGenero,
                    activo = true,
                    fecha_creacion = DateTime.UtcNow
                };
                _context.Persona.Add(persona);
                await _context.SaveChangesAsync(ct);

                var randomBytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);
                var token = Convert.ToBase64String(randomBytes);
                var tokenHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));

                var entidad = new usuario
                {
                    id_persona = persona.id_persona,
                    id_rol = usuario.IdRol,
                    correo = usuario.Correo,
                    activo = false,
                    password_hash = Convert.ToHexString(tokenHash),
                    fecha_creacion = DateTime.UtcNow
                };

                _context.Usuario.Add(entidad);
                await _context.SaveChangesAsync(ct);

                var tokenRegistro = new tokens_usuarios
                {
                    id_usuario = entidad.id_usuario,
                    token_hash = tokenHash,
                    tipo_token = "activacion",
                    fecha_solicitud = DateTime.UtcNow,
                    fecha_expiracion = DateTime.UtcNow.AddHours(24),
                    usado = false
                };
                _context.TokensUsuarios.Add(tokenRegistro);
                await _context.SaveChangesAsync(ct);

                var tokenUrl = Uri.EscapeDataString(token);
                var enlace = $"{_frontendBaseUrl.TrimEnd('/')}/activar-cuenta?token={tokenUrl}";

                var asunto = "Activación de cuenta - Laboratorio Clínico La Inmaculada";
                var cuerpo = $@"
                    <p>Hola <strong>{persona.nombres} {persona.apellidos}</strong>,</p>

                    <p>Se ha creado una cuenta para ti en la página del Laboratorio Clínico La Inmaculada.</p>

                    <p>Para activar tu cuenta y establecer tu contraseña, haz clic en el siguiente enlace:</p>

                    <p>
                        <a href=""{enlace}"" target=""_blank"">
                            Activar mi cuenta
                        </a>
                    </p>

                    <p>Este enlace estará disponible durante 24 horas.</p>

                    <p>Si no solicitaste este registro, puedes ignorar este mensaje.</p>";

                await _emailService.EnviarCorreoAsync(entidad.correo, $"{persona.nombres} {persona.apellidos}", asunto, cuerpo);

                await transaccion.CommitAsync(ct);

                return entidad.id_usuario;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error al guardar el usuario en la base de datos. Verifica que el correo electrónico no esté duplicado.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al crear el usuario. No se pudo completar el proceso de registro.", ex);
            }
        }

        public async Task<bool> GuardarUsuarioAsync(UsuarioEditarDto usuario, CancellationToken ct = default)
        {
            try
            {
                if (!ValidarCedula(usuario.Cedula))
                    throw new ArgumentException("La cédula ingresada no es válida.");

                await using var transaccion = await _context.Database.BeginTransactionAsync(ct);

                var entidad = await _context.Usuario
                    .Include(u => u.persona_navigation)!.ThenInclude(p => p.genero_navigation)
                    .Include(u => u.rol_navigation)
                    .FirstOrDefaultAsync(x => x.id_usuario == usuario.IdUsuario && x.rol_navigation.nombre != "paciente", ct);
                if (entidad == null) return false;

                var persona = entidad.persona_navigation;
                persona.cedula = usuario.Cedula;
                persona.nombres = (usuario.Nombres ?? string.Empty).ToUpperInvariant();
                persona.apellidos = (usuario.Apellidos ?? string.Empty).ToUpperInvariant();
                persona.fecha_nacimiento = usuario.FechaNacimiento == default ? null : DateOnly.FromDateTime(usuario.FechaNacimiento);
                persona.telefono = usuario.Telefono;
                persona.direccion = (usuario.Direccion ?? string.Empty).ToUpperInvariant();
                if (usuario.IdGenero <= 0) throw new ArgumentException("El género es obligatorio.");
                persona.id_genero = usuario.IdGenero;
                persona.fecha_actualizacion = DateTime.UtcNow;

                entidad.correo = usuario.Correo;
                entidad.id_rol = usuario.IdRol;
                entidad.fecha_actualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync(ct);
                await transaccion.CommitAsync(ct);
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario con ID {IdUsuario} en la base de datos.", usuario.IdUsuario);
                throw new Exception($"Error al actualizar el usuario con ID {usuario.IdUsuario} en la base de datos.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al actualizar el usuario con ID {IdUsuario}.", usuario.IdUsuario);
                throw new Exception($"Error inesperado al actualizar el usuario con ID {usuario.IdUsuario}.", ex);
            }
        }

        public async Task<bool> CambiarEstadoUsuarioAsync(int idUsuario, bool activo, string correoUsuarioActual, CancellationToken ct = default)
        {
            try
            {
                var entidad = await _context.Usuario.FirstOrDefaultAsync(u => u.id_usuario == idUsuario && u.rol_navigation.nombre != "paciente", ct);
                if (entidad == null) return false;
                if (!string.IsNullOrWhiteSpace(correoUsuarioActual) &&
                    (entidad.correo ?? string.Empty).Trim().ToLowerInvariant() == correoUsuarioActual.Trim().ToLowerInvariant())
                    throw new InvalidOperationException("No puedes deshabilitar tu propio usuario.");
                entidad.activo = activo;
                entidad.fecha_actualizacion = DateTime.UtcNow;
                if (entidad.activo == false)
                {
                    entidad.fecha_fin = entidad.fecha_fin ?? DateTime.UtcNow;
                }
                else
                {
                    entidad.fecha_fin = null;
                }
                await _context.SaveChangesAsync(ct);
                return true;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al cambiar el estado del usuario con ID {IdUsuario} en la base de datos.", idUsuario);
                throw new Exception($"Error al cambiar el estado del usuario con ID {idUsuario} en la base de datos.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cambiar el estado del usuario con ID {IdUsuario}.", idUsuario);
                throw new Exception($"Error inesperado al cambiar el estado del usuario con ID {idUsuario}.", ex);
            }
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
    }
}
