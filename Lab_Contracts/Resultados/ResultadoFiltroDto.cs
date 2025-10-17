using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Resultados
{
    public class ResultadoFiltroDto
    {
        public string? NumeroResultado { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool? Anulado { get; set; }
    }
}




