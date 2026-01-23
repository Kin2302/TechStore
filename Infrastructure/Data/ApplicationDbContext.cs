using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Data
{
    // Kế thừa IdentityDbContext để có sẵn bảng User, Role, Login...
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các bảng
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Cấu hình đệ quy cho Category (Cha - Con)
            builder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // Xóa cha không xóa con, để tránh lỗi

            // 2. Cấu hình kiểu dữ liệu tiền tệ (Decimal)
            // SQL Server cần biết độ chính xác, nếu không sẽ warning
            var decimalProps = builder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => (p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)));

            foreach (var p in decimalProps)
            {
                p.SetColumnType("decimal(18,2)");
            }

            // 3. Cấu hình quan hệ OrderDetail
            // Khi xóa Order thì xóa luôn OrderDetail (Cascade)
            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Nhưng khi xóa Product thì KHÔNG được xóa OrderDetail cũ (để giữ lịch sử)
            builder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}