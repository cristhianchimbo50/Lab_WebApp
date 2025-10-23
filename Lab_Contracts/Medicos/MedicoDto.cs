using System.ComponentModel.DataAnnotations;

namespace Lab_Contracts.Medicos
{
    public class MedicoDto
    {
        public int IdMedico { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string NombreMedico { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especialidad es obligatoria")]
        [StringLength(100)]
        public string Especialidad { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Formato de teléfono no válido")]
        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        [StringLength(150)]
        public string Correo { get; set; } = string.Empty;

        public bool Anulado { get; set; }
    }
}
