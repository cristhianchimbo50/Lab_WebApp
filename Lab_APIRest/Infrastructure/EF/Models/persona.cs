using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class persona
{
    public int id_persona { get; set; }

    public string cedula { get; set; } = null!;

    public string nombres { get; set; } = null!;

    public string apellidos { get; set; } = null!;

    public int id_genero { get; set; }

    public DateOnly? fecha_nacimiento { get; set; }

    public string? telefono { get; set; }

    public string? direccion { get; set; }

    public bool activo { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public virtual genero genero_navigation { get; set; } = null!;

    public virtual ICollection<usuario> usuario { get; set; } = new List<usuario>();

    public virtual ICollection<paciente> paciente { get; set; } = new List<paciente>();
}