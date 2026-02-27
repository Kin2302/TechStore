using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechStore.Infrastructure.Data;
using TechStore.Infrastructure.Services;
using Application.Interfaces;
using TechStore.Infrastructure.Plugins;
using Microsoft.SemanticKernel;

namespace WebApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly("TechStore.Infrastructure")));

            // ✅ KÍCH HOẠT IDENTITY + ROLES
            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false; 
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false; 
            })

            .AddRoles<IdentityRole>() // 
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages(); // 
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // ✅ ĐĂNG KÝ SERVICES (Clean Architecture)
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ProductPlugin>();
            builder.Services.AddScoped<CartPlugin>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            // ✅ Tạo Kernel với Gemini + Plugins
            builder.Services.AddScoped<Kernel>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var apiKey = config["Gemini:ApiKey"];

                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Gemini API Key not found in configuration!");
                }

                var kernelBuilder = Kernel.CreateBuilder();
                
                // Add Gemini với model stable
                kernelBuilder.AddGoogleAIGeminiChatCompletion(
                    modelId: "gemini-2.5-flash",
                    apiKey: apiKey
                );

                // ✅ Add Plugins
                var productPlugin = sp.GetRequiredService<ProductPlugin>();
                var cartPlugin = sp.GetRequiredService<CartPlugin>();
                
                kernelBuilder.Plugins.AddFromObject(productPlugin, "ProductTools");
                kernelBuilder.Plugins.AddFromObject(cartPlugin, "CartTools");

                return kernelBuilder.Build();
            });

            // ✅ GeminiService sử dụng Kernel đã tạo
            builder.Services.AddScoped<IGeminiService, GeminiService>();

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

            app.UseRouting(); // ✅ THÊM DÒNG NÀY

            app.UseAuthentication();
            app.UseAuthorization();  // ✅ THÊM: Phân quyền

            app.UseSession(); // ✅ CHỈ 1 LẦN, SAU Authorization

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages(); // ✅ THÊM: Map Identity Pages (/Identity/Account/Login...)

            await app.RunAsync();
        }
    }
}