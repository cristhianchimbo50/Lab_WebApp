using System.ComponentModel.DataAnnotations;

namespace Lab_Contracts.Pacientes
{
    public class PacienteDto
    {
        public int IdPaciente { get; set; }

        [Required(ErrorMessage = "La cedula es obligatoria")]
        [StringLength(20, ErrorMessage = "La cedula no puede superar 20 caracteres")]
        public string CedulaPaciente { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre es muy largo")]
        public string NombrePaciente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        public DateTime FechaNacPaciente { get; set; }

        [Range(0, 130, ErrorMessage = "La edad no es valida")]
        public int EdadPaciente { get; set; }

        [StringLength(150, ErrorMessage = "La direccoin es muy larga")]
        public string DireccionPaciente { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Correo no valido")]
        public string CorreoElectronicoPaciente { get; set; } = string.Empty;

        [StringLength(30, ErrorMessage = "El telefono es muy largo")]
        public string TelefonoPaciente { get; set; } = string.Empty;

        public DateTime? FechaRegistro { get; set; }

        public bool Anulado { get; set; }

        public int? IdUsuario { get; set; }

        public string? ContraseniaTemporal { get; set; }

        public bool? EsContraseniaTemporal { get; set; }
    }
}
