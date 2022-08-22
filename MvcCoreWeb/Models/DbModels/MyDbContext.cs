using Microsoft.EntityFrameworkCore;

namespace MvcCoreWeb.Models.DbModels
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
    }
}
