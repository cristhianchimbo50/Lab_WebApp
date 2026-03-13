using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Persona
{
    public int IdPersona { get; set; }

    public string Cedula { get; set; } = null!;

    public string Nombres { get; set; } = null!;

    public string Apellidos { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public virtual ICollection<Usuario> Usuario { get; set; } = new List<Usuario>();

    public virtual ICollection<Paciente> Paciente { get; set; } = new List<Paciente>();
}
