using DozoDashBoard.Data;
using DozoDashBoard.Extensions;
using DozoDashBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace DozoDashBoardAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DozoDashBoardDbContext _db;

        public UserController(DozoDashBoardDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetCurrentUser")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var currentUser = CurrentUser();

            return Ok(currentUser);
        }
        [HttpGet("GetUserList")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserModel>>> GetUserList()
        {
            var users = await _db.Users.ToListAsync();
            return Ok(users);
        }
        [HttpDelete("DeleteUser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid UserId)
        {
            var user = await _db.Users.FindAsync(UserId);
            if (user == null)
            {
                return NotFound();
            }
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return NoContent();
        }
        private UserModel CurrentUser()
        {

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                return new UserModel
                {
                    UserName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value,
                    Email = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                    FirstName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.GivenName)?.Value,
                    LastName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value,
                    Role = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value
                };
            }
            return null;

        }

    }
}
