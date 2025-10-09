using System.ComponentModel.DataAnnotations;

namespace Lab_Contracts.Examenes
{
    public class ExamenDto
    {
        public int IdExamen { get; set; }
        [Required]
        public string NombreExamen { get; set; } = string.Empty;
        public string? ValorReferencia { get; set; }
        public string? Unidad { get; set; }
        public decimal Precio { get; set; }
        public bool Anulado { get; set; }
        public string? Estudio { get; set; }
        public string? TipoMuestra { get; set; }
        public string? TiempoEntrega { get; set; }
        public string? TipoExamen { get; set; }
        public string? Tecnica { get; set; }
        public string? TituloExamen { get; set; }
    }
}
