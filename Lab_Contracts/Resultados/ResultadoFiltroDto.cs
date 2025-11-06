using System;

namespace Lab_Contracts.Resultados
{
    public class ResultadoFiltroDto
    {
        public string? NumeroOrden { get; set; }
        public string? NumeroResultado { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool? Anulado { get; set; }

        public int? IdPaciente { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = nameof(ResultadoListadoDto.FechaResultado);
        public bool SortAsc { get; set; } = false;
    }
}




