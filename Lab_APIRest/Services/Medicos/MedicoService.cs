using Lab_Contracts.Medicos;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Lab_Contracts.Common;

namespace Lab_APIRest.Services.Medicos
{
    public class MedicoService : IMedicoService
    {
        private readonly LabDbContext _context;

        public MedicoService(LabDbContext context)
        {
            _context = context;
        }

        private static MedicoDto MapMedico(Medico entidadMedico) => new()
        {
            IdMedico = entidadMedico.IdMedico,
            NombreMedico = entidadMedico.NombreMedico,
            Especialidad = entidadMedico.Especialidad,
            Telefono = entidadMedico.Telefono,
            Correo = entidadMedico.Correo,
            Anulado = !entidadMedico.Activo
        };

        public async Task<List<MedicoDto>> ListarMedicosAsync()
        {
            return await _context.Medico
                .Where(entidadMedico => entidadMedico.Activo)
                .Select(entidadMedico => MapMedico(entidadMedico))
                .ToListAsync();
        }

        public async Task<MedicoDto?> ObtenerDetalleMedicoAsync(int idMedico)
        {
            var entidadMedico = await _context.Medico.FindAsync(idMedico);
            return entidadMedico == null ? null : MapMedico(entidadMedico);
        }

        public async Task<MedicoDto?> ObtenerMedicoPorCorreoAsync(string correo)
        {
            var entidadMedico = await _context.Medico.FirstOrDefaultAsync(x => x.Correo == correo);
            return entidadMedico == null ? null : MapMedico(entidadMedico);
        }

        public async Task<List<MedicoDto>> ListarMedicosPorNombreAsync(string nombre)
        {
            return await _context.Medico
                .Where(entidadMedico => entidadMedico.NombreMedico.Contains(nombre) && entidadMedico.Activo)
                .Select(entidadMedico => MapMedico(entidadMedico))
                .ToListAsync();
        }

        public async Task<List<MedicoDto>> ListarMedicosPorEspecialidadAsync(string especialidad)
        {
            return await _context.Medico
                .Where(entidadMedico => entidadMedico.Especialidad.Contains(especialidad) && entidadMedico.Activo)
                .Select(entidadMedico => MapMedico(entidadMedico))
                .ToListAsync();
        }

        public async Task<List<MedicoDto>> ListarMedicosPorCorreoAsync(string correo)
        {
            return await _context.Medico
                .Where(entidadMedico => entidadMedico.Correo != null && entidadMedico.Correo.Contains(correo) && entidadMedico.Activo)
                .Select(entidadMedico => MapMedico(entidadMedico))
                .ToListAsync();
        }

        public async Task<MedicoDto> GuardarMedicoAsync(MedicoDto medicoDto)
        {
            var entidadMedico = new Medico
            {
                NombreMedico = medicoDto.NombreMedico,
                Especialidad = medicoDto.Especialidad,
                Telefono = medicoDto.Telefono,
                Correo = medicoDto.Correo,
                Activo = true,
                FechaCreacion = DateTime.UtcNow
            };
            _context.Medico.Add(entidadMedico);
            await _context.SaveChangesAsync();
            return MapMedico(entidadMedico);
        }

        public async Task<bool> GuardarMedicoAsync(int idMedico, MedicoDto medicoDto)
        {
            var entidadMedico = await _context.Medico.FindAsync(idMedico);
            if (entidadMedico == null) return false;
            entidadMedico.NombreMedico = medicoDto.NombreMedico;
            entidadMedico.Especialidad = medicoDto.Especialidad;
            entidadMedico.Telefono = medicoDto.Telefono;
            entidadMedico.Correo = medicoDto.Correo;
            entidadMedico.Activo = !medicoDto.Anulado;
            entidadMedico.FechaActualizacion = DateTime.UtcNow;
            if (!entidadMedico.Activo)
            {
                entidadMedico.FechaFin = entidadMedico.FechaFin ?? DateTime.UtcNow;
            }
            else
            {
                entidadMedico.FechaFin = null;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularMedicoAsync(int idMedico)
        {
            var entidadMedico = await _context.Medico.FindAsync(idMedico);
            if (entidadMedico == null) return false;
            if (!entidadMedico.Activo) return true;
            entidadMedico.Activo = false;
            entidadMedico.FechaFin = DateTime.UtcNow;
            entidadMedico.FechaActualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ResultadoPaginadoDto<MedicoDto>> ListarMedicosPaginadosAsync(MedicoFiltroDto filtro)
        {
            var query = _context.Medico.AsNoTracking().AsQueryable();
            if (!(filtro.IncluirAnulados && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnulados && !filtro.IncluirVigentes)
                    query = query.Where(entidadMedico => entidadMedico.Activo == false);
                else if (!filtro.IncluirAnulados && filtro.IncluirVigentes)
                    query = query.Where(entidadMedico => entidadMedico.Activo == true);
            }
            if (!string.IsNullOrWhiteSpace(filtro.ValorBusqueda) && !string.IsNullOrWhiteSpace(filtro.CriterioBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "nombre": query = query.Where(entidadMedico => (entidadMedico.NombreMedico ?? "").ToLower().Contains(val)); break;
                    case "especialidad": query = query.Where(entidadMedico => (entidadMedico.Especialidad ?? "").ToLower().Contains(val)); break;
                    case "correo": query = query.Where(entidadMedico => (entidadMedico.Correo ?? "").ToLower().Contains(val)); break;
                    case "telefono": query = query.Where(entidadMedico => (entidadMedico.Telefono ?? "").ToLower().Contains(val)); break;
                }
            }
            var total = await query.CountAsync();
            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(MedicoDto.NombreMedico) => asc ? query.OrderBy(entidadMedico => entidadMedico.NombreMedico) : query.OrderByDescending(entidadMedico => entidadMedico.NombreMedico),
                nameof(MedicoDto.Especialidad) => asc ? query.OrderBy(entidadMedico => entidadMedico.Especialidad) : query.OrderByDescending(entidadMedico => entidadMedico.Especialidad),
                nameof(MedicoDto.Correo) => asc ? query.OrderBy(entidadMedico => entidadMedico.Correo) : query.OrderByDescending(entidadMedico => entidadMedico.Correo),
                nameof(MedicoDto.Telefono) => asc ? query.OrderBy(entidadMedico => entidadMedico.Telefono) : query.OrderByDescending(entidadMedico => entidadMedico.Telefono),
                _ => asc ? query.OrderBy(entidadMedico => entidadMedico.IdMedico) : query.OrderByDescending(entidadMedico => entidadMedico.IdMedico)
            };
            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(entidadMedico => MapMedico(entidadMedico)).ToListAsync();
            return new ResultadoPaginadoDto<MedicoDto>
            {
                TotalCount = total,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Items = items
            };
        }
    }
}
