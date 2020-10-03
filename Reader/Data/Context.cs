using Microsoft.EntityFrameworkCore;
using Reader.Models;

namespace Reader.Data
{
    public class Context : DbContext
    {
        public DbSet<Feed> Feeds { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=Reader.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Feed>().ToTable("Feed")
                .HasAlternateKey(f => f.Uri);
        }
    }
}
