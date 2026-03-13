using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string? PasswordHash { get; set; }

    public int IdRol { get; set; }

    public int IdPersona { get; set; }

    public bool? Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? UltimoAcceso { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public virtual Rol IdRolNavigation { get; set; } = null!;

    public virtual Persona IdPersonaNavigation { get; set; } = null!;

    public virtual ICollection<TokensUsuarios> TokensUsuarios { get; set; } = new List<TokensUsuarios>();

    public virtual ICollection<Resultado> Resultado { get; set; } = new List<Resultado>();
}
