using System;
using System.Collections.Generic;

namespace Lab_Contracts.Resultados
{
    public class ResultadoGuardarDto
    {
        public int IdOrden { get; set; }
        public int IdPaciente { get; set; }
        public DateTime? FechaResultado { get; set; }
        public List<ResultadoExamenDto> Examenes { get; set; } = new();
        public string? ObservacionesGenerales { get; set; }
    }
}
