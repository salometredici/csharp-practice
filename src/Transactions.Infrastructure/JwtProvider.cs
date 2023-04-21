using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Transactions.Domain.Authentication;
using Transactions.Domain.Users;

namespace Transactions.Infrastructure
{
    public interface IJwtProvider
    {
        JwtTokenResponse GetJwtTokenResponse(UserDto user);
    }

    public class JwtProvider : IJwtProvider
    {
        private readonly IConfiguration _configuration;

        public JwtProvider(IConfiguration configuration) => _configuration = configuration;

        public JwtTokenResponse GetJwtTokenResponse(UserDto user)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Surname, user.UserSurname),
                new Claim(ClaimTypes.Email, user.UserEmail)
            };

            var lifetimeHours = _configuration.GetSection("Jwt:LifetimeHours").Value!;
            var expirationDate = DateTime.Now.AddHours(double.Parse(lifetimeHours));

            var symmetricKey = _configuration.GetSection("Jwt:Key").Value;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(symmetricKey!));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetSection("Jwt:Issuer").Value,
                audience: _configuration.GetSection("Jwt:Audience").Value,
                claims: claims,
                notBefore: DateTime.Now,
                expires: expirationDate,
                signingCredentials: signingCredentials
                );

            return new JwtTokenResponse()
            {
                ExpirationDate = expirationDate,
                BearerToken = new JwtSecurityTokenHandler().WriteToken(token)
            };
        }
    }
}
