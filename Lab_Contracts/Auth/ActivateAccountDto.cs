namespace Lab_Contracts.Auth;

public class ActivateAccountDto
{
    public int IdUsuario { get; set; }
    public string Token { get; set; } = default!;
}
