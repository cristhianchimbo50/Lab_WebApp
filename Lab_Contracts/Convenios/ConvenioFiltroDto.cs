using System;

namespace Lab_Contracts.Convenios
{
    public class ConvenioFiltroDto
    {
        public string? CriterioBusqueda { get; set; }
        public string? ValorBusqueda { get; set; }
        public DateOnly? FechaDesde { get; set; }
        public DateOnly? FechaHasta { get; set; }
        public bool IncluirAnuladas { get; set; } = true;
        public bool IncluirVigentes { get; set; } = true;

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool SortAsc { get; set; } = false;
    }
}
