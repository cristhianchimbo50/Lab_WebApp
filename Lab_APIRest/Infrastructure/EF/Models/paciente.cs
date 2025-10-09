using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class paciente
{
    public int id_paciente { get; set; }

    public string cedula_paciente { get; set; } = null!;

    public string nombre_paciente { get; set; } = null!;

    public DateOnly fecha_nac_paciente { get; set; }

    public string? direccion_paciente { get; set; }

    public string? correo_electronico_paciente { get; set; }

    public string? telefono_paciente { get; set; }

    public DateTime? fecha_registro { get; set; }

    public bool? anulado { get; set; }

    public int? id_usuario { get; set; }

    public virtual usuario? id_usuarioNavigation { get; set; }

    public virtual ICollection<orden> ordens { get; set; } = new List<orden>();

    public virtual ICollection<resultado> resultados { get; set; } = new List<resultado>();
}
