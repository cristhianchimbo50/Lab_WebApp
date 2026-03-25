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

    public int id_estado_pago { get; set; }

    public int id_estado_orden { get; set; }

    public DateTime? fecha_completado { get; set; }

    public int? id_medico { get; set; }

    public string? observacion { get; set; }

    public int? id_convenio { get; set; }

    public DateTime? fecha_fin { get; set; }

    public bool activo { get; set; }

    public virtual ICollection<detalle_orden> detalle_orden { get; set; } = new List<detalle_orden>();

    public virtual convenio? convenio_navigation { get; set; }

    public virtual medico? medico_navigation { get; set; }

    public virtual paciente? paciente_navigation { get; set; }

    public virtual ICollection<pago> pago { get; set; } = new List<pago>();

    public virtual ICollection<resultado> resultado { get; set; } = new List<resultado>();

    public virtual estado_orden estado_orden_navigation { get; set; } = null!;

    public virtual estado_pago? estado_pago_navigation { get; set; }
}