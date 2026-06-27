-- ============================================================================
-- Migration 001: add a Role column to Users for role-based authorization.
-- Run this ONCE against the PandaKeyDB database before starting the extended
-- API. Safe to re-run: it checks for the column first.
-- ============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE Name = N'Role'
      AND Object_ID = Object_ID(N'dbo.Users')
)
BEGIN
    ALTER TABLE dbo.Users
        ADD Role NVARCHAR(20) NOT NULL CONSTRAINT DF_Users_Role DEFAULT N'user';
END
GO

-- Optional: promote a specific account to administrator.
-- Replace the e-mail with the account you want to make an admin, then run:
--
--   UPDATE dbo.Users SET Role = N'admin' WHERE Email = N'admin@pandakey.local';
--
