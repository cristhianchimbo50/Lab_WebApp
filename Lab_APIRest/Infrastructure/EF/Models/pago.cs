using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Pago
{
    public int IdPago { get; set; }

    public int? IdOrden { get; set; }

    public DateTime? FechaPago { get; set; }

    public decimal? MontoPagado { get; set; }

    public string? Observacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<DetallePago> DetallePago { get; set; } = new List<DetallePago>();

    public virtual Orden? IdOrdenNavigation { get; set; }
}
