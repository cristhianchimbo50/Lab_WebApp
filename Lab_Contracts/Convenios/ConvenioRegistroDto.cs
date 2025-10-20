namespace Lab_Contracts.Convenios
{
    public class ConvenioRegistroDto
    {
        public int? IdMedico { get; set; }
        public DateOnly FechaConvenio { get; set; }
        public decimal PorcentajeComision { get; set; }
        public decimal MontoTotal { get; set; }
        public List<DetalleConvenioRegistroDto> Ordenes { get; set; } = new();
    }

    public class DetalleConvenioRegistroDto
    {
        public int IdOrden { get; set; }
        public decimal Subtotal { get; set; }
    }
}
