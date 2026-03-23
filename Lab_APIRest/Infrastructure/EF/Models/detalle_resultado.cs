using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class detalle_resultado
{
    public int id_resultado { get; set; }

    public int id_examen { get; set; }

    public string valor { get; set; } = null!;

    public virtual examen examen_navigation { get; set; } = null!;

    public virtual resultado resultado_navigation { get; set; } = null!;

    public virtual ICollection<movimiento_reactivo> movimiento_reactivo { get; set; } = new List<movimiento_reactivo>();
}
