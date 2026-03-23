using Lab_Contracts.Pagos;

namespace Lab_Contracts.Ordenes
{
    public class OrdenPagoGuardarDto
    {
        public OrdenCompletaDto Orden { get; set; } = new();
        public PagoDto Pago { get; set; } = new();
    }
}
