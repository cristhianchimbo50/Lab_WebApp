namespace Lab_Contracts.Usuarios
{
    public class UsuarioListadoDto
    {
        public int IdUsuario { get; set; }
        public int IdPersona { get; set; }

        public string Cedula { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int IdGenero { get; set; }
        public string NombreGenero { get; set; } = string.Empty;

        public int IdRol { get; set; }
        public string NombreRol { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }
}
