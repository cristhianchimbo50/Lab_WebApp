using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class DetalleResultado
{
    public int IdResultado { get; set; }

    public int IdExamen { get; set; }

    public string Valor { get; set; } = null!;

    public virtual Examen IdExamenNavigation { get; set; } = null!;

    public virtual Resultado IdResultadoNavigation { get; set; } = null!;

    public virtual ICollection<MovimientoReactivo> MovimientoReactivo { get; set; } = new List<MovimientoReactivo>();
}
