using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class estado_pago
{
    public int id_estado_pago { get; set; }
    public string? nombre { get; set; }
    public string? descripcion { get; set; }
    public bool activo { get; set; }
    public DateTime fecha_creacion { get; set; }
    public DateTime? fecha_actualizacion { get; set; }
    public DateTime? fecha_fin { get; set; }

    public virtual ICollection<orden> orden { get; set; } = new List<orden>();
}
