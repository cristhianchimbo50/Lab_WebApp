using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class detalle_pago
{
    public int id_detalle_pago { get; set; }

    public int? id_pago { get; set; }

    public string? tipo_pago { get; set; }

    public decimal? monto { get; set; }
    public bool? anulado { get; set; }

    public virtual pago? id_pagoNavigation { get; set; }
}
