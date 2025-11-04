namespace Lab_Contracts.Auth;

public class LoginResponseDto
{
    public int IdUsuario { get; set; }
    public string CorreoUsuario { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public string Rol { get; set; } = default!;
    public bool EsContraseniaTemporal { get; set; }
    public string AccessToken { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
    public string Mensaje { get; set; }
}
