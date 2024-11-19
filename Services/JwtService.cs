using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AonFreelancing.Services
{
    public class JwtService(IConfiguration config)
    {
        public string GenerateJwt(Models.User user, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config[$"Jwt:{Utilities.Constants.JWT_KEY}"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: config[$"Jwt:{Utilities.Constants.JWT_ISSUER}"],
                audience: config[$"Jwt:{Utilities.Constants.JWT_AUDIENCE}"],
                claims:
                [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Role, role)
                ],
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(config[$"Jwt:{Utilities.Constants.JWT_EXPIRATION}"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
