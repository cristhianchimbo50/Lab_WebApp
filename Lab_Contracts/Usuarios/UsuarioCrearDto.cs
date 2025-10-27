namespace Lab_Contracts.Usuarios
{
    public class UsuarioCrearDto
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoUsuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
