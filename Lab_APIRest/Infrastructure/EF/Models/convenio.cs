using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Convenio
{
    public int IdConvenio { get; set; }

    public int? IdMedico { get; set; }

    public DateOnly FechaConvenio { get; set; }

    public decimal PorcentajeComision { get; set; }

    public decimal MontoTotal { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; }

    public virtual Medico? IdMedicoNavigation { get; set; }

    public virtual ICollection<Orden> Orden { get; set; } = new List<Orden>();
}
