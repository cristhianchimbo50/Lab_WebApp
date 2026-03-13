using System;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class ReferenciaExamen
{
    public int IdReferenciaExamen { get; set; }
    public int IdExamen { get; set; }
    public decimal? ValorMin { get; set; }
    public decimal? ValorMax { get; set; }
    public string? ValorTexto { get; set; }
    public string? Unidad { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaFin { get; set; }

    public virtual Examen IdExamenNavigation { get; set; } = null!;
}
