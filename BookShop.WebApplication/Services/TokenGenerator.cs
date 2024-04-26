using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookShop.WebApplication.Services
{
    public class TokenGenerator
    {
        public string ? Value { get; set; }
        public DateTime ? ValidTo { get; set; }

        public string Generate(string email, IConfiguration configuration)
        {
            List<Claim> authorizationClaims = 
                [new Claim(ClaimTypes.Name, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())];

            authorizationClaims.Add(new Claim(ClaimTypes.Role, ApplicationConstants.TokenClaimRole));

            var authorizationSignInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!));

            var expare = DateTime.Now.AddHours(1);

            var token = new JwtSecurityToken
                (
                issuer: configuration["JWT:ValidIssuer"],
                audience: configuration["JWT:ValidAudience"],
                expires: expare,
                claims: authorizationClaims,
                signingCredentials: new SigningCredentials(authorizationSignInKey, SecurityAlgorithms.HmacSha256)
                );

            Value = new JwtSecurityTokenHandler().WriteToken(token);
            ValidTo = expare;

            return Value;
        }
    }
}
