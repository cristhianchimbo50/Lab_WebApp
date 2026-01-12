using System;
using System.Collections.Generic;

namespace Lab_Contracts.Resultados
{
    public class ResultadoActualizarDto
    {
        public int IdResultado { get; set; }
        public DateTime? FechaResultado { get; set; }
        public string? ObservacionesGenerales { get; set; }
        public List<ResultadoExamenDto> Examenes { get; set; } = new();
    }
}
