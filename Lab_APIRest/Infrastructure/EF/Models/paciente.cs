using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Paciente
{
    public int IdPaciente { get; set; }

    public DateOnly FechaNacPaciente { get; set; }

    public int? IdGenero { get; set; }

    public int IdPersona { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public bool Activo { get; set; }

    public virtual Genero? IdGeneroNavigation { get; set; }

    public virtual Persona IdPersonaNavigation { get; set; } = null!;

    public virtual ICollection<Orden> Orden { get; set; } = new List<Orden>();
}
