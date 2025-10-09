using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class usuario_token_activacion
{
    public long id_token { get; set; }

    public int id_usuario { get; set; }

    public string token_hash { get; set; } = null!;

    public DateTime emitido_en { get; set; }

    public DateTime expira_en { get; set; }

    public bool usado { get; set; }

    public DateTime? usado_en { get; set; }

    public virtual usuario id_usuarioNavigation { get; set; } = null!;
}
