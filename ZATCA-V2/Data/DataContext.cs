using Microsoft.EntityFrameworkCore;
using ZATCA_V2.Models;

namespace ZATCA_V2.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {   
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyCredentials> CompanyCredentials { get; set; }
        public DbSet<CompanyInfo> CompanyInfos { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Company>()
                .HasMany(c => c.CompanyCredentials)
                .WithOne(cc => cc.Company)
                .HasForeignKey(cc => cc.CompanyId);
            
            modelBuilder.Entity<Company>()
                .HasOne(c => c.CompanyInfo)
                .WithOne(ci => ci.Company)
                .HasForeignKey<CompanyInfo>(ci => ci.ID); 

        }
    }
}