using Microsoft.EntityFrameworkCore;
using SegalAPI.Models;

namespace SegalAPI.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Token> Tokens { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }

}
