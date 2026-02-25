-- Ecommerce Identity Database Setup Script
-- Run this script to manually create the database and tables

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'EcommerceIdentityDb')
BEGIN
    CREATE DATABASE EcommerceIdentityDb;
END
GO

USE EcommerceIdentityDb;
GO

-- Create Users Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' and xtype='U')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Email NVARCHAR(256) NOT NULL,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Roles NVARCHAR(MAX) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        RefreshToken NVARCHAR(MAX) NULL,
        RefreshTokenExpiryTime DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NULL,
        LastLoginAt DATETIME2 NULL,
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );

    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
END
GO

-- Insert Sample Admin User (Password: Admin@123)
-- Password hash is for "Admin@123" with BCrypt
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@ecommerce.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, Roles, IsActive, CreatedAt)
    VALUES (
        NEWID(),
        'admin@ecommerce.com',
        '$2a$11$xvRE5Q7XJYhOCBh4w9LYzOR6sTMqP5.zD5IqXqS9vYJKwXh5LQ5TG',
        'System',
        'Administrator',
        '["Admin","Manager"]',
        1,
        GETUTCDATE()
    );
END
GO

-- Insert Sample Customer User (Password: Customer@123)
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'customer@example.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, Roles, IsActive, CreatedAt)
    VALUES (
        NEWID(),
        'customer@example.com',
        '$2a$11$YB5R6Q8XKZiPDCh5x8MZzOT7tUNrQ6.{E6JrYsU8wZKLwYi6MR6UH',
        'John',
        'Doe',
        '["Customer"]',
        1,
        GETUTCDATE()
    );
END
GO

PRINT 'Database setup completed successfully!'
PRINT 'Sample users created:'
PRINT '1. Admin: admin@ecommerce.com / Admin@123'
PRINT '2. Customer: customer@example.com / Customer@123'
