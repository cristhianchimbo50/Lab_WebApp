namespace Lab_APIRest.Services.Email
{
    public interface IEmailService
    {
        Task EnviarCorreoAsync(string destinatario, string nombreDestinatario, string asunto, string cuerpoHtml);
        Task SendTemporaryPasswordEmailAsync(string toEmail, string temporaryPassword);
    }

}
