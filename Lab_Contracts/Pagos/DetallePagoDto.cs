namespace Lab_Contracts.Pagos
{
    public class DetallePagoDto
    {
        public int IdDetallePago { get; set; }
        public int IdPago { get; set; }

        public string TipoPago { get; set; }
        public decimal Monto { get; set; }

        public bool? Anulado { get; set; }

        public DateTime? FechaPago { get; set; }
    }
}
