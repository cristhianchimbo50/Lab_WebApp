using System;

namespace Lab_Contracts.Examenes
{
    public class ExamenFiltroDto
    {
        public string? CriterioBusqueda { get; set; }
        public string? ValorBusqueda { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = nameof(ExamenDto.NombreExamen);
        public bool SortAsc { get; set; } = true;
        public bool IncluirAnulados { get; set; } = true;
        public bool IncluirVigentes { get; set; } = true;
    }
}
