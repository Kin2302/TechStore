using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TechStore.Infrastructure.Data;

namespace TechStore.Infrastructure
{
    // Provides a design-time factory so EF tools can create the DbContext without loading the WebApp startup
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Try to read connection string from environment first, then from WebApp/appsettings.json
            var envConn = Environment.GetEnvironmentVariable("DefaultConnection");

            var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "WebApp"));
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();
            var conn = envConn ?? config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(conn))
            {
                // fallback to localdb for developer convenience
                conn = "Server=(localdb)\\mssqllocaldb;Database=TechStore;Trusted_Connection=True;MultipleActiveResultSets=true";
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(conn, sql => sql.MigrationsAssembly("TechStore.Infrastructure"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
