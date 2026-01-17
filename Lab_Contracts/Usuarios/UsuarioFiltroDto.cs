namespace Lab_Contracts.Usuarios
{
    public class UsuarioFiltroDto
    {
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public int? IdRol { get; set; }
        public bool? Activo { get; set; }
    }
}
