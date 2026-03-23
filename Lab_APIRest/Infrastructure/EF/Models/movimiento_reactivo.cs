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

    public string? observacion { get; set; }

    public int? id_detalle_resultado { get; set; }

    public int? id_resultado { get; set; }

    public int? id_examen { get; set; }

    public virtual detalle_resultado? detalle_resultado_navigation { get; set; }

    public virtual reactivo? reactivo_navigation { get; set; }
}