using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RentCarX.Application.Interfaces.JWT;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace RentCarX.Infrastructure.Services.JWT
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager; 

        public JwtTokenService(IConfiguration config, UserManager<User> userManager) 
        {
            _config = config;
            _userManager = userManager;
        }

        public async Task<string> GenerateToken(User user)
        {
            var claims = new List<Claim> 
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                new Claim(ClaimTypes.Email, user.Email), 
                new Claim(ClaimTypes.Name, user.UserName) 
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims, 
                expires: DateTime.Now.AddHours(12), 
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token); 
        }
    }
}
