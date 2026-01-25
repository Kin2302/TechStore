using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            // 1. Đảm bảo Database đã được tạo
            await context.Database.MigrateAsync();

            // 2. Kiểm tra nếu đã có dữ liệu Hãng (Brand) thì thôi, không seed nữa
            if (await context.Brands.AnyAsync()) return;

            // --- TẠO DỮ LIỆU MẪU ---

            // A. Tạo Hãng (Brands)
            var brandArduino = new Brand { Name = "Arduino", Origin = "Italy", LogoUrl = "arduino-logo.png" };
            var brandEspressif = new Brand { Name = "Espressif", Origin = "China", LogoUrl = "espressif-logo.png" };
            var brandST = new Brand { Name = "STMicroelectronics", Origin = "Switzerland" };

            context.Brands.AddRange(brandArduino, brandEspressif, brandST);
            await context.SaveChangesAsync(); // Lưu Brand trước để lấy ID dùng cho Product

            // B. Tạo Danh mục (Categories)
            var catDevBoard = new Category { Name = "Board Phát Triển", Slug = "board-phat-trien" };
            var catSensor = new Category { Name = "Cảm Biến", Slug = "cam-bien" };
            var catIC = new Category { Name = "Linh Kiện Bán Dẫn", Slug = "linh-kien-ban-dan" };

            context.Categories.AddRange(catDevBoard, catSensor, catIC);
            await context.SaveChangesAsync();

            // C. Tạo Sản phẩm (Products)
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Arduino Uno R3 (Chính hãng)",
                    Code = "ARD-UNO-R3",
                    Slug = "arduino-uno-r3",
                    ShortDescription = "Mạch lập trình phổ biến nhất cho người mới.",
                    Description = "<p>Chip ATmega328P, điện áp 5V, 14 chân Digital...</p>",
                    Price = 450000,
                    Stock = 100,
                    BrandId = brandArduino.Id,     // Gán Brand vừa tạo
                    CategoryId = catDevBoard.Id,   // Gán Category vừa tạo
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Vi điều khiển", Value = "ATmega328P" },
                        new ProductSpecification { Name = "Điện áp", Value = "5V" }
                    },
                    Images = new List<ProductImage>
                    {
                        new ProductImage { ImageUrl = "https://example.com/arduino.jpg", IsMain = true }
                    }
                },
                new Product
                {
                    Name = "Module WiFi ESP32-WROOM-32",
                    Code = "ESP32-WROOM",
                    Slug = "esp32-wifi-ble",
                    ShortDescription = "Combo WiFi + Bluetooth cực mạnh.",
                    Price = 120000,
                    Stock = 500,
                    BrandId = brandEspressif.Id,
                    CategoryId = catIC.Id,
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "CPU", Value = "Dual-Core 32-bit" },
                        new ProductSpecification { Name = "WiFi", Value = "2.4 GHz" }
                    }
                },
                new Product
                {
                    Name = "Cảm biến nhiệt độ DHT11",
                    Code = "SEN-DHT11",
                    Slug = "cam-bien-dht11",
                    ShortDescription = "Đo nhiệt độ độ ẩm giá rẻ.",
                    Price = 25000,
                    Stock = 200,
                    CategoryId = catSensor.Id, // Không cần Brand
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Độ ẩm", Value = "20-90%" }
                    }
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // D. Tạo Admin User (Để sau này đăng nhập)
            var adminEmail = "admin@techstore.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "Admin@123"); // Mật khẩu mặc định
            }
        }
    }
}