using mathProject.Models;
using Microsoft.EntityFrameworkCore;

namespace mathProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<FunktsiooniUurimine> FunktsiooniUurimised { get; set; } = null!;
    }
}