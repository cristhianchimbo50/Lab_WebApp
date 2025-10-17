using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Resultados
{
    public class ResultadoCompletoDto
    {
        public string NumeroOrden { get; set; } = string.Empty;
        public string NumeroResultado { get; set; } = string.Empty;
        public DateTime FechaResultado { get; set; }

        public string NombrePaciente { get; set; } = string.Empty;
        public string CedulaPaciente { get; set; } = string.Empty;
        public int EdadPaciente { get; set; }
        public string MedicoSolicitante { get; set; } = string.Empty;

        public List<ResultadoDetalleDto> Detalles { get; set; } = new();
    }
}
