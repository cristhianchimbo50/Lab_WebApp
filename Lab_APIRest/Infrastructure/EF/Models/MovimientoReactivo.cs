using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class MovimientoReactivo
{
    public int IdMovimientoReactivo { get; set; }

    public int? IdReactivo { get; set; }

    public string? TipoMovimiento { get; set; }

    public decimal? Cantidad { get; set; }

    public DateTime FechaMovimiento { get; set; }

    public string? Observacion { get; set; }

    public int? IdDetalleResultado { get; set; }

    public int? IdResultado { get; set; }

    public int? IdExamen { get; set; }

    public virtual DetalleResultado? DetalleResultado { get; set; }

    public virtual Reactivo? IdReactivoNavigation { get; set; }
}
