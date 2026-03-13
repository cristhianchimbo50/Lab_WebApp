using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class EstadoOrden
{
    public int IdEstadoOrden { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public virtual ICollection<Orden> Orden { get; set; } = new List<Orden>();
}
