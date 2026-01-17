namespace Lab_Contracts.Usuarios
{
    public class UsuarioCrearDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoUsuario { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string? NombreRol { get; set; }
    }
}
