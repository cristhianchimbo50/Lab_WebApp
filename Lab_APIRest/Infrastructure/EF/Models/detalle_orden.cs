using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class detalle_orden
{
    public int id_detalle_orden { get; set; }

    public int? id_orden { get; set; }

    public int? id_examen { get; set; }

    public decimal? precio { get; set; }

    public int? id_resultado { get; set; }
    public bool? anulado { get; set; }

    public virtual examen? id_examenNavigation { get; set; }

    public virtual orden? id_ordenNavigation { get; set; }

    public virtual resultado? id_resultadoNavigation { get; set; }
}
