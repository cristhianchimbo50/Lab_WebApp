using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lab_APIRest.Infrastructure.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public EmailService(IConfiguration config)
        {
            _config = config;
            _http = new HttpClient
            {
                BaseAddress = new Uri("https://api.brevo.com/v3/")
            };
        }

        public async Task EnviarCorreoAsync(string destinatario, string nombreDestinatario, string asunto, string cuerpoHtml)
        {
            var apiKey = _config["Brevo:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("No se ha configurado la API Key de Brevo.");

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("accept", "application/json");
            _http.DefaultRequestHeaders.Add("api-key", apiKey);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                sender = new
                {
                    email = _config["Brevo:SenderEmail"],
                    name = _config["Brevo:SenderName"]
                },
                to = new[]
                {
                    new { email = destinatario, name = nombreDestinatario }
                },
                subject = asunto,
                htmlContent = cuerpoHtml
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync("smtp/email", content);
            response.EnsureSuccessStatusCode();
        }
    }
}