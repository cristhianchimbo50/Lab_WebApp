using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class Orden
{
    public int IdOrden { get; set; }

    public string NumeroOrden { get; set; } = null!;

    public int? IdPaciente { get; set; }

    public DateOnly FechaOrden { get; set; }

    public decimal Total { get; set; }

    public decimal? SaldoPendiente { get; set; }

    public decimal? TotalPagado { get; set; }

    public string EstadoPago { get; set; } = null!;

    public string EstadoOrden { get; set; } = "EN_PROCESO";

    public DateTime? FechaFinalizacion { get; set; }

    public bool ResultadosHabilitados { get; set; }

    public int? IdMedico { get; set; }

    public string? Observacion { get; set; }

    public int? IdConvenio { get; set; }

    public DateTime? FechaFin { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<DetalleOrden> DetalleOrden { get; set; } = new List<DetalleOrden>();

    public virtual Convenio? IdConvenioNavigation { get; set; }

    public virtual Medico? IdMedicoNavigation { get; set; }

    public virtual Paciente? IdPacienteNavigation { get; set; }

    public virtual ICollection<Pago> Pago { get; set; } = new List<Pago>();

    public virtual ICollection<Resultado> Resultado { get; set; } = new List<Resultado>();
}
