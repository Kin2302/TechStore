using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherUsage> VoucherUsages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            var decimalProps = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => (p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)));

            foreach (var p in decimalProps)
            {
                p.SetColumnType("decimal(18,2)");
            }

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WishlistItem>()
                .HasIndex(x => new { x.UserId, x.ProductId })
                .IsUnique();

            builder.Entity<WishlistItem>()
                .Property(x => x.UserId)
                .HasMaxLength(450)
                .IsRequired();

            builder.Entity<WishlistItem>()
                .HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Voucher>()
                .HasIndex(x => x.Code)
                .IsUnique();

            builder.Entity<Voucher>()
                .Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            builder.Entity<Voucher>()
                .Property(x => x.Type)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Entity<VoucherUsage>()
                .HasIndex(x => new { x.VoucherId, x.UserId, x.OrderId })
                .IsUnique();

            builder.Entity<VoucherUsage>()
                .Property(x => x.UserId)
                .HasMaxLength(450)
                .IsRequired();

            builder.Entity<VoucherUsage>()
                .HasOne(x => x.Voucher)
                .WithMany(x => x.Usages)
                .HasForeignKey(x => x.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VoucherUsage>()
                .HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}