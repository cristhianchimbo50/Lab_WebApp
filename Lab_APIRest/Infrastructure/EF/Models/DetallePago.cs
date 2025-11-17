using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class DetallePago
{
    public int IdDetallePago { get; set; }

    public int? IdPago { get; set; }

    public string? TipoPago { get; set; }

    public decimal? Monto { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; }

    public virtual Pago? IdPagoNavigation { get; set; }
}
