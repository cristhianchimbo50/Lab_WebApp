using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class examen_reactivo
{
    public int id_examen { get; set; }

    public int id_reactivo { get; set; }

    public decimal cantidad_usada { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public bool activo { get; set; }

    public virtual examen examen_navigation { get; set; } = null!;

    public virtual reactivo reactivo_navigation { get; set; } = null!;
}
