using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class detalle_resultado
{
    public int id_detalle_resultado { get; set; }

    public int? id_resultado { get; set; }

    public int? id_examen { get; set; }

    public string? valor { get; set; }

    public string? unidad { get; set; }

    public string? observacion { get; set; }

    public bool? anulado { get; set; }

    public string? valor_referencia { get; set; }

    public virtual examen? id_examenNavigation { get; set; }

    public virtual resultado? id_resultadoNavigation { get; set; }
}
