using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class usuario
{
    public int id_usuario { get; set; }

    public string correo_usuario { get; set; } = null!;

    public string clave_usuario { get; set; } = null!;

    public string rol { get; set; } = null!;

    public string nombre { get; set; } = null!;

    public bool? es_contraseña_temporal { get; set; }

    public bool? estado_registro { get; set; }

    public string estado { get; set; } = null!;

    public virtual ICollection<paciente> pacientes { get; set; } = new List<paciente>();

    public virtual ICollection<usuario_token_activacion> usuario_token_activacions { get; set; } = new List<usuario_token_activacion>();
}
