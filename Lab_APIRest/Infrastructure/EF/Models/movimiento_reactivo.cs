using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class movimiento_reactivo
{
    public int id_movimiento_reactivo { get; set; }

    public int? id_reactivo { get; set; }

    public string? tipo_movimiento { get; set; }

    public decimal? cantidad { get; set; }

    public DateTime fecha_movimiento { get; set; }

    public int? id_orden { get; set; }

    public string? observacion { get; set; }

    public virtual orden? id_ordenNavigation { get; set; }

    public virtual reactivo? id_reactivoNavigation { get; set; }
}
