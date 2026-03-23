using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class paciente
{
    public int id_paciente { get; set; }

    public int id_persona { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public bool activo { get; set; }

    public virtual persona persona_navigation { get; set; } = null!;

    public virtual ICollection<orden> orden { get; set; } = new List<orden>();
}