using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class DetalleOrden
{
    public int IdOrden { get; set; }

    public int IdExamen { get; set; }

    public decimal? Precio { get; set; }

    public virtual Examen IdExamenNavigation { get; set; } = null!;

    public virtual Orden IdOrdenNavigation { get; set; } = null!;
}
