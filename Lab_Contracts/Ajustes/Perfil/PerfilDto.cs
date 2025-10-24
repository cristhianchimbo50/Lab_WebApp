namespace Lab_Contracts.Ajustes.Perfil
{

    public class PerfilDto
    {

        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;


        public string? Cedula { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }

        public DateTime? UltimoAcceso { get; set; }


        public DateTime? FechaRegistro { get; set; }
    }
}
