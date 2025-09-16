using EcoRide.Server.Model;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace EcoRide.Server.Data
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly double _expirationMinutes;

        public JwtService(IConfiguration config)
        {
            _secret = config["Jwt:Secret"];
            _expirationMinutes = double.Parse(config["Jwt:ExpirationMinutes"]);
        }

        public string GenerateToken(Utilisateur utilisateur)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, utilisateur.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, utilisateur.Email),
            new Claim("pseudo", utilisateur.Pseudo),
        };

            var token = new JwtSecurityToken(
                issuer: "EcoRide",
                audience: "EcoRideClient",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
