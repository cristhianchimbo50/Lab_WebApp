using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Resultados
{
    public class ResultadoListadoDto
    {
        public int IdResultado { get; set; }
        public string NumeroResultado { get; set; } = "";
        public string CedulaPaciente { get; set; } = "";
        public string NombrePaciente { get; set; } = "";
        public DateTime FechaResultado { get; set; }
        public bool Anulado { get; set; }
        public string Observaciones { get; set; } = "";
        public int IdPaciente { get; set; }
    }
}
