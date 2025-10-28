using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Resultados
{
    public class ResultadoDetalleDto
    {
        public int IdResultado { get; set; }
        public string NumeroResultado { get; set; } = "";
        public string CedulaPaciente { get; set; } = "";
        public string NombrePaciente { get; set; } = "";
        public DateTime FechaResultado { get; set; }
        public string Observaciones { get; set; } = "";
        public bool Anulado { get; set; }
        public List<DetalleResultadoDto> Detalles { get; set; } = new();
        public int IdPaciente { get; set; }
    }

    public class DetalleResultadoDto
    {
        public int IdDetalleResultado { get; set; }
        public string NombreExamen { get; set; } = "";
        public string Valor { get; set; } = "";
        public string Unidad { get; set; } = "";
        public string Observacion { get; set; } = "";
        public string ValorReferencia { get; set; } = "";
        public bool Anulado { get; set; }
        public string? TituloExamen { get; set; }

    }
}
