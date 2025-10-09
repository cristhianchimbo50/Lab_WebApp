using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class pago
{
    public int id_pago { get; set; }

    public int? id_orden { get; set; }

    public DateTime? fecha_pago { get; set; }

    public decimal? monto_pagado { get; set; }

    public string? observacion { get; set; }

    public bool? anulado { get; set; }

    public virtual ICollection<detalle_pago> detalle_pagos { get; set; } = new List<detalle_pago>();

    public virtual orden? id_ordenNavigation { get; set; }
}
