using DozoDashBoard.Models;
using DozoDashBoard.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.CodeDom.Compiler;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DozoDashBoardAPI.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]

    [ApiController]
    public class CredentialsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DozoDashBoardDbContext _db;

        public CredentialsController(IConfiguration configuration, DozoDashBoardDbContext db)
        {
            _configuration = configuration;
            _db = db;
        }
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            var user = Authenticate(userLogin);
            if (user != null)
            {
                var token = Generate(user);
                return Ok(token);
            }
            return NotFound("User Not Found");
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserRegister userRegister)
        {
            if (await IsUsernameTakenAsync(userRegister.UserName))
            {
                return BadRequest("Username is already taken.");
            }
            if (!IsPasswordValid(userRegister.Password))
            {
                return BadRequest("Invalid password. Password should meet the required constraints.");
            }
            string hashedPassword = HashPassword(userRegister.Password);

            // Create a new user entity with the provided registration data
            var newUser = new UserModel
            {
                UserId = Guid.NewGuid(),
                FirstName = userRegister.FirstName,
                LastName = userRegister.LastName,
                UserName = userRegister.UserName,
                Email = userRegister.Email,
                Password = hashedPassword,
                Role = "User",
            };

            // Save the new user to the database
            await _db.Users.AddAsync(newUser);
            await _db.SaveChangesAsync();

            return Ok("User registration successful.");
        }

        private string Generate(UserModel user)
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

        private UserModel Authenticate(UserLogin userLogin)
        {
            string hashedPassword = HashPassword(userLogin.Password);

            var currentUser = _db.Users.FirstOrDefault(o => o.UserName.ToLower() == userLogin.UserName.ToLower());
            if (currentUser != null && BCrypt.Net.BCrypt.Verify(userLogin.Password, currentUser.Password))
            {
                return currentUser;
            }
            return null;
        }
        private bool IsPasswordValid(string password)
        {
            // Example constraints (customize based on your requirements)
            const int MinimumLength = 8;
            const int MinimumUppercase = 1;
            const int MinimumLowercase = 1;
            const int MinimumDigit = 1;

            // Check minimum length
            if (password.Length < MinimumLength)
            {
                return false;
            }

            // Check for minimum uppercase, lowercase, and digit characters
            int uppercaseCount = password.Count(char.IsUpper);
            int lowercaseCount = password.Count(char.IsLower);
            int digitCount = password.Count(char.IsDigit);

            return uppercaseCount >= MinimumUppercase
                && lowercaseCount >= MinimumLowercase
                && digitCount >= MinimumDigit;
        }
        private async Task<bool> IsUsernameTakenAsync(string username)
        {
            return await _db.Users.AnyAsync(u => u.UserName == username);
        }
        private string HashPassword(string password)
        {
            // Generate a secure salt
            string salt = BCrypt.Net.BCrypt.GenerateSalt();

            // Hash the password using the generated salt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            return hashedPassword;
        }
    }
}
