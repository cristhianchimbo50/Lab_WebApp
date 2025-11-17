using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class ExamenComposicion
{
    public int IdExamenPadre { get; set; }

    public int IdExamenHijo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; }

    public virtual Examen IdExamenHijoNavigation { get; set; } = null!;

    public virtual Examen IdExamenPadreNavigation { get; set; } = null!;
}
