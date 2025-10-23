namespace Lab_Contracts.Auth;

public class RegisterRequestDto
{
    public string CorreoUsuario { get; set; } = default!;
    public string Clave { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string Rol { get; set; } = "paciente";
}
