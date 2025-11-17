using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class ExamenReactivo
{
    public int IdExamen { get; set; }

    public int IdReactivo { get; set; }

    public decimal CantidadUsada { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; }

    public virtual Examen IdExamenNavigation { get; set; } = null!;

    public virtual Reactivo IdReactivoNavigation { get; set; } = null!;
}
