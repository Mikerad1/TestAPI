using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MichaelAPI
{
    public class UserService
    {
        private readonly Context _context;
        private readonly IConfiguration _configuration;
        public UserService(Context context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        public User AuthenticateAsync(string username, string password)
        {

            var user = _context.Users.Where(_ => _.UserName == username).FirstOrDefault();

            // return null if user not found
            if (user == null)
                return null;

            bool verified = false;
            if(password == user.Password)
            {
                verified = true;
            }

            if (verified)
            {

                user.Token = CreateToken(user);
                return user;
            }

            return null;
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
          

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_configuration.GetSection("Settings:AuthToken").Value));

            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
