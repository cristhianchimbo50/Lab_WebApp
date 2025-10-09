namespace Lab_Contracts.Auth;

public class ChangePasswordDto
{
    public int IdUsuario { get; set; }
    public string ClaveActual { get; set; } = default!;
    public string NuevaClave { get; set; } = default!;
}
