using Lab_APIRest.Services.Email;
using Microsoft.AspNetCore.Mvc;

namespace Lab_APIRest.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestEmailController : ControllerBase
    {
        private readonly IEmailService _email;

        public TestEmailController(IEmailService email)
        {
            _email = email;
        }

        [HttpGet("email")]
        public async Task<IActionResult> ProbarEmail()
        {
            try
            {
                await _email.EnviarCorreoAsync(
                    "chimbocristhian994@gmail.com",
                    "Usuario de Prueba",
                    "Prueba de correo desde API",
                    "<h2>Correo enviado con éxito desde el backend</h2>"
                );

                return Ok("Correo enviado correctamente");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }
    }
}
