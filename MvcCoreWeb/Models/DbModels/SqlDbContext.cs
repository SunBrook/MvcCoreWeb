using Microsoft.EntityFrameworkCore;

namespace MvcCoreWeb.Models.DbModels
{

    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options) : base(options)
        {

        }

        public DbSet<Account> Account { get; set; }
    }
}
