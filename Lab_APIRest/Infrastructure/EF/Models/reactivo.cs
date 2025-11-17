using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Reactivo
{
    public int IdReactivo { get; set; }

    public string NombreReactivo { get; set; } = null!;

    public string? Fabricante { get; set; }

    public string? Unidad { get; set; }

    public decimal? CantidadDisponible { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<ExamenReactivo> ExamenReactivo { get; set; } = new List<ExamenReactivo>();

    public virtual ICollection<MovimientoReactivo> MovimientoReactivo { get; set; } = new List<MovimientoReactivo>();
}
