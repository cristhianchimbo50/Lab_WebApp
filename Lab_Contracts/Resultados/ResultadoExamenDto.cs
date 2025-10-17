using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Resultados
{
    public class ResultadoExamenDto
    {
        public int IdExamen { get; set; }
        public string NombreExamen { get; set; } = "";
        public string? Valor { get; set; }
        public string? Unidad { get; set; }
        public string? Observacion { get; set; }
        public string ValorReferencia { get; set; } = "";
        public string? TituloExamen { get; set; }
    }
}