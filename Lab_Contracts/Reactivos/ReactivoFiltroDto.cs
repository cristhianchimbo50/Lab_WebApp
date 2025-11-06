namespace Lab_Contracts.Reactivos
{
    public class ReactivoFiltroDto
    {
        public string? CriterioBusqueda { get; set; }
        public string? ValorBusqueda { get; set; }
        public bool IncluirAnulados { get; set; } = true;
        public bool IncluirVigentes { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = nameof(ReactivoDto.NombreReactivo);
        public bool SortAsc { get; set; } = true;
    }
}
