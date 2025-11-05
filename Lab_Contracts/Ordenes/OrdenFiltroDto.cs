using System;

namespace Lab_Contracts.Ordenes
{
    public class OrdenFiltroDto
    {
        public string? CriterioBusqueda { get; set; } // numero, cedula, nombre, estadoPago
        public string? ValorBusqueda { get; set; }
        public DateOnly? FechaDesde { get; set; }
        public DateOnly? FechaHasta { get; set; }
        public bool IncluirAnuladas { get; set; } = true;
        public bool IncluirVigentes { get; set; } = true;
        public int? IdPaciente { get; set; }

        // Paginación y ordenamiento
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } // NumeroOrden, CedulaPaciente, NombrePaciente, FechaOrden, Total, TotalPagado, SaldoPendiente
        public bool SortAsc { get; set; } = false;
    }
}
