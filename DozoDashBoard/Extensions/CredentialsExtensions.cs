using DozoDashBoard.Data;
using DozoDashBoard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DozoDashBoard.Extensions
{
    public class CredentialsExtensions
    {
        private readonly IConfiguration _configuration;
        private readonly DozoDashBoardDbContext _db;

        public CredentialsExtensions(IConfiguration configuration, DozoDashBoardDbContext db)
        {
            _configuration = configuration;
            _db = db;
        }
        public string Generate(UserModel user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier,user.UserName),
                new Claim(ClaimTypes.Email,user.Email),
                new Claim(ClaimTypes.GivenName,user.FirstName),
                new Claim(ClaimTypes.Surname,user.LastName),
                new Claim(ClaimTypes.Role,user.Role)
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
            );


            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public UserModel Authenticate(UserLogin userLogin)
        {
            string hashedPassword = HashPassword(userLogin.Password);

            var currentUser = _db.Users.FirstOrDefault(o => o.UserName.ToLower() == userLogin.UserName.ToLower());
            if (currentUser != null && BCrypt.Net.BCrypt.Verify(userLogin.Password, currentUser.Password))
            {
                return currentUser;
            }
            return null;
        }
        public bool IsPasswordValid(string password)
        {
            
            const int MinimumLength = 8;
            const int MinimumUppercase = 1;
            const int MinimumLowercase = 1;
            const int MinimumDigit = 1;

            
            if (password.Length < MinimumLength)
            {
                return false;
            }

            int uppercaseCount = password.Count(char.IsUpper);
            int lowercaseCount = password.Count(char.IsLower);
            int digitCount = password.Count(char.IsDigit);

            return uppercaseCount >= MinimumUppercase
                && lowercaseCount >= MinimumLowercase
                && digitCount >= MinimumDigit;
        }
        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            return await _db.Users.AnyAsync(u => u.UserName == username);
        }
        public string HashPassword(string password)
        {
            
            string salt = BCrypt.Net.BCrypt.GenerateSalt();

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return hashedPassword;
        }
    }
}
