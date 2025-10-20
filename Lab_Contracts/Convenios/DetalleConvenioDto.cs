namespace Lab_Contracts.Convenios
{
    public class DetalleConvenioDto
    {
        public int IdDetalleConvenio { get; set; }
        public int IdConvenio { get; set; }
        public int IdOrden { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string? Paciente { get; set; }
        public DateOnly? FechaOrden { get; set; }
        public decimal Subtotal { get; set; }
    }
}
