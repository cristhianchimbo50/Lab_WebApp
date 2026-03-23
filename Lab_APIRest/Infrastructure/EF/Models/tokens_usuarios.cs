using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class tokens_usuarios
{
    public long id_token { get; set; }

    public int id_usuario { get; set; }

    public byte[] token_hash { get; set; } = null!;

    public DateTime fecha_solicitud { get; set; }

    public DateTime fecha_expiracion { get; set; }

    public bool usado { get; set; }

    public DateTime? usado_en { get; set; }

    public string? ip_solicitud { get; set; }

    public string? dispositivo_origen { get; set; }

    public string? tipo_token { get; set; }

    public virtual usuario usuario_navigation { get; set; } = null!;
}