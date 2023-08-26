using DozoDashBoard.Data;
using DozoDashBoard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DozoDashBoard.Extensions
{
    public class CredentialsExtensions
    {
        private readonly IConfiguration _configuration;
        private readonly DozoDashBoardDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CredentialsExtensions(IConfiguration configuration, DozoDashBoardDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _db = db;
            _httpContextAccessor = httpContextAccessor;
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
        public RefreshToken CreateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };
            return refreshToken;
        }
        public async Task SetRefreshToken(RefreshToken refreshToken, UserModel user)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.Expires,
                
            };
            _httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);

            user.RefreshToken = refreshToken.Token;
            user.TokenCreated = refreshToken.Created;
            user.TokenExpires = refreshToken.Expires;

            await _db.SaveChangesAsync();
        }
        public async Task<AuthResponse> Refresh()
        {
            var refreshToken = _httpContextAccessor?.HttpContext?.Request.Cookies["refreshToken"];
            var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
            if(user == null)
            {
                return new AuthResponse { Message = "Invalid Refresh Token" };
            }
            else if(user.TokenExpires < DateTime.UtcNow)
            {
                return new AuthResponse { Message = "Token Expired" };
            }
            string token = Generate(user);
            var newRefreshToken = CreateRefreshToken();
            await SetRefreshToken(newRefreshToken, user);

            return new AuthResponse
            {
                Success = true,
                Token = token,
                RefreshToken = newRefreshToken.Token,
                TokenExpires = newRefreshToken.Expires
            };
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
