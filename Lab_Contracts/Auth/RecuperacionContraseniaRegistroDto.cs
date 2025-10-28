using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Auth
{
    public class RecuperacionContraseniaRegistroDto
    {
        public int IdUsuario { get; set; }
        public byte[] TokenHash { get; set; } = Array.Empty<byte>();
        public DateTime FechaExpiracion { get; set; }
        public string? IpSolicitud { get; set; }
        public string? UserAgent { get; set; }
    }
}
