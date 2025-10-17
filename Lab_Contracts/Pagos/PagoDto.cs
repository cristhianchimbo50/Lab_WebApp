using Lab_Contracts.Ordenes;

namespace Lab_Contracts.Pagos
{
    public class PagoDto
    {
        public int IdPago { get; set; }
        public int IdOrden { get; set; }
        public DateTime? FechaPago { get; set; }
        public decimal MontoPagado { get; set; }
        public string Observacion { get; set; }
        public bool Anulado { get; set; }
        public List<DetallePagoDto> DetallePagos { get; set; } = new();
    }
}
