-- TechStore SQL Server schema generated from EF Core model
-- Run this script in SQL Server (SSMS / Azure Data Studio) to create the schema

CREATE TABLE dbo.AspNetRoles (
    Id nvarchar(450) NOT NULL PRIMARY KEY,
    Name nvarchar(256),
    NormalizedName nvarchar(256),
    ConcurrencyStamp nvarchar(max)
);
CREATE UNIQUE INDEX IX_AspNetRoles_NormalizedName ON dbo.AspNetRoles(NormalizedName) WHERE NormalizedName IS NOT NULL;

CREATE TABLE dbo.AspNetUsers (
    Id nvarchar(450) NOT NULL PRIMARY KEY,
    UserName nvarchar(256),
    NormalizedUserName nvarchar(256),
    Email nvarchar(256),
    NormalizedEmail nvarchar(256),
    EmailConfirmed bit NOT NULL,
    PasswordHash nvarchar(max),
    SecurityStamp nvarchar(max),
    ConcurrencyStamp nvarchar(max),
    PhoneNumber nvarchar(max),
    PhoneNumberConfirmed bit NOT NULL,
    TwoFactorEnabled bit NOT NULL,
    LockoutEnd datetimeoffset NULL,
    LockoutEnabled bit NOT NULL,
    AccessFailedCount int NOT NULL
);
CREATE INDEX IX_AspNetUsers_NormalizedEmail ON dbo.AspNetUsers(NormalizedEmail);
CREATE UNIQUE INDEX IX_AspNetUsers_UserName ON dbo.AspNetUsers(NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;

CREATE TABLE dbo.AspNetRoleClaims (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    RoleId nvarchar(450) NOT NULL,
    ClaimType nvarchar(max),
    ClaimValue nvarchar(max),
    CONSTRAINT FK_AspNetRoleClaims_Role FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.AspNetUserClaims (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId nvarchar(450) NOT NULL,
    ClaimType nvarchar(max),
    ClaimValue nvarchar(max),
    CONSTRAINT FK_AspNetUserClaims_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.AspNetUserLogins (
    LoginProvider nvarchar(128) NOT NULL,
    ProviderKey nvarchar(128) NOT NULL,
    ProviderDisplayName nvarchar(max),
    UserId nvarchar(450) NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.AspNetUserRoles (
    UserId nvarchar(450) NOT NULL,
    RoleId nvarchar(450) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_Role FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.AspNetUserTokens (
    UserId nvarchar(450) NOT NULL,
    LoginProvider nvarchar(128) NOT NULL,
    Name nvarchar(128) NOT NULL,
    Value nvarchar(max),
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_User FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
);

CREATE TABLE dbo.Brands (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name nvarchar(max) NOT NULL,
    LogoUrl nvarchar(max) NULL,
    Origin nvarchar(max) NULL,
    CreatedDate datetime2 NOT NULL,
    UpdatedDate datetime2 NULL,
    IsDeleted bit NOT NULL
);

CREATE TABLE dbo.Categories (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name nvarchar(max) NOT NULL,
    Slug nvarchar(max) NOT NULL,
    Description nvarchar(max) NULL,
    IconUrl nvarchar(max) NULL,
    ParentId int NULL,
    CreatedDate datetime2 NOT NULL,
    UpdatedDate datetime2 NULL,
    IsDeleted bit NOT NULL,
    CONSTRAINT FK_Categories_Parent FOREIGN KEY (ParentId) REFERENCES dbo.Categories(Id) ON DELETE NO ACTION
);

CREATE TABLE dbo.Products (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Name nvarchar(max) NOT NULL,
    Code nvarchar(max) NOT NULL,
    Slug nvarchar(max) NOT NULL,
    ShortDescription nvarchar(max) NULL,
    Description nvarchar(max) NULL,
    Price decimal(18,2) NOT NULL,
    DiscountPrice decimal(18,2) NULL,
    Stock int NOT NULL,
    SoldCount int NOT NULL,
    IsActive bit NOT NULL,
    IsFeatured bit NOT NULL,
    CategoryId int NOT NULL,
    BrandId int NULL,
    CreatedDate datetime2 NOT NULL,
    UpdatedDate datetime2 NULL,
    IsDeleted bit NOT NULL,
    CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Products_Brand FOREIGN KEY (BrandId) REFERENCES dbo.Brands(Id)
);
CREATE INDEX IX_Products_BrandId ON dbo.Products(BrandId);
CREATE INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);

CREATE TABLE dbo.Orders (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId nvarchar(max) NOT NULL,
    OrderDate datetime2 NOT NULL,
    FullName nvarchar(max) NOT NULL,
    PhoneNumber nvarchar(max) NOT NULL,
    ShippingAddress nvarchar(max) NOT NULL,
    Note nvarchar(max) NULL,
    TotalAmount decimal(18,2) NOT NULL,
    PaymentMethod nvarchar(max) NOT NULL,
    Status int NOT NULL,
    ShippingFee decimal(18,2) NOT NULL,
    ShippingProvider nvarchar(max) NULL,
    ShippingCode nvarchar(max) NULL,
    ShippingStatusRaw nvarchar(max) NULL,
    CreatedDate datetime2 NOT NULL,
    UpdatedDate datetime2 NULL,
    IsDeleted bit NOT NULL
);

CREATE TABLE dbo.OrderDetails (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    OrderId int NOT NULL,
    ProductId int NOT NULL,
    Quantity int NOT NULL,
    Price decimal(18,2) NOT NULL,
    CreatedDate datetime2 NOT NULL,
    UpdatedDate datetime2 NULL,
    IsDeleted bit NOT NULL,
    CONSTRAINT FK_OrderDetails_Order FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE NO ACTION
);
CREATE INDEX IX_OrderDetails_OrderId ON dbo.OrderDetails(OrderId);
CREATE INDEX IX_OrderDetails_ProductId ON dbo.OrderDetails(ProductId);

CREATE TABLE dbo.ProductImages (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ImageUrl nvarchar(max) NOT NULL,
    IsMain bit NOT NULL,
    SortOrder int NOT NULL,
    ProductId int NOT NULL,
    CreatedDate datetime2 NOT NULL,
    UpdatedDate datetime2 NULL,
    IsDeleted bit NOT NULL,
    CONSTRAINT FK_ProductImages_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
);
CREATE INDEX IX_ProductImages_ProductId ON dbo.ProductImages(ProductId);

CREATE TABLE dbo.ProductSpecifications (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ProductId int NOT NULL,
    Name nvarchar(max) NOT NULL,
    Value nvarchar(max) NOT NULL,
    CreatedDate datetime2 NOT NULL,
    UpdatedDate datetime2 NULL,
    IsDeleted bit NOT NULL,
    CONSTRAINT FK_ProductSpecifications_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
);
CREATE INDEX IX_ProductSpecifications_ProductId ON dbo.ProductSpecifications(ProductId);

CREATE TABLE dbo.Reviews (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ProductId int NOT NULL,
    UserId nvarchar(max) NOT NULL,
    UserName nvarchar(max) NOT NULL,
    Rating int NOT NULL,
    Comment nvarchar(max) NULL,
    CreatedDate datetime2 NOT NULL,
    UpdatedDate datetime2 NULL,
    IsDeleted bit NOT NULL,
    CONSTRAINT FK_Reviews_Product FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE CASCADE
);
CREATE INDEX IX_Reviews_ProductId ON dbo.Reviews(ProductId);
