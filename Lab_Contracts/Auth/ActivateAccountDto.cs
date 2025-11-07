namespace Lab_Contracts.Auth
{
    public class ActivateAccountDto
    {
        public string Token { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
    }
}
