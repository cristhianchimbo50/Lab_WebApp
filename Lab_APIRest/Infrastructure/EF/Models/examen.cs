using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class examen
{
    public int id_examen { get; set; }

    public string? nombre_examen { get; set; }

    public string? titulo_examen { get; set; }

    public decimal precio { get; set; }

    public int? tiempo_entrega_minutos { get; set; }

    public int? id_estudio { get; set; }

    public int? id_grupo_examen { get; set; }

    public int? id_tipo_muestra { get; set; }

    public int? id_tipo_examen { get; set; }

    public int? id_tecnica { get; set; }

    public int? id_tipo_registro { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public bool activo { get; set; }

    public virtual estudio? estudio_navigation { get; set; }

    public virtual grupo_examen? grupo_examen_navigation { get; set; }

    public virtual tipo_muestra? tipo_muestra_navigation { get; set; }

    public virtual tipo_examen? tipo_examen_navigation { get; set; }

    public virtual tecnica? tecnica_navigation { get; set; }

    public virtual tipo_registro? tipo_registro_navigation { get; set; }

    public virtual ICollection<referencia_examen> referencia_examen { get; set; } = new List<referencia_examen>();

    public virtual ICollection<detalle_orden> detalle_orden { get; set; } = new List<detalle_orden>();

    public virtual ICollection<detalle_resultado> detalle_resultado { get; set; } = new List<detalle_resultado>();

    public virtual ICollection<examen_composicion> examen_composicion_hijo { get; set; } = new List<examen_composicion>();

    public virtual ICollection<examen_composicion> examen_composicion_padre { get; set; } = new List<examen_composicion>();

    public virtual ICollection<examen_reactivo> examen_reactivo { get; set; } = new List<examen_reactivo>();
}