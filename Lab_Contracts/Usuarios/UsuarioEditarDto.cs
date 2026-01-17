namespace Lab_Contracts.Usuarios
{
    public class UsuarioEditarDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoUsuario { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string? NombreRol { get; set; }
        public bool Activo { get; set; }
    }
}
