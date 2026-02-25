using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    /// <summary>
    /// DbContext with global query filters for multitenancy.
    /// All queries are automatically filtered by TenantId.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        private readonly string _tenantId;

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; } = null!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantService tenantService)
            : base(options)
        {
            _tenantId = tenantService.GetCurrentTenantId();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => new { e.Username, e.TenantId }).IsUnique();
                entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).HasDefaultValue("User");
                entity.Property(e => e.TenantId).HasMaxLength(100).IsRequired();

                // Global query filter for multitenancy
                entity.HasQueryFilter(e => e.TenantId == _tenantId);
            });

            // Configure Product entity
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TenantId).HasMaxLength(100).IsRequired();

                // Global query filter for multitenancy
                entity.HasQueryFilter(e => e.TenantId == _tenantId);
            });

            // Configure PasswordResetRequest entity
            modelBuilder.Entity<PasswordResetRequest>(entity =>
            {
                entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
                entity.Property(e => e.ResetToken).HasMaxLength(100).IsRequired();
            });

            // Seed initial data for two tenants
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed users for Tenant A
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@tenanta.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = "Admin",
                    TenantId = "tenant-a",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 2,
                    Username = "user1",
                    Email = "user1@tenanta.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                    Role = "User",
                    TenantId = "tenant-a",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed users for Tenant B
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 3,
                    Username = "admin",
                    Email = "admin@tenantb.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = "Admin",
                    TenantId = "tenant-b",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = 4,
                    Username = "user1",
                    Email = "user1@tenantb.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                    Role = "User",
                    TenantId = "tenant-b",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed products for Tenant A
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Laptop HP",
                    Description = "Laptop HP Pavilion 15",
                    Price = 899.99m,
                    Stock = 10,
                    TenantId = "tenant-a",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Product
                {
                    Id = 2,
                    Name = "Mouse Logitech",
                    Description = "Mouse inalámbrico Logitech MX",
                    Price = 49.99m,
                    Stock = 50,
                    TenantId = "tenant-a",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // Seed products for Tenant B
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 3,
                    Name = "iPhone 15",
                    Description = "Apple iPhone 15 Pro Max",
                    Price = 1199.99m,
                    Stock = 5,
                    TenantId = "tenant-b",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Product
                {
                    Id = 4,
                    Name = "AirPods Pro",
                    Description = "Apple AirPods Pro 2nd Gen",
                    Price = 249.99m,
                    Stock = 20,
                    TenantId = "tenant-b",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }

        /// <summary>
        /// Override SaveChanges to automatically set TenantId on new entities.
        /// </summary>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.TenantId = _tenantId;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
