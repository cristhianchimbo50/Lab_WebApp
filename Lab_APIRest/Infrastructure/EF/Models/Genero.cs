using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class genero
{
    public int id_genero { get; set; }

    public string nombre { get; set; } = null!;

    public string? descripcion { get; set; }

    public bool activo { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public virtual ICollection<persona> persona { get; set; } = new List<persona>();
}
