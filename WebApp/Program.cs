using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechStore.Infrastructure.Data;
namespace WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Lấy chuỗi kết nối từ appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly("TechStore.Infrastructure")));

            // ✅ KÍCH HOẠT IDENTITY + ROLES
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false; // Không cần xác nhận email
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false; // Không cần ký tự đặc biệt
            })

            .AddRoles<IdentityRole>() // ⚠️ QUAN TRỌNG: Thêm Role (Admin/User)
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages(); // ✅ Cần cho Identity UI
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // ✅ SEED DATA (Bỏ comment)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    // Gọi hàm tạo dữ liệu mẫu
                    await DbInitializer.InitializeAsync(context, userManager, roleManager);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Đã xảy ra lỗi khi khởi tạo dữ liệu (Seeding Data).");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession(); // ✅ ĐẶT TRƯỚC UseRouting()

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();  // ✅ THÊM: Phân quyền
            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages(); // ✅ THÊM: Map Identity Pages (/Identity/Account/Login...)

            await app.RunAsync();
        }
    }
}