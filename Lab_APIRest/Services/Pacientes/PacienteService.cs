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
            IdPersona = entidadPaciente.IdPersona,
            Cedula = entidadPaciente.IdPersonaNavigation?.Cedula ?? string.Empty,
            Nombres = entidadPaciente.IdPersonaNavigation?.Nombres ?? string.Empty,
            Apellidos = entidadPaciente.IdPersonaNavigation?.Apellidos ?? string.Empty,
            Correo = entidadPaciente.IdPersonaNavigation?.Correo ?? string.Empty,
            Telefono = entidadPaciente.IdPersonaNavigation?.Telefono ?? string.Empty,
            Direccion = entidadPaciente.IdPersonaNavigation?.Direccion ?? string.Empty,
            FechaNacimiento = entidadPaciente.FechaNacPaciente.ToDateTime(TimeOnly.MinValue),
            IdGenero = entidadPaciente.IdGenero,
            NombreGenero = entidadPaciente.IdGeneroNavigation?.Nombre,
            Activo = entidadPaciente.Activo
        };

        public async Task<List<PacienteDto>> ListarPacientesAsync()
        {
            var lista = await _context.Paciente
                .Include(p => p.IdGeneroNavigation)
                .Include(p => p.IdPersonaNavigation)
                .ToListAsync();
            return lista.Select(MapPaciente).ToList();
        }

        public async Task<PacienteDto?> ObtenerDetallePacienteAsync(int idPaciente)
        {
            var entidadPaciente = await _context.Paciente
                .Include(p => p.IdGeneroNavigation)
                .Include(p => p.IdPersonaNavigation)
                .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);
            return entidadPaciente == null ? null : MapPaciente(entidadPaciente);
        }

        public async Task<List<PacienteDto>?> ListarPacientesAsync(string criterio, string valor)
        {
            if (string.IsNullOrWhiteSpace(criterio) || string.IsNullOrWhiteSpace(valor))
                return new List<PacienteDto>();

            var campoLower = criterio.ToLower();
            IQueryable<Paciente> query = _context.Paciente
                .Include(p => p.IdGeneroNavigation)
                .Include(p => p.IdPersonaNavigation);
            switch (campoLower)
            {
                case "cedula":
                    var porCedula = await query.FirstOrDefaultAsync(x => x.IdPersonaNavigation!.Cedula == valor);
                    return porCedula == null ? new List<PacienteDto>() : new List<PacienteDto> { MapPaciente(porCedula) };
                case "nombre":
                    return await query.Where(p => (p.IdPersonaNavigation!.Nombres + " " + p.IdPersonaNavigation!.Apellidos).Contains(valor)).Select(p => MapPaciente(p)).ToListAsync();
                case "correo":
                    return await query.Where(p => p.IdPersonaNavigation!.Correo.Contains(valor)).Select(p => MapPaciente(p)).ToListAsync();
                default:
                    return new List<PacienteDto>();
            }
        }

        public async Task<ResultadoPaginadoDto<PacienteDto>> ListarPacientesPaginadosAsync(PacienteFiltroDto filtro)
        {
            var query = _context.Paciente
                .Include(p => p.IdGeneroNavigation)
                .Include(p => p.IdPersonaNavigation)
                .AsNoTracking()
                .AsQueryable();

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
                    case "cedula": query = query.Where(p => (p.IdPersonaNavigation!.Cedula ?? "").ToLower().Contains(val)); break;
                    case "nombre": query = query.Where(p => ((p.IdPersonaNavigation!.Nombres + " " + p.IdPersonaNavigation!.Apellidos) ?? "").ToLower().Contains(val)); break;
                    case "correo": query = query.Where(p => (p.IdPersonaNavigation!.Correo ?? "").ToLower().Contains(val)); break;
                }
            }

            var total = await query.CountAsync();

            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(PacienteDto.Cedula) => asc ? query.OrderBy(p => p.IdPersonaNavigation!.Cedula) : query.OrderByDescending(p => p.IdPersonaNavigation!.Cedula),
                nameof(PacienteDto.Nombres) => asc ? query.OrderBy(p => p.IdPersonaNavigation!.Nombres) : query.OrderByDescending(p => p.IdPersonaNavigation!.Nombres),
                nameof(PacienteDto.Apellidos) => asc ? query.OrderBy(p => p.IdPersonaNavigation!.Apellidos) : query.OrderByDescending(p => p.IdPersonaNavigation!.Apellidos),
                nameof(PacienteDto.FechaNacimiento) => asc ? query.OrderBy(p => p.FechaNacPaciente) : query.OrderByDescending(p => p.FechaNacPaciente),
                nameof(PacienteDto.Correo) => asc ? query.OrderBy(p => p.IdPersonaNavigation!.Correo) : query.OrderByDescending(p => p.IdPersonaNavigation!.Correo),
                nameof(PacienteDto.Telefono) => asc ? query.OrderBy(p => p.IdPersonaNavigation!.Telefono) : query.OrderByDescending(p => p.IdPersonaNavigation!.Telefono),
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
            if (!ValidarCedula(dto.Cedula))
                return (false, "La cédula ingresada no es válida.", null);

            await using var transaccion = await _context.Database.BeginTransactionAsync();

            var persona = new Persona
            {
                Cedula = dto.Cedula,
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                Correo = dto.Correo,
                Telefono = dto.Telefono,
                Direccion = dto.Direccion,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Persona.Add(persona);
            await _context.SaveChangesAsync();

            var entidadPaciente = new Paciente
            {
                IdPersona = persona.IdPersona,
                FechaNacPaciente = DateOnly.FromDateTime(dto.FechaNacimiento),
                IdGenero = dto.IdGenero,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Paciente.Add(entidadPaciente);
            await _context.SaveChangesAsync();

            await transaccion.CommitAsync();

            return (true, "Paciente registrado correctamente", MapPaciente(entidadPaciente));
        }

        public async Task<bool> GuardarPacienteAsync(int idPaciente, PacienteDto dto)
        {
            var entidadPaciente = await _context.Paciente
                .Include(p => p.IdPersonaNavigation)
                .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);
            if (entidadPaciente == null) return false;

            var persona = entidadPaciente.IdPersonaNavigation!;
            persona.Cedula = dto.Cedula;
            persona.Nombres = dto.Nombres;
            persona.Apellidos = dto.Apellidos;
            persona.Correo = dto.Correo;
            persona.Telefono = dto.Telefono;
            persona.Direccion = dto.Direccion;
            persona.FechaActualizacion = DateTime.UtcNow;

            entidadPaciente.FechaNacPaciente = DateOnly.FromDateTime(dto.FechaNacimiento);
            entidadPaciente.IdGenero = dto.IdGenero;
            entidadPaciente.Activo = dto.Activo;
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

        public async Task<List<GeneroDto>> ListarGenerosAsync()
        {
            return await _context.Genero
                .AsNoTracking()
                .Select(g => new GeneroDto
                {
                    IdGenero = g.IdGenero,
                    Nombre = g.Nombre,
                    Descripcion = g.Descripcion,
                    Activo = g.Activo
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

        private static int CalcularEdad(DateTime fechaNacimiento)
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - fechaNacimiento.Year;
            if (fechaNacimiento > hoy.AddYears(-edad)) edad--;
            return edad;
        }
    }
}
