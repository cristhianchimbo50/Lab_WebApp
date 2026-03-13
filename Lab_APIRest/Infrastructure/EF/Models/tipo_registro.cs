using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class TipoRegistro
{
    public int IdTipoRegistro { get; set; }
    public string? Nombre { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaFin { get; set; }

    public virtual ICollection<Examen> Examen { get; set; } = new List<Examen>();
}
