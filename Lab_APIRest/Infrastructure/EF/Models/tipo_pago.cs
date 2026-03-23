using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class tipo_pago
{
    public int id_tipo_pago { get; set; }

    public string? nombre { get; set; }

    public string? descripcion { get; set; }

    public bool activo { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public virtual ICollection<detalle_pago> detalle_pago { get; set; } = new List<detalle_pago>();
}