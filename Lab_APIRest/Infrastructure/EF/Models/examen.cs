using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Examen
{
    public int IdExamen { get; set; }

    public string? NombreExamen { get; set; }

    public string? TituloExamen { get; set; }

    public decimal Precio { get; set; }

    public int? TiempoEntregaMinutos { get; set; }

    public int? IdEstudio { get; set; }

    public int? IdGrupoExamen { get; set; }

    public int? IdTipoMuestra { get; set; }

    public int? IdTipoExamen { get; set; }

    public int? IdTecnica { get; set; }

    public int? IdTipoRegistro { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaActualizacion { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; }

    public virtual Estudio? IdEstudioNavigation { get; set; }

    public virtual GrupoExamen? IdGrupoExamenNavigation { get; set; }

    public virtual TipoMuestra? IdTipoMuestraNavigation { get; set; }

    public virtual TipoExamen? IdTipoExamenNavigation { get; set; }

    public virtual Tecnica? IdTecnicaNavigation { get; set; }

    public virtual TipoRegistro? IdTipoRegistroNavigation { get; set; }

    public virtual ICollection<ReferenciaExamen> ReferenciaExamen { get; set; } = new List<ReferenciaExamen>();

    public virtual ICollection<DetalleOrden> DetalleOrden { get; set; } = new List<DetalleOrden>();

    public virtual ICollection<DetalleResultado> DetalleResultado { get; set; } = new List<DetalleResultado>();

    public virtual ICollection<ExamenComposicion> ExamenComposicionIdExamenHijoNavigation { get; set; } = new List<ExamenComposicion>();

    public virtual ICollection<ExamenComposicion> ExamenComposicionIdExamenPadreNavigation { get; set; } = new List<ExamenComposicion>();

    public virtual ICollection<ExamenReactivo> ExamenReactivo { get; set; } = new List<ExamenReactivo>();
}
