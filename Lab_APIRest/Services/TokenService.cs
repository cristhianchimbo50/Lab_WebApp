using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lab_APIRest.Infrastructure.Services
{
    public class TokenService
    {
        private readonly IConfiguration _config;
        private readonly ServerSessionKey _serverSessionKey;

        public TokenService(IConfiguration config, ServerSessionKey serverSessionKey)
        {
            _config = config;
            _serverSessionKey = serverSessionKey;
        }

        public (string token, DateTime expiresAtUtc) CreateToken(
            int idUsuario,
            string correoUsuario,
            string nombre,
            string rol,
            bool esContraseniaTemporal,
            int? idPaciente = null)
        {
            var claimsList = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, idUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, idUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, correoUsuario),
                new Claim(ClaimTypes.Name, nombre ?? ""),
                new Claim(ClaimTypes.Role, rol ?? ""),
                new Claim("temp_pwd", esContraseniaTemporal ? "1" : "0"),

                new Claim("server_key", _serverSessionKey.CurrentKey)
            };

            if (rol == "paciente" && idPaciente.HasValue)
                claimsList.Add(new Claim("IdPaciente", idPaciente.Value.ToString()));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claimsList,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }
    }
}
