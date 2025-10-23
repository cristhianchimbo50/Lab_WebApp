using System.ComponentModel.DataAnnotations;

namespace Lab_Contracts.Auth
{
    public class ChangePasswordDto
    {
        [Required]
        public string CorreoUsuario { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string ClaveActual { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NuevaClave { get; set; } = string.Empty;
    }
}
