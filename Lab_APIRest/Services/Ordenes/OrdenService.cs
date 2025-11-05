using Lab_APIRest.Infrastructure.EF;
using Lab_APIRest.Infrastructure.EF.Models;
using Lab_APIRest.Infrastructure.Services;
using Lab_APIRest.Services.PDF;
using Lab_Contracts.Ordenes;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Lab_APIRest.Services.Ordenes
{
    public class OrdenService : IOrdenService
    {
        private readonly LabDbContext Contexto;
        private readonly PdfTicketService ServicioPdfTicket;

        public OrdenService(LabDbContext Contexto, PdfTicketService ServicioPdfTicket)
        {
            this.Contexto = Contexto;
            this.ServicioPdfTicket = ServicioPdfTicket;
        }

        public async Task<List<object>> ObtenerOrdenesAsync()
        {
            var ListaOrdenes = await Contexto.ordens
                .Include(o => o.id_pacienteNavigation)
                .Select(o => new
                {
                    IdOrden = o.id_orden,
                    NumeroOrden = o.numero_orden,
                    CedulaPaciente = o.id_pacienteNavigation!.cedula_paciente,
                    NombrePaciente = o.id_pacienteNavigation!.nombre_paciente,
                    FechaOrden = o.fecha_orden,
                    Total = o.total,
                    TotalPagado = o.total_pagado ?? 0,
                    SaldoPendiente = o.saldo_pendiente ?? 0,
                    EstadoPago = o.estado_pago,
                    Anulado = o.anulado ?? false
                })
                .OrderByDescending(x => x.IdOrden)
                .ToListAsync();

            return ListaOrdenes.Cast<object>().ToList();
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenOriginalAsync(int IdOrden)
        {
            var entidadOrden = await Contexto.ordens
                .Include(o => o.id_pacienteNavigation)
                .Include(o => o.id_medicoNavigation)
                .Include(o => o.detalle_ordens)
                    .ThenInclude(d => d.id_examenNavigation)
                .Include(o => o.detalle_ordens)
                    .ThenInclude(d => d.id_resultadoNavigation)
                .FirstOrDefaultAsync(o => o.id_orden == IdOrden);

            if (entidadOrden == null)
                return null;

            var detalleDto = new OrdenDetalleDto
            {
                IdOrden = entidadOrden.id_orden,
                NumeroOrden = entidadOrden.numero_orden,
                FechaOrden = entidadOrden.fecha_orden,
                EstadoPago = entidadOrden.estado_pago,
                IdPaciente = (int)entidadOrden.id_paciente,
                CedulaPaciente = entidadOrden.id_pacienteNavigation?.cedula_paciente,
                NombrePaciente = entidadOrden.id_pacienteNavigation?.nombre_paciente,
                DireccionPaciente = entidadOrden.id_pacienteNavigation?.direccion_paciente,
                CorreoPaciente = entidadOrden.id_pacienteNavigation?.correo_electronico_paciente,
                TelefonoPaciente = entidadOrden.id_pacienteNavigation?.telefono_paciente,
                IdMedico = entidadOrden.id_medico,
                NombreMedico = entidadOrden.id_medicoNavigation?.nombre_medico,
                Anulado = entidadOrden.anulado ?? false,
                Examenes = entidadOrden.detalle_ordens.Select(d => new ExamenDetalleDto
                {
                    IdExamen = d.id_examen ?? 0,
                    NombreExamen = d.id_examenNavigation!.nombre_examen,
                    NombreEstudio = d.id_examenNavigation!.estudio,
                    IdResultado = d.id_resultado,
                    NumeroResultado = d.id_resultadoNavigation != null ? d.id_resultadoNavigation.numero_resultado : null
                }).ToList()
            };

            return detalleDto;
        }

        public async Task<bool> AnularOrdenAsync(int IdOrden)
        {
            var entidadOrden = await Contexto.ordens
                .Include(o => o.detalle_ordens)
                    .ThenInclude(d => d.id_resultadoNavigation)
                        .ThenInclude(r => r.detalle_resultados)
                .FirstOrDefaultAsync(o => o.id_orden == IdOrden);

            if (entidadOrden == null)
                return false;

            entidadOrden.anulado = true;

            var listaResultados = entidadOrden.detalle_ordens
                .Where(d => d.id_resultadoNavigation != null)
                .Select(d => d.id_resultadoNavigation!)
                .Distinct()
                .ToList();

            foreach (var resultado in listaResultados)
            {
                resultado.anulado = true;
                foreach (var detalle in resultado.detalle_resultados)
                    detalle.anulado = true;
            }

            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<OrdenRespuestaDto?> CrearOrdenAsync(OrdenCompletaDto DatosOrden)
        {
            var ultimaOrden = await Contexto.ordens.OrderByDescending(o => o.id_orden).FirstOrDefaultAsync();
            int numeroSiguiente = 1;

            if (ultimaOrden != null && !string.IsNullOrEmpty(ultimaOrden.numero_orden))
            {
                var partes = ultimaOrden.numero_orden.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimoNumero))
                    numeroSiguiente = ultimoNumero + 1;
            }

            string numeroOrden = $"ORD-{numeroSiguiente:D5}";
            var ordenDto = DatosOrden.Orden;

            var entidad = new orden
            {
                id_paciente = ordenDto.IdPaciente,
                fecha_orden = ordenDto.FechaOrden,
                id_medico = ordenDto.IdMedico,
                observacion = ordenDto.Observacion,
                estado_pago = ordenDto.EstadoPago,
                anulado = false,
                liquidado_convenio = false,
                numero_orden = numeroOrden,
                total = ordenDto.Total,
                total_pagado = ordenDto.TotalPagado,
                saldo_pendiente = ordenDto.SaldoPendiente,
                detalle_ordens = ordenDto.Detalles.Select(d => new detalle_orden
                {
                    id_examen = d.IdExamen,
                    precio = d.Precio
                }).ToList()
            };

            Contexto.ordens.Add(entidad);
            await Contexto.SaveChangesAsync();

            return new OrdenRespuestaDto
            {
                IdOrden = entidad.id_orden,
                NumeroOrden = entidad.numero_orden
            };
        }

        public async Task<byte[]?> ObtenerTicketPdfAsync(int IdOrden)
        {
            var entidadOrden = await Contexto.ordens
                .Include(o => o.id_pacienteNavigation)
                .Include(o => o.id_medicoNavigation)
                .Include(o => o.detalle_ordens!)
                    .ThenInclude(d => d.id_examenNavigation)
                .FirstOrDefaultAsync(o => o.id_orden == IdOrden);

            if (entidadOrden == null)
                return null;

            int edadPaciente = 0;

            if (entidadOrden.id_pacienteNavigation?.fecha_nac_paciente is DateOnly fechaNacimiento)
            {
                var hoy = DateTime.Today;
                var fechaNac = fechaNacimiento.ToDateTime(TimeOnly.MinValue);

                edadPaciente = hoy.Year - fechaNac.Year;
                if (fechaNac > hoy.AddYears(-edadPaciente))
                    edadPaciente--;
            }

            var ordenDto = new OrdenTicketDto
            {
                NumeroOrden = entidadOrden.numero_orden,
                FechaOrden = entidadOrden.fecha_orden.ToDateTime(TimeOnly.MinValue),
                NombrePaciente = entidadOrden.id_pacienteNavigation?.nombre_paciente ?? "(Sin nombre)",
                CedulaPaciente = entidadOrden.id_pacienteNavigation?.cedula_paciente ?? "(Sin cédula)",
                EdadPaciente = edadPaciente,
                NombreMedico = entidadOrden.id_medicoNavigation?.nombre_medico ?? "(Sin médico)",
                Total = entidadOrden.total,
                TotalPagado = entidadOrden.total_pagado ?? 0,
                SaldoPendiente = entidadOrden.saldo_pendiente ?? 0,
                TipoPago = entidadOrden.estado_pago ?? "Desconocido",
                Examenes = entidadOrden.detalle_ordens.Select(d => new ExamenTicketDto
                {
                    NombreExamen = d.id_examenNavigation?.nombre_examen ?? "(Sin examen)",
                    Precio = d.precio ?? 0
                }).ToList()
            };
            return ServicioPdfTicket.GenerarTicketOrden(ordenDto);
        }

        public async Task<bool> AnularOrdenCompletaAsync(int IdOrden)
        {
            var entidadOrden = await Contexto.ordens
                .Include(o => o.detalle_ordens)
                .Include(o => o.resultados)
                    .ThenInclude(r => r.detalle_resultados)
                .Include(o => o.pagos)
                    .ThenInclude(p => p.detalle_pagos)
                .FirstOrDefaultAsync(o => o.id_orden == IdOrden);

            if (entidadOrden == null || entidadOrden.anulado == true)
                return false;

            entidadOrden.anulado = true;
            entidadOrden.estado_pago = "ANULADO";

            foreach (var detalle in entidadOrden.detalle_ordens)
                detalle.anulado = true;

            foreach (var resultado in entidadOrden.resultados)
            {
                resultado.anulado = true;
                foreach (var detalleResultado in resultado.detalle_resultados)
                    detalleResultado.anulado = true;
            }

            foreach (var pago in entidadOrden.pagos)
            {
                pago.anulado = true;
                foreach (var detallePago in pago.detalle_pagos)
                    detallePago.anulado = true;
            }

            await Contexto.SaveChangesAsync();
            return true;
        }

        public async Task<List<object>> ObtenerOrdenesPorPacienteAsync(int IdPaciente)
        {
            var ListaOrdenes = await Contexto.ordens
                .Include(o => o.id_pacienteNavigation)
                .Where(o => o.id_paciente == IdPaciente)
                .Select(o => new
                {
                    IdOrden = o.id_orden,
                    NumeroOrden = o.numero_orden,
                    FechaOrden = o.fecha_orden,
                    Total = o.total,
                    TotalPagado = o.total_pagado ?? 0,
                    SaldoPendiente = o.saldo_pendiente ?? 0,
                    EstadoPago = o.estado_pago,
                    Anulado = o.anulado ?? false
                })
                .OrderByDescending(x => x.IdOrden)
                .ToListAsync();

            return ListaOrdenes.Cast<object>().ToList();
        }

        public async Task<OrdenDetalleDto?> ObtenerDetalleOrdenPacienteAsync(int IdOrden)
        {
            return await ObtenerDetalleOrdenOriginalAsync(IdOrden);
        }

        private static readonly ConcurrentDictionary<int, bool> OrdenesNotificadas = new();

        public async Task VerificarYNotificarResultadosCompletosAsync(int IdOrden)
        {
            var entidadOrden = await Contexto.ordens
                .Include(o => o.id_pacienteNavigation)
                .Include(o => o.detalle_ordens)
                    .ThenInclude(d => d.id_resultadoNavigation)
                .FirstOrDefaultAsync(o => o.id_orden == IdOrden);

            if (entidadOrden == null) return;

            bool todosConResultado = entidadOrden.detalle_ordens.All(d => d.id_resultado != null);
            if (!todosConResultado || OrdenesNotificadas.ContainsKey(IdOrden))
                return;

            var correoPaciente = entidadOrden.id_pacienteNavigation?.correo_electronico_paciente;
            var nombrePaciente = entidadOrden.id_pacienteNavigation?.nombre_paciente;
            if (string.IsNullOrWhiteSpace(correoPaciente)) return;

            string asunto = "Resultados disponibles - Laboratorio La Inmaculada";
            string cuerpo = $@"
        <div style='font-family:Arial,sans-serif;color:#333;'>
            <h3>Estimado/a {nombrePaciente},</h3>
            <p>Le informamos que todos los resultados de su orden <strong>{entidadOrden.numero_orden}</strong> están disponibles.</p>
            <p>Puede consultarlos ingresando a su cuenta en:
            <a href='https://labinmaculada.com/login'>Portal de Resultados</a>.</p>
            <p style='margin-top:20px;'>Gracias por confiar en nosotros.<br>
            <strong>Laboratorio Clínico La Inmaculada</strong></p>
        </div>";

            var emailService = new EmailService(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());
            await emailService.EnviarCorreoAsync(correoPaciente, nombrePaciente ?? "Paciente", asunto, cuerpo);
            OrdenesNotificadas.TryAdd(IdOrden, true);

        }


    }
}
