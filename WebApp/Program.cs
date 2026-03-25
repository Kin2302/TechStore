using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechStore.Infrastructure.Data;
using TechStore.Infrastructure.Services;
using TechStore.Infrastructure.Plugins;
using Infrastructure.Services;
using Application.Interfaces.Catalog;
using Application.Interfaces.Orders;
using Application.Interfaces.Admin;
using Infrastructure.Services.Admin;
using Application.Interfaces.Integration;
using Application.DTOs.Integration;
using TechStore.Infrastructure.Services.Admin;
using TechStore.Infrastructure.Services.Integration;

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

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultUI();

            // Google OAuth
            builder.Services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
                    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
                });

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // === SERVICES (Clean Architecture) ===
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<IAdminOrderService, AdminOrderService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IAdminProductService, AdminProductService>();
            builder.Services.AddScoped<IAdminCategoryService, AdminCategoryService>();
            builder.Services.AddScoped<IAdminBrandService, AdminBrandService>();
            builder.Services.AddScoped<ICompareService, CompareService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // MoMo
            builder.Services.Configure<MoMoOptions>(builder.Configuration.GetSection("MoMo"));
            builder.Services.AddHttpClient<IMoMoService, MoMoService>();

            // GHN
            builder.Services.Configure<GHNOptions>(builder.Configuration.GetSection("GHN"));
            builder.Services.AddHttpClient<IGHNService, GHNService>();

            // SMTP
            builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
            builder.Services.AddScoped<IEmailService, EmailService>();

            // === AI PLUGINS ===
            builder.Services.AddScoped<ProductPlugin>();
            builder.Services.AddScoped<CartPlugin>();
            builder.Services.AddScoped<OrderPlugin>();
            builder.Services.AddScoped<StoreInfoPlugin>();
            builder.Services.AddScoped<AdminReportPlugin>();
            builder.Services.AddScoped<AdminProductPlugin>();
            builder.Services.AddScoped<AdminOrderPlugin>();

            // === KERNEL FACTORY + GEMINI SERVICE ===
            builder.Services.AddScoped<IKernelFactory, KernelFactory>();
            builder.Services.AddScoped<IGeminiService, GeminiService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
            );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            await app.RunAsync();
        }
    }
}
