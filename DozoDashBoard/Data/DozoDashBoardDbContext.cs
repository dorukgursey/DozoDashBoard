using DozoDashBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace DozoDashBoard.Data
{
    public class DozoDashBoardDbContext : DbContext
    {
        public DozoDashBoardDbContext(DbContextOptions<DozoDashBoardDbContext> options) : base(options)
        {
        }
        public DbSet<UserModel> Users { get; set; }
    }
}
