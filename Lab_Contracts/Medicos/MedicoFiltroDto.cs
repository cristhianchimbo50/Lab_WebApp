namespace Lab_Contracts.Medicos
{
    public class MedicoFiltroDto
    {
        public string? CriterioBusqueda { get; set; } 
        public string? ValorBusqueda { get; set; }
        public bool IncluirAnulados { get; set; } = true;
        public bool IncluirVigentes { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; } = nameof(MedicoDto.IdMedico);
        public bool SortAsc { get; set; } = true;
    }
}
