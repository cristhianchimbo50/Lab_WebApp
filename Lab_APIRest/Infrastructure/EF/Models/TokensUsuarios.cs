using System;
using System.Collections.Generic;

namespace Lab_APIRest.Infrastructure.EF.Models;

public partial class TokensUsuarios
{
    public long IdToken { get; set; }

    public int IdUsuario { get; set; }

    public byte[] TokenHash { get; set; } = null!;

    public DateTime FechaSolicitud { get; set; }

    public DateTime FechaExpiracion { get; set; }

    public bool Usado { get; set; }

    public DateTime? UsadoEn { get; set; }

    public string? IpSolicitud { get; set; }

    public string? DispositivoOrigen { get; set; }

    public string? TipoToken { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
