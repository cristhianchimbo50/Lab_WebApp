namespace Lab_Contracts.Ordenes
{
    public class DetallePagoDto
    {
        public int IdDetallePago { get; set; }
        public int IdPago { get; set; }
        public string TipoPago { get; set; }
        public decimal Monto { get; set; }
        public DateTime? FechaAnulacion { get; set; }
    }
}
