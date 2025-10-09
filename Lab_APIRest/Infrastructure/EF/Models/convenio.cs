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

    public bool? anulado { get; set; }

    public virtual ICollection<detalle_convenio> detalle_convenios { get; set; } = new List<detalle_convenio>();

    public virtual medico? id_medicoNavigation { get; set; }
}
