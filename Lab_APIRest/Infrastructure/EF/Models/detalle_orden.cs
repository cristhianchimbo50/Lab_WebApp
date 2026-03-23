using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class detalle_orden
{
    public int id_orden { get; set; }

    public int id_examen { get; set; }

    public decimal? precio { get; set; }

    public virtual examen examen_navigation { get; set; } = null!;

    public virtual orden orden_navigation { get; set; } = null!;
}
