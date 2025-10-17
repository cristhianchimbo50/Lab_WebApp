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

    public bool? anulado { get; set; }

    public virtual ICollection<convenio> convenios { get; set; } = new List<convenio>();

    public virtual ICollection<orden> ordens { get; set; } = new List<orden>();
}
