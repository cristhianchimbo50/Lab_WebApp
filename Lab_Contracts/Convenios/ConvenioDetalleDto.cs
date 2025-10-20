namespace Lab_Contracts.Convenios
{
    public class ConvenioDetalleDto
    {
        public int IdConvenio { get; set; }
        public int? IdMedico { get; set; }
        public string? NombreMedico { get; set; }
        public DateOnly FechaConvenio { get; set; }
        public decimal PorcentajeComision { get; set; }
        public decimal MontoTotal { get; set; }
        public bool? Anulado { get; set; }

        public List<DetalleConvenioDto> Ordenes { get; set; } = new();
    }
}
