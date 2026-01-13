namespace Lab_Contracts.Dashboard
{
    public class RecepcionistaDashboardDto
    {
        public int OrdenesRegistradas { get; set; }
        public int CuentasPorCobrar { get; set; }
        public int ResultadosDisponibles { get; set; }
    }

    public class RecepcionistaOrdenRecienteDto
    {
        public int IdOrden { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string Paciente { get; set; } = string.Empty;
        public string Medico { get; set; } = string.Empty;
        public string EstadoOrden { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
        public bool ListoParaEntrega { get; set; }
    }

    public class RecepcionistaAlertasDto
    {
        public List<string> Mensajes { get; set; } = new();
    }

    public class RecepcionistaHomeDto
    {
        public RecepcionistaDashboardDto Resumen { get; set; } = new();
        public List<RecepcionistaOrdenRecienteDto> OrdenesRecientes { get; set; } = new();
        public RecepcionistaAlertasDto Alertas { get; set; } = new();
    }
}
