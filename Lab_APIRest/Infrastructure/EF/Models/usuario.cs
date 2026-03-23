using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class usuario
{
    public int id_usuario { get; set; }

    public string correo { get; set; } = null!;

    public string? password_hash { get; set; }

    public int id_rol { get; set; }

    public int id_persona { get; set; }

    public bool? activo { get; set; }

    public DateTime fecha_creacion { get; set; }

    public DateTime? ultimo_acceso { get; set; }

    public DateTime? fecha_actualizacion { get; set; }

    public DateTime? fecha_fin { get; set; }

    public virtual rol rol_navigation { get; set; } = null!;

    public virtual persona persona_navigation { get; set; } = null!;

    public virtual ICollection<tokens_usuarios> tokens_usuarios { get; set; } = new List<tokens_usuarios>();

    public virtual ICollection<resultado> resultado { get; set; } = new List<resultado>();
}