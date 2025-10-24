using System.ComponentModel.DataAnnotations;

namespace Lab_Contracts.Auth
{
    public class CambiarContraseniaDto
    {
        [Required]
        public string CorreoUsuario { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string ContraseniaActual { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NuevaContrasenia { get; set; } = string.Empty;
    }
}