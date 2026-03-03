using Microsoft.EntityFrameworkCore;

namespace ErayBarbekuSomine.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Image> Images { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
    }
}
