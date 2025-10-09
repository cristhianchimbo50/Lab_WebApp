using System.ComponentModel.DataAnnotations;

namespace Lab_Contracts.Ordenes
{
    public class OrdenDto
    {
        public int IdOrden { get; set; }
        public string NumeroOrden { get; set; }
        public int IdPaciente { get; set; }
        public DateOnly FechaOrden { get; set; }
        public decimal Total { get; set; }
        public decimal SaldoPendiente { get; set; }
        public decimal TotalPagado { get; set; }
        public string EstadoPago { get; set; }
        public bool Anulado { get; set; }
        public bool LiquidadoConvenio { get; set; }
        public int? IdMedico { get; set; }
        public string Observacion { get; set; }
        public List<DetalleOrdenDto> Detalles { get; set; }
    }


}


