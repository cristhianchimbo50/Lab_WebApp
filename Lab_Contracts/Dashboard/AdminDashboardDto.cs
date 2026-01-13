namespace Lab_Contracts.Dashboard
{
    public class AdminDashboardDto
    {
        public int OrdenesHoy { get; set; }
        public int ResultadosPendientesAprobacion { get; set; }
        public int ReactivosCriticos { get; set; }
        public int UsuariosTotales { get; set; }
        public int ResultadosCorreccion { get; set; }
    }

    public class AdminResultadoPendienteDto
    {
        public int IdResultado { get; set; }
        public int IdOrden { get; set; }
        public string NumeroOrden { get; set; } = string.Empty;
        public string Paciente { get; set; } = string.Empty;
        public string TipoExamen { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }

    public class AdminAlertasDto
    {
        public List<string> Mensajes { get; set; } = new();
    }

    public class AdminHomeDto
    {
        public AdminDashboardDto Resumen { get; set; } = new();
        public List<AdminResultadoPendienteDto> Pendientes { get; set; } = new();
        public AdminAlertasDto Alertas { get; set; } = new();
    }
}
