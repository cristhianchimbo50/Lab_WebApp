namespace Lab_Contracts.Usuarios
{
    public class UsuarioEditarDto
    {
        public int IdUsuario { get; set; }
        public int IdPersona { get; set; }

        // Datos de persona
        public string Cedula { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        // Datos de usuario
        public int IdRol { get; set; }
        public string? NombreRol { get; set; }
        public bool Activo { get; set; }
    }
}
