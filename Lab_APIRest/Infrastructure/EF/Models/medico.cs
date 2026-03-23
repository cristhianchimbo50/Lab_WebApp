using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class medico
{
    public int id_medico { get; set; }

    public string nombre_medico { get; set; } = null!;

    public string especialidad { get; set; } = null!;

    public string? telefono { get; set; }

    public string? correo { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public bool activo { get; set; }

    public virtual ICollection<convenio> convenio { get; set; } = new List<convenio>();

    public virtual ICollection<orden> orden { get; set; } = new List<orden>();
}