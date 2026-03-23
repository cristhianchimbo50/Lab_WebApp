using Lab_Contracts.Medicos;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.EF;
using Microsoft.EntityFrameworkCore;
using Lab_Contracts.Common;
using System;
using System.Linq;

namespace Lab_APIRest.Services.Medicos
{
    public class MedicoService : IMedicoService
    {
        private readonly LabDbContext _context;

        public MedicoService(LabDbContext context)
        {
            _context = context;
        }

        private static MedicoDto MapMedico(medico entidadMedico) => new()
        {
            IdMedico = entidadMedico.id_medico,
            NombreMedico = entidadMedico.nombre_medico,
            Especialidad = entidadMedico.especialidad,
            Telefono = entidadMedico.telefono ?? string.Empty,
            Correo = entidadMedico.correo ?? string.Empty,
            Anulado = !entidadMedico.activo
        };

        public async Task<List<MedicoDto>> ListarMedicosAsync()
        {
            return await _context.Medico
                .Where(entidadMedico => entidadMedico.activo)
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
            var entidadMedico = await _context.Medico.FirstOrDefaultAsync(x => x.correo == correo);
            return entidadMedico == null ? null : MapMedico(entidadMedico);
        }

        public async Task<List<MedicoDto>> ListarMedicosPorNombreAsync(string nombre)
        {
            return await _context.Medico
                .Where(entidadMedico => entidadMedico.nombre_medico.Contains(nombre) && entidadMedico.activo)
                .Select(entidadMedico => MapMedico(entidadMedico))
                .ToListAsync();
        }

        public async Task<List<MedicoDto>> ListarMedicosPorEspecialidadAsync(string especialidad)
        {
            return await _context.Medico
                .Where(entidadMedico => entidadMedico.especialidad.Contains(especialidad) && entidadMedico.activo)
                .Select(entidadMedico => MapMedico(entidadMedico))
                .ToListAsync();
        }

        public async Task<List<MedicoDto>> ListarMedicosPorCorreoAsync(string correo)
        {
            return await _context.Medico
                .Where(entidadMedico => entidadMedico.correo != null && entidadMedico.correo.Contains(correo) && entidadMedico.activo)
                .Select(entidadMedico => MapMedico(entidadMedico))
                .ToListAsync();
        }

        public async Task<MedicoDto> GuardarMedicoAsync(MedicoDto medicoDto)
        {
            var correoNormalizado = await NormalizarCorreoOpcionalAsync(medicoDto.Correo);
            var entidadMedico = new medico
            {
                nombre_medico = medicoDto.NombreMedico,
                especialidad = medicoDto.Especialidad,
                telefono = medicoDto.Telefono,
                correo = correoNormalizado,
                activo = true,
                fecha_creacion = DateTime.UtcNow
            };
            _context.Medico.Add(entidadMedico);
            await _context.SaveChangesAsync();
            return MapMedico(entidadMedico);
        }

        public async Task<bool> GuardarMedicoAsync(int idMedico, MedicoDto medicoDto)
        {
            var entidadMedico = await _context.Medico.FindAsync(idMedico);
            if (entidadMedico == null) return false;
            var correoNormalizado = await NormalizarCorreoOpcionalAsync(medicoDto.Correo, idMedico);
            entidadMedico.nombre_medico = medicoDto.NombreMedico;
            entidadMedico.especialidad = medicoDto.Especialidad;
            entidadMedico.telefono = medicoDto.Telefono;
            entidadMedico.correo = correoNormalizado;
            entidadMedico.activo = !medicoDto.Anulado;
            entidadMedico.fecha_actualizacion = DateTime.UtcNow;
            if (!entidadMedico.activo)
            {
                entidadMedico.fecha_fin = entidadMedico.fecha_fin ?? DateTime.UtcNow;
            }
            else
            {
                entidadMedico.fecha_fin = null;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AnularMedicoAsync(int idMedico)
        {
            var entidadMedico = await _context.Medico.FindAsync(idMedico);
            if (entidadMedico == null) return false;
            if (!entidadMedico.activo) return true;
            entidadMedico.activo = false;
            entidadMedico.fecha_fin = DateTime.UtcNow;
            entidadMedico.fecha_actualizacion = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ResultadoPaginadoDto<MedicoDto>> ListarMedicosPaginadosAsync(MedicoFiltroDto filtro)
        {
            var query = _context.Medico.AsNoTracking().AsQueryable();
            if (!(filtro.IncluirAnulados && filtro.IncluirVigentes))
            {
                if (filtro.IncluirAnulados && !filtro.IncluirVigentes)
                    query = query.Where(entidadMedico => entidadMedico.activo == false);
                else if (!filtro.IncluirAnulados && filtro.IncluirVigentes)
                    query = query.Where(entidadMedico => entidadMedico.activo == true);
            }
            if (!string.IsNullOrWhiteSpace(filtro.ValorBusqueda) && !string.IsNullOrWhiteSpace(filtro.CriterioBusqueda))
            {
                var val = filtro.ValorBusqueda.ToLower();
                switch (filtro.CriterioBusqueda)
                {
                    case "nombre": query = query.Where(entidadMedico => (entidadMedico.nombre_medico ?? "").ToLower().Contains(val)); break;
                    case "especialidad": query = query.Where(entidadMedico => (entidadMedico.especialidad ?? "").ToLower().Contains(val)); break;
                    case "correo": query = query.Where(entidadMedico => (entidadMedico.correo ?? "").ToLower().Contains(val)); break;
                    case "telefono": query = query.Where(entidadMedico => (entidadMedico.telefono ?? "").ToLower().Contains(val)); break;
                }
            }
            var total = await query.CountAsync();
            bool asc = filtro.SortAsc;
            query = filtro.SortBy switch
            {
                nameof(MedicoDto.NombreMedico) => asc ? query.OrderBy(entidadMedico => entidadMedico.nombre_medico) : query.OrderByDescending(entidadMedico => entidadMedico.nombre_medico),
                nameof(MedicoDto.Especialidad) => asc ? query.OrderBy(entidadMedico => entidadMedico.especialidad) : query.OrderByDescending(entidadMedico => entidadMedico.especialidad),
                nameof(MedicoDto.Correo) => asc ? query.OrderBy(entidadMedico => entidadMedico.correo) : query.OrderByDescending(entidadMedico => entidadMedico.correo),
                nameof(MedicoDto.Telefono) => asc ? query.OrderBy(entidadMedico => entidadMedico.telefono) : query.OrderByDescending(entidadMedico => entidadMedico.telefono),
                _ => asc ? query.OrderBy(entidadMedico => entidadMedico.id_medico) : query.OrderByDescending(entidadMedico => entidadMedico.id_medico)
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

        private async Task<string?> NormalizarCorreoOpcionalAsync(string? correo, int? idMedicoActual = null)
        {
            if (string.IsNullOrWhiteSpace(correo))
            {
                return null;
            }

            var correoNormalizado = correo.Trim();
            bool existe = await _context.Medico.AnyAsync(m => m.correo != null && m.correo == correoNormalizado && (!idMedicoActual.HasValue || m.id_medico != idMedicoActual.Value));
            if (existe)
            {
                throw new InvalidOperationException("Ya existe un médico registrado con ese correo.");
            }

            return correoNormalizado;
        }
    }
}
