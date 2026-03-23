using System;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class referencia_examen
{
    public int id_referencia_examen { get; set; }

    public int id_examen { get; set; }

    public decimal? valor_min { get; set; }

    public decimal? valor_max { get; set; }

    public string? valor_texto { get; set; }

    public string? unidad { get; set; }

    public bool activo { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public virtual examen examen_navigation { get; set; } = null!;
}