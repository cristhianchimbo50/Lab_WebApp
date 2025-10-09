using System.ComponentModel.DataAnnotations;

namespace Lab_Contracts.Ordenes
{
    public class DetalleOrdenDto
    {
        public int IdDetalleOrden { get; set; }
        public int IdOrden { get; set; }
        public int IdExamen { get; set; }
        public decimal Precio { get; set; }
        public int? IdResultado { get; set; }
    }

}

