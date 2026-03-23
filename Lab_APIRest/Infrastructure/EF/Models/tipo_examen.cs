using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class tipo_examen
{
    public int id_tipo_examen { get; set; }

    public string? nombre { get; set; }

    public bool activo { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public virtual ICollection<examen> examen { get; set; } = new List<examen>();
}