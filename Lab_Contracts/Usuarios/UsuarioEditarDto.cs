namespace Lab_Contracts.Usuarios
{
    public class UsuarioEditarDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoUsuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
