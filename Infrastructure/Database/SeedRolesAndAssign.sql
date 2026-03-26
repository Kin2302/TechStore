-- Seed roles and assign to users (SQL Server)
-- Run in SSMS or Azure Data Studio against the application's database.
-- NOTE: Creating users with passwords via SQL is not recommended. Use the app or UserManager to create users.

-- 1) Insert roles if not exists
IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'ADMIN')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, [Name], NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Admin', 'ADMIN', NEWID());
END

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE NormalizedName = 'USER')
BEGIN
    INSERT INTO dbo.AspNetRoles (Id, [Name], NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'User', 'USER', NEWID());
END

-- 2) Assign role to an existing user by email
-- Replace the email below with the user's email you want to assign
DECLARE @emailToAssign nvarchar(256) = 'admin@techstore.com';
DECLARE @roleNormalized nvarchar(256) = 'ADMIN';

DECLARE @userId nvarchar(450) = (SELECT TOP(1) Id FROM dbo.AspNetUsers WHERE Email = @emailToAssign);
DECLARE @roleId nvarchar(450) = (SELECT TOP(1) Id FROM dbo.AspNetRoles WHERE NormalizedName = @roleNormalized);

IF @userId IS NULL
BEGIN
    RAISERROR('User with email %s not found. Create user via the app or UserManager, then run this script again.', 16, 1, @emailToAssign);
END
ELSE IF @roleId IS NULL
BEGIN
    RAISERROR('Role %s not found. The role insert step should have created it; check AspNetRoles.', 16, 1, @roleNormalized);
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId = @userId AND RoleId = @roleId)
    BEGIN
        INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
        VALUES (@userId, @roleId);
    END
END

-- 3) Example: assign User role to another email
-- DECLARE @email2 nvarchar(256) = 'user@techstore.com';
-- DECLARE @userId2 nvarchar(450) = (SELECT TOP(1) Id FROM dbo.AspNetUsers WHERE Email = @email2);
-- DECLARE @userRoleId nvarchar(450) = (SELECT TOP(1) Id FROM dbo.AspNetRoles WHERE NormalizedName = 'USER');
-- IF @userId2 IS NOT NULL AND @userRoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.AspNetUserRoles WHERE UserId=@userId2 AND RoleId=@userRoleId)
-- INSERT INTO dbo.AspNetUserRoles (UserId, RoleId) VALUES (@userId2, @userRoleId);

-- 4) Quick checks
-- SELECT Id, Name, NormalizedName FROM dbo.AspNetRoles;
-- SELECT Id, Email, UserName FROM dbo.AspNetUsers WHERE Email IN ('admin@techstore.com','user@techstore.com');
-- SELECT * FROM dbo.AspNetUserRoles;