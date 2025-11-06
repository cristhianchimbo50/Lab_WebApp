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

        public async Task<List<MedicoDto>> ListarMedicosAsync()
        {
            return await _context.medicos
                .Where(m => m.anulado == false || m.anulado == null)
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

        public async Task<MedicoDto?> ObtenerDetalleMedicoAsync(int idMedico)
        {
            var m = await _context.medicos.FindAsync(idMedico);
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

        public async Task<MedicoDto?> ObtenerMedicoPorCorreoAsync(string correo)
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

        public async Task<List<MedicoDto>> ListarMedicosPorNombreAsync(string nombre)
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

        public async Task<List<MedicoDto>> ListarMedicosPorEspecialidadAsync(string especialidad)
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

        public async Task<List<MedicoDto>> ListarMedicosPorCorreoAsync(string correo)
        {
            return await _context.medicos
                .Where(m => m.correo.Contains(correo))
                .Select(m => new MedicoDto
                {
                    IdMedico = m.id_medico,
                    NombreMedico = m.nombre_medico,
                    Especialidad = m.especialidad,
                    Telefono = m.telefono,
                    Correo = m.correo,
                    Anulado = m.anulado ?? false
                }).ToListAsync();
        }

        public async Task<MedicoDto> GuardarMedicoAsync(MedicoDto medicoDto)
        {
            var medico = new medico
            {
                nombre_medico = medicoDto.NombreMedico,
                especialidad = medicoDto.Especialidad,
                telefono = medicoDto.Telefono,
                correo = medicoDto.Correo,
                anulado = false
            };
            _context.medicos.Add(medico);
            await _context.SaveChangesAsync();
            medicoDto.IdMedico = medico.id_medico;
            medicoDto.Anulado = false;
            return medicoDto;
        }

        public async Task<bool> GuardarMedicoAsync(int idMedico, MedicoDto medicoDto)
        {
            var medico = await _context.medicos.FindAsync(idMedico);
            if (medico == null) return false;
            medico.nombre_medico = medicoDto.NombreMedico;
            medico.especialidad = medicoDto.Especialidad;
            medico.telefono = medicoDto.Telefono;
            medico.correo = medicoDto.Correo;
            medico.anulado = medicoDto.Anulado;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularMedicoAsync(int idMedico)
        {
            var medico = await _context.medicos.FindAsync(idMedico);
            if (medico == null) return false;
            medico.anulado = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ResultadoPaginadoDto<MedicoDto>> ListarMedicosPaginadosAsync(MedicoFiltroDto filtro)
        {
            var query = _context.medicos.AsNoTracking().AsQueryable();
            if (!(filtro.IncluirAnulados && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnulados && !filtro.IncluirVigentes)
                    query = query.Where(m => m.anulado == true);
                else if (!filtro.IncluirAnulados && filtro.IncluirVigentes)
                    query = query.Where(m => m.anulado == false || m.anulado == null);
            }
            if (!string.IsNullOrWhiteSpace(filtro.ValorBusqueda) && !string.IsNullOrWhiteSpace(filtro.CriterioBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "nombre": query = query.Where(m => (m.nombre_medico ?? "").ToLower().Contains(val)); break;
                    case "especialidad": query = query.Where(m => (m.especialidad ?? "").ToLower().Contains(val)); break;
                    case "correo": query = query.Where(m => (m.correo ?? "").ToLower().Contains(val)); break;
                    case "telefono": query = query.Where(m => (m.telefono ?? "").ToLower().Contains(val)); break;
                }
            }
            var total = await query.CountAsync();
            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(MedicoDto.NombreMedico) => asc ? query.OrderBy(m => m.nombre_medico) : query.OrderByDescending(m => m.nombre_medico),
                nameof(MedicoDto.Especialidad) => asc ? query.OrderBy(m => m.especialidad) : query.OrderByDescending(m => m.especialidad),
                nameof(MedicoDto.Correo) => asc ? query.OrderBy(m => m.correo) : query.OrderByDescending(m => m.correo),
                nameof(MedicoDto.Telefono) => asc ? query.OrderBy(m => m.telefono) : query.OrderByDescending(m => m.telefono),
                _ => asc ? query.OrderBy(m => m.id_medico) : query.OrderByDescending(m => m.id_medico)
            };
            var pageNumber = filtro.PageNumber < 1 ? 1 : filtro.PageNumber;
            var pageSize = filtro.PageSize <= 0 ? 10 : Math.Min(filtro.PageSize, 200);
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize)
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
