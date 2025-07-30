using Microsoft.EntityFrameworkCore;
using TravelTourCrawler.Models;

namespace TravelTourCrawler.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Tour> Tours { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // ✅ Add MySQL configuration here
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = "server=localhost;port=3306;database=Travel;user=root;password=";
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 36)); // chỉnh lại nếu bạn dùng version khác
                optionsBuilder.UseMySql(connectionString, serverVersion);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Optional: Add index or constraints here if needed
            // modelBuilder.Entity<Tour>()
            //     .HasIndex(t => t.Url)
            //     .IsUnique();
        }
    }
}
