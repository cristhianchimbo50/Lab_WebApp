namespace Lab_Contracts.Convenios
{
    public class OrdenDisponibleDto
    {
        public int IdOrden { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string Paciente { get; set; } = string.Empty;
        public DateOnly FechaOrden { get; set; }
        public decimal Total { get; set; }
    }
}
