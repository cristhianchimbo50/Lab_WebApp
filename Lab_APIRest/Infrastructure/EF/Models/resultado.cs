using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class resultado
{
    public int id_resultado { get; set; }

    public string numero_resultado { get; set; } = null!;

    public DateTime fecha_resultado { get; set; }

    public string? observaciones { get; set; }

    public DateTime? fecha_fin { get; set; }

    public bool activo { get; set; }

    public int id_orden { get; set; }

    public int id_estado_resultado { get; set; }

    public string? observacion_revision { get; set; }

    public int? id_revisor { get; set; }

    public DateTime? fecha_revision { get; set; }

    public virtual ICollection<detalle_resultado> detalle_resultado { get; set; } = new List<detalle_resultado>();

    public virtual orden orden_navigation { get; set; } = null!;

    public virtual usuario? revisor_navigation { get; set; }

    public virtual estado_resultado estado_resultado_navigation { get; set; } = null!;
}