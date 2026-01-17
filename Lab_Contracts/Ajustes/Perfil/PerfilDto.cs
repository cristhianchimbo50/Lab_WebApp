namespace Lab_Contracts.Ajustes.Perfil
{

    public class PerfilDto
    {

        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public bool Activo { get; set; }


        public string? Cedula { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }

        public DateTime? UltimoAcceso { get; set; }


        public DateTime? FechaRegistro { get; set; }
    }
}
