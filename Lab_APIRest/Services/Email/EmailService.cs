using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Lab_APIRest.Services.Email;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lab_APIRest.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, HttpClient http, ILogger<EmailService> logger)
        {
            _config = config;
            _http = http;
            _logger = logger;
            _http.BaseAddress ??= new Uri("https://api.brevo.com/v3/");
        }

        public async Task EnviarCorreoAsync(string destinatario, string nombreDestinatario, string asunto, string cuerpoHtml)
        {
            var apiKey = _config["Brevo:ApiKey"];
            var senderEmail = _config["Brevo:SenderEmail"];
            var senderName = _config["Brevo:SenderName"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("No se ha configurado la API Key de Brevo.");
            if (string.IsNullOrWhiteSpace(senderEmail))
                throw new InvalidOperationException("No se ha configurado Brevo:SenderEmail.");

            var payload = new
            {
                sender = new
                {
                    email = senderEmail,
                    name = senderName
                },
                to = new[]
                {
                    new { email = destinatario, name = nombreDestinatario }
                },
                subject = asunto,
                htmlContent = cuerpoHtml
            };

            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, "smtp/email")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("accept", "application/json");
            request.Headers.Add("api-key", apiKey);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _http.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Error enviando correo por Brevo. Status: {StatusCode}. Destinatario: {Destinatario}. Respuesta: {Respuesta}", response.StatusCode, destinatario, responseBody);
                throw new HttpRequestException($"Brevo devolvió estado {(int)response.StatusCode}: {responseBody}");
            }

            _logger.LogInformation("Correo enviado correctamente a {Destinatario}", destinatario);
        }

        public async Task SendTemporaryPasswordEmailAsync(string toEmail, string temporaryPassword)
        {
            var asunto = "Contraseña temporal - Laboratorio Clínico La Inmaculada";
            var cuerpoHtml = $@"
                <p>Hola,</p>
                <p>Tu contraseña temporal es: <strong>{temporaryPassword}</strong></p>
                <p>Por seguridad, cámbiala al iniciar sesión.</p>
                <p><strong>Laboratorio Clínico La Inmaculada</strong></p>";

            await EnviarCorreoAsync(toEmail, "Usuario", asunto, cuerpoHtml);
        }
    }
}