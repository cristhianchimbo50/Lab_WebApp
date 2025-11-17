using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Examen
{
    public int IdExamen { get; set; }

    public string? NombreExamen { get; set; }

    public string? ValorReferencia { get; set; }

    public string? Unidad { get; set; }

    public decimal Precio { get; set; }

    public string? Estudio { get; set; }

    public string? TipoMuestra { get; set; }

    public string? TiempoEntrega { get; set; }

    public string? TipoExamen { get; set; }

    public string? Tecnica { get; set; }

    public string? TituloExamen { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<DetalleOrden> DetalleOrden { get; set; } = new List<DetalleOrden>();

    public virtual ICollection<DetalleResultado> DetalleResultado { get; set; } = new List<DetalleResultado>();

    public virtual ICollection<ExamenComposicion> ExamenComposicionIdExamenHijoNavigation { get; set; } = new List<ExamenComposicion>();

    public virtual ICollection<ExamenComposicion> ExamenComposicionIdExamenPadreNavigation { get; set; } = new List<ExamenComposicion>();

    public virtual ICollection<ExamenReactivo> ExamenReactivo { get; set; } = new List<ExamenReactivo>();
}
