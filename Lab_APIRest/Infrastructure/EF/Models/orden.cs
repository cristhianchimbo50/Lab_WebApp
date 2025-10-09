using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class orden
{
    public int id_orden { get; set; }

    public string numero_orden { get; set; } = null!;

    public int? id_paciente { get; set; }

    public DateOnly fecha_orden { get; set; }

    public decimal total { get; set; }

    public decimal? saldo_pendiente { get; set; }

    public decimal? total_pagado { get; set; }

    public string estado_pago { get; set; } = null!;

    public bool? anulado { get; set; }

    public bool? liquidado_convenio { get; set; }

    public int? id_medico { get; set; }

    public string? observacion { get; set; }

    public virtual ICollection<detalle_convenio> detalle_convenios { get; set; } = new List<detalle_convenio>();

    public virtual ICollection<detalle_orden> detalle_ordens { get; set; } = new List<detalle_orden>();

    public virtual medico? id_medicoNavigation { get; set; }

    public virtual paciente? id_pacienteNavigation { get; set; }

    public virtual ICollection<movimiento_reactivo> movimiento_reactivos { get; set; } = new List<movimiento_reactivo>();

    public virtual ICollection<pago> pagos { get; set; } = new List<pago>();

    public virtual ICollection<resultado> resultados { get; set; } = new List<resultado>();
}
