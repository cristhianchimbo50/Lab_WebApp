namespace Lab_Contracts.Reactivos
{
    public class MovimientoReactivoEgresoDto
    {
        public int IdReactivo { get; set; }
        public string NombreReactivo { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public decimal Cantidad { get; set; }
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;
        public string Observacion { get; set; } = string.Empty;
        public int? IdDetalleResultado { get; set; }
        public string? NumeroResultado { get; set; }
    }
}
