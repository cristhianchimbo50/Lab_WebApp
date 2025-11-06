using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Pagos
{
    public class PagoFiltroDto
    {
        public string? NumeroOrden { get; set; }
        public string? Cedula { get; set; }
        public string? NombrePaciente { get; set; }

        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public bool? IncluirAnulados { get; set; }

        public string? EstadoPago { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortAsc { get; set; } = false;
    }
}
