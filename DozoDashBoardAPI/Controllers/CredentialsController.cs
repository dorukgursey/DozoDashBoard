using DozoDashBoard.Models;
using DozoDashBoard.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.CodeDom.Compiler;

namespace DozoDashBoardAPI.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/controller")]
    [ApiController]
    public class CredentialsController : ControllerBase
    {
        private IConfiguration _configuration;
        private readonly DozoDashBoardDbContext _db;

        public CredentialsController(IConfiguration configuration, DozoDashBoardDbContext db)
        {
            _configuration = configuration;
            _db = db;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] UserLogin userLogin)
        {
            var user = Authenticate(userLogin);
            if (user == null)
            {
                var token = Generate(user);
                return Ok(token);
            }
            return NotFound("User Not Found");
        }

        private string Generate(UserModel user)
        {
            throw new NotImplementedException();
        }

        private UserModel Authenticate(UserLogin userLogin)
        {
            var currentUser = _db.Users.FirstOrDefault(o => o.UserName.ToLower() == userLogin.UserName.ToLower() && o.Password == userLogin.Password);
            if (currentUser != null)
            {
                return currentUser;
            }
            return null;
        }
    }
}
