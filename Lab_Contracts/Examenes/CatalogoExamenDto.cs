using System.ComponentModel.DataAnnotations;

namespace Lab_Contracts.Examenes
{
    public class CatalogoExamenDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;
    }
}
