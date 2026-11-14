using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AlertaComApi.Application.Interfaces;
using AlertaComApi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AlertaComApi.Infrastructure.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user, out DateTime expiresAt)
        {
            var secretKey = _configuration["Jwt:SecretKey"] ?? "ChaveMestraSuperSeguraAlertaComGlobalSolutionFIAP2026_JWT_Token_Key_987654321";
            var issuer = _configuration["Jwt:Issuer"] ?? "AlertaComApi";
            var audience = _configuration["Jwt:Audience"] ?? "AlertaComMobile";
            var expirationHours = int.TryParse(_configuration["Jwt:ExpirationHours"], out var hours) ? hours : 24;

            expiresAt = DateTime.UtcNow.AddHours(expirationHours);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.GivenName, user.Nome),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new("bairro", user.Bairro),
                new("cpf", user.Cpf)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
