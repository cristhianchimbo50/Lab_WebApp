using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_Contracts.Auth
{
    public class RestablecerContraseniaDto
    {
        public string Token { get; set; } = string.Empty;
        public string NuevaContrasenia { get; set; } = string.Empty;
        public string ConfirmarContrasenia { get; set; } = string.Empty;
    }
}
