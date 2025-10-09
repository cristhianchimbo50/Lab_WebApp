namespace Lab_Contracts.Auth;

public class LoginRequestDto
{
    public string CorreoUsuario { get; set; } = default!;
    public string Clave { get; set; } = default!;
}
