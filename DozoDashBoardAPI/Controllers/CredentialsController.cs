using DozoDashBoard.Models;
using DozoDashBoard.Data;
using DozoDashBoard.Extensions;
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
        private readonly DozoDashBoardDbContext _db;
        private readonly CredentialsExtensions _credentialsExtensions;

        public CredentialsController(DozoDashBoardDbContext db, CredentialsExtensions credentialsExtensions)
        {
            _db = db;
            _credentialsExtensions = credentialsExtensions;
        }
        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<AuthResponse> Login([FromBody] UserLogin userLogin)
        {
            var user = _credentialsExtensions.Authenticate(userLogin);
            if (user != null)
            {
                var token = _credentialsExtensions.Generate(user);
                var refreshToken = _credentialsExtensions.CreateRefreshToken();
                await _credentialsExtensions.SetRefreshToken(refreshToken, user);

                return new AuthResponse
                {
                    Success = true,
                    Token = token,
                    RefreshToken = refreshToken.Token,
                    TokenExpires = refreshToken.Expires,
                };
            }
            return new AuthResponse { Message = "NotFound"};
        }
        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var response = await _credentialsExtensions.Refresh();
            if (response.Success)
                return Ok(response);

            return BadRequest(response.Message);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] UserRegister userRegister)
        {
            if (await _credentialsExtensions.IsUsernameTakenAsync(userRegister.UserName))
            {
                return BadRequest("Username is already taken.");
            }
            if (!_credentialsExtensions.IsPasswordValid(userRegister.Password))
            {
                return BadRequest("Invalid password. Password should meet the required constraints.\n" +
                    "* Minimum Length should be 8\n" +
                    "* Should contain at least 1 Uppercase\n" +
                    "* Should contain at least 1 Lowercase\n" +
                    "* Should contain at least 1 Digit");
            }
            string hashedPassword = _credentialsExtensions.HashPassword(userRegister.Password);

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

            await _db.Users.AddAsync(newUser);
            await _db.SaveChangesAsync();

            return Ok("User registration successful.");
        }
    }
}
