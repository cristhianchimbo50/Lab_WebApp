using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class detalle_convenio
{
    public int id_detalle_convenio { get; set; }

    public int? id_convenio { get; set; }

    public decimal subtotal { get; set; }

    public int id_orden { get; set; }

    public virtual convenio? id_convenioNavigation { get; set; }

    public virtual orden id_ordenNavigation { get; set; } = null!;
}
