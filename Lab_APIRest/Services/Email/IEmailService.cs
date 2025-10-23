namespace Lab_APIRest.Services.Email
{
    public interface IEmailService
    {
        Task SendTemporaryPasswordEmailAsync(string toEmail, string temporaryPassword);
    }

}
