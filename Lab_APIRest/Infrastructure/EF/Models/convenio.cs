using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class convenio
{
    public int id_convenio { get; set; }

    public int? id_medico { get; set; }

    public DateOnly fecha_convenio { get; set; }

    public decimal porcentaje_comision { get; set; }

    public decimal monto_total { get; set; }

    public DateTime? fecha_fin { get; set; }

    public bool activo { get; set; }

    public virtual medico? medico_navigation { get; set; }

    public virtual ICollection<orden> orden { get; set; } = new List<orden>();
}
