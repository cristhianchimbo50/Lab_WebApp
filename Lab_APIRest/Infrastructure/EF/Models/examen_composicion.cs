using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class examen_composicion
{
    public int id_examen_padre { get; set; }

    public int id_examen_hijo { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public bool activo { get; set; }

    public virtual examen examen_hijo_navigation { get; set; } = null!;

    public virtual examen examen_padre_navigation { get; set; } = null!;
}
