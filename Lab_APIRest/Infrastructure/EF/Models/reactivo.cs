using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class reactivo
{
    public int id_reactivo { get; set; }

    public string nombre_reactivo { get; set; } = null!;

    public string? fabricante { get; set; }

    public string? unidad { get; set; }

    public decimal? cantidad_disponible { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public bool activo { get; set; }

    public virtual ICollection<examen_reactivo> examen_reactivo { get; set; } = new List<examen_reactivo>();

    public virtual ICollection<movimiento_reactivo> movimiento_reactivo { get; set; } = new List<movimiento_reactivo>();
}