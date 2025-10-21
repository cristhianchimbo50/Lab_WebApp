using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Reactivos
{
    public class MovimientoReactivoFiltroDto
    {
        public string? IdMovimientoReactivo { get; set; }
        public string? NombreReactivo { get; set; }
        public string? Observacion { get; set; }

        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }

        public bool IncluirIngresos { get; set; } = true;
        public bool IncluirEgresos { get; set; } = true;
    }
}
