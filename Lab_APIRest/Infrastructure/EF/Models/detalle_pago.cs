using System;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class detalle_pago
{
    public int id_detalle_pago { get; set; }
    public int? id_pago { get; set; }
    public int? id_tipo_pago { get; set; }
    public decimal? monto { get; set; }
    public string? numero_comprobante { get; set; }
    public DateTime? fecha_creacion { get; set; }
    public DateTime? fecha_fin { get; set; }
    public bool activo { get; set; }

    public virtual pago? pago_navigation { get; set; }
    public virtual tipo_pago? tipo_pago_navigation { get; set; }
}
