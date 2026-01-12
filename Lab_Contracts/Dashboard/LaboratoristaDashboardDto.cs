namespace Lab_Contracts.Dashboard
{
    public class LaboratoristaDashboardDto
    {
        public int OrdenesPendientes { get; set; }
        public int ResultadosPorRegistrar { get; set; }
        public int ReactivosStockBajo { get; set; }
    }

    public class LaboratoristaOrdenRecienteDto
    {
        public int IdOrden { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string NombrePaciente { get; set; } = string.Empty;
        public string EstadoOrden { get; set; } = string.Empty;
    }

    public class LaboratoristaHomeDto
    {
        public LaboratoristaDashboardDto Resumen { get; set; } = new();
        public List<LaboratoristaOrdenRecienteDto> OrdenesRecientes { get; set; } = new();
    }
}
