using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class examen
{
    public int id_examen { get; set; }

    public string? nombre_examen { get; set; }

    public string? valor_referencia { get; set; }

    public string? unidad { get; set; }

    public decimal precio { get; set; }

    public bool? anulado { get; set; }

    public string? estudio { get; set; }

    public string? tipo_muestra { get; set; }

    public string? tiempo_entrega { get; set; }

    public string? tipo_examen { get; set; }

    public string? tecnica { get; set; }

    public string? titulo_examen { get; set; }

    public virtual ICollection<detalle_orden> detalle_ordens { get; set; } = new List<detalle_orden>();

    public virtual ICollection<detalle_resultado> detalle_resultados { get; set; } = new List<detalle_resultado>();

    public virtual ICollection<examen_reactivo> examen_reactivos { get; set; } = new List<examen_reactivo>();

    public virtual ICollection<examen_composicion> id_examen_hijos { get; set; } = new List<examen_composicion>();
    public virtual ICollection<examen_composicion> id_examen_padres { get; set; } = new List<examen_composicion>();

}
