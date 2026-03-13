namespace Lab_Contracts.Usuarios
{
    public class UsuarioCrearDto
    {
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
    }
}
