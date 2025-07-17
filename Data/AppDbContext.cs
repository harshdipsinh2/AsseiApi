using AssetManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Employee> tb_Employees { get; set; }
        public DbSet<RoleMaster> tb_RoleMaster { get; set; }
        public DbSet<PhysicalAsset> tb_Assets { get; set; }
        public DbSet<SoftwareAsset> tb_SoftwareAssets { get; set; }
        public DbSet<AssetRequest> tb_AssetRequests { get; set; } // Added AssetRequest table
        public DbSet<EmployeeAssetTransaction> tb_EmployeeAssetTransactions { get; set; }
        public DbSet<EmployeeSubscription> tb_EmployeeSubscriptions { get; set; }

        // Junction Tables 
        public DbSet<EmployeePhysicalAsset> tb_EmployeePhysicalAssets { get; set; }
        public DbSet<EmployeeSoftwareAsset> tb_EmployeeSoftwareAssets { get; set; }


        // Login and Registration
        public DbSet<User> tb_Users { get; set; }
        public DbSet<Otp> Otp { get; set; }
        public DbSet<Company> tb_Companies { get; set; }



        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>().ToTable("tb_Employees");
            modelBuilder.Entity<PhysicalAsset>().ToTable("tb_Assets");
            modelBuilder.Entity<SoftwareAsset>().ToTable("tb_SoftwareAssets");
            modelBuilder.Entity<User>().ToTable("tb_Users");
            modelBuilder.Entity<RoleMaster>().ToTable("tb_RoleMaster");
            modelBuilder.Entity<Otp>().ToTable("tb_OTP");

            // Define User -> Email as an alternate key
            modelBuilder.Entity<User>()
                .HasAlternateKey(u => u.Email);  // Mark Email as alternate key

            // Define EmployeeSubscriptions Table
            modelBuilder.Entity<EmployeeSubscription>()
                .ToTable("tb_EmployeeSubscriptions")
                .HasKey(s => s.Id);

            modelBuilder.Entity<EmployeeSubscription>()
                .HasOne(s => s.User)
                .WithMany() // Assuming one User can have many EmployeeSubscriptions
                .HasForeignKey(s => s.Email)  // Foreign key is Email
                .HasPrincipalKey(u => u.Email) // Use Email as the principal key
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeePhysicalAsset>()
                .ToTable("tb_EmployeePhysicalAssets")
                .HasKey(e => new { e.EmployeeId, e.AssetId });

            modelBuilder.Entity<EmployeeSoftwareAsset>()
                .ToTable("tb_EmployeeSoftwareAssets")
                .HasKey(e => new { e.EmployeeId, e.SoftwareID });

            modelBuilder.Entity<EmployeePhysicalAsset>()
                .HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeePhysicalAsset>()
                .HasOne(e => e.Asset)
                .WithMany()
                .HasForeignKey(e => e.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeSoftwareAsset>()
                .HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeSoftwareAsset>()
                .HasOne(e => e.SoftwareAsset)
                .WithMany()
                .HasForeignKey(e => e.SoftwareID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeePhysicalAsset>()
                .HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EmployeeSoftwareAsset>()
                .HasOne(e => e.Company)
                .WithMany()
                .HasForeignKey(e => e.CompanyID)
                .OnDelete(DeleteBehavior.Restrict);

            // Explicitly Define User -> RoleMaster Relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.Cascade);
        
            modelBuilder.Entity<EmployeeAssetTransaction>()
                .ToTable("tb_EmployeeAssetTransactions")
                .HasKey(t => t.TransactionID);

            modelBuilder.Entity<EmployeeAssetTransaction>()
                .HasOne(t => t.Employee)
                .WithMany()
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent deletion if employee has transactions

            modelBuilder.Entity<EmployeeAssetTransaction>()
                .HasOne(t => t.Asset)
                .WithMany()
                .HasForeignKey(t => t.AssetID)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent deletion if asset has transactions

            modelBuilder.Entity<Company>()
                .ToTable("tb_Companies")
                .HasKey(c => c.CompanyID);

            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Company)
                .WithMany(c => c.Employees)
                .HasForeignKey(e => e.CompanyID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if employees exist



            // Define AssetRequest Table Mapping
            modelBuilder.Entity<AssetRequest>().ToTable("tb_AssetRequests");
        }
    }
}
