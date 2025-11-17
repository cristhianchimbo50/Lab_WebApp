using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string CorreoUsuario { get; set; } = null!;

    public string? ClaveUsuario { get; set; }

    public string Rol { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public virtual ICollection<Paciente> Paciente { get; set; } = new List<Paciente>();

    public virtual ICollection<TokensUsuarios> TokensUsuarios { get; set; } = new List<TokensUsuarios>();
}
