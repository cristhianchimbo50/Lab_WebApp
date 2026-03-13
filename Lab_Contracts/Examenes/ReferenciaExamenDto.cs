namespace Lab_Contracts.Examenes
{
    public class ReferenciaExamenDto
    {
        public int IdReferenciaExamen { get; set; }
        public int IdExamen { get; set; }
        public decimal? ValorMin { get; set; }
        public decimal? ValorMax { get; set; }
        public string? ValorTexto { get; set; }
        public string? Unidad { get; set; }
        public bool Activo { get; set; } = true;
    }
}
