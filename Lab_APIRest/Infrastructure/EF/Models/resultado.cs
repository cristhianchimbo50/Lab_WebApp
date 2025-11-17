using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Resultado
{
    public int IdResultado { get; set; }

    public string NumeroResultado { get; set; } = null!;

    public DateTime FechaResultado { get; set; }

    public string? Observaciones { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; }

    public int IdOrden { get; set; }

    public virtual ICollection<DetalleResultado> DetalleResultado { get; set; } = new List<DetalleResultado>();

    public virtual Orden IdOrdenNavigation { get; set; } = null!;
}
