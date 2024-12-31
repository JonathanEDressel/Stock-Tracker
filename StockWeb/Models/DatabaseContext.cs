using Microsoft.EntityFrameworkCore;

namespace Portfolio_Tracker.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<StockModel> Stocks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("MyDb");
        }
    }
}