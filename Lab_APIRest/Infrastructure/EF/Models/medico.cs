using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Medico
{
    public int IdMedico { get; set; }

    public string NombreMedico { get; set; } = null!;

    public string Especialidad { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Convenio> Convenio { get; set; } = new List<Convenio>();

    public virtual ICollection<Orden> Orden { get; set; } = new List<Orden>();
}
