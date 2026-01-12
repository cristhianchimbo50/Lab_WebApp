using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Ordenes
{
    public class OrdenDetalleDto
    {
        public int IdOrden { get; set; }
        public string? NumeroOrden { get; set; }

        public DateOnly FechaOrden { get; set; }
        public string? EstadoPago { get; set; }

        public int IdPaciente { get; set; }
        public string? CedulaPaciente { get; set; }
        public string? NombrePaciente { get; set; }
        public string? DireccionPaciente { get; set; }
        public string? CorreoPaciente { get; set; }
        public string? TelefonoPaciente { get; set; }

        public int? IdMedico { get; set; }
        public string? NombreMedico { get; set; }

        public bool? Anulado { get; set; }

        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }

        public List<ExamenDetalleDto> Examenes { get; set; } = new();
    }

    public class ExamenDetalleDto
    {
        public int IdExamen { get; set; }
        public string? NombreExamen { get; set; }
        public string? NombreEstudio { get; set; }
        public int? IdResultado { get; set; }
        public string? NumeroResultado { get; set; }
        public string? EstadoResultado { get; set; }

        public bool Seleccionado { get; set; } = false;
    }
}
