using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Reactivos
{
    public class MovimientoReactivoIngresoDto
    {
        public int IdReactivo { get; set; }
        public string NombreReactivo { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;

        public decimal Cantidad { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string Observacion { get; set; } = string.Empty;
    }
}
