using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class pago
{
    public int id_pago { get; set; }

    public int? id_orden { get; set; }

    public DateTime fecha_pago { get; set; }

    public decimal monto_recibido { get; set; }

    public decimal monto_aplicado { get; set; }

    public decimal monto_vuelto { get; set; }

    public string? observacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public bool activo { get; set; }

    public virtual ICollection<detalle_pago> detalle_pago { get; set; } = new List<detalle_pago>();

    public virtual orden? orden_navigation { get; set; }
}