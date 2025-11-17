using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class resultado
{
    public int id_resultado { get; set; }

    public string numero_resultado { get; set; } = null!;


    public DateTime fecha_resultado { get; set; }

    public string? observaciones { get; set; }

    public int? id_orden { get; set; }

    public bool? anulado { get; set; }

    public virtual ICollection<detalle_orden> detalle_ordens { get; set; } = new List<detalle_orden>();

    public virtual ICollection<detalle_resultado> detalle_resultados { get; set; } = new List<detalle_resultado>();

    public virtual orden? id_ordenNavigation { get; set; }

}
