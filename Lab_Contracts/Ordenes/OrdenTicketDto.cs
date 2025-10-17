using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Ordenes
{
    public class OrdenTicketDto
    {
        public string NumeroOrden { get; set; }
        public DateTime FechaOrden { get; set; }
        public string NombrePaciente { get; set; }
        public string CedulaPaciente { get; set; }
        public int EdadPaciente { get; set; }
        public string NombreMedico { get; set; }
        public decimal Total { get; set; }
        public decimal TotalPagado { get; set; }
        public decimal SaldoPendiente { get; set; }
        public string TipoPago { get; set; }
        public List<ExamenTicketDto> Examenes { get; set; } = new();
    }

    public class ExamenTicketDto
    {
        public string NombreExamen { get; set; }
        public decimal Precio { get; set; }
    }
}

