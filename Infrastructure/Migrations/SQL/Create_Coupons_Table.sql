IF OBJECT_ID('dbo.Coupons', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Coupons](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Code] NVARCHAR(MAX) NOT NULL,
        [IsPercent] BIT NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [StartDate] DATETIME2 NULL,
        [EndDate] DATETIME2 NULL,
        [UsageLimit] INT NULL,
        [UsedCount] INT NOT NULL CONSTRAINT DF_Coupons_UsedCount DEFAULT(0),
        [IsActive] BIT NOT NULL CONSTRAINT DF_Coupons_IsActive DEFAULT(1),
        [CreatedDate] DATETIME2 NOT NULL,
        [UpdatedDate] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT DF_Coupons_IsDeleted DEFAULT(0)
    );
END
ELSE
BEGIN
    PRINT 'Table dbo.Coupons already exists.';
END
