using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechStore.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShippingTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use conditional SQL to avoid duplicate column errors if columns already exist
            migrationBuilder.Sql(@"
IF NOT EXISTS(
    SELECT 1 FROM sys.columns
    WHERE Name = 'ShippingCode' AND Object_ID = Object_ID('dbo.Orders')
)
BEGIN
    ALTER TABLE [Orders] ADD [ShippingCode] nvarchar(max) NULL;
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS(
    SELECT 1 FROM sys.columns
    WHERE Name = 'ShippingProvider' AND Object_ID = Object_ID('dbo.Orders')
)
BEGIN
    ALTER TABLE [Orders] ADD [ShippingProvider] nvarchar(max) NULL;
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS(
    SELECT 1 FROM sys.columns
    WHERE Name = 'ShippingStatusRaw' AND Object_ID = Object_ID('dbo.Orders')
)
BEGIN
    ALTER TABLE [Orders] ADD [ShippingStatusRaw] nvarchar(max) NULL;
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS(
    SELECT 1 FROM sys.columns
    WHERE Name = 'ShippingCode' AND Object_ID = Object_ID('dbo.Orders')
)
BEGIN
    ALTER TABLE [Orders] DROP COLUMN [ShippingCode];
END
");

            migrationBuilder.Sql(@"
IF EXISTS(
    SELECT 1 FROM sys.columns
    WHERE Name = 'ShippingProvider' AND Object_ID = Object_ID('dbo.Orders')
)
BEGIN
    ALTER TABLE [Orders] DROP COLUMN [ShippingProvider];
END
");

            migrationBuilder.Sql(@"
IF EXISTS(
    SELECT 1 FROM sys.columns
    WHERE Name = 'ShippingStatusRaw' AND Object_ID = Object_ID('dbo.Orders')
)
BEGIN
    ALTER TABLE [Orders] DROP COLUMN [ShippingStatusRaw];
END
");
        }
    }
}
