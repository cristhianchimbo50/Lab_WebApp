using System.ComponentModel.DataAnnotations;

namespace Lab_Contracts.Pacientes
{
    public class PacienteDto
    {
        public int IdPaciente { get; set; }

        public int IdPersona { get; set; }

        [Required(ErrorMessage = "La cedula es obligatoria")]
        [StringLength(20, ErrorMessage = "La cedula no puede superar 20 caracteres")]
        public string Cedula { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los nombres son obligatorios")]
        [StringLength(150, ErrorMessage = "Los nombres son muy largos")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios")]
        [StringLength(150, ErrorMessage = "Los apellidos son muy largos")]
        public string Apellidos { get; set; } = string.Empty;

        [RegularExpression(@"^$|^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Correo no valido")]
        public string? Correo { get; set; }

        [StringLength(30, ErrorMessage = "El telefono es muy largo")]
        public string? Telefono { get; set; }

        [StringLength(255, ErrorMessage = "La direccion es muy larga")]
        public string? Direccion { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El género es obligatorio")]
        public int? IdGenero { get; set; }

        public string? NombreGenero { get; set; }

        public bool Activo { get; set; }
    }
}
