using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class usuario
{
    public int id_usuario { get; set; }
    public string correo_usuario { get; set; } = null!;
    public string? clave_usuario { get; set; }
    public string rol { get; set; } = null!;
    public string nombre { get; set; } = null!;
    public bool activo { get; set; }
    public DateTime fecha_creacion { get; set; }
    public DateTime? ultimo_acceso { get; set; }
    public virtual ICollection<paciente> pacientes { get; set; } = new List<paciente>();
}
