using Lab_APIRest.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestEmailController : ControllerBase
    {
        private readonly EmailService _email;

        public TestEmailController(EmailService email)
        {
            _email = email;
        }

        [HttpGet("email")]
        public async Task<IActionResult> ProbarEmail()
        {
            await _email.EnviarCorreoAsync(
                "chimbocristhian994@gmail.com",
                "Usuario de Prueba",
                "Prueba de correo desde API",
                "<h2>Correo enviado con éxito desde el backend</h2>"
            );
            return Ok("Correo enviado correctamente");
        }
    }
}
