using Microsoft.EntityFrameworkCore;
namespace WeddingPlanner.Models
{
    public class WeddingPlannerContext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public WeddingPlannerContext(DbContextOptions options) : base(options) { 
	                          
        }
        public DbSet<User> Users { get; set; }
    }
}