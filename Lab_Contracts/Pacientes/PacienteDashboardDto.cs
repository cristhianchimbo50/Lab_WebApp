namespace Lab_Contracts.Pacientes
{
    public class PacienteDashboardDto
    {
        public int OrdenesActivas { get; set; }
        public int ResultadosDisponibles { get; set; }
        public DateOnly? FechaUltimaOrden { get; set; }
        public string? NumeroUltimaOrden { get; set; }
    }
}
