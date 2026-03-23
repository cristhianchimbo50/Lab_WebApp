using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class estado_resultado
{
    public int id_estado_resultado { get; set; }

    public string nombre { get; set; } = null!;

    public string? descripcion { get; set; }

    public bool activo { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public virtual ICollection<resultado> resultado { get; set; } = new List<resultado>();
}
