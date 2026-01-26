using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await context.Database.MigrateAsync();

            // ✅ TẠO ROLES
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // ✅ TẠO HOẶC RESET ADMIN
            await CreateOrResetUserAsync(userManager, "admin@techstore.com", "Admin@123", "Admin");

            // ✅ TẠO HOẶC RESET USER
            await CreateOrResetUserAsync(userManager, "user@techstore.com", "User@123", "User");

            // Seed product data
            if (await context.Brands.AnyAsync()) return;
            await SeedProductDataAsync(context);
        }

        private static async Task CreateOrResetUserAsync(
            UserManager<IdentityUser> userManager,
            string email,
            string password,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Tạo mới
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            else
            {
                // ✅ ĐẢM BẢO EMAIL CONFIRMED
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await userManager.UpdateAsync(user);
                }

                // ✅ RESET PASSWORD (nếu cần)
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, password);

                // ✅ ĐẢM BẢO CÓ ROLE
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }

        private static async Task SeedProductDataAsync(ApplicationDbContext context)
        {
            var brandArduino = new Brand { Name = "Arduino", Origin = "Italy", LogoUrl = "arduino-logo.png" };
            var brandEspressif = new Brand { Name = "Espressif", Origin = "China", LogoUrl = "espressif-logo.png" };
            var brandST = new Brand { Name = "STMicroelectronics", Origin = "Switzerland" };

            context.Brands.AddRange(brandArduino, brandEspressif, brandST);
            await context.SaveChangesAsync();

            var catDevBoard = new Category { Name = "Board Phát Triển", Slug = "board-phat-trien" };
            var catSensor = new Category { Name = "Cảm Biến", Slug = "cam-bien" };
            var catIC = new Category { Name = "Linh Kiện Bán Dẫn", Slug = "linh-kien-ban-dan" };

            context.Categories.AddRange(catDevBoard, catSensor, catIC);
            await context.SaveChangesAsync();

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
                    BrandId = brandArduino.Id,
                    CategoryId = catDevBoard.Id,
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Vi điều khiển", Value = "ATmega328P" },
                        new ProductSpecification { Name = "Điện áp", Value = "5V" }
                    },
                    Images = new List<ProductImage>
                    {
                        new ProductImage { ImageUrl = "https://via.placeholder.com/400x300?text=Arduino+Uno", IsMain = true }
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
                    },
                    Images = new List<ProductImage>
                    {
                        new ProductImage { ImageUrl = "https://via.placeholder.com/400x300?text=ESP32", IsMain = true }
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
                    CategoryId = catSensor.Id,
                    Specifications = new List<ProductSpecification>
                    {
                        new ProductSpecification { Name = "Độ ẩm", Value = "20-90%" }
                    },
                    Images = new List<ProductImage>
                    {
                        new ProductImage { ImageUrl = "https://via.placeholder.com/400x300?text=DHT11", IsMain = true }
                    }
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}