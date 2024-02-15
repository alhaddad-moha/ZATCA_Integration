using Microsoft.EntityFrameworkCore;
using ZATCA_V2.Models;

namespace ZATCA_V2.Data
{

    public class DataContext : DbContext
    {

        public DataContext()
        {
        }
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyCredentials> CompanyCredentials { get; set; }

     
    }
}
