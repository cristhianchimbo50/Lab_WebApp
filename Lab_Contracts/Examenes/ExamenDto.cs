using System.ComponentModel.DataAnnotations;

namespace Lab_Contracts.Examenes
{
    public class ExamenDto
    {
        public int IdExamen { get; set; }
        [Required]
        public string NombreExamen { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public bool Anulado { get; set; }

        public string? TituloExamen { get; set; }

        // Compatibilidad con versión previa (se derivan de catálogos)
        public string? Estudio { get; set; }
        public string? TipoMuestra { get; set; }
        public string? TiempoEntrega { get; set; }
        public string? TipoExamen { get; set; }
        public string? Tecnica { get; set; }
        public string? ValorReferencia { get; set; }
        public string? Unidad { get; set; }

        public int? TiempoEntregaMinutos { get; set; }
        public int? IdEstudio { get; set; }
        public string? NombreEstudio { get; set; }
        public int? IdGrupoExamen { get; set; }
        public string? NombreGrupoExamen { get; set; }
        public int? IdTipoMuestra { get; set; }
        public string? NombreTipoMuestra { get; set; }
        public int? IdTipoExamen { get; set; }
        public string? NombreTipoExamen { get; set; }
        public int? IdTecnica { get; set; }
        public string? NombreTecnica { get; set; }
        public int? IdTipoRegistro { get; set; }
        public string? NombreTipoRegistro { get; set; }

        public List<ReferenciaExamenDto> Referencias { get; set; } = new();
    }
}
