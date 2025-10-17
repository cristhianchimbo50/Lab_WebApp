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
        public decimal Valor { get; set; }
        public string? Unidad { get; set; }
        public string? ValorReferencia { get; set; }
        public string? Observacion { get; set; }
    }
}