PRINT CHAR(13)
PRINT CHAR(13)
PRINT CHAR(13)
PRINT CHAR(13)
PRINT '-----------------------------'
PRINT 'START OF: LinkoExchange Setup'
PRINT '-----------------------------'
PRINT CHAR(13)


-------------------------------- Create LinkoExchange DB --------------------------------
PRINT CHAR(13)
PRINT CHAR(13)
PRINT 'Create LinkoExchange DB'

USE [master]
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = 'LinkoExchange')
BEGIN
	CREATE DATABASE [LinkoExchange]
	 ON  PRIMARY 
	( NAME = N'LinkoExchange_Primary', FILENAME = N'D:\mssql\data\LinkoExchange.mdf' , SIZE = 2GB , MAXSIZE = UNLIMITED, FILEGROWTH = 2GB )
    , 
     FILEGROUP [LinkoExchange_FG1_Data] DEFAULT
    ( NAME = N'LinkoExchange_FG1_Data', FILENAME = N'D:\mssql\data\LinkoExchange_FG1_Data.ndf' , SIZE = 2GB , MAXSIZE = UNLIMITED, FILEGROWTH = 2GB )
    ,
     FILEGROUP [LinkoExchange_FG2_Data]
    ( NAME = N'LinkoExchange_FG2_Data', FILENAME = N'D:\mssql\data\LinkoExchange_FG2_Data.ndf' , SIZE = 2GB , MAXSIZE = UNLIMITED, FILEGROWTH = 2GB )
    ,
     FILEGROUP [LinkoExchange_FG3_LOB]
    ( NAME = N'LinkoExchange_FG3_LOB', FILENAME = N'D:\mssql\data\LinkoExchange_FG3_LOB.ndf' , SIZE = 2GB , MAXSIZE = UNLIMITED, FILEGROWTH = 2GB )
	 LOG ON 
	( NAME = N'LinkoExchange_Log', FILENAME = N'D:\mssql\logs\LinkoExchange_Log.ldf' , SIZE = 2GB , MAXSIZE = UNLIMITED, FILEGROWTH = 2GB )
	
	-- SQL Server 2014
	ALTER DATABASE [LinkoExchange] SET COMPATIBILITY_LEVEL = 120
END
GO

-------------------------------- Set LinkoExchange DB user --------------------------------
PRINT CHAR(13)
PRINT CHAR(13)
PRINT 'Set LinkoExchange DB user'

USE [LinkoExchange]
GO

-- Uncomment the following section if this script is run on QA
--IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT 1 FROM master.sys.server_principals WHERE name = 'exnet')
--BEGIN
--    CREATE LOGIN exnet WITH PASSWORD = N'test$1234'
--END
--GO

--IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'exnet')
--BEGIN
--	CREATE USER exnet FOR LOGIN exnet
--	EXEC sp_addrolemember 'db_datareader', 'exnet'
--	EXEC sp_addrolemember 'db_datawriter', 'exnet'
--	EXEC sp_addrolemember 'db_owner', 'exnet'
--END
--GO


-------------------------------- Create new tables for LinkoExchange --------------------------------
PRINT CHAR(13)
PRINT CHAR(13)
PRINT 'Create new tables for LinkoExchange'

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tTimeZone') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tTimeZone'
    PRINT '----------------'

    CREATE TABLE dbo.tTimeZone 
    (
        TimeZoneId                      int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(100) NOT NULL
        , StandardAbbreviation          varchar(5) NOT NULL
        , DaylightAbbreviation          varchar(5) NULL
		, CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tTimeZone PRIMARY KEY CLUSTERED 
        (
	        TimeZoneId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tTimeZone_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tTimeZone ADD CONSTRAINT DF_tTimeZone_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tJurisdiction') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tJurisdiction'
    PRINT '--------------------'
    
    CREATE TABLE dbo.tJurisdiction 
    (
        JurisdictionId                  int IDENTITY(1,1) NOT NULL  
        , CountryId                     int NOT NULL  
        , StateId                       int NOT NULL  
        , Code                          char(2) NOT NULL  
        , Name                          varchar(100) NOT NULL  
        , ParentId                      int NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL 
        
        CONSTRAINT PK_tJurisdiction PRIMARY KEY CLUSTERED 
        (
	        JurisdictionID ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data] 
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tJurisdiction ADD CONSTRAINT DF_tJurisdiction_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tRegulatoryProgram') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tRegulatoryProgram'
    PRINT '-------------------------'
    
    CREATE TABLE dbo.tRegulatoryProgram 
    (
        RegulatoryProgramId             int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(100) NOT NULL  
        , Description                   varchar(500) NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
        
        CONSTRAINT PK_tRegulatoryProgram PRIMARY KEY CLUSTERED 
        (
	        RegulatoryProgramId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tRegulatoryProgram_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tRegulatoryProgram ADD CONSTRAINT DF_tRegulatoryProgram_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tOrganizationType') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tOrganizationType'
    PRINT '------------------------'
    
    CREATE TABLE dbo.tOrganizationType 
    (
        OrganizationTypeId              int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(100) NOT NULL  
        , Description                   varchar(500) NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
        
        CONSTRAINT PK_tOrganizationType PRIMARY KEY CLUSTERED 
        (
	        OrganizationTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tOrganizationType_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tOrganizationType ADD CONSTRAINT DF_tOrganizationType_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tPrivacyPolicy') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tPrivacyPolicy'
    PRINT '---------------------'

    CREATE TABLE dbo.tPrivacyPolicy 
    (
        PrivacyPolicyId                 int IDENTITY(1,1) NOT NULL  
        , Content                       varchar(max) NOT NULL
        , EffectiveDateTimeUtc          datetimeoffset(0) NOT NULL
		, CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tPrivacyPolicy PRIMARY KEY CLUSTERED 
        (
	       PrivacyPolicyId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tPrivacyPolicy ADD CONSTRAINT DF_tPrivacyPolicy_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tTermCondition') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tTermCondition'
    PRINT '---------------------'

    CREATE TABLE dbo.tTermCondition 
    (
        TermConditionId                 int IDENTITY(1,1) NOT NULL  
        , Content                       varchar(max) NOT NULL
		, CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tTermCondition PRIMARY KEY CLUSTERED 
        (
	       TermConditionId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tTermCondition ADD CONSTRAINT DF_tTermCondition_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tUserProfile') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tUserProfile'
    PRINT '-------------------'

    CREATE TABLE dbo.tUserProfile 
    (
        UserProfileId                       int IDENTITY(1,1) NOT NULL  
        , FirstName                         varchar(50) NOT NULL  
        , LastName                          varchar(50) NOT NULL  
        , TitleRole                         varchar(250) NULL  
        , BusinessName                      varchar(100) NOT NULL  
        , AddressLine1                      varchar(100) NOT NULL  
        , AddressLine2                      varchar(100) NULL  
        , CityName                          varchar(100) NOT NULL  
        , ZipCode                           varchar(50) NOT NULL  
        , JurisdictionId                    int NULL    
        , PhoneExt                          int NULL  
        , IsAccountLocked                   bit NOT NULL  
        , IsAccountResetRequired            bit NOT NULL  
        , IsIdentityProofed                 bit NOT NULL  
        , IsInternalAccount                 bit NOT NULL  
        , OldEmailAddress                   nvarchar(256) NULL
        , TermConditionId                   int NOT NULL
        , TermConditionAgreedDateTimeUtc    datetimeoffset(0) NOT NULL
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , Id                                nvarchar(128) NOT NULL  
        , Email                             nvarchar(256) NOT NULL  
        , EmailConfirmed                    bit NOT NULL  
        , PasswordHash                      nvarchar(max) NULL  
        , SecurityStamp                     nvarchar(max) NULL
        , PhoneNumber                       nvarchar(max) NOT NULL  
        , PhoneNumberConfirmed              bit NOT NULL  
        , TwoFactorEnabled                  bit NOT NULL  
        , LockoutEndDateUtc                 datetime NULL  
        , LockoutEnabled                    bit NOT NULL  
        , AccessFailedCount                 int NOT NULL  
        , UserName                          nvarchar(256) NOT NULL  
        
        CONSTRAINT PK_tUserProfile PRIMARY KEY NONCLUSTERED 
        (
	        Id ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tUserProfile_UserProfileId UNIQUE 
        (
            UserProfileId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tUserProfile_tJurisdiction FOREIGN KEY 
		(
			JurisdictionId
		) REFERENCES dbo.tJurisdiction(JurisdictionId)
        , CONSTRAINT FK_tUserProfile_tTermCondition FOREIGN KEY 
		(
			TermConditionId
		) REFERENCES dbo.tTermCondition(TermConditionId)
        , CONSTRAINT AK_tUserProfile_Email UNIQUE 
        (
            Email ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tUserProfile_UserName UNIQUE 
        (
            UserName ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tUserProfile ADD CONSTRAINT DF_tUserProfile_IsAccountLocked DEFAULT 0 FOR IsAccountLocked
    ALTER TABLE dbo.tUserProfile ADD CONSTRAINT DF_tUserProfile_IsAccountResetRequired DEFAULT 0 FOR IsAccountResetRequired
    ALTER TABLE dbo.tUserProfile ADD CONSTRAINT DF_tUserProfile_IsIdentityProofed DEFAULT 0 FOR IsIdentityProofed
    ALTER TABLE dbo.tUserProfile ADD CONSTRAINT DF_tUserProfile_IsInternalAccount DEFAULT 0 FOR IsInternalAccount
    ALTER TABLE dbo.tUserProfile ADD CONSTRAINT DF_tUserProfile_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tUserProfile_JurisdictionId ON dbo.tUserProfile 
    (
	    JurisdictionId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tUserProfile_TermConditionId ON dbo.tUserProfile 
    (
	    TermConditionId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tOrganization') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tOrganization'
    PRINT '--------------------'
    
    CREATE TABLE dbo.tOrganization 
    (
        OrganizationId                  int IDENTITY(1000,1) NOT NULL  
        , OrganizationTypeId            int NOT NULL  
        , Name                          varchar(254) NOT NULL  
        , AddressLine1                  varchar(100) NULL  
        , AddressLine2                  varchar(100) NULL  
        , CityName                      varchar(100) NULL  
        , ZipCode                       varchar(50) NULL  
        , JurisdictionId                int NULL  
        , PhoneNumber                   varchar(25) NULL  
        , PhoneExt                      int NULL  
        , FaxNumber                     varchar(25) NULL  
        , WebsiteURL                    varchar(256) NULL
        , Signer                        varchar(250) NULL
		, Classification                varchar(50) NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
        
        CONSTRAINT PK_tOrganization PRIMARY KEY CLUSTERED 
        (
	        OrganizationId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tOrganization_tOrganizationType FOREIGN KEY 
		(
			OrganizationTypeId
		) REFERENCES dbo.tOrganizationType(OrganizationTypeId)
		NOT FOR REPLICATION
		, CONSTRAINT FK_tOrganization_tJurisdiction FOREIGN KEY 
		(
			JurisdictionId
		) REFERENCES dbo.tJurisdiction(JurisdictionId)
		NOT FOR REPLICATION
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tOrganization ADD CONSTRAINT DF_tOrganization_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganization_OrganizationTypeId ON dbo.tOrganization 
    (
	    OrganizationTypeId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tOrganization_JurisdictionId ON dbo.tOrganization 
    (
	    JurisdictionId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tOrganizationTypeRegulatoryProgram') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tOrganizationTypeRegulatoryProgram'
    PRINT '-----------------------------------------'
    
    CREATE TABLE dbo.tOrganizationTypeRegulatoryProgram 
    (
        OrganizationTypeRegulatoryProgramId int IDENTITY(1,1) NOT NULL  
        , RegulatoryProgramId               int NOT NULL  
        , OrganizationTypeId                int NOT NULL
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
        
        CONSTRAINT PK_tOrganizationTypeRegulatoryProgram PRIMARY KEY CLUSTERED 
        (
	        OrganizationTypeRegulatoryProgramId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tOrganizationTypeRegulatoryProgram_RegulatoryProgramId_OrganizationTypeId UNIQUE 
        (
            RegulatoryProgramId ASC
            , OrganizationTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tOrganizationTypeRegulatoryProgram_tRegulatoryProgram FOREIGN KEY 
		(
			RegulatoryProgramId
		) REFERENCES dbo.tRegulatoryProgram(RegulatoryProgramId)
		NOT FOR REPLICATION
		, CONSTRAINT FK_tOrganizationTypeRegulatoryProgram_tOrganizationType FOREIGN KEY 
		(
			OrganizationTypeId
		) REFERENCES dbo.tOrganizationType(OrganizationTypeId)
		NOT FOR REPLICATION
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tOrganizationTypeRegulatoryProgram ADD CONSTRAINT DF_tOrganizationTypeRegulatoryProgram_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationTypeRegulatoryProgram_RegulatoryProgramId ON dbo.tOrganizationTypeRegulatoryProgram 
    (
	    RegulatoryProgramId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationTypeRegulatoryProgram_OrganizationTypeId ON dbo.tOrganizationTypeRegulatoryProgram 
    (
	    OrganizationTypeId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tOrganizationRegulatoryProgram') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tOrganizationRegulatoryProgram'
    PRINT '-------------------------------------'
    
    CREATE TABLE dbo.tOrganizationRegulatoryProgram 
    (
        OrganizationRegulatoryProgramId int IDENTITY(1,1) NOT NULL  
        , RegulatoryProgramId           int NOT NULL  
        , OrganizationId                int NOT NULL  
        , RegulatorOrganizationId       int NULL  
        , AssignedTo                    varchar(50) NULL
        , ReferenceNumber               varchar(50) NULL  
        , IsEnabled                     bit NOT NULL  
        , IsRemoved                     bit NOT NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL
        
        CONSTRAINT PK_tOrganizationRegulatoryProgram PRIMARY KEY CLUSTERED 
        (
	        OrganizationRegulatoryProgramId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tOrganizationRegulatoryProgram_OrganizationId_RegulatoryProgramId UNIQUE 
        (
            OrganizationId ASC
            , RegulatoryProgramId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tOrganizationRegulatoryProgram_tRegulatoryProgram FOREIGN KEY 
		(
			RegulatoryProgramId
		) REFERENCES dbo.tRegulatoryProgram(RegulatoryProgramId) 
		, CONSTRAINT FK_tOrganizationRegulatoryProgram_tOrganization_OrganizationId FOREIGN KEY 
		(
			OrganizationId
		) REFERENCES dbo.tOrganization(OrganizationId) 
		, CONSTRAINT FK_tOrganizationRegulatoryProgram_tOrganization_RegulatorOrganizationId FOREIGN KEY 
		(
			RegulatorOrganizationId
		) REFERENCES dbo.tOrganization(OrganizationId) 
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tOrganizationRegulatoryProgram ADD CONSTRAINT DF_tOrganizationRegulatoryProgram_IsEnabled DEFAULT 0 FOR IsEnabled
    ALTER TABLE dbo.tOrganizationRegulatoryProgram ADD CONSTRAINT DF_tOrganizationRegulatoryProgram_IsRemoved DEFAULT 0 FOR IsRemoved
    ALTER TABLE dbo.tOrganizationRegulatoryProgram ADD CONSTRAINT DF_tOrganizationRegulatoryProgram_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgram_RegulatoryProgramId ON dbo.tOrganizationRegulatoryProgram
	(
		RegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgram_OrganizationId ON dbo.tOrganizationRegulatoryProgram
	(
		OrganizationId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgram_RegulatorOrganizationId ON dbo.tOrganizationRegulatoryProgram
	(
		RegulatorOrganizationId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tSettingTemplate') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tSettingTemplate'
    PRINT '-----------------------'
    
    CREATE TABLE dbo.tSettingTemplate 
    (
        SettingTemplateId               int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(100) NOT NULL  
        , Description                   varchar(500) NULL  
        , DefaultValue                  varchar(500) NOT NULL
        , OrganizationTypeId            int NOT NULL  
        , RegulatoryProgramId           int NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
        
        CONSTRAINT PK_tSettingTemplate PRIMARY KEY CLUSTERED 
        (
	        SettingTemplateId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tSettingTemplate_tOrganizationType FOREIGN KEY 
		(
			OrganizationTypeId
		) REFERENCES dbo.tOrganizationType(OrganizationTypeId)
		NOT FOR REPLICATION
		, CONSTRAINT FK_tSettingTemplate_tRegulatoryProgram FOREIGN KEY 
		(
			RegulatoryProgramId
		) REFERENCES dbo.tRegulatoryProgram(RegulatoryProgramId)
		NOT FOR REPLICATION
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tSettingTemplate ADD CONSTRAINT DF_tSettingTemplate_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tSettingTemplate_Name ON dbo.tSettingTemplate 
    (
	    Name ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tSettingTemplate_OrganizationTypeId ON dbo.tSettingTemplate 
    (
	    OrganizationTypeId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tSettingTemplate_RegulatoryProgramId ON dbo.tSettingTemplate 
    (
	    RegulatoryProgramId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tSystemSetting') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tSystemSetting'
    PRINT '---------------------'
    
    CREATE TABLE dbo.tSystemSetting 
    (
        SystemSettingId                 int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(100) NOT NULL  
        , Value                         varchar(500) NOT NULL  
        , Description                   varchar(500) NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL 
        
        CONSTRAINT PK_tSystemSetting PRIMARY KEY CLUSTERED 
        (
	        SystemSettingId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data] 
        , CONSTRAINT AK_tSystemSetting_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tSystemSetting ADD CONSTRAINT DF_tSystemSetting_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tOrganizationSetting') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tOrganizationSetting'
    PRINT '---------------------------'
    
    CREATE TABLE dbo.tOrganizationSetting 
    (
        OrganizationSettingId           int IDENTITY(1,1) NOT NULL  
        , OrganizationId                int NOT NULL  
        , SettingTemplateId             int NOT NULL  
        , Value                         varchar(500) NOT NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
        
        CONSTRAINT PK_tOrganizationSetting PRIMARY KEY CLUSTERED 
        (
	        OrganizationSettingId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tOrganizationSetting_OrganizationId_SettingTemplateId UNIQUE 
        (
            OrganizationId ASC
            , SettingTemplateId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tOrganizationSetting_tOrganization FOREIGN KEY 
		(
			OrganizationId
		) REFERENCES dbo.tOrganization(OrganizationId)
		, CONSTRAINT FK_tOrganizationSetting_tSettingTemplate FOREIGN KEY 
		(
			SettingTemplateId
		) REFERENCES dbo.tSettingTemplate(SettingTemplateId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tOrganizationSetting ADD CONSTRAINT DF_tOrganizationSetting_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationSetting_OrganizationId ON dbo.tOrganizationSetting
	(
		OrganizationId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationSetting_SettingTemplateId ON dbo.tOrganizationSetting
	(
		SettingTemplateId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tOrganizationRegulatoryProgramSetting') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tOrganizationRegulatoryProgramSetting'
    PRINT '--------------------------------------------'
    
    CREATE TABLE dbo.tOrganizationRegulatoryProgramSetting 
    (
        OrganizationRegulatoryProgramSettingId  int IDENTITY(1,1) NOT NULL  
        , OrganizationRegulatoryProgramId       int NOT NULL  
        , SettingTemplateId                     int NOT NULL  
        , Value                                 varchar(500) NOT NULL  
        , CreationDateTimeUtc                   datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc           datetimeoffset(0) NULL  
        , LastModifierUserId                    int NULL
        
        CONSTRAINT PK_tOrganizationRegulatoryProgramSetting PRIMARY KEY CLUSTERED 
        (
	        OrganizationRegulatoryProgramSettingId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tOrganizationRegulatoryProgramSetting_OrganizationRegulatoryProgramId_SettingTemplateId UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , SettingTemplateId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tOrganizationRegulatoryProgramSetting_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
		, CONSTRAINT FK_tOrganizationRegulatoryProgramSetting_tSettingTemplate FOREIGN KEY 
		(
			SettingTemplateId
		) REFERENCES dbo.tSettingTemplate(SettingTemplateId)  
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tOrganizationRegulatoryProgramSetting ADD CONSTRAINT DF_tOrganizationRegulatoryProgramSetting_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramSetting_OrganizationRegulatoryProgramId ON dbo.tOrganizationRegulatoryProgramSetting
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramSetting_SettingTemplateId ON dbo.tOrganizationRegulatoryProgramSetting
	(
		SettingTemplateId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tInvitation') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tInvitation'
    PRINT '------------------'
    
    CREATE TABLE dbo.tInvitation 
    (
        InvitationId                                nvarchar(128) NOT NULL  
        , FirstName                                 varchar(50) NOT NULL  
        , LastName                                  varchar(50) NOT NULL  
        , EmailAddress                              nvarchar(256) NOT NULL  
        , InvitationDateTimeUtc                     datetimeoffset(0) NOT NULL  
        , SenderOrganizationRegulatoryProgramId     int NOT NULL  
        , RecipientOrganizationRegulatoryProgramId  int NOT NULL
        , IsResetInvitation                         bit NOT NULL  
        
        CONSTRAINT PK_tInvitation PRIMARY KEY NONCLUSTERED 
        (
	        InvitationId ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tInvitation_tOrganizationRegulatoryProgram_SenderOrganizationRegulatoryProgramId FOREIGN KEY 
		(
			SenderOrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
		NOT FOR REPLICATION
		, CONSTRAINT FK_tInvitation_tOrganizationRegulatoryProgram_RecipientOrganizationRegulatoryProgramId FOREIGN KEY 
		(
			RecipientOrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
		NOT FOR REPLICATION
	) ON [LinkoExchange_FG1_Data]
	
    ALTER TABLE dbo.tInvitation ADD CONSTRAINT DF_tInvitation_IsResetInvitation DEFAULT 0 FOR IsResetInvitation

	CREATE NONCLUSTERED INDEX IX_tInvitation_SenderOrganizationRegulatoryProgramId ON dbo.tInvitation 
    (
	    SenderOrganizationRegulatoryProgramId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tInvitation_RecipientOrganizationRegulatoryProgramId ON dbo.tInvitation 
    (
	    RecipientOrganizationRegulatoryProgramId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.AspNetUserClaims') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create AspNetUserClaims'
    PRINT '-----------------------'
    
    CREATE TABLE dbo.AspNetUserClaims 
    (
        Id              int IDENTITY(1,1) NOT NULL
        , UserId        nvarchar(128) NULL  
        , ClaimType     nvarchar(max) NULL  
        , ClaimValue    nvarchar(max) NULL    
        
        CONSTRAINT PK_AspNetUserClaims PRIMARY KEY CLUSTERED 
		(
			Id ASC
		) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
		, CONSTRAINT FK_AspNetUserClaims_tUserProfile FOREIGN KEY 
		(
			UserId
		) REFERENCES dbo.tUserProfile(Id) 
		ON DELETE CASCADE
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_AspNetUserClaims_UserId ON dbo.AspNetUserClaims
	(
		UserId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.AspNetUserLogins') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create AspNetUserLogins'
    PRINT '-----------------------'
    
    CREATE TABLE dbo.AspNetUserLogins 
    (
        LoginProvider   nvarchar(128) NOT NULL  
        , ProviderKey   nvarchar(128) NOT NULL  
        , UserId        nvarchar(128) NOT NULL
        
        CONSTRAINT PK_AspNetUserLogins PRIMARY KEY CLUSTERED 
		(
			LoginProvider ASC
			, ProviderKey ASC
			, UserId ASC
		) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
		, CONSTRAINT FK_AspNetUserLogins_tUserProfile FOREIGN KEY 
		(
			UserId
		) REFERENCES dbo.tUserProfile(Id) 
		ON DELETE CASCADE  
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_AspNetUserLogins_UserId ON dbo.AspNetUserLogins
	(
		UserId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.AspNetRoles') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create AspNetRoles'
    PRINT '------------------'
    
    CREATE TABLE dbo.AspNetRoles 
    (
        Id      nvarchar(128) NOT NULL  
        , Name  nvarchar(256) NOT NULL  
        
        CONSTRAINT PK_AspNetRoles PRIMARY KEY CLUSTERED 
        (
	        Id ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_AspNetRoles_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.AspNetUserRoles') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create AspNetUserRoles'
    PRINT '----------------------'
    
    CREATE TABLE dbo.AspNetUserRoles 
    (
        UserId      nvarchar(128) NOT NULL 
        , RoleId    nvarchar(128) NOT NULL  
        
        CONSTRAINT PK_AspNetUserRoles PRIMARY KEY CLUSTERED 
		(
			UserId ASC
			, RoleId ASC
		) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
		, CONSTRAINT FK_AspNetUserRoles_AspNetRoles FOREIGN KEY 
		(
			RoleId
		) REFERENCES dbo.AspNetRoles(Id) 
		ON DELETE CASCADE
		, CONSTRAINT FK_AspNetUserRoles_tUserProfile FOREIGN KEY 
		(
			UserId
		) REFERENCES dbo.tUserProfile(Id) 
		ON DELETE CASCADE
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_AspNetUserRoles_UserId ON dbo.AspNetUserRoles
	(
		UserId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

	CREATE NONCLUSTERED INDEX IX_AspNetUserRoles_RoleId ON dbo.AspNetUserRoles
	(
		RoleId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tQuestionType') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tQuestionType'
    PRINT '--------------------'
    
    CREATE TABLE dbo.tQuestionType 
    (
        QuestionTypeId                  int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(100) NOT NULL
        , Description                   varchar(500) NULL
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL 
        
        CONSTRAINT PK_tQuestionType PRIMARY KEY CLUSTERED 
        (
	        QuestionTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tQuestionType_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data] 
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tQuestionType ADD CONSTRAINT DF_tQuestionType_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tQuestion') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tQuestion'
    PRINT '----------------'
    
    CREATE TABLE dbo.tQuestion 
    (
        QuestionID                      int IDENTITY(1,1) NOT NULL  
        , Content                       varchar(500) NOT NULL  
        , QuestionTypeId                int NOT NULL  
        , IsActive                      bit NOT NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
        
        CONSTRAINT PK_tQuestion PRIMARY KEY CLUSTERED 
        (
	        QuestionID ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tQuestion_tQuestionType FOREIGN KEY 
		(
			QuestionTypeId
		) REFERENCES dbo.tQuestionType(QuestionTypeId)  
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tQuestion ADD CONSTRAINT DF_tQuestion_IsActive DEFAULT 1 FOR IsActive
    ALTER TABLE dbo.tQuestion ADD CONSTRAINT DF_tQuestion_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tQuestion_QuestionTypeId ON dbo.tQuestion
	(
		QuestionTypeId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tUserQuestionAnswer') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tUserQuestionAnswer'
    PRINT '--------------------------'
    
    CREATE TABLE dbo.tUserQuestionAnswer 
    (
        UserQuestionAnswerId            int IDENTITY(1,1) NOT NULL  
        , Content                       nvarchar(max) NOT NULL  
        , QuestionId                    int NOT NULL  
        , UserProfileId                 int NOT NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        
        CONSTRAINT PK_tUserQuestionAnswer PRIMARY KEY CLUSTERED 
        (
	        UserQuestionAnswerId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tUserQuestionAnswer_UserProfileId_QuestionId UNIQUE 
        (
            UserProfileId ASC
            , QuestionId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data] 
        , CONSTRAINT FK_tUserQuestionAnswer_tQuestion FOREIGN KEY 
		(
			QuestionId
		) REFERENCES dbo.tQuestion(QuestionId)
        , CONSTRAINT FK_tUserQuestionAnswer_tUserProfile FOREIGN KEY 
		(
			UserProfileId
		) REFERENCES dbo.tUserProfile(UserProfileId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tUserQuestionAnswer ADD CONSTRAINT DF_tUserQuestionAnswer_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tUserQuestionAnswer_QuestionId ON dbo.tUserQuestionAnswer
	(
		QuestionId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tUserQuestionAnswer_UserProfileId ON dbo.tUserQuestionAnswer
	(
		UserProfileId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tUserPasswordHistory') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tUserPasswordHistory'
    PRINT '---------------------------'
    
    CREATE TABLE dbo.tUserPasswordHistory 
    (
        UserPasswordHistoryId         int IDENTITY(1,1) NOT NULL  
        , PasswordHash                nvarchar(max) NOT NULL  
        , UserProfileId               int NOT NULL  
        , LastModificationDateTimeUtc datetimeoffset(0) NOT NULL
        
        CONSTRAINT PK_tUserPasswordHistory PRIMARY KEY CLUSTERED 
		(
			UserPasswordHistoryId ASC
		) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
		, CONSTRAINT FK_tUserPasswordHistory_tUserProfile FOREIGN KEY 
		(
			UserProfileId
		) REFERENCES dbo.tUserProfile(UserProfileId)   
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tUserPasswordHistory ADD CONSTRAINT DF_tUserPasswordHistory_LastModificationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR LastModificationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tUserPasswordHistory_UserProfileId ON dbo.tUserPasswordHistory
	(
		UserProfileId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tAuditLogTemplate') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tAuditLogTemplate'
    PRINT '------------------------'

    CREATE TABLE dbo.tAuditLogTemplate 
    (
        AuditLogTemplateId              int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(100) NOT NULL  
        , TemplateType                  varchar(15) NOT NULL  
        , EventCategory                 varchar(30) NOT NULL  
        , EventType                     varchar(50) NOT NULL  
        , SubjectTemplate               varchar(500) NULL  
        , MessageTemplate               varchar(max) NOT NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
        
        CONSTRAINT PK_tAuditLogTemplate PRIMARY KEY CLUSTERED 
        (
	        AuditLogTemplateId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT AK_tAuditLogTemplate_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT AK_tAuditLogTemplate_TemplateType_EventCategory_EventType UNIQUE 
        (
            TemplateType ASC
            , EventCategory ASC
            , EventType ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]  
    ) ON [LinkoExchange_FG2_Data]
    
    ALTER TABLE dbo.tAuditLogTemplate ADD CONSTRAINT DF_tAuditLogTemplate_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tEmailAuditLog') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tEmailAuditLog'
    PRINT '---------------------'

    CREATE TABLE dbo.tEmailAuditLog 
    (
        EmailAuditLogId                     int IDENTITY(1,1) NOT NULL  
        , AuditLogTemplateId                int NOT NULL  
        , SenderRegulatoryProgramId         int NULL  
        , SenderOrganizationId              int NULL  
        , SenderRegulatorOrganizationId     int NULL  
        , SenderUserProfileId               int NULL
        , SenderUserName                    nvarchar(256) NULL  
        , SenderFirstName                   varchar(50) NULL  
        , SenderLastName                    varchar(50) NULL  
        , SenderEmailAddress                nvarchar(256) NOT NULL  
        , RecipientRegulatoryProgramId      int NULL  
        , RecipientOrganizationId           int NULL  
        , RecipientRegulatorOrganizationId  int NULL
        , RecipientUserProfileId            int NULL  
        , RecipientUserName                 nvarchar(256) NULL  
        , RecipientFirstName                varchar(50) NULL  
        , RecipientLastName                 varchar(50) NULL  
        , RecipientEmailAddress             nvarchar(256) NOT NULL  
        , Subject                           varchar(500) NOT NULL  
        , Body                              varchar(max) NOT NULL
        , Token                             nvarchar(128) NULL  
        , SentDateTimeUtc                   datetimeoffset(0) NOT NULL
        
        CONSTRAINT PK_tEmailAuditLog PRIMARY KEY CLUSTERED 
        (
	        EmailAuditLogId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT FK_tEmailAuditLog_tAuditLogTemplate FOREIGN KEY 
		(
			AuditLogTemplateId
		) REFERENCES dbo.tAuditLogTemplate(AuditLogTemplateId)  
    ) ON [LinkoExchange_FG2_Data]
    
    CREATE NONCLUSTERED INDEX IX_tEmailAuditLog_AuditLogTemplateId ON dbo.tEmailAuditLog
	(
		AuditLogTemplateId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
	
	CREATE NONCLUSTERED INDEX IX_tEmailAuditLog_Token ON dbo.tEmailAuditLog
	(
		Token ASC
	) WITH FILLFACTOR = 90 ON [LinkoExchange_FG2_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tCromerrAuditLog') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tCromerrAuditLog'
    PRINT '-----------------------'

    CREATE TABLE dbo.tCromerrAuditLog 
    (
        CromerrAuditLogId           int IDENTITY(1,1) NOT NULL  
        , AuditLogTemplateId        int NOT NULL  
        , RegulatoryProgramId       int NULL  
        , OrganizationId            int NULL  
        , RegulatorOrganizationId   int NULL
		, UserProfileId				int NULL  
        , UserName                  nvarchar(256) NOT NULL  
        , UserFirstName             varchar(50) NULL  
        , UserLastName              varchar(50) NULL  
        , UserEmailAddress          nvarchar(256) NULL  
        , IPAddress                 varchar(50) NULL  
        , HostName                  varchar(256) NULL  
        , Comment                   varchar(max) NOT NULL  
        , LogDateTimeUtc            datetimeoffset(0) NOT NULL 
        
        CONSTRAINT PK_tCromerrAuditLog PRIMARY KEY CLUSTERED 
        (
	        CromerrAuditLogId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT FK_tCromerrAuditLog_tAuditLogTemplate FOREIGN KEY 
		(
			AuditLogTemplateId
		) REFERENCES dbo.tAuditLogTemplate(AuditLogTemplateId)  
    ) ON [LinkoExchange_FG2_Data]
    
    CREATE NONCLUSTERED INDEX IX_tCromerrAuditLog_AuditLogTemplateId ON dbo.tCromerrAuditLog
	(
		AuditLogTemplateId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tModule') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tModule'
    PRINT '--------------'

    CREATE TABLE dbo.tModule 
    (
        ModuleId                                int IDENTITY(1,1) NOT NULL  
        , Name                                  varchar(100) NOT NULL  
        , Description                           varchar(500) NULL  
        , OrganizationTypeRegulatoryProgramId   int NOT NULL  
        , CreationDateTimeUtc                   datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc           datetimeoffset(0) NULL  
        , LastModifierUserId                    int NULL  
    
        CONSTRAINT PK_tModule PRIMARY KEY CLUSTERED 
        (
	        ModuleId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tModule_OrganizationTypeRegulatoryProgramId_Name UNIQUE 
        (
            OrganizationTypeRegulatoryProgramId ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tModule_tOrganizationTypeRegulatoryProgram FOREIGN KEY 
		(
			OrganizationTypeRegulatoryProgramId
		) REFERENCES dbo.tOrganizationTypeRegulatoryProgram(OrganizationTypeRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tModule ADD CONSTRAINT DF_tModule_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tModule_Name ON dbo.tModule
	(
		Name ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tModule_OrganizationTypeRegulatoryProgramId ON dbo.tModule
	(
		OrganizationTypeRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tOrganizationRegulatoryProgramModule') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tOrganizationRegulatoryProgramModule'
    PRINT '-------------------------------------------'

    CREATE TABLE dbo.tOrganizationRegulatoryProgramModule 
    (
        OrganizationRegulatoryProgramModuleId   int IDENTITY(1,1) NOT NULL  
        , OrganizationRegulatoryProgramId       int NOT NULL
        , ModuleId                              int NOT NULL  
        , CreationDateTimeUtc                   datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc           datetimeoffset(0) NULL  
        , LastModifierUserId                    int NULL  
    
        CONSTRAINT PK_tOrganizationRegulatoryProgramModule PRIMARY KEY CLUSTERED 
        (
	        OrganizationRegulatoryProgramModuleId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tOrganizationRegulatoryProgramModule_OrganizationRegulatoryProgramId_ModuleId UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , ModuleId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tOrganizationRegulatoryProgramModule_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
		, CONSTRAINT FK_tOrganizationRegulatoryProgramModule_tModule FOREIGN KEY 
		(
			ModuleId
		) REFERENCES dbo.tModule(ModuleId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tOrganizationRegulatoryProgramModule ADD CONSTRAINT DF_tOrganizationRegulatoryProgramModule_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramModule_OrganizationRegulatoryProgramId ON dbo.tOrganizationRegulatoryProgramModule
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramModule_ModuleId ON dbo.tOrganizationRegulatoryProgramModule
	(
		ModuleId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tPermission') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tPermission'
    PRINT '------------------'
	
	CREATE TABLE dbo.tPermission 
	(
		PermissionId                  int IDENTITY(1,1) NOT NULL  
		, Name                        varchar(100) NOT NULL  
		, Description                 varchar(500) NULL  
		, ModuleId                    int NOT NULL  
		, CreationDateTimeUtc         datetimeoffset(0) NOT NULL  
		, LastModificationDateTimeUtc datetimeoffset(0) NULL  
		, LastModifierUserId          int NULL  
	
	    CONSTRAINT PK_tPermission PRIMARY KEY CLUSTERED 
        (
	        PermissionId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tPermission_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tPermission_tModule FOREIGN KEY 
		(
			ModuleId
		) REFERENCES dbo.tModule(ModuleId)
		NOT FOR REPLICATION
	) ON [LinkoExchange_FG1_Data]
	
	ALTER TABLE dbo.tPermission ADD CONSTRAINT DF_tPermission_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

	CREATE NONCLUSTERED INDEX IX_tPermission_ModuleId ON dbo.tPermission 
    (
	    ModuleId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tPermissionGroupTemplate') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tPermissionGroupTemplate'
    PRINT '-------------------------------'

    CREATE TABLE dbo.tPermissionGroupTemplate 
    (
        PermissionGroupTemplateId               int IDENTITY(1,1) NOT NULL  
        , Name                                  varchar(100) NOT NULL  
        , Description                           varchar(500) NULL  
        , OrganizationTypeRegulatoryProgramId   int  NOT NULL  
        , CreationDateTimeUtc                   datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc           datetimeoffset(0)  NULL  
        , LastModifierUserId                    int NULL  
        
        CONSTRAINT PK_tPermissionGroupTemplate PRIMARY KEY CLUSTERED 
        (
	        PermissionGroupTemplateId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tPermissionGroupTemplate_Name_OrganizationTypeRegulatoryProgramId UNIQUE 
        (
            OrganizationTypeRegulatoryProgramId ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tPermissionGroupTemplate_tOrganizationTypeRegulatoryProgram FOREIGN KEY 
		(
			OrganizationTypeRegulatoryProgramId
		) REFERENCES dbo.tOrganizationTypeRegulatoryProgram(OrganizationTypeRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tPermissionGroupTemplate ADD CONSTRAINT DF_tPermissionGroupTemplate_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroupTemplate_Name ON dbo.tPermissionGroupTemplate
	(
		Name ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroupTemplate_OrganizationTypeRegulatoryProgramId ON dbo.tPermissionGroupTemplate
	(
		OrganizationTypeRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tPermissionGroupTemplatePermission') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tPermissionGroupTemplatePermission'
    PRINT '-----------------------------------------'

    CREATE TABLE dbo.tPermissionGroupTemplatePermission 
    (
        PermissionGroupTemplatePermissionId int IDENTITY(1,1) NOT NULL  
        , PermissionGroupTemplateId         int NOT NULL  
        , PermissionId                      int NOT NULL
        
        CONSTRAINT PK_tPermissionGroupTemplatePermission PRIMARY KEY CLUSTERED 
        (
	        PermissionGroupTemplatePermissionId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tPermissionGroupTemplatePermission_PermissionGroupTemplateId_PermissionId UNIQUE 
        (
            PermissionGroupTemplateId ASC
            , PermissionId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tPermissionGroupTemplatePermission_tPermissionGroupTemplate FOREIGN KEY 
		(
			PermissionGroupTemplateId
		) REFERENCES dbo.tPermissionGroupTemplate(PermissionGroupTemplateId)
		NOT FOR REPLICATION
		, CONSTRAINT FK_tPermissionGroupTemplatePermission_tPermission FOREIGN KEY 
		(
			PermissionId
		) REFERENCES dbo.tPermission(PermissionId)
		NOT FOR REPLICATION  
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroupTemplatePermission_PermissionGroupTemplateId ON dbo.tPermissionGroupTemplatePermission 
    (
	    PermissionGroupTemplateId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroupTemplatePermission_PermissionId ON dbo.tPermissionGroupTemplatePermission 
    (
	    PermissionId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tPermissionGroup') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tPermissionGroup'
    PRINT '-----------------------'

    CREATE TABLE dbo.tPermissionGroup 
    (
        PermissionGroupId                   int IDENTITY(1,1) NOT NULL  
        , Name                              varchar(100) NOT NULL  
        , Description                       varchar(500) NULL  
        , OrganizationRegulatoryProgramId   int NOT NULL  
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
        
        CONSTRAINT PK_tPermissionGroup PRIMARY KEY CLUSTERED 
        (
	        PermissionGroupId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tPermissionGroup_Name_OrganizationRegulatoryProgramId UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tPermissionGroup_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)  
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tPermissionGroup ADD CONSTRAINT DF_tPermissionGroup_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroup_Name ON dbo.tPermissionGroup
	(
		Name ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroup_OrganizationRegulatoryProgramId ON dbo.tPermissionGroup
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tPermissionGroupPermission') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tPermissionGroupPermission'
    PRINT '---------------------------------'

    CREATE TABLE dbo.tPermissionGroupPermission 
    (
        PermissionGroupPermissionId int IDENTITY(1,1) NOT NULL  
        , PermissionGroupId         int NOT NULL  
        , PermissionId              int NOT NULL  
    
        CONSTRAINT PK_tPermissionGroupPermission PRIMARY KEY CLUSTERED 
        (
	        PermissionGroupPermissionId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tPermissionGroupPermission_PermissionGroupId_PermissionId UNIQUE 
        (
            PermissionGroupId ASC
            , PermissionId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tPermissionGroupPermission_tPermissionGroup FOREIGN KEY 
		(
			PermissionGroupId
		) REFERENCES dbo.tPermissionGroup(PermissionGroupId)
		, CONSTRAINT FK_tPermissionGroupPermission_tPermission FOREIGN KEY 
		(
			PermissionId
		) REFERENCES dbo.tPermission(PermissionId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroupPermission_PermissionGroupId ON dbo.tPermissionGroupPermission
	(
		PermissionGroupId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tPermissionGroupPermission_PermissionId ON dbo.tPermissionGroupPermission
	(
		PermissionId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tOrganizationRegulatoryProgramUser') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tOrganizationRegulatoryProgramUser'
    PRINT '-----------------------------------------'
    
    CREATE TABLE dbo.tOrganizationRegulatoryProgramUser 
    (
        OrganizationRegulatoryProgramUserId         int IDENTITY(1,1) NOT NULL
        , UserProfileId                             int NOT NULL    
        , OrganizationRegulatoryProgramId           int NOT NULL  
        , InviterOrganizationRegulatoryProgramId    int NOT NULL  
        , PermissionGroupId                         int NULL  
        , RegistrationDateTimeUtc                   datetimeoffset(0) NOT NULL
        , IsRegistrationApproved                    bit NOT NULL  
        , IsRegistrationDenied                      bit NOT NULL  
        , IsEnabled                                 bit NOT NULL  
        , IsRemoved                                 bit NOT NULL  
        , IsSignatory                               bit NOT NULL  
        , CreationDateTimeUtc                       datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc               datetimeoffset(0) NULL  
        , LastModifierUserId                        int NULL  
    
        CONSTRAINT PK_tOrganizationRegulatoryProgramUser PRIMARY KEY CLUSTERED 
        (
	        OrganizationRegulatoryProgramUserId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tOrganizationRegulatoryProgramUser_OrganizationRegulatoryProgramId_UserProfileId UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , UserProfileId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tOrganizationRegulatoryProgramUser_tUserProfile FOREIGN KEY 
		(
			UserProfileId
		) REFERENCES dbo.tUserProfile(UserProfileId)
		, CONSTRAINT FK_tOrganizationRegulatoryProgramUser_tOrganizationRegulatoryProgram_OrganizationRegulatoryProgramId FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
        , CONSTRAINT FK_tOrganizationRegulatoryProgramUser_tOrganizationRegulatoryProgram_InviterOrganizationRegulatoryProgramId FOREIGN KEY 
		(
			InviterOrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
		, CONSTRAINT FK_tOrganizationRegulatoryProgramUser_tPermissionGroup FOREIGN KEY 
		(
			PermissionGroupId
		) REFERENCES dbo.tPermissionGroup(PermissionGroupId) 
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tOrganizationRegulatoryProgramUser ADD CONSTRAINT DF_tOrganizationRegulatoryProgramUser_RegistrationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR RegistrationDateTimeUtc
    ALTER TABLE dbo.tOrganizationRegulatoryProgramUser ADD CONSTRAINT DF_tOrganizationRegulatoryProgramUser_IsRegistrationApproved DEFAULT 0 FOR IsRegistrationApproved
    ALTER TABLE dbo.tOrganizationRegulatoryProgramUser ADD CONSTRAINT DF_tOrganizationRegulatoryProgramUser_IsRegistrationDenied DEFAULT 0 FOR IsRegistrationDenied
    ALTER TABLE dbo.tOrganizationRegulatoryProgramUser ADD CONSTRAINT DF_tOrganizationRegulatoryProgramUser_IsEnabled DEFAULT 0 FOR IsEnabled
    ALTER TABLE dbo.tOrganizationRegulatoryProgramUser ADD CONSTRAINT DF_tOrganizationRegulatoryProgramUser_IsRemoved DEFAULT 0 FOR IsRemoved
    ALTER TABLE dbo.tOrganizationRegulatoryProgramUser ADD CONSTRAINT DF_tOrganizationRegulatoryProgramUser_IsSignatory DEFAULT 0 FOR IsSignatory
    ALTER TABLE dbo.tOrganizationRegulatoryProgramUser ADD CONSTRAINT DF_tOrganizationRegulatoryProgramUser_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramUser_UserProfileId ON dbo.tOrganizationRegulatoryProgramUser
	(
		UserProfileId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramUser_OrganizationRegulatoryProgramId ON dbo.tOrganizationRegulatoryProgramUser
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramUser_InviterOrganizationRegulatoryProgramId ON dbo.tOrganizationRegulatoryProgramUser
	(
		InviterOrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramUser_PermissionGroupId ON dbo.tOrganizationRegulatoryProgramUser
	(
		PermissionGroupId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tSignatoryRequestStatus') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tSignatoryRequestStatus'
    PRINT '------------------------------'
    
    CREATE TABLE dbo.tSignatoryRequestStatus 
    (
        SignatoryRequestStatusId        int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(100) NOT NULL  
        , Description                   varchar(500) NULL  
        , CreationDateTimeUtc           datetimeoffset(0)  NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0)  NULL  
        , LastModifierUserId            int  NULL  
        
        CONSTRAINT PK_tSignatoryRequestStatus PRIMARY KEY CLUSTERED 
        (
	        SignatoryRequestStatusId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tSignatoryRequestStatus_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tSignatoryRequestStatus ADD CONSTRAINT DF_tSignatoryRequestStatus_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tSignatoryRequest') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tSignatoryRequest'
    PRINT '------------------------'
    
    CREATE TABLE dbo.tSignatoryRequest 
    (
        SignatoryRequestId                      int IDENTITY(1,1) NOT NULL  
        , RequestDateTimeUtc                    datetimeoffset(0) NOT NULL  
        , GrantDenyDateTimeUtc                  datetimeoffset(0) NOT NULL  
        , RevokeDateTimeUtc                     datetimeoffset(0) NOT NULL  
        , OrganizationRegulatoryProgramUserId   int NOT NULL  
        , SignatoryRequestStatusId              int NOT NULL  
        
        CONSTRAINT PK_tSignatoryRequest PRIMARY KEY CLUSTERED 
        (
	        SignatoryRequestId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tSignatoryRequest_tOrganizationRegulatoryProgramUser FOREIGN KEY 
		(
			OrganizationRegulatoryProgramUserId
		) REFERENCES dbo.tOrganizationRegulatoryProgramUser(OrganizationRegulatoryProgramUserId)
		, CONSTRAINT FK_tSignatoryRequest_tSignatoryRequestStatus FOREIGN KEY 
		(
			SignatoryRequestStatusId
		) REFERENCES dbo.tSignatoryRequestStatus(SignatoryRequestStatusId)
    ) ON [LinkoExchange_FG1_Data]
     
    CREATE NONCLUSTERED INDEX IX_tSignatoryRequest_OrganizationRegulatoryProgramUserId ON dbo.tSignatoryRequest
	(
		OrganizationRegulatoryProgramUserId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tSignatoryRequest_SignatoryRequestStatusId ON dbo.tSignatoryRequest
	(
		SignatoryRequestStatusId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tMonitoringPoint') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tMonitoringPoint'
    PRINT '-----------------------'
    
    CREATE TABLE dbo.tMonitoringPoint 
    (
        MonitoringPointId                   int IDENTITY(1,1) NOT NULL  
        , Name                              varchar(50) NOT NULL  
        , Description                       varchar(500) NULL  
        , OrganizationRegulatoryProgramId   int NOT NULL
        , IsEnabled                         bit NOT NULL
        , IsRemoved                         bit NOT NULL
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int  NULL  
        
        CONSTRAINT PK_tMonitoringPoint PRIMARY KEY CLUSTERED 
        (
	        MonitoringPointId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tMonitoringPoint_Name_OrganizationRegulatoryProgramId UNIQUE 
        (
            Name ASC
            , OrganizationRegulatoryProgramId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tMonitoringPoint_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tMonitoringPoint ADD CONSTRAINT DF_tMonitoringPoint_IsEnabled DEFAULT 1 FOR IsEnabled
    ALTER TABLE dbo.tMonitoringPoint ADD CONSTRAINT DF_tMonitoringPoint_IsRemoved DEFAULT 0 FOR IsRemoved
    ALTER TABLE dbo.tMonitoringPoint ADD CONSTRAINT DF_tMonitoringPoint_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tMonitoringPoint_OrganizationRegulatoryProgramId ON dbo.tMonitoringPoint
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tCtsEventType') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tCtsEventType'
    PRINT '--------------------'
    
    CREATE TABLE dbo.tCtsEventType 
    (
        CtsEventTypeId                      int IDENTITY(1,1) NOT NULL
        , Name                              varchar(50) NOT NULL
        , Description                       varchar(500) NULL
        , CtsEventCategoryName              varchar(50) NOT NULL
        , OrganizationRegulatoryProgramId   int NOT NULL  
        , IsEnabled                         bit NOT NULL 
        , IsRemoved                         bit NOT NULL  
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tCtsEventType PRIMARY KEY CLUSTERED 
        (
	        CtsEventTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tCtsEventType_OrganizationRegulatoryProgramId_CtsEventCategoryName_Name UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , CtsEventCategoryName ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
		, CONSTRAINT FK_tCtsEventType_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tCtsEventType ADD CONSTRAINT DF_tCtsEventType_IsEnabled DEFAULT 1 FOR IsEnabled
    ALTER TABLE dbo.tCtsEventType ADD CONSTRAINT DF_tCtsEventType_IsRemoved DEFAULT 0 FOR IsRemoved
    ALTER TABLE dbo.tCtsEventType ADD CONSTRAINT DF_tCtsEventType_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
	CREATE NONCLUSTERED INDEX IX_tCtsEventType_OrganizationRegulatoryProgramId ON dbo.tCtsEventType
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tCollectionMethodType') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tCollectionMethodType'
    PRINT '----------------------------'
    
    CREATE TABLE dbo.tCollectionMethodType 
    (
        CollectionMethodTypeId          int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(100) NOT NULL  
        , Description                   varchar(500) NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int  NULL  
        
        CONSTRAINT PK_tCollectionMethodType PRIMARY KEY CLUSTERED 
        (
	        CollectionMethodTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tCollectionMethodType_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tCollectionMethodType ADD CONSTRAINT DF_tCollectionMethodType_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tCollectionMethod') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tCollectionMethod'
    PRINT '------------------------'
    
    CREATE TABLE dbo.tCollectionMethod 
    (
        CollectionMethodId              int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(50) NOT NULL  
        , Description                   varchar(500) NULL  
        , OrganizationId                int NOT NULL
        , CollectionMethodTypeId        int NOT NULL
        , IsEnabled                     bit NOT NULL
        , IsRemoved                     bit NOT NULL
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int  NULL  
        
        CONSTRAINT PK_tCollectionMethod PRIMARY KEY CLUSTERED 
        (
	        CollectionMethodId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tCollectionMethod_OrganizationId_Name UNIQUE 
        (
            OrganizationId ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tCollectionMethod_tOrganization FOREIGN KEY 
		(
			OrganizationId
		) REFERENCES dbo.tOrganization(OrganizationId)
        , CONSTRAINT FK_tCollectionMethod_tCollectionMethodType FOREIGN KEY 
		(
			CollectionMethodTypeId
		) REFERENCES dbo.tCollectionMethodType(CollectionMethodTypeId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tCollectionMethod ADD CONSTRAINT DF_tCollectionMethod_IsEnabled DEFAULT 1 FOR IsEnabled
    ALTER TABLE dbo.tCollectionMethod ADD CONSTRAINT DF_tCollectionMethod_IsRemoved DEFAULT 0 FOR IsRemoved
    ALTER TABLE dbo.tCollectionMethod ADD CONSTRAINT DF_tCollectionMethod_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tCollectionMethod_OrganizationId ON dbo.tCollectionMethod
	(
		OrganizationId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tCollectionMethod_CollectionMethodTypeId ON dbo.tCollectionMethod
	(
		CollectionMethodTypeId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tUnit') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tUnit'
    PRINT '------------'
    
    CREATE TABLE dbo.tUnit 
    (
        UnitId                          int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(50) NOT NULL  
        , Description                   varchar(500) NULL  
        , IsFlowUnit                    bit NOT NULL
        , OrganizationId                int NOT NULL
        , IsRemoved                     bit NOT NULL
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int  NULL  
        
        CONSTRAINT PK_tUnit PRIMARY KEY CLUSTERED 
        (
	        UnitId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tUnit_OrganizationId_Name UNIQUE 
        (
            OrganizationId ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tUnit_tOrganization FOREIGN KEY 
		(
			OrganizationId
		) REFERENCES dbo.tOrganization(OrganizationId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tUnit ADD CONSTRAINT DF_tUnit_IsFlowUnit DEFAULT 0 FOR IsFlowUnit
    ALTER TABLE dbo.tUnit ADD CONSTRAINT DF_tUnit_IsRemoved DEFAULT 0 FOR IsRemoved
    ALTER TABLE dbo.tUnit ADD CONSTRAINT DF_tUnit_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tUnit_OrganizationId ON dbo.tUnit
	(
		OrganizationId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tParameter') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tParameter'
    PRINT '-----------------'
    
    CREATE TABLE dbo.tParameter 
    (
        ParameterId                         int IDENTITY(1,1) NOT NULL
        , Name                              varchar(254) NOT NULL
        , Description                       varchar(500) NULL
        , DefaultUnitId                     int NOT NULL
        , TrcFactor                         float NULL
        , IsFlowForMassLoadingCalculation   bit NOT NULL
        , OrganizationRegulatoryProgramId   int NOT NULL  
        , IsRemoved                         bit NOT NULL  
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tParameter PRIMARY KEY CLUSTERED 
        (
	        ParameterId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tParameter_OrganizationRegulatoryProgramId_Name UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tParameter_tUnit FOREIGN KEY 
		(
			DefaultUnitId
		) REFERENCES dbo.tUnit(UnitId)
		, CONSTRAINT FK_tParameter_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tParameter ADD CONSTRAINT DF_tParameter_IsFlowForMassLoadingCalculation DEFAULT 0 FOR IsFlowForMassLoadingCalculation
    ALTER TABLE dbo.tParameter ADD CONSTRAINT DF_tParameter_IsRemoved DEFAULT 0 FOR IsRemoved
    ALTER TABLE dbo.tParameter ADD CONSTRAINT DF_tParameter_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tParameter_DefaultUnitId ON dbo.tParameter
	(
		DefaultUnitId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tParameter_OrganizationRegulatoryProgramId ON dbo.tParameter
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tParameterGroup') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tParameterGroup'
    PRINT '----------------------'
    
    CREATE TABLE dbo.tParameterGroup 
    (
        ParameterGroupId                    int IDENTITY(1,1) NOT NULL
        , Name                              varchar(100) NOT NULL
        , Description                       varchar(500) NULL
        , OrganizationRegulatoryProgramId   int NOT NULL  
        , IsActive                          bit NOT NULL  
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tParameterGroup PRIMARY KEY CLUSTERED 
        (
	        ParameterGroupId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tParameterGroup_OrganizationRegulatoryProgramId_Name UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
		, CONSTRAINT FK_tParameterGroup_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tParameterGroup ADD CONSTRAINT DF_tParameterGroup_IsActive DEFAULT 0 FOR IsActive
    ALTER TABLE dbo.tParameterGroup ADD CONSTRAINT DF_tParameterGroup_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
	CREATE NONCLUSTERED INDEX IX_tParameterGroup_OrganizationRegulatoryProgramId ON dbo.tParameterGroup
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tParameterGroupParameter') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tParameterGroupParameter'
    PRINT '-------------------------------'

    CREATE TABLE dbo.tParameterGroupParameter 
    (
        ParameterGroupParameterId   int IDENTITY(1,1) NOT NULL  
        , ParameterGroupId          int NOT NULL  
        , ParameterId               int NOT NULL  
    
        CONSTRAINT PK_tParameterGroupParameter PRIMARY KEY CLUSTERED 
        (
	        ParameterGroupParameterId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tParameterGroupParameter_ParameterGroupId_ParameterId UNIQUE 
        (
            ParameterGroupId ASC
            , ParameterId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tParameterGroupParameter_tParameterGroup FOREIGN KEY 
		(
			ParameterGroupId
		) REFERENCES dbo.tParameterGroup(ParameterGroupId)
		, CONSTRAINT FK_tParameterGroupParameter_tParameter FOREIGN KEY 
		(
			ParameterId
		) REFERENCES dbo.tParameter(ParameterId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tParameterGroupParameter_ParameterGroupId ON dbo.tParameterGroupParameter
	(
		ParameterGroupId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tParameterGroupParameter_ParameterId ON dbo.tParameterGroupParameter
	(
		ParameterId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tLimitType') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tLimitType'
    PRINT '-----------------'
    
    CREATE TABLE dbo.tLimitType 
    (
        LimitTypeId                     int IDENTITY(1,1) NOT NULL
        , Name                          varchar(25) NOT NULL
        , Description                   varchar(30) NULL
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tLimitType PRIMARY KEY CLUSTERED 
        (
	        LimitTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tLimitType_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tLimitType ADD CONSTRAINT DF_tLimitType_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tLimitBasis') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tLimitBasis'
    PRINT '------------------'
    
    CREATE TABLE dbo.tLimitBasis 
    (
        LimitBasisId                    int IDENTITY(1,1) NOT NULL
        , Name                          varchar(25) NOT NULL
        , Description                   varchar(30) NULL
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tLimitBasis PRIMARY KEY CLUSTERED 
        (
	        LimitBasisId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tLimitBasis_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tLimitBasis ADD CONSTRAINT DF_tLimitBasis_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tMonitoringPointParameter') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tMonitoringPointParameter'
    PRINT '--------------------------------'
    
    CREATE TABLE dbo.tMonitoringPointParameter 
    (
        MonitoringPointParameterId          int IDENTITY(1,1) NOT NULL
        , MonitoringPointId                 int NOT NULL
        , ParameterId                       int NOT NULL
        , DefaultUnitId                     int NULL
        , EffectiveDateTime                 datetime NOT NULL
        , RetirementDateTime                datetime NOT NULL   
    
        CONSTRAINT PK_tMonitoringPointParameter PRIMARY KEY CLUSTERED 
        (
	        MonitoringPointParameterId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tMonitoringPointParameter_MonitoringPointId_ParameterId_EffectiveDateTime UNIQUE 
        (
            MonitoringPointId ASC
            , ParameterId ASC
            , EffectiveDateTime ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tMonitoringPointParameter_tMonitoringPoint FOREIGN KEY 
		(
			MonitoringPointId
		) REFERENCES dbo.tMonitoringPoint(MonitoringPointId)
        , CONSTRAINT FK_tMonitoringPointParameter_tParameter FOREIGN KEY 
		(
			ParameterId
		) REFERENCES dbo.tParameter(ParameterId)
        , CONSTRAINT FK_tMonitoringPointParameter_tUnit FOREIGN KEY 
		(
			DefaultUnitId
		) REFERENCES dbo.tUnit(UnitId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tMonitoringPointParameter_MonitoringPointId ON dbo.tMonitoringPointParameter
	(
		MonitoringPointId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tMonitoringPointParameter_ParameterId ON dbo.tMonitoringPointParameter
	(
		ParameterId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tMonitoringPointParameter_DefaultUnitId ON dbo.tMonitoringPointParameter
	(
		DefaultUnitId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tMonitoringPointParameterLimit') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tMonitoringPointParameterLimit'
    PRINT '-------------------------------------'
    
    CREATE TABLE dbo.tMonitoringPointParameterLimit 
    (
        MonitoringPointParameterLimitId     int IDENTITY(1,1) NOT NULL
        , MonitoringPointParameterId        int NOT NULL
        , Name                              varchar(200) NOT NULL
        , Description                       varchar(500) NULL
        , MinimumValue                      float NULL
        , MaximumValue                      float NOT NULL
        , BaseUnitId                        int NOT NULL
        , CollectionMethodTypeId            int NULL
        , LimitTypeId                       int NOT NULL
        , LimitBasisId                      int NOT NULL
        , IsAlertOnly                       bit NOT NULL
    
        CONSTRAINT PK_tMonitoringPointParameterLimit PRIMARY KEY CLUSTERED 
        (
	        MonitoringPointParameterLimitId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tMonitoringPointParameterLimit_MonitoringPointParameterId_Name UNIQUE 
        (
            MonitoringPointParameterId ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
		, CONSTRAINT FK_tMonitoringPointParameterLimit_tMonitoringPointParameter FOREIGN KEY 
		(
			MonitoringPointParameterId
		) REFERENCES dbo.tMonitoringPointParameter(MonitoringPointParameterId)
        , CONSTRAINT FK_tMonitoringPointParameterLimit_tUnit FOREIGN KEY 
		(
			BaseUnitId
		) REFERENCES dbo.tUnit(UnitId)
        , CONSTRAINT FK_tMonitoringPointParameterLimit_tCollectionMethodType FOREIGN KEY 
		(
			CollectionMethodTypeId
		) REFERENCES dbo.tCollectionMethodType(CollectionMethodTypeId)
        , CONSTRAINT FK_tMonitoringPointParameterLimit_tLimitType FOREIGN KEY 
		(
			LimitTypeId
		) REFERENCES dbo.tLimitType(LimitTypeId)
        , CONSTRAINT FK_tMonitoringPointParameterLimit_tLimitBasis FOREIGN KEY 
		(
			LimitBasisId
		) REFERENCES dbo.tLimitBasis(LimitBasisId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tMonitoringPointParameterLimit ADD CONSTRAINT DF_tMonitoringPointParameterLimit_IsAlertOnly DEFAULT 0 FOR IsAlertOnly

	CREATE NONCLUSTERED INDEX IX_tMonitoringPointParameterLimit_MonitoringPointParameterId ON dbo.tMonitoringPointParameterLimit
	(
		MonitoringPointParameterId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tMonitoringPointParameterLimit_BaseUnitId ON dbo.tMonitoringPointParameterLimit
	(
		BaseUnitId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tMonitoringPointParameterLimit_CollectionMethodTypeId ON dbo.tMonitoringPointParameterLimit
	(
		CollectionMethodTypeId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tMonitoringPointParameterLimit_LimitTypeId ON dbo.tMonitoringPointParameterLimit
	(
		LimitTypeId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tMonitoringPointParameterLimit_LimitBasisId ON dbo.tMonitoringPointParameterLimit
	(
		LimitBasisId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tSampleRequirement') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tSampleRequirement'
    PRINT '-------------------------'
    
    CREATE TABLE dbo.tSampleRequirement 
    (
        SampleRequirementId					int IDENTITY(1,1) NOT NULL
        , MonitoringPointParameterId		int NOT NULL
        , PeriodStartDateTime			    datetime NOT NULL  
        , PeriodEndDateTime				    datetime NOT NULL
        , SamplesRequired					int NOT NULL
        , ByOrganizationRegulatoryProgramId	int NOT NULL
    
        CONSTRAINT PK_tSampleRequirement PRIMARY KEY CLUSTERED 
        (
	        SampleRequirementId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tSampleRequirement_tMonitoringPointParameter FOREIGN KEY 
		(
			MonitoringPointParameterId
		) REFERENCES dbo.tMonitoringPointParameter(MonitoringPointParameterId)
        , CONSTRAINT FK_tSampleRequirement_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			ByOrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tSampleRequirement_MonitoringPointParameterId ON dbo.tSampleRequirement
	(
		MonitoringPointParameterId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tSampleRequirement_ByOrganizationRegulatoryProgramId ON dbo.tSampleRequirement
	(
		ByOrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tSampleFrequency') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tSampleFrequency'
    PRINT '-----------------------'
    
    CREATE TABLE dbo.tSampleFrequency 
    (
        SampleFrequencyId               int IDENTITY(1,1) NOT NULL
        , MonitoringPointParameterId    int NOT NULL
        , CollectionMethodId            int NOT NULL
        , IUSampleFrequency             varchar(50) NULL
        , AuthoritySampleFrequency      varchar(50) NULL
    
        CONSTRAINT PK_tSampleFrequency PRIMARY KEY CLUSTERED 
        (
	        SampleFrequencyId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tSampleFrequency_MonitoringPointParameterId_CollectionMethodId UNIQUE 
        (
            MonitoringPointParameterId ASC
            , CollectionMethodId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tSampleFrequency_tMonitoringPointParameter FOREIGN KEY 
		(
			MonitoringPointParameterId
		) REFERENCES dbo.tMonitoringPointParameter(MonitoringPointParameterId)
        , CONSTRAINT FK_tSampleFrequency_tCollectionMethod FOREIGN KEY 
		(
			CollectionMethodId
		) REFERENCES dbo.tCollectionMethod(CollectionMethodId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tSampleFrequency_MonitoringPointParameterId ON dbo.tSampleFrequency
	(
		MonitoringPointParameterId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tSampleFrequency_CollectionMethodId ON dbo.tSampleFrequency
	(
		CollectionMethodId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tSample') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tSample'
    PRINT '--------------'
    
    CREATE TABLE dbo.tSample 
    (
        SampleId								int IDENTITY(1,1) NOT NULL
        , Name									varchar(50) NOT NULL
        , MonitoringPointId						int NOT NULL
        , MonitoringPointName					varchar(50) NOT NULL
        , CtsEventTypeId						int NOT NULL
        , CtsEventTypeName						varchar(50) NOT NULL
        , CtsEventCategoryName					varchar(50) NOT NULL
        , CollectionMethodId					int NOT NULL
        , CollectionMethodName					varchar(50) NOT NULL
        , LabSampleIdentifier					varchar(50) NULL
        , StartDateTimeUtc						datetimeoffset(0) NOT NULL
        , EndDateTimeUtc						datetimeoffset(0) NOT NULL
        , IsSystemGenerated						bit NOT NULL
        , IsReadyToReport						bit NOT NULL
        , FlowUnitValidValues                   varchar(50) NULL
        , ResultQualifierValidValues            varchar(50) NULL
        , MassLoadingConversionFactorPounds     float NULL
        , MassLoadingCalculationDecimalPlaces   int NULL
        , IsMassLoadingResultToUseLessThanSign  bit NOT NULL
        , ByOrganizationRegulatoryProgramId     int NOT NULL
        , ForOrganizationRegulatoryProgramId	int NOT NULL
        , CreationDateTimeUtc					datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc			datetimeoffset(0) NULL  
        , LastModifierUserId					int NULL  
    
        CONSTRAINT PK_tSample PRIMARY KEY CLUSTERED 
        (
	        SampleId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tSample_tOrganizationRegulatoryProgram_By FOREIGN KEY 
		(
			ByOrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
		, CONSTRAINT FK_tSample_tOrganizationRegulatoryProgram_For FOREIGN KEY 
		(
			ForOrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tSample ADD CONSTRAINT DF_tSample_IsSystemGenerated DEFAULT 0 FOR IsSystemGenerated
    ALTER TABLE dbo.tSample ADD CONSTRAINT DF_tSample_IsReadyToReport DEFAULT 0 FOR IsReadyToReport
    ALTER TABLE dbo.tSample ADD CONSTRAINT DF_tSample_IsMassLoadingResultToUseLessThanSign DEFAULT 0 FOR IsMassLoadingResultToUseLessThanSign
    ALTER TABLE dbo.tSample ADD CONSTRAINT DF_tSample_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tSample_OrganizationRegulatoryProgramId_By ON dbo.tSample
	(
		ByOrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tSample_OrganizationRegulatoryProgramId_For ON dbo.tSample
	(
		ForOrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tSampleResult') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tSampleResult'
    PRINT '--------------------'
    
    CREATE TABLE dbo.tSampleResult 
    (
        SampleResultId                      int IDENTITY(1,1) NOT NULL
        , SampleId                          int NOT NULL
        , ParameterId                       int NOT NULL
        , ParameterName                     varchar(254) NOT NULL
        , Qualifier                         varchar(2) NULL
        , EnteredValue                      varchar(50) NULL
        , Value                             float NULL
        , UnitId                            int NOT NULL
        , UnitName                          varchar(50) NOT NULL
        , EnteredMethodDetectionLimit       varchar(50) NULL
        , MethodDetectionLimit              float NULL
        , AnalysisMethod                    varchar(50) NULL
        , AnalysisDateTimeUtc               datetimeoffset(0) NULL
        , IsApprovedEPAMethod               bit NOT NULL
        , IsMassLoadingCalculationRequired  bit NOT NULL
        , IsCalculated                      bit NOT NULL
        , LimitTypeId                       int NULL
        , LimitBasisId                      int NOT NULL
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tSampleResult PRIMARY KEY CLUSTERED 
        (
	        SampleResultId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tSampleResult_SampleId_ParameterId_LimitBasisId UNIQUE 
        (
            SampleId ASC
            , ParameterId ASC
            , LimitBasisId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tSampleResult_tSample FOREIGN KEY 
		(
			SampleId
		) REFERENCES dbo.tSample(SampleId)
        , CONSTRAINT FK_tSampleResult_tLimitType FOREIGN KEY 
		(
			LimitTypeId
		) REFERENCES dbo.tLimitType(LimitTypeId)
        , CONSTRAINT FK_tSampleResult_tLimitBasis FOREIGN KEY 
		(
			LimitBasisId
		) REFERENCES dbo.tLimitBasis(LimitBasisId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tSampleResult ADD CONSTRAINT DF_tSampleResult_IsApprovedEPAMethod DEFAULT 0 FOR IsApprovedEPAMethod
    ALTER TABLE dbo.tSampleResult ADD CONSTRAINT DF_tSampleResult_IsMassLoadingCalculationRequired DEFAULT 0 FOR IsMassLoadingCalculationRequired
    ALTER TABLE dbo.tSampleResult ADD CONSTRAINT DF_tSampleResult_IsCalculated DEFAULT 0 FOR IsCalculated
    ALTER TABLE dbo.tSampleResult ADD CONSTRAINT DF_tSampleResult_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tSampleResult_SampleId ON dbo.tSampleResult
	(
		SampleId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tSampleResult_LimitTypeId ON dbo.tSampleResult
	(
		LimitTypeId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tSampleResult_LimitBasisId ON dbo.tSampleResult
	(
		LimitBasisId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tFileType') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tFileType'
    PRINT '----------------'
    
    CREATE TABLE dbo.tFileType 
    (
        FileTypeId                          int IDENTITY(1,1) NOT NULL
        , Extension                         varchar(5) NOT NULL
        , Description                       varchar(500) NULL
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tFileType PRIMARY KEY CLUSTERED 
        (
	        FileTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tFileType_Extension UNIQUE 
        (
            Extension ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tFileType ADD CONSTRAINT DF_tFileType_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tFileStore') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tFileStore'
    PRINT '-----------------'
    
    CREATE TABLE dbo.tFileStore
    (
        FileStoreId                         int IDENTITY(1,1) NOT NULL
        , Name                              varchar(256) NOT NULL
        , Description                       varchar(500) NULL
        , OriginalName                      varchar(256) NOT NULL
        , SizeByte                          float NOT NULL
        , MediaType                         varchar(100) NULL
        , FileTypeId                        int NOT NULL
        , ReportElementTypeId               int NOT NULL
        , ReportElementTypeName             varchar(100) NOT NULL
        , OrganizationRegulatoryProgramId   int NOT NULL
        , UploadDateTimeUtc                 datetimeoffset(0) NOT NULL  
        , UploaderUserId                    int NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tFileStore PRIMARY KEY CLUSTERED 
        (
	        FileStoreId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tReportElementCategory_OrganizationRegulatoryProgramId_Name UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tFileStore_tFileType FOREIGN KEY 
		(
			FileTypeId
		) REFERENCES dbo.tFileType(FileTypeId)
        , CONSTRAINT FK_tFileStore_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
        , CONSTRAINT FK_tFileStore_tUserProfile FOREIGN KEY 
		(
			UploaderUserId
		) REFERENCES dbo.tUserProfile(UserProfileId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tFileStore ADD CONSTRAINT DF_tFileStore_UploadDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR UploadDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tFileStore_FileTypeId ON dbo.tFileStore
	(
		FileTypeId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tFileStore_OrganizationRegulatoryProgramId ON dbo.tFileStore
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tFileStore_UploaderUserId ON dbo.tFileStore
	(
		UploaderUserId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tFileStoreData') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tFileStoreData'
    PRINT '---------------------'
    
    CREATE TABLE dbo.tFileStoreData 
    (
        FileStoreId int NOT NULL
        , Data      varbinary(max) NOT NULL    
    
        CONSTRAINT PK_tFileStoreData PRIMARY KEY CLUSTERED 
        (
	        FileStoreId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG3_LOB]
        , CONSTRAINT FK_tFileStoreData_tFileStore FOREIGN KEY 
		(
			FileStoreId
		) REFERENCES dbo.tFileStore(FileStoreId)
    ) ON [LinkoExchange_FG3_LOB]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportElementCategory') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportElementCategory'
    PRINT '-----------------------------'
    
    CREATE TABLE dbo.tReportElementCategory 
    (
        ReportElementCategoryId             int IDENTITY(1,1) NOT NULL
        , Name                              varchar(100) NOT NULL
        , Description                       varchar(500) NULL
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tReportElementCategory PRIMARY KEY CLUSTERED 
        (
	        ReportElementCategoryId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tReportElementCategory_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tReportElementCategory ADD CONSTRAINT DF_tReportElementCategory_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportElementType') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportElementType'
    PRINT '-------------------------'
    
    CREATE TABLE dbo.tReportElementType 
    (
        ReportElementTypeId                 int IDENTITY(1,1) NOT NULL
        , Name                              varchar(100) NOT NULL
        , Description                       varchar(500) NULL
        , Content                           varchar(2000) NULL
        , IsContentProvided                 bit NOT NULL
        , CtsEventTypeId                    int NULL
        , ReportElementCategoryId           int NOT NULL
        , OrganizationRegulatoryProgramId   int NOT NULL    
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tReportElementType PRIMARY KEY CLUSTERED 
        (
	        ReportElementTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tReportElementType_Name_OrganizationRegulatoryProgramId UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tReportElementType_tCtsEventType FOREIGN KEY 
		(
			CtsEventTypeId
		) REFERENCES dbo.tCtsEventType(CtsEventTypeId)
        , CONSTRAINT FK_tReportElementType_tReportElementCategory FOREIGN KEY 
		(
			ReportElementCategoryId
		) REFERENCES dbo.tReportElementCategory(ReportElementCategoryId)
		, CONSTRAINT FK_tReportElementType_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tReportElementType ADD CONSTRAINT DF_tReportElementType_IsContentProvided DEFAULT 0 FOR IsContentProvided
    ALTER TABLE dbo.tReportElementType ADD CONSTRAINT DF_tReportElementType_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tReportElementType_CtsEventTypeId ON dbo.tReportElementType
	(
		CtsEventTypeId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tReportElementType_ReportElementCategoryId ON dbo.tReportElementType
	(
		ReportElementCategoryId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

	CREATE NONCLUSTERED INDEX IX_tReportElementType_OrganizationRegulatoryProgramId ON dbo.tReportElementType
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportPackageTemplate') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportPackageTemplate'
    PRINT '-----------------------------'
    
    CREATE TABLE dbo.tReportPackageTemplate 
    (
        ReportPackageTemplateId             int IDENTITY(1,1) NOT NULL
        , Name                              varchar(100) NOT NULL
        , Description                       varchar(500) NULL
        , EffectiveDateTimeUtc              datetimeoffset(0) NOT NULL
        , RetirementDateTimeUtc             datetimeoffset(0) NULL
        , IsSubmissionBySignatoryRequired   bit NOT NULL
        , CtsEventTypeId                    int NULL
        , OrganizationRegulatoryProgramId   int NOT NULL
        , IsActive                          bit NOT NULL    
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tReportPackageTemplate PRIMARY KEY CLUSTERED 
        (
	        ReportPackageTemplateId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tReportPackageTemplate_Name_EffectiveDateTimeUtc_OrganizationRegulatoryProgramId UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , Name ASC
            , EffectiveDateTimeUtc ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tReportPackageTemplate_tCtsEventType FOREIGN KEY 
		(
			CtsEventTypeId
		) REFERENCES dbo.tCtsEventType(CtsEventTypeId)
		, CONSTRAINT FK_tReportPackageTemplate_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tReportPackageTemplate ADD CONSTRAINT DF_tReportPackageTemplate_EffectiveDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR EffectiveDateTimeUtc
    ALTER TABLE dbo.tReportPackageTemplate ADD CONSTRAINT DF_tReportPackageTemplate_IsSubmissionBySignatoryRequired DEFAULT 0 FOR IsSubmissionBySignatoryRequired
    ALTER TABLE dbo.tReportPackageTemplate ADD CONSTRAINT DF_tReportPackageTemplate_IsActive DEFAULT 0 FOR IsActive
    ALTER TABLE dbo.tReportPackageTemplate ADD CONSTRAINT DF_tReportPackageTemplate_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tReportPackageTemplate_CtsEventTypeId ON dbo.tReportPackageTemplate
	(
		CtsEventTypeId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

	CREATE NONCLUSTERED INDEX IX_tReportPackageTemplate_OrganizationRegulatoryProgramId ON dbo.tReportPackageTemplate
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportPackageTemplateAssignment') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportPackageTemplateAssignment'
    PRINT '---------------------------------------'

    CREATE TABLE dbo.tReportPackageTemplateAssignment
    (
        ReportPackageTemplateAssignmentId   int IDENTITY(1,1) NOT NULL  
        , ReportPackageTemplateId           int NOT NULL  
        , OrganizationRegulatoryProgramId   int NOT NULL  
    
        CONSTRAINT PK_tReportPackageTemplateAssignment PRIMARY KEY CLUSTERED 
        (
	        ReportPackageTemplateAssignmentId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tReportPackageTemplateAssignment_ReportPackageTemplateId_OrganizationRegulatoryProgramId UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , ReportPackageTemplateId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tReportPackageTemplateAssignment_tReportPackageTemplate FOREIGN KEY 
		(
			ReportPackageTemplateId
		) REFERENCES dbo.tReportPackageTemplate(ReportPackageTemplateId)
		, CONSTRAINT FK_tReportPackageTemplateAssignment_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tReportPackageTemplateAssignment_ReportPackageTemplateId ON dbo.tReportPackageTemplateAssignment
	(
		ReportPackageTemplateId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tReportPackageTemplateAssignment_OrganizationRegulatoryProgramId ON dbo.tReportPackageTemplateAssignment
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportPackageTemplateElementCategory') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportPackageTemplateElementCategory'
    PRINT '--------------------------------------------'

    CREATE TABLE dbo.tReportPackageTemplateElementCategory
    (
        ReportPackageTemplateElementCategoryId  int IDENTITY(1,1) NOT NULL  
        , ReportPackageTemplateId               int NOT NULL  
        , ReportElementCategoryId               int NOT NULL
        , SortOrder                             int NOT NULL  
    
        CONSTRAINT PK_tReportPackageTemplateElementCategory PRIMARY KEY CLUSTERED 
        (
	        ReportPackageTemplateElementCategoryId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tReportPackageTemplateElementCategory_ReportPackageTemplateId_ReportElementCategoryId UNIQUE 
        (
            ReportPackageTemplateId ASC
            , ReportElementCategoryId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tReportPackageTemplateElementCategory_tReportPackageTemplate FOREIGN KEY 
		(
			ReportPackageTemplateId
		) REFERENCES dbo.tReportPackageTemplate(ReportPackageTemplateId)
		, CONSTRAINT FK_tReportPackageTemplateElementCategory_tReportElementCategory FOREIGN KEY 
		(
			ReportElementCategoryId
		) REFERENCES dbo.tReportElementCategory(ReportElementCategoryId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tReportPackageTemplateElementCategory ADD CONSTRAINT DF_tReportPackageTemplateElementCategory_SortOrder DEFAULT 0 FOR SortOrder

    CREATE NONCLUSTERED INDEX IX_tReportPackageTemplateElementCategory_ReportPackageTemplateId ON dbo.tReportPackageTemplateElementCategory
	(
		ReportPackageTemplateId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tReportPackageTemplateElementCategory_ReportElementCategoryId ON dbo.tReportPackageTemplateElementCategory
	(
		ReportElementCategoryId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportPackageTemplateElementType') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportPackageTemplateElementType'
    PRINT '----------------------------------------'

    CREATE TABLE dbo.tReportPackageTemplateElementType
    (
        ReportPackageTemplateElementTypeId          int IDENTITY(1,1) NOT NULL  
        , ReportPackageTemplateElementCategoryId    int NOT NULL  
        , ReportElementTypeId                       int NOT NULL
        , IsRequired                                bit NOT NULL
        , SortOrder                                 int NOT NULL  
    
        CONSTRAINT PK_tReportPackageTemplateElementType PRIMARY KEY CLUSTERED 
        (
	        ReportPackageTemplateElementTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tReportPackageTemplateElementType_ReportPackageTemplateElementCategoryId_ReportElementTypeId UNIQUE 
        (
            ReportPackageTemplateElementCategoryId ASC
            , ReportElementTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tReportPackageTemplateElementType_tReportPackageTemplateElementCategory FOREIGN KEY 
		(
			ReportPackageTemplateElementCategoryId
		) REFERENCES dbo.tReportPackageTemplateElementCategory(ReportPackageTemplateElementCategoryId)
		, CONSTRAINT FK_tReportPackageTemplateElementType_tReportElementType FOREIGN KEY 
		(
			ReportElementTypeId
		) REFERENCES dbo.tReportElementType(ReportElementTypeId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tReportPackageTemplateElementType ADD CONSTRAINT DF_tReportPackageTemplateElementType_IsRequired DEFAULT 0 FOR IsRequired
    ALTER TABLE dbo.tReportPackageTemplateElementType ADD CONSTRAINT DF_tReportPackageTemplateElementType_SortOrder DEFAULT 0 FOR SortOrder

    CREATE NONCLUSTERED INDEX IX_tReportPackageTemplateElementType_ReportPackageTemplateElementCategoryId ON dbo.tReportPackageTemplateElementType
	(
		ReportPackageTemplateElementCategoryId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
	
	CREATE NONCLUSTERED INDEX IX_tReportPackageTemplateElementType_ReportElementTypeId ON dbo.tReportPackageTemplateElementType
	(
		ReportElementTypeId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportStatus') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportStatus'
    PRINT '--------------------'
    
    CREATE TABLE dbo.tReportStatus 
    (
        ReportStatusId                  int IDENTITY(1,1) NOT NULL
        , Name                          varchar(100) NOT NULL
        , Description                   varchar(500) NULL
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tReportStatus PRIMARY KEY CLUSTERED 
        (
	        ReportStatusId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tReportStatus_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tReportStatus ADD CONSTRAINT DF_tReportStatus_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tRepudiationReason') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tRepudiationReason'
    PRINT '-------------------------'
    
    CREATE TABLE dbo.tRepudiationReason 
    (
        RepudiationReasonId                 int IDENTITY(1,1) NOT NULL
        , Name                              varchar(100) NOT NULL
        , Description                       varchar(500) NULL
        , OrganizationRegulatoryProgramId   int NOT NULL
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tRepudiationReason PRIMARY KEY CLUSTERED 
        (
	        RepudiationReasonId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tRepudiationReason_OrganizationRegulatoryProgramId_Name UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , Name ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tRepudiationReason_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tRepudiationReason ADD CONSTRAINT DF_tRepudiationReason_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tRepudiationReason_OrganizationRegulatoryProgramId ON dbo.tRepudiationReason
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportPackage') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportPackage'
    PRINT '---------------------'
    
    CREATE TABLE dbo.tReportPackage 
    (
        ReportPackageId                         int IDENTITY(1,1) NOT NULL
        , Name                                  varchar(100) NOT NULL
        , Description                           varchar(500) NULL
        , PeriodStartDateTimeUtc                datetimeoffset(0) NOT NULL
        , PeriodEndDateTimeUtc                  datetimeoffset(0) NOT NULL
        , CtsEventTypeId                        int NULL
        , CtsEventTypeName                      varchar(50) NULL
        , CtsEventCategoryName                  varchar(50) NULL
        , Comments                              varchar(500) NULL
        , IsSubmissionBySignatoryRequired       bit NOT NULL
        , ReportPackageTemplateId               int NOT NULL
        , ReportStatusId                        int NOT NULL
        
        , OrganizationRegulatoryProgramId       int NOT NULL
        , OrganizationReferenceNumber           varchar(50) NULL
        , OrganizationName                      varchar(254) NOT NULL
        , OrganizationAddressLine1              varchar(100) NULL
        , OrganizationAddressLine2              varchar(100) NULL
        , OrganizationCityName                  varchar(100) NULL
        , OrganizationJurisdictionName          varchar(100) NULL
        , OrganizationZipCode                   varchar(50) NULL
        , RecipientOrganizationName             varchar(254) NOT NULL
        , RecipientOrganizationAddressLine1     varchar(100) NULL
        , RecipientOrganizationAddressLine2     varchar(100) NULL
        , RecipientOrganizationCityName         varchar(100) NULL
        , RecipientOrganizationJurisdictionName varchar(100) NULL
        , RecipientOrganizationZipCode          varchar(50) NULL
        
        , SubmissionDateTimeUtc                 datetimeoffset(0) NULL
        , SubmitterUserId                       int NULL
        , SubmitterFirstName                    varchar(50) NULL
        , SubmitterLastName                     varchar(50) NULL
        , SubmitterTitleRole                    varchar(250) NULL
        , SubmitterIPAddress                    varchar(50) NULL
        , SubmitterUserName                     varchar(256) NULL

        , SubmissionReviewDateTimeUtc           datetimeoffset(0) NULL
        , SubmissionReviewerUserId              int NULL
        , SubmissionReviewerFirstName           varchar(50) NULL
        , SubmissionReviewerLastName            varchar(50) NULL
        , SubmissionReviewerTitleRole           varchar(250) NULL
        , SubmissionReviewComments              varchar(500) NULL

        , RepudiationDateTimeUtc                datetimeoffset(0) NULL
        , RepudiatorUserId                      int NULL
        , RepudiatorFirstName                   varchar(50) NULL
        , RepudiatorLastName                    varchar(50) NULL
        , RepudiatorTitleRole                   varchar(250) NULL
        , RepudiationReasonId                   int NULL
        , RepudiationReasonName                 varchar(100) NULL
        , RepudiationComments                   varchar(500) NULL

        , RepudiationReviewDateTimeUtc          datetimeoffset(0) NULL
        , RepudiationReviewerUserId             int NULL
        , RepudiationReviewerFirstName          varchar(50) NULL
        , RepudiationReviewerLastName           varchar(50) NULL
        , RepudiationReviewerTitleRole          varchar(250) NULL
        , RepudiationReviewComments             varchar(500) NULL

        , LastSentDateTimeUtc                   datetimeoffset(0) NULL
        , LastSenderUserId                      int NULL
        , LastSenderFirstName                   varchar(50) NULL
        , LastSenderLastName                    varchar(50) NULL

        , CreationDateTimeUtc                   datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc           datetimeoffset(0) NULL  
        , LastModifierUserId                    int NULL  
    
        CONSTRAINT PK_tReportPackage PRIMARY KEY CLUSTERED 
        (
	        ReportPackageId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT FK_tReportPackage_tReportStatus FOREIGN KEY 
		(
			ReportStatusId
		) REFERENCES dbo.tReportStatus(ReportStatusId)
		, CONSTRAINT FK_tReportPackage_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG2_Data]
    
    ALTER TABLE dbo.tReportPackage ADD CONSTRAINT DF_tReportPackage_IsSubmissionBySignatoryRequired DEFAULT 1 FOR IsSubmissionBySignatoryRequired
    ALTER TABLE dbo.tReportPackage ADD CONSTRAINT DF_tReportPackage_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tReportPackage_ReportStatusId ON dbo.tReportPackage
	(
		ReportStatusId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]

	CREATE NONCLUSTERED INDEX IX_tReportPackage_OrganizationRegulatoryProgramId ON dbo.tReportPackage
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportPackageElementCategory') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportPackageElementCategory'
    PRINT '------------------------------------'

    CREATE TABLE dbo.tReportPackageElementCategory
    (
        ReportPackageElementCategoryId  int IDENTITY(1,1) NOT NULL  
        , ReportPackageId               int NOT NULL  
        , ReportElementCategoryId       int NOT NULL
        , SortOrder                     int NOT NULL  
    
        CONSTRAINT PK_tReportPackageElementCategory PRIMARY KEY CLUSTERED 
        (
	        ReportPackageElementCategoryId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT AK_tReportPackageElementCategory_ReportPackageId_ReportElementCategoryId UNIQUE 
        (
            ReportPackageId ASC
            , ReportElementCategoryId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT FK_tReportPackageElementCategory_tReportPackage FOREIGN KEY 
		(
			ReportPackageId
		) REFERENCES dbo.tReportPackage(ReportPackageId)
		, CONSTRAINT FK_tReportPackageElementCategory_tReportElementCategory FOREIGN KEY 
		(
			ReportElementCategoryId
		) REFERENCES dbo.tReportElementCategory(ReportElementCategoryId)
    ) ON [LinkoExchange_FG2_Data]
    
    ALTER TABLE dbo.tReportPackageElementCategory ADD CONSTRAINT DF_tReportPackageElementCategory_SortOrder DEFAULT 0 FOR SortOrder

    CREATE NONCLUSTERED INDEX IX_tReportPackageElementCategory_ReportPackageId ON dbo.tReportPackageElementCategory
	(
		ReportPackageId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
	
	CREATE NONCLUSTERED INDEX IX_tReportPackageElementCategory_ReportElementCategoryId ON dbo.tReportPackageElementCategory
	(
		ReportElementCategoryId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportPackageElementType') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportPackageElementType'
    PRINT '--------------------------------'

    CREATE TABLE dbo.tReportPackageElementType
    (
        ReportPackageElementTypeId              int IDENTITY(1,1) NOT NULL  
        , ReportPackageElementCategoryId        int NOT NULL  
        , ReportElementTypeId                   int NOT NULL
        , ReportElementTypeName                 varchar(100) NOT NULL
        , ReportElementTypeContent              varchar(2000) NULL
        , ReportElementTypeIsContentProvided    bit NOT NULL
        , CtsEventTypeId                        int NULL
        , CtsEventTypeName                      varchar(50) NULL
        , CtsEventCategoryName                  varchar(50) NULL
        , IsRequired                            bit NOT NULL
		, IsIncluded                            bit NOT NULL
        , SortOrder                             int NOT NULL  
    
        CONSTRAINT PK_tReportPackageElementType PRIMARY KEY CLUSTERED 
        (
	        ReportPackageElementTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT AK_tReportPackageElementType_ReportPackageElementCategoryId_ReportElementTypeName UNIQUE 
        (
            ReportPackageElementCategoryId ASC
            , ReportElementTypeName ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT FK_tReportPackageElementType_tReportPackageElementCategory FOREIGN KEY 
		(
			ReportPackageElementCategoryId
		) REFERENCES dbo.tReportPackageElementCategory(ReportPackageElementCategoryId)
    ) ON [LinkoExchange_FG2_Data]
    
    ALTER TABLE dbo.tReportPackageElementType ADD CONSTRAINT DF_tReportPackageElementType_IsRequired DEFAULT 0 FOR IsRequired
	ALTER TABLE dbo.tReportPackageElementType ADD CONSTRAINT DF_tReportPackageElementType_IsIncluded DEFAULT 0 FOR IsIncluded
    ALTER TABLE dbo.tReportPackageElementType ADD CONSTRAINT DF_tReportPackageElementType_SortOrder DEFAULT 0 FOR SortOrder

    CREATE NONCLUSTERED INDEX IX_tReportPackageElementType_ReportPackageElementCategoryId ON dbo.tReportPackageElementType
	(
		ReportPackageElementCategoryId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportSample') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportSample'
    PRINT '--------------------'

    CREATE TABLE dbo.tReportSample
    (
        ReportSampleId                  int IDENTITY(1,1) NOT NULL  
        , ReportPackageElementTypeId    int NOT NULL  
        , SampleId                      int NOT NULL  
    
        CONSTRAINT PK_tReportSample PRIMARY KEY CLUSTERED 
        (
	        ReportSampleId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT AK_tReportSample_ReportPackageElementTypeId_SampleId UNIQUE 
        (
            ReportPackageElementTypeId ASC
            , SampleId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT FK_tReportSample_tReportPackageElementType FOREIGN KEY 
		(
			ReportPackageElementTypeId
		) REFERENCES dbo.tReportPackageElementType(ReportPackageElementTypeId)
		, CONSTRAINT FK_tReportSample_tSample FOREIGN KEY 
		(
			SampleId
		) REFERENCES dbo.tSample(SampleId)
    ) ON [LinkoExchange_FG2_Data]
    
    CREATE NONCLUSTERED INDEX IX_tReportSample_ReportPackageElementTypeId ON dbo.tReportSample
	(
		ReportPackageElementTypeId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
	
	CREATE NONCLUSTERED INDEX IX_tReportSample_SampleId ON dbo.tReportSample
	(
		SampleId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tReportFile') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tReportFile'
    PRINT '------------------'

    CREATE TABLE dbo.tReportFile
    (
        ReportFileId                    int IDENTITY(1,1) NOT NULL  
        , ReportPackageElementTypeId    int NOT NULL  
        , FileStoreId                   int NOT NULL  
    
        CONSTRAINT PK_tReportFile PRIMARY KEY CLUSTERED 
        (
	        ReportFileId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT AK_tReportFile_ReportPackageElementTypeId_FileStoreId UNIQUE 
        (
            ReportPackageElementTypeId ASC
            , FileStoreId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
        , CONSTRAINT FK_tReportFile_tReportPackageElementType FOREIGN KEY 
		(
			ReportPackageElementTypeId
		) REFERENCES dbo.tReportPackageElementType(ReportPackageElementTypeId)
		, CONSTRAINT FK_tReportFile_tFileStore FOREIGN KEY 
		(
			FileStoreId
		) REFERENCES dbo.tFileStore(FileStoreId)
    ) ON [LinkoExchange_FG2_Data]
    
    CREATE NONCLUSTERED INDEX IX_tReportFile_ReportPackageElementTypeId ON dbo.tReportFile
	(
		ReportPackageElementTypeId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
	
	CREATE NONCLUSTERED INDEX IX_tReportFile_FileStoreId ON dbo.tReportFile
	(
		FileStoreId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG2_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tCopyOfRecordCertificate') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tCopyOfRecordCertificate'
    PRINT '-------------------------------'
    
    CREATE TABLE dbo.tCopyOfRecordCertificate 
    (
        CopyOfRecordCertificateId           int IDENTITY(1,1) NOT NULL
        , PhysicalPath                      varchar(256) NOT NULL
        , FileName                          varchar(256) NOT NULL
        , Password                          varchar(50) NOT NULL
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tCopyOfRecordCertificate PRIMARY KEY CLUSTERED 
        (
	        CopyOfRecordCertificateId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tCopyOfRecordCertificate ADD CONSTRAINT DF_tCopyOfRecordCertificate_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tCopyOfRecord') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tCopyOfRecord'
    PRINT '--------------------'
    
    CREATE TABLE dbo.tCopyOfRecord 
    (
        ReportPackageId             int NOT NULL
        , Signature                 varchar(350) NOT NULL
        , SignatureAlgorithm        varchar(10) NOT NULL
        , Hash                      varchar(100) NOT NULL
        , HashAlgorithm             varchar(10) NOT NULL
        , Data                      varbinary(max) NOT NULL
        , CopyOfRecordCertificateId int NOT NULL
    
        CONSTRAINT PK_tCopyOfRecord PRIMARY KEY CLUSTERED 
        (
	        ReportPackageId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG3_LOB]
        , CONSTRAINT FK_tCopyOfRecord_tReportPackage FOREIGN KEY 
		(
			ReportPackageId
		) REFERENCES dbo.tReportPackage(ReportPackageId)
        , CONSTRAINT FK_tCopyOfRecord_tCopyOfRecordCertificate FOREIGN KEY 
		(
			CopyOfRecordCertificateId
		) REFERENCES dbo.tCopyOfRecordCertificate(CopyOfRecordCertificateId)
    ) ON [LinkoExchange_FG3_LOB]
    
    CREATE NONCLUSTERED INDEX IX_tCopyOfRecord_ReportPackageId ON dbo.tCopyOfRecord
	(
		ReportPackageId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG3_LOB]

    CREATE NONCLUSTERED INDEX IX_tCopyOfRecord_CopyOfRecordCertificateId ON dbo.tCopyOfRecord
	(
		CopyOfRecordCertificateId ASC
	) WITH FILLFACTOR = 100 ON [LinkoExchange_FG3_LOB]
END
GO


IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tTimeZone)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tTimeZone'
    PRINT '------------------------'

	INSERT INTO dbo.tTimeZone (Name, StandardAbbreviation, DaylightAbbreviation)
		VALUES ('Hawaiian Standard Time', 'HAST', 'HADT')
    INSERT INTO dbo.tTimeZone (Name, StandardAbbreviation, DaylightAbbreviation)
		VALUES ('Alaskan Standard Time', 'AKST', 'AKDT')    
	INSERT INTO dbo.tTimeZone (Name, StandardAbbreviation, DaylightAbbreviation)
		VALUES ('Pacific Standard Time', 'PST', 'PDT')
	INSERT INTO dbo.tTimeZone (Name, StandardAbbreviation, DaylightAbbreviation)
		VALUES ('Mountain Standard Time', 'MST', 'MDT')
	INSERT INTO dbo.tTimeZone (Name, StandardAbbreviation, DaylightAbbreviation)
		VALUES ('Central Standard Time', 'CST', 'CDT')
	INSERT INTO dbo.tTimeZone (Name, StandardAbbreviation, DaylightAbbreviation)
		VALUES ('Eastern Standard Time', 'EST', 'EDT')
	INSERT INTO dbo.tTimeZone (Name, StandardAbbreviation, DaylightAbbreviation)
		VALUES ('Atlantic Standard Time', 'AST', 'ADT')
	INSERT INTO dbo.tTimeZone (Name, StandardAbbreviation, DaylightAbbreviation)
		VALUES ('Newfoundland Standard Time', 'NST', 'NDT')
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tJurisdiction)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tJurisdiction'
    PRINT '----------------------------'
    
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (0, 0, 'CA', 'Canada', 0)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (0, 0, 'US', 'United States', 0)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'AB', 'Alberta', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'BC', 'British Columbia', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'MB', 'Manitoba', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'NB', 'New Brunswick', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'NF', 'Newfoundland and Labrador', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'NT', 'Northwest Territories', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'NS', 'Nova Scotia', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'NU', 'Nunavut', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'ON', 'Ontario', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'PE', 'Prince Edward Island', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'QC', 'Quebec', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'SK', 'Saskatchewan', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (1, 0, 'YT', 'Yukon', 1)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'AL', 'Alabama', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'AK', 'Alaska', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'AZ', 'Arizona', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'AR', 'Arkansas', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'CA', 'California', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'CO', 'Colorado', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'CT', 'Connecticut', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'DE', 'Delaware', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'DC', 'District Of Columbia', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'FL', 'Florida', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'GA', 'Georgia', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'GU', 'Guam', 0)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'HI', 'Hawaii', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'ID', 'Idaho', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'IL', 'Illinois', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'IN', 'Indiana', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'IA', 'Iowa', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'KS', 'Kansas', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'KY', 'Kentucky', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'LA', 'Louisiana', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'ME', 'Maine', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'MD', 'Maryland', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'MA', 'Massachusetts', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'MI', 'Michigan', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'MN', 'Minnesota', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'MS', 'Mississippi', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'MO', 'Missouri', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'MT', 'Montana', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'NE', 'Nebraska', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'NV', 'Nevada', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'NH', 'New Hampshire', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'NJ', 'New Jersey', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'NM', 'New Mexico', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'NY', 'New York', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'NC', 'North Carolina', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'ND', 'North Dakota', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'OH', 'Ohio', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'OK', 'Oklahoma', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'OR', 'Oregon', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'PA', 'Pennsylvania', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'PR', 'Puerto Rico', 0)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'RI', 'Rhode Island', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'SC', 'South Carolina', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'SD', 'South Dakota', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'TN', 'Tennessee', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'TX', 'Texas', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'VI', 'U.S. Virgin Islands', 0)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'UT', 'Utah', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'VT', 'Vermont', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'VA', 'Virginia', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'WA', 'Washington', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'WV', 'West Virginia', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'WI', 'Wisconsin', 2)
	INSERT INTO dbo.tJurisdiction (CountryID, StateID, Code, Name, ParentID)
		VALUES (2, 0, 'WY', 'Wyoming', 2)
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tRegulatoryProgram)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tRegulatoryProgram'
    PRINT '---------------------------------'
    
    INSERT INTO dbo.tRegulatoryProgram (Name, Description)
		VALUES ('IPP', 'Industrial Pretreatment Program')  
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganizationType)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganizationType'
    PRINT '--------------------------------'
    
    INSERT INTO dbo.tOrganizationType (Name, Description)
		VALUES ('Authority', 'The regulator who is using LinkoExchange to receive Report Packages.') 
	INSERT INTO dbo.tOrganizationType (Name, Description)
		VALUES ('Industry', 'A company that discharges industrial process waters to the City’s sewers, permitted by Authority. Industries will be using LinkoExchange to submit reports electronically to Authority.') 
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tPrivacyPolicy)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tPrivacyPolicy'
    PRINT '-----------------------------'
    
    INSERT INTO dbo.tPrivacyPolicy (EffectiveDateTimeUtc, Content)
		VALUES (
        TODATETIMEOFFSET('2017-08-01 00:00:00', '-07:00')
        ,
        '<style>
            p.MsoListBullet
            {
                font-size: 10.0pt;
                margin-bottom: .0001pt;
                margin-left: .75in;
                text-indent: -.29in;
            }

            p.MsoBodyText
            {
                font-size: 10.0pt;
                margin-top: 12.0pt;
                text-align: justify;
            }
        </style>

        <div>
            <p class="MsoBodyText" style="text-align: center;">
                <b>LinkoExchange Privacy Policy</b>
            </p>

            <p class="MsoBodyText">
                <b>EFFECTIVE DATE</b>: August 1, 2017
            </p>

            <p class="MsoBodyText">
                <b>OUR COMMITMENT TO PRIVACY</b>
            </p>

            <p class="MsoBodyText">
                Linko Technology Inc. (“<b>we</b>” or “<b>Linko</b>”)<b> </b>is committed to maintaining the security, confidentiality and privacy of your personal information.
            </p>

            <p class="MsoBodyText">
                This is our Privacy Policy. It documents our on-going commitment to you.
            </p>

            <p class="MsoBodyText">
                <b>SCOPE OF POLICY</b>
            </p>

            <p class="MsoBodyText">
                This Policy applies to our collection, use and disclosure of your personal information you enter into the LinkoExchange Web Site.
            </p>

            <p class="MsoBodyText">
                This Policy does not impose any limits on the collection, use or disclosure of the following information by Linko:
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; business contact information; and
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; certain publicly available information.
            </p>

            <p class="MsoBodyText">
                This Policy also does not apply to other customer data, such as confidential, non-personal information. That information is addressed in our Client Services
                Agreement. Your use of our Services, and any dispute over privacy, is subject to this Privacy Policy and any other applicable agreements between you and us including
                any applicable limitations on damages and the resolution of disputes.
            </p>

            <p class="MsoBodyText">
                <b>ACCOUNTABILITY</b>
            </p>

            <p class="MsoBodyText">
                We have designated a Privacy Officer who is responsible for our compliance with this Policy. The Privacy Officer may be contacted as described below.
            </p>

            <p class="MsoBodyText">
                <b>COLLECTION</b>
            </p>

            <p class="MsoBodyText">
                Information we collect includes Name, Address, Company, Phone, email, login/username, password, knowledge based questions and answers, associations with
                Organizations such as Industries or Authorities that use the Web Site and your roles within those Organizations. We use the information you provide for such
                purposes as confirming your identity, tracking what data records in the software you added, edited or created and sending electronic confirmations
                (email, text messages, phone calls) to you regarding actions you have taken on the Web Site such as personal information changes, account status changes
                (locked, unlocked, enabled, disabled, removed, role) and report or data submissions.
            </p>

            <p class="MsoBodyText">
                <b>PURPOSES</b>
            </p>

            <p class="MsoBodyText">
                When collecting personal information, we will state the purpose of collection and will provide, on request, contact information for the Privacy Officer who can
                answer questions about the collection.
            </p>

            <p class="MsoBodyText">
                We collect, use and disclose your personal information for the following purposes:
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; to provide and administer products and services requested, and to disclose the information for any purpose related to requested products and services;
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; to communicate with you regarding our products and services;
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; to authenticate your identity;
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; to monitor your compliance with any of your agreements with us;
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; to provide personal information to our third party suppliers of products and services, in connection with any products and services we provide to you;
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; to protect us, yourself and others, including from fraud and error and to safeguard our business interests;
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; to comply with legal and regulatory requirements;
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; to facilitate a merger, acquisition, sale or transfer of a business unit or assets, as part of a bankruptcy proceeding, or as part of any
                other similar business transfer.
            </p>

            <p class="MsoBodyText">
                The above collections, uses and disclosures are a reasonably necessary part of your relationship with us.
            </p>

            <p class="MsoBodyText">
                We may also collect, use and disclose your personal information to offer additional or alternate products
                and services to you, and we may add your personal information to customer lists for this purpose. You may instruct us to refrain from using or sharing your
                personal information in this way by providing written notification to our Privacy Officer. We acknowledge that the sharing of your personal information
                in this way is at your option and we will not refuse you access to any product or service merely because you have advised us to stop doing so.
            </p>

            <p class="MsoBodyText">
                In addition, we will also disclose your information to (i) current or future affiliates or subsidiaries and (ii) vendors, service
                providers, agents or others who perform functions on our behalf. We may also disclose aggregate, anonymous or de-identified information about users for marketing,
                advertising, research, compliance or other purposes.
            </p>

            <p class="MsoBodyText">
                When your personal information is to be used for a purpose not previously identified, the new purpose will be
                disclosed to you prior to such use, and your consent will be sought unless the use is authorized or required by law.
            </p>

            <p class="MsoBodyText">
                <b>COOKIES</b>
            </p>

            <p class="MsoBodyText">
                We may also use cookies to collect your personal information. Cookies are unique identifiers which are used to customize your
                website experience and allow our systems to recognize you and your device. Most web browsers automatically accept cookies, but you can usually change your
                browser to prevent or notify you whenever you are sent a cookie. This gives you the chance to decide whether or not to accept the cookie. The use of cookies
                is essential to the use of the website. Disabling cookies will prevent you from using the website and its services.
            </p>

            <p class="MsoBodyText">
                <b>DO NOT TRACK</b>
            </p>

            <p class="MsoBodyText">
                Our online services do not respond to Do Not Track signals. For more information about Do Not Track signals, please visit
                <a href="https://allaboutdnt.com/">https://allaboutdnt.com/</a>. You may, however,
                disable certain tracking as discussed in the Cookies section above (<i>e.g.</i>, by disabling cookies).
            </p>

            <p class="MsoBodyText">
                <b>CROSS-BORDER TRANSFERS</b>
            </p>

            <p class="MsoBodyText">
                Except to the extent we agree otherwise in writing, we may transfer your personal information across provincial, state or national borders
                to fulfill any of the above purposes, including to service providers located in the United States, Canada and other jurisdictions who may be subject to
                applicable disclosure laws in those jurisdictions. You may contact our Privacy Officer (whose contact information is provided below) to obtain information
                about our policies and practices regarding our use of service providers outside of your country, or to ask questions about the collection, use, disclosure or
                storage of personal information by our foreign service providers.
            </p>

            <p class="MsoBodyText">
                <b>CONSENT</b>
            </p>

            <p class="MsoBodyText">
                We will obtain your consent to collect, use or disclose personal information except where we are authorized or required by law to do so without consent.
                For example, we may collect, use or disclose personal information without your knowledge or consent where:
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; the information is publicly available, as defined by statute or regulation;
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; we are obtaining legal advice; or
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; we reasonably expect that obtaining consent would compromise an investigation or proceeding.
            </p>

            <p class="MsoBodyText">
                Other exceptions may apply.
            </p>

            <p class="MsoBodyText">
                Your consent can be express, implied or given through an authorized representative such as a lawyer, agent or broker.
            </p>

            <p class="MsoBodyText">
                Consent may be provided orally, in writing, electronically, through inaction (such as when you fail to notify us that you do not wish your personal information
                collected/used/disclosed for various purposes after you have received notice of those purposes) or otherwise.
            </p>

            <p class="MsoBodyText">
                You may withdraw consent at any time, subject to legal, contractual and other restrictions, provided that you give reasonable notice of withdrawal of consent
                to us. On receipt of notice of withdrawal of consent, we will inform you of the likely consequences of the withdrawal of consent, which may include the
                inability of us to provide certain services for which that information is necessary.
            </p>

            <p class="MsoBodyText">
                <b>
                    LIMITS ON COLLECTION OF PERSONAL INFORMATION
                </b>
            </p>

            <p class="MsoBodyText">
                We will not collect personal information indiscriminately but will limit collection of personal information to that which is reasonable and necessary.
                We will also collect personal information as authorized by law.
            </p>

            <p class="MsoBodyText">
                <b>
                    LIMITS FOR USING, DISCLOSING AND RETAINING PERSONAL INFORMATION
                </b>
            </p>

            <p class="MsoBodyText">
                Your personal information will only be used or disclosed for the purposes set out above and as authorized by law.
            </p>

            <p class="MsoBodyText">
                We will keep personal information used to make a decision affecting you for at least one year after using it to make the decision.
            </p>

            <p class="MsoBodyText">
                We will destroy, erase or make anonymous documents or other records containing personal information as soon as it is reasonable to assume that the original
                purpose is no longer being served by retention of the information and retention is no longer necessary for a legal or business purpose.
            </p>

            <p class="MsoBodyText">
                We will take due care when destroying personal information so as to prevent unauthorized access to the information.
            </p>

            <p class="MsoBodyText">
                <b>ACCURACY</b>
            </p>

            <p class="MsoBodyText">
                We will make a reasonable effort to ensure that personal information we are using or disclosing is accurate and complete.
            </p>

            <p class="MsoBodyText">
                If you demonstrate the inaccuracy or incompleteness of personal information, we will amend the information as required. If appropriate, we will send the
                amended information to third parties to whom the information has been disclosed.
            </p>

            <p class="MsoBodyText">
                When a challenge regarding the accuracy of personal information is not resolved to your satisfaction, we will annotate the personal information under our control
                with a note that the correction was requested but not made.
            </p>

            <p class="MsoBodyText">
                <b>
                    SAFEGUARDING PERSONAL INFORMATION
                </b>
            </p>

            <p class="MsoBodyText">
                We protect the personal information in our custody or control by making reasonable security arrangements to prevent unauthorized access, collection, use,
                disclosure, copying, modification, disposal or similar risks.
            </p>

            <p class="MsoBodyText">
                We will take reasonable steps, through contractual or other reasonable means, to ensure that a comparable level of personal information protection is
                implemented by the suppliers and agents who assist in providing services to us. Some specific safeguards include:
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; physical measures such as locked filing cabinets;
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; organizational measures such as restricting employee access to files and databases as appropriate;
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; electronic measures such as passwords and firewalls; and
            </p>

            <p class="MsoListBullet">
                <span style="font-family: Symbol">·</span>
                &nbsp;&nbsp;&nbsp;&nbsp; investigative measures where we have reasonable grounds to believe that personal information is being inappropriately collected, used or disclosed.
            </p>

            <p class="MsoBodyText">
                Note that confidentiality and security are not assured when information is transmitted through e-mail or other wireless communication. Please notify Linko’s Privacy
                Officer in writing if you do not want Linko to communicate with you through these means.
            </p>

            <p class="MsoBodyText">
                <b> PROVIDING ACCESS </b>
            </p>

            <p class="MsoBodyText">
                You have a right to access your personal information held by Linko.
            </p>

            <p class="MsoBodyText">
                Upon written request to our Privacy Officer, and authentication of identity, we will provide you your personal information under our control. We will also give you
                information about the ways in which that information is being used and a description of the individuals and organizations to whom that information has
                been disclosed. We may charge you a reasonable fee for doing so.
            </p>

            <p class="MsoBodyText">
                We will make the information available within 30 days of receiving a written request or provide written notice where additional time is required to fulfill the request.
            </p>

            <p class="MsoBodyText">
                In some situations, Linko may not be able to provide access to certain personal information (e.g., if disclosure would reveal personal information about
                another individual, the personal information is protected by solicitor/client privilege, the information was collected for the purposes of an investigation
                or where disclosure of the information would reveal confidential commercial information that could harm the competitive position of Linko).
                Linko may also be prevented by law from providing access to certain personal information.
            </p>

            <p class="MsoBodyText">
                Where an access request is refused, we will notify you in writing, document the reasons for refusal and outline further steps which are available to you.
            </p>

            <p class="MsoBodyText">
                <b> CHILDREN </b>
            </p>

            <p class="MsoBodyText">
                Our services are not targeted to children under thirteen (13) years of age and we do not knowingly collect personal information from children under 13. If we
                discover that a child under 13 has provided us with personal information, we will promptly delete such personal information from our systems.
            </p>

            <p class="MsoBodyText">
                <b> COMPLAINTS </b>
            </p>

            <p class="MsoBodyText">
                We will, on request, provide information regarding our complaint procedure.
            </p>

            <p class="MsoBodyText">
                Any inquiries, complaints or questions regarding this Policy should be directed in writing to our Privacy Officer as follows:
            </p>

            <p class="MsoBodyText" style="margin-bottom: .0001pt;">
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Address:
            </p>

            <p class="MsoBodyText" style="margin-bottom: .0001pt; margin-top: 0;">
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Linko Technology Inc

            </p>

            <p class="MsoBodyText" style="margin-bottom: .0001pt; margin-top: 0;">
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Attn: Privacy Officer

            </p>

            <p class="MsoBodyText" style="margin-bottom: .0001pt; margin-top: 0;">
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; 4251 Kipling Street Suite 220

            </p>

            <p class="MsoBodyText" style="margin-top: 0;">
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Wheat Ridge, CO 80033

            </p>

            <p class="MsoBodyText">
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; E-mail:&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                <a href="mailto:privacy@linkotechnology.com">privacy@linkotechnology.com</a>
            </p>

            <p class="MsoBodyText">
                <b> CHANGES TO THIS PRIVACY POLICY </b>
            </p>

            <p class="MsoBodyText">
                This Privacy Policy is current as of the Effective Date set forth above. We may change this Privacy Policy from time to time, so please be sure to check back
                periodically. We will post any changes, including any material changes, to this Privacy Policy on
                <a href="https://client.linkoexchange.com/privacypolicy">https://client.linkoexchange.com/privacypolicy</a>
            </p>
        </div>')
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tTermCondition)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tTermCondition'
    PRINT '-----------------------------'
    
    INSERT INTO dbo.tTermCondition (Content)
		VALUES (
		'<style>
			p.MsoNormal, li.MsoNormal, div.MsoNormal
			{
				font-size: 10.0pt;
			}

			h1
			{
				color: black;
				font-size: 10.0pt;
				font-weight: normal;
				text-align: justify;
			}

			.textUnderline 
			{
				text-decoration: underline;
			}
		</style>

		<div>

			<p align="center" class="MsoNormal" style="text-align: center">
				<b>
					End User Licence Agreement – All Users
				</b>
			</p>

			<p align="center" class="MsoNormal" style="text-align: center">
				<b>
					<span style="font-size: 11.0pt;">
						PLEASE READ THIS DOCUMENT CAREFULLY.<br>
						IT SIGNIFICANTLY ALTERS YOUR LEGAL RIGHTS AND REMEDIES.
					</span>
				</b>
			</p>

			<p class="MsoNormal">
				This is a legal agreement between Linko Technology Inc. (“<b>Linko</b>” or “<b>we</b>” or “<b>us</b>”) of 4251 Kipling Street, Suite 220, Wheat Ridge, Colorado 80033
				and you regarding your use of any Linko application(s), such as the Hosted-Linko™ Pretreatment and FOG Software, POM Portal and Linko™ Exchange, and other modules or
				applications (collectively, the “<b>Application(s)</b>”). If you do not accept these terms, you must not use the Application(s).
			</p>

			<h1>
				<b>BACKGROUND</b>
			</h1>

			<h1>
				Cities, municipalities and other government entities (each, a “<b>Government Entity</b>”) use the Application(s) to collect and process data pursuant to a Client Service
				Agreement or similar agreement between Linko and the Government Entity (the “<b>CSA</b>”).
			</h1>

			<h1>
				Industry entities (each an “<b>Industry Entity</b>”) use the Application(s) to submit data to Government Entities pursuant to agreements between the Industry Entity
				and Government Entity (each, a “<b>GEA</b>”).
			</h1>

			<h1>
				&nbsp;
			</h1>

			<h1>
				<b>
					AGREEMENT
				</b>
			</h1>

			<p class="MsoNormal">
				By accessing or using the Application(s), you agree to the terms and conditions set out here. Linko may change these terms and conditions at our discretion
				any time without notice, so please check this page regularly for updates.
			</p>

			<h1>
				1.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>The Application(s).</b>
				You must not access or use any of the Application(s) unless either:
			</h1>

			<h1 style="margin-left: .35in;">
				a) you work for a Government Entity and you have permission to access or use the Application(s) pursuant to a CSA between Linko and that Government Entity; or
			</h1>

			<h1 style="margin-left: .35in;">
				b) you work for an Industry Entity that is required by a Government Entity to use the Application(s) to submit data, reports or other information to that Government Entity.
			</h1>

			<h1>
				You must only access and use the Application(s) for the internal business purposes of that Government Entity or Industry Entity, and only to the extent permitted
				by the CSA between Linko and that Government Entity and by any GEA between the Government Entity and the Industry Entity.
			</h1>

			<h1>
				2.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Rules.</b>
				Whenever you access or use the Application(s) you must comply with any acceptable use policies and other policies Linko implements with respect to the Application(s)
				from time-to-time, as posted by Linko on or through the Application(s), and all applicable laws. You must protect the confidentiality of your user name and
				password, and you must immediately notify Linko if you believe that confidentiality may have been compromised. You must not permit any third party to access or use your user name and password.
			</h1>

			<h1>
				3.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Prohibitions.</b>
				You must not:
			</h1>

			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				reverse engineer, de-compile, hack, disable, disrupt, interfere with, disassemble, copy, rent, lease, loan, sell, distribute, decrypt, reassemble, modify,
				supplement, translate, adapt or enhance, or create derivative works from any of the Application(s) or the hardware or software used to provide the
				Application(s), or any of the services provided through the Application(s) (the “<b>Services</b>”);
			</p>

			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				attempt to access any data, information or content of any third party through the Application(s), except as authorized by Linko;
			</p>

			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				upload to or transmit via the Application(s) any data, file, software or link that contains or redirects to a virus, Trojan horse, worm or other harmful component;
			</p>

			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				upload to or transmit via the Application(s) any content, link or anything else that (if reproduced, published, transmitted or used) may:
			</p>

			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp;
				be defamatory, threatening, abusive, harassing, hateful, obscene, pornographic, harmful or invasive of anyone’s privacy, or violent,
			</p>
    
			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp;
				violate any law including intellectual property, privacy or other laws;
			</p>
    
			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp;
				impersonate any person;
			</p>
    
			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp; 
				give rise to civil or other liability; or
			</p>

			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp; 
				relate to illegal drugs, weapons, gambling or other illegal activities; upload to or transmit from the Application any data, file, software or link that contains or
				redirects to a virus, Trojan horse, worm or other harmful component;
			</p>
    
			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				interfere with the any third party’s use of the Application(s) or Services;
			</p>

   
			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				access the Application(s) by any means other than through the interface that is provided by Linko;
			</p>

    
			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				use the Application(s) to do or attempt to do any of the following without Linko’s prior written permission:
			</p>

			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp; 
				send spam or other bulk messages;
			</p>

			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp;
				gain unauthorized access to any data, network or system;
			</p>

			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp;
				conduct or promote any commercial activity, other than the purchase of products and services from Linko;
			</p>

			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp;
				monitor data or traffic on any network or system;
			</p>
    
			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp;
				obtain an email address, user name or other information about a third party without their consent;
			</p>
    
			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp;
				use any misleading, false or deceptive TCP/IP header information in any email or posting; or
			</p>
    
			<p class="MsoNormal" style="margin-left: .55in;">
				<span style=''font-family: "Courier New"; font-size: 10.0pt;''>o</span>&nbsp;&nbsp;&nbsp;
				conduct or instigate any denial of service attack against Linko’s website or network, or any third party’s website or network;
			</p>

			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				improperly make complaints or use “flag” buttons or make false reports via the Application(s);
			</p>
    
			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				falsify any data or information available on the Application(s);
			</p>

			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				delete or modify any copyright or other intellectual property notice on the Application(s);
			</p>

			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				avoid, circumvent, or disable any access control technology, security device, procedure, protocol, or technological protection mechanism that may be included
				or established in or as part of any the Application(s) or any hardware/software used to provide the Application(s), or third party hardware/software or services; or
			</p>

			<p class="MsoNormal" style="margin-left: .35in;">
				<span style="font-family: Symbol">·</span>&nbsp;&nbsp;&nbsp;
				authorize or encourage any third party to do any of the above.
			</p>

			<h1>
				4.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Your Content.</b>
				Any feedback, comments, photos, text, data and other content you provide to or through the Application(s) (collectively, “<b>Your</b>
				<b>Content</b>”) may be shared with anyone as permitted by the CSA between the relevant Government Entity and Linko, and you hereby grant Linko an
				irrevocable, perpetual licence to copy, modify, display and otherwise use Your Content in that manner. You represent and warrant to Linko that: (a) you
				either own Your Content or have permission from the copyright owner to make Your Content available on or through the Application(s); (b) Your Content
				is accurate and correct; (c) you have the right to grant the above licence to Linko; and (d) the reproduction, display, distribution, use and other
				exploitation of Your Content by Linko and our service providers, users and licensees as permitted by the above licence will not infringe or violate the
				rights of any third party, and will not violate any law.
			</h1>

			<h1>
				5.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Linko’s Content.</b>
				As between Linko and you, Linko owns: (a) the Application(s) and all content provided on or through the Application(s),
				including all text, photos, videos, templates, images, icons, software, designs, Application(s), data, and other content and all intellectual property
				rights in the content; (b) all tools, hardware and software used to provide the Application(s); and (c) the graphical design, user interface and
				the look and feel of the Application(s).
			</h1>

			<h1>
				6.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Other Content.</b>
				Linko does not endorse all of the content on the Application(s), and that content may contain viruses, incorrect statements, and material that is not suitable for
				you. Linko will not be responsible or liable for content that is generated by other users of the Application(s) or Linko’s website.
			</h1>

			<h1>
				7.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Monitoring and Disclosure.</b>
				Linko and its representatives may monitor your use of the Application(s). Linko also reserves the right to remove or edit content that Linko determines, in its sole
				discretion, is objectionable for any reason; however, Linko will not have any liability for any failure to remove, or delay in removing, any content. Linko may disclose any
				information that is necessary to satisfy any law, regulation or lawful request or as necessary to operate the Application(s) or to protect the rights or property of Linko or others.
			</h1>

			<h1>
				8.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Links from the Application(s).</b>
				The Application(s) may provide links to third-party websites and other content. Those sites and content are independent from Linko. We have no control over, and no liability for, those
				sites, their content, or your use of them. We provide links for your convenience only, and you access them at your own risk.
			</h1>

			<h1>
				9.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Your Privacy.</b>
				Linko and its representatives may collect personal information about you through the Application(s) to provide the Application(s) and Services to you, to allow us to communicate with
				you (including through the use of commercial electronic messages), to manage your account with us, to monitor your compliance with this agreement and any other agreements between you
				and Linko, and for any other purposes as described in our privacy policies, copies of which are available here
				<a href="https://client.linkoexchange.com/privacy-policy/">https://client.linkoexchange.com/privacy-policy/</a>
			</h1>

			<p class="MsoNormal">
				We may disclose your personal information electronically or in writing to our service providers for the above purposes.&nbsp; If we suspect you of prohibited activities, we may
				disclose your personal information to the police or other authorities. We may also otherwise disclose your personal information as permitted by our privacy policies, mentioned above.
			</p>

			<p class="MsoNormal">
				YOU HEREBY CONSENT TO LINKO AND ITS REPRESENTATIVES COLLECTING, USING AND DISCLOSING YOUR PERSONAL INFORMATION IN THE MANNER DESCRIBED ABOVE.
			</p>

			<h1>
				10.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Interruptions/Errors.</b>
				Your use of the Application(s) and Services might be interrupted and might not be error-free, accurate, complete or current at all or any times. The
				Application(s) and Services may also be unavailable from time-to-time due to routine maintenance, upgrades, hardware/software malfunctions, repairs, power
				outages, hackers, denial of service attacks and unforeseeably large service demands.
			</h1>

			<h1>
				11.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Disclaimers, Liability Exclusions/Limitations and Indemnity.</b>
				YOUR ACCESS TO AND USE OF THE APPLICATION(S) AND SERVICES IS AT YOUR OWN RISK. THE APPLICATION(S) AND SERVICES ARE PROVIDED ON AN “AS IS” AND “AS AVAILABLE”
				BASIS, <span class="textUnderline">WITHOUT</span> ANY REPRESENTATIONS, WARRANTIES OR CONDITIONS OF ANY KIND, WHETHER EXPRESS OR IMPLIED, INCLUDING IMPLIED REPRESENTATIONS, WARRANTIES
				OR CONDITIONS OF OR RELATING TO ACCURACY, ACCESSIBILITY, AVAILABILITY, COMPLETENESS, DURABILITY, ERRORS, FITNESS FOR A PARTICULAR PURPOSE, LACK OF
				NEGLIGENCE, MERCHANTABILITY, NON-INFRINGEMENT, PERFORMANCE, PRIVACY, QUALITY, RESULTS, SECURITY, SERVICE, TIMELINESS, TITLE, VIRUSES OR WORKMANLIKE EFFORT,
				ALL OF WHICH ARE HEREBY WAIVED BY YOU AND DISCLAIMED BY LINKO TO THE FULLEST EXTENT PERMITTED BY LAW.
			</h1>

			<h1>
				12.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>LIABILITY EXCLUSIONS.</b>
				WITHOUT LIMITING THE GENERALITY OF THE FOREGOING AND NOTWITHSTANDING ANY OTHER PROVISION OF THIS AGREEMENT, BUT EXCEPT
				AS OTHERWISE REQUIRED BY LAW, UNDER NO CIRCUMSTANCES WILL LINKO OR ANY OF ITS DIRECTORS, OFFICERS, EMPLOYEES, AGENTS, SERVICE PROVIDERS, SUPPLIERS,
				SUB-CONTRACTORS, LICENSORS AND LICENSEES (COLLECTIVELY THE “<b>LINKO</b> <b>ENTITIES</b>”) EVER BE LIABLE TO YOU OR ANY OTHER PERSON FOR ANY INDIRECT, INCIDENTAL,
				CONSEQUENTIAL, SPECIAL, PUNITIVE OR EXEMPLARY LOSS OR DAMAGE ARISING FROM, CONNECTED WITH, OR RELATING TO THE APPLICATION(S) OR THE SERVICES OR THIS
				AGREEMENT, INCLUDING LOSS OF DATA, BUSINESS, MARKETS, SAVINGS, INCOME, PROFITS, USE, PRODUCTION, REPUTATION OR GOODWILL, ANTICIPATED OR OTHERWISE, OR ECONOMIC
				LOSS, UNDER ANY THEORY OF LIABILITY (WHETHER IN CONTRACT, TORT, STRICT LIABILITY OR ANY OTHER THEORY OR LAW OR EQUITY), EVEN IF LINKO HAS BEEN ADVISED
				OF THE POSSIBILITY OF SUCH LOSS OR DAMAGE BEING INCURRED.
			</h1>

			<h1>
				13.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>INDEMNITY.</b>
				YOU WILL INDEMNIFY, DEFEND AND HOLD THE LINKO ENTITIES HARMLESS FROM AND AGAINST ANY AND ALL LIABILITIES, EXPENSES AND COSTS, INCLUDING REASONABLE LEGAL FEES
				AND EXPENSES, INCURRED BY THE LINKO ENTITIES IN CONNECTION WITH ANY CLAIM, DEMAND OR PROCEEDING ARISING OUT OF, RELATED TO, OR CONNECTED WITH YOUR BREACH
				OF THIS AGREEMENT, THE UNTRUTHFULNESS OR INACCURACY OF ANY OF YOUR REPRESENTATIONS OR WARRANTIES TO LINKO, OR ANY WRONGFUL CONDUCT BY YOU OR ANY
				OTHER PERSON FOR WHOM YOU ARE RESPONSIBLE UNDER THIS AGREEMENT OR AT LAW. YOU WILL ASSIST AND CO-OPERATE AS FULLY AS REASONABLY REQUIRED BY LINKO IN THE
				DEFENSE OF ANY SUCH CLAIM, DEMAND OR PROCEEDING.
			</h1>

			<h1>
				14.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Termination.</b>
				You will immediately cease using the Application(s) and Services once the CSA between the relevant Government Entity and Linko has been terminated or has
				expired, or once your permission to use the Application(s) and Services under that CSA or the GEA has ended. Subject to that CSA, Linko may in its sole
				discretion: (a) change, discontinue, modify, restrict, suspend or terminate the Application(s) or any part of it without any notice or liability to you or any
				other person, and (b) for its convenience at any time terminate this agreement or your permission to access and use the Application(s), without any prior
				notice or liability to you or any other person.
			</h1>

			<p class="MsoNormal">
				If this agreement or your permission to access or use any or all of the Application(s) is terminated for any reason, then sections 4, 5, 9 and 11 - 16 of this agreement
				and all other provisions necessary for their interpretation or enforcement, will survive indefinitely after the termination of this agreement and remain in full force and effect.
			</p>

			<h1>
				15.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Laws and Courts.</b>
				Use of the Application(s) and Services is governed exclusively by, and will be enforced, construed and interpreted exclusively in accordance with, the laws applicable
				in Colorado. All disputes under this agreement will be resolved by the courts of Colorado in Denver; however, nothing in this section prohibits either party
				from obtaining an injunction against the other party in any other jurisdiction.
			</h1>

			<h1>
				16.&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<b>Other.</b>&nbsp;
				<span class="textUnderline">Invalidity</span>: If any provision of this agreement is held to be invalid or unenforceable for any reason, then that provision will be deemed to be severed from this agreement
				and the remaining provisions will continue in full force and effect without being impaired or invalidated in any way, unless as a result of any such
				severance this agreement would fail in its essential purpose. <span class="textUnderline">Entire Agreement:</span> This agreement supersedes all prior agreements and understandings between
				Linko and you relating the Application(s). <span class="textUnderline">Enurement:</span> This agreement enures to the benefit of and is binding upon each of Linko and its successors, assigns
				and related persons, and you and each of your heirs, executors, administrators, successors, permitted assigns and personal representatives. <span class="textUnderline">Assignment:</span> You
				must not assign this agreement or the rights and obligations under this agreement. Linko may assign this agreement and its rights and obligations under this agreement
				without your consent. This agreement contains provisions for the benefit of the Linko Entities, each of whom has the right to assert and enforce such provisions
				directly or on their own behalf. <span class="textUnderline">Waiver:</span> No consent or waiver by any party to or of any breach or default by any other party in its performance of
				its obligations under this agreement will be: (a) deemed or construed to be a consent to or waiver of a continuing breach or default or any other breach or
				default of those or any other obligations of that party; or (b) effective unless in writing and signed by all parties. <span class="textUnderline">English</span>: The parties have
				expressly requested and required that this agreement and all other related documents be drawn up in the English language.
				<i>Les parties conviennentet exigent expressement que ce Contrat et tous les documents qui s’y rapportent soient rediges en Anglais.</i>
			</h1>

			<p class="MsoNormal">
				Any rights not expressly granted by this agreement are reserved by Linko.
			</p>
    
			<h1>
				&nbsp;
			</h1>

			<h1>
				<b>BY SELECTING ‘I AGREE TO THE TERMS AND CONDITIONS’, YOU ACKNOWLEDGE THAT YOU HAVE READ AND UNDERSTOOD THIS AGREEMENT AND AGREE TO BE BOUND BY IT.</b>
			</h1>
		</div>') 
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tUserProfile)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tUserProfile'
    PRINT '---------------------------'
    
    INSERT INTO dbo.tUserProfile
    (
        Id
        , FirstName
        , LastName
        , TitleRole
        , BusinessName
        , AddressLine1
        , AddressLine2
        , CityName
        , ZipCode
        , JurisdictionId
        , IsIdentityProofed
        , IsInternalAccount
        , TermConditionId
        , TermConditionAgreedDateTimeUtc
        , Email
        , EmailConfirmed
        , PasswordHash
        , SecurityStamp
        , PhoneNumber
        , PhoneNumberConfirmed
        , TwoFactorEnabled
        , LockoutEnabled
        , AccessFailedCount
        , UserName
    )
    VALUES
    (
        NEWID()
        , 'Linko'
        , 'Support'
        , NULL
        , 'Linko Technology Inc'
        , '4251 Kipling Street'
        , 'Suite 220'
        , 'Wheat Ridge'
        , '80033'
        , (SELECT JurisdictionId FROM dbo.tJurisdiction WHERE Code = 'CO')
        , 1
        , 1
        , 1
        , SYSDATETIMEOFFSET()
        , 'support@linkotechnology.com'
        , 1
        , 'APTVnusF39VRg9PHU64tpgbMY0lE09A8c7u+ZRbEHx/nvflMIFS04TAB3sG2H6q0WQ=='
        , '2a6c392a-1379-4ede-88f3-15f7d02ccf40'
        , '(303) 275-9968'
        , 0
        , 0
        , 1
        , 0 
        , 'Linko'
    )
END


DECLARE @JurisdictionId_MI int
SELECT @JurisdictionId_MI = JurisdictionId 
FROM dbo.tJurisdiction 
WHERE Code = 'MI'


DECLARE @RegulatoryProgramId_IPP int
SELECT @RegulatoryProgramId_IPP = RegulatoryProgramId 
FROM dbo.tRegulatoryProgram 
WHERE Name = 'IPP'

DECLARE @OrganizationTypeId_Authority int
SELECT @OrganizationTypeId_Authority = OrganizationTypeId 
FROM dbo.tOrganizationType 
WHERE Name = 'Authority'

DECLARE @OrganizationTypeId_Industry int
SELECT @OrganizationTypeId_Industry = OrganizationTypeId 
FROM dbo.tOrganizationType 
WHERE Name = 'Industry'

DECLARE @UserProfileId_Linko int
SELECT @UserProfileId_Linko = UserProfileId 
FROM dbo.tUserProfile 
WHERE UserName = 'Linko'


IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganization)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganization'
    PRINT '----------------------------'
    
    -- GRESD
    INSERT INTO dbo.tOrganization 
    (
        OrganizationTypeId
        , Name
        , AddressLine1
        , AddressLine2
        , CityName
        , ZipCode
        , JurisdictionId
        , PhoneNumber
        , FaxNumber
        , WebsiteURL
    )
	VALUES 
	(
        @OrganizationTypeId_Authority
	    , 'City of Grand Rapids'
	    , '1300 Market Ave., S.W.'
	    , NULL
	    , 'Grand Rapids'
	    , '49503'
	    , @JurisdictionId_MI
	    , '616-456-3261'
	    , '616-456-3711'
        , NULL
	) 
END


DECLARE @OrganizationId_GRESD int
SELECT @OrganizationId_GRESD = OrganizationId 
FROM dbo.tOrganization 
WHERE Name = 'City of Grand Rapids'


IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganizationTypeRegulatoryProgram)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganizationTypeRegulatoryProgram'
    PRINT '-------------------------------------------------'
    
    INSERT INTO dbo.tOrganizationTypeRegulatoryProgram (RegulatoryProgramId, OrganizationTypeId)
		VALUES 
		(
		    @RegulatoryProgramId_IPP
		    , @OrganizationTypeId_Authority
		)
	INSERT INTO dbo.tOrganizationTypeRegulatoryProgram (RegulatoryProgramId, OrganizationTypeId)
		VALUES 
		(
		    @RegulatoryProgramId_IPP
		    , @OrganizationTypeId_Industry
		) 
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganizationRegulatoryProgram)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganizationRegulatoryProgram'
    PRINT '---------------------------------------------'
    
    -- GRESD
    INSERT INTO dbo.tOrganizationRegulatoryProgram (RegulatoryProgramId, OrganizationId, RegulatorOrganizationId, ReferenceNumber, IsEnabled)
		VALUES 
		(
		    @RegulatoryProgramId_IPP
		    , @OrganizationId_GRESD
		    , NULL
            , 'MI0026069'
		    , 1
		)
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tSettingTemplate)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tSettingTemplate'
    PRINT '-------------------------------'
    
    -- Authority Organization Type setting
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'FailedPasswordAttemptMaxCount'
		    , 'Number of allowed failed password attempts'
		    , '3'
            , @OrganizationTypeId_Authority
		    , NULL
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'FailedKBQAttemptMaxCount'
		    , 'Number of allowed failed KBQ attempts'
            , '3'
		    , @OrganizationTypeId_Authority
		    , NULL
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'InvitationExpiredHours'
		    , 'Invitation expires this many hours after sending'
            , '72'
		    , @OrganizationTypeId_Authority
		    , NULL
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'PasswordChangeRequiredDays'
		    , 'Number of days before requiring a password change'
            , '180'
		    , @OrganizationTypeId_Authority
		    , NULL
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'PasswordHistoryMaxCount'
		    , 'Number of passwords used in password history'
            , '10'
		    , @OrganizationTypeId_Authority
		    , NULL
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'TimeZone'
		    , 'Time zone'
            , (SELECT TimeZoneId FROM dbo.tTimeZone WHERE Name = 'Eastern Standard Time')
		    , @OrganizationTypeId_Authority
		    , NULL
		)

    -- Authority IPP Regulatory Program setting
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'ReportRepudiatedDays'
		    , 'Maximum number of days after report period end date to repudiate'
            , '180' 
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
	 INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'ComplianceDeterminationDate'
		    , 'Use Start or End Date Sampled when calculating compliance and determining what parameters to include in a compliance period'
            , 'EndDateSampled'
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'MassLoadingConversionFactorPounds'
		    , 'Mass loadings pounds conversion factor'
            , '8.34'
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'MassLoadingResultToUseLessThanSign'
		    , 'Use < sign for Mass Loading Results?'
            , ''
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'MassLoadingCalculationDecimalPlaces'
		    , 'Use these decimal places for Loading calculations'
            , ''
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'EmailContactInfoName'
		    , 'Contact information name on emails'
            , ''
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'EmailContactInfoPhone'
		    , 'Contact information phone on emails'
            , ''
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'EmailContactInfoEmailAddress'
		    , 'Contact information email address on emails'
            , ''
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'AuthorityUserLicenseTotalCount'
		    , 'Number of Authority user licenses Available. This is the number of Authority Portal users for the current regulatory program. When inviting Authority users, system cannot invite more than what is set here.'
            , '15'
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'IndustryLicenseTotalCount'
		    , 'Number of Industry licenses available. This is the number of Industries that an Authority can add to their LinkoExchange Regulatory Program. This will be enforced by LinkoCTS when the list of Industries is sent from CTS to LE.'
            , '200'
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'UserPerIndustryMaxCount'
		    , 'Maximum number of users per Industry. This is how many users can be added to an Industry for the current Regulatory Program. We are using one flat number for all Industries right now. When invitations are being sent to Industry users by anyone, system cannot invite more than what is set here.'
            , '5' 
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'ResultQualifierValidValues'
		    , 'Result qualifier values that Industries can use in the samples and results data screen.'
            , '>,<,ND,NF' 
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'FlowUnitValidValues'
		    , 'Flow units that Industries can use in the samples and results data screen.'
            , 'gpd,mgd' 
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'SampleNameCreationRule'
		    , 'Sample names sent to CTS.'
            , 'SampleEventType'
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'SendToCtsDatabaseServerName'
		    , 'Database server name where the LinkoCTS related data are sent.'
            , 'LINKO-SQL1'
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
    INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
		VALUES 
		(
		    'SendToCtsDatabaseName'
		    , 'Database name where the LinkoCTS related data are sent.'
            , 'CTS_Import'
		    , @OrganizationTypeId_Authority
		    , @RegulatoryProgramId_IPP
		)
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tSystemSetting)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tSystemSetting'
    PRINT '-----------------------------'
    
    INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'SystemEmailFirstName'
		    , 'LinkoExchange'
		    , ''
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'SystemEmailLastName'
		    , 'System'
		    , ''
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'SystemEmailEmailAddress'
		    , 'noreply@linkoexchange.com'
		    , ''
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'PasswordRequiredLength'
		    , '8'
		    , ''
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'PasswordRequiredMaxLength'
		    , '16'
		    , ''
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'PasswordRequireDigit'
		    , '1'
		    , ''
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'PasswordExpiredDays'
		    , '90'
		    , ''
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'SupportPhoneNumber'
		    , '+1-604-418-3201'
		    , ''
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'SupportEmailAddress'
		    , 'support@linkotechnology.com'
		    , ''
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'EmailServer'
		    , 'wtraxadc2.watertrax.local'
		    , ''
		)
    INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'MassLoadingUnitName'
		    , 'ppd'
		    , 'Unit name for mass loading calculation'
		)
    INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'FileAvailableToAttachMaxAgeMonths'
		    , '16'
		    , 'Maximum age in months that a file is available to be selected as an attachment'
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'CTSDatabaseMinVersion'
		    , '16.0'
		    , 'Minimum supported version of CTS_DATA database'
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'CTSDatabaseMinPatch'
		    , '07/15/2017'
		    , 'Minimum supported patch of CTS_DATA database'
		)
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganizationSetting)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganizationSetting'
    PRINT '-----------------------------------'
    
    -- GRESD
    INSERT INTO dbo.tOrganizationSetting (OrganizationId, SettingTemplateId, Value)
	SELECT @OrganizationId_GRESD, SettingTemplateId, DefaultValue
    FROM dbo.tSettingTemplate
    WHERE OrganizationTypeId = @OrganizationTypeId_Authority 
        AND RegulatoryProgramId IS NULL
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganizationRegulatoryProgramSetting)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganizationRegulatoryProgramSetting'
    PRINT '----------------------------------------------------'

    -- GRESD IPP
    INSERT INTO dbo.tOrganizationRegulatoryProgramSetting (OrganizationRegulatoryProgramId, SettingTemplateId, Value)
	SELECT OrganizationRegulatoryProgramId, SettingTemplateId, DefaultValue
    FROM dbo.tSettingTemplate st
        INNER JOIN dbo.tOrganizationRegulatoryProgram orp ON orp.RegulatoryProgramId = st.RegulatoryProgramId
    WHERE st.OrganizationTypeId = @OrganizationTypeId_Authority 
        AND st.RegulatoryProgramId = @RegulatoryProgramId_IPP
        AND orp.OrganizationId = @OrganizationId_GRESD
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tQuestionType)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tQuestionType'
    PRINT '----------------------------'
    
    INSERT INTO dbo.tQuestionType (Name, Description)
		VALUES ('KBQ', 'Knowledge Based Question. KBQs are questions that only the user knows. KBQs are part of a User''s profile and are used in part of the Electronic Signature process.')
	INSERT INTO dbo.tQuestionType (Name, Description)
		VALUES ('SQ', 'Security Question. Same as the KBQs except that the SQs are used for unlocking user accounts and for resetting user passwords if they are forgotten.')
END

DECLARE @QuestionTypeId_KBQ int
SELECT @QuestionTypeId_KBQ = QuestionTypeId 
FROM dbo.tQuestionType 
WHERE Name = 'KBQ'

DECLARE @QuestionTypeId_SQ int
SELECT @QuestionTypeId_SQ = QuestionTypeId 
FROM dbo.tQuestionType 
WHERE Name = 'SQ'

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tQuestion)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tQuestion'
    PRINT '------------------------'
    
    -- KBQs
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is the first and middle name of your oldest sibling?', @QuestionTypeId_KBQ)
	INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your favorite vacation destination?', @QuestionTypeId_KBQ)
	INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What year and model (yyyy-name) was your first car?', @QuestionTypeId_KBQ)	
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your favorite TV show?', @QuestionTypeId_KBQ)
	INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('Where did you first meet your spouse?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your favorite book?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What was your first pet''s name?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your favorite movie?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What street was your high school located on?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is the name of your home town newspaper?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your favorite hobby?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is the name of the hospital where you were born?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('Who is your favorite all-time entertainer?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What was your high school''s mascot?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your favorite song?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your best friend''s name?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is the last name of your favorite teacher?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('Where did you graduate from high school?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your favorite pet''s name?', @QuestionTypeId_KBQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('Who is your favorite author?', @QuestionTypeId_KBQ)
		
	-- SQs
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is the name of your favorite sports team?', @QuestionTypeId_SQ)
	INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What street was your childhood home located on?', @QuestionTypeId_SQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is the middle name of your oldest child?', @QuestionTypeId_SQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What town was your wedding held in?', @QuestionTypeId_SQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your grandmother''s maiden name?', @QuestionTypeId_SQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('Where did you attend college?', @QuestionTypeId_SQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your favorite food?', @QuestionTypeId_SQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('Who is your favorite comedian?', @QuestionTypeId_SQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your favorite color?', @QuestionTypeId_SQ)
    INSERT INTO dbo.tQuestion (Content, QuestionTypeId)
		VALUES ('What is your spouse''s birth date?', @QuestionTypeId_SQ)
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tUserQuestionAnswer)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tUserQuestionAnswer'
    PRINT '----------------------------------'
    
    -- Linko Support KBQs
    -- string: Hash tHiS answer
    INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    'AKuOPfPEFZoHj9FLjgGatC34IIgOfou3ImkGJSew5HNRmJpgHWpG20VkoY/mU0kpVw=='
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite vacation destination?')
		    , @UserProfileId_Linko
		)

    -- string: Hash tHiS answer 2
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    'AE+w46NmQpmYTdIShLn6Kt5m97tLl/iaAAMXO5KBm9QaqPRurxHOWxlYHrDcyJO+Tg=='
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What was your first pet''s name?')
		    , @UserProfileId_Linko
		)

    -- string: Hash tHiS answer 3
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    'APkqDsEGZEKJAUkZ/0jxHAPFeYszYflXH8QYTgkRAAQt4BHmugJXGMV+PQXfDwQ47A=='
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is the name of your home town newspaper?')
		    , @UserProfileId_Linko
		)

    -- string: Hash tHiS answer 4
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    'AEAsu7xLtE5tAiJLa6ljvx+INXQMjV2n4Nv2xpTdw7LCUGTZMOjC/SA8UlDVcJCcgw=='
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite hobby?')
		    , @UserProfileId_Linko
		)

    -- string: Hash tHiS answer 5
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    'ANiN5YMCvnaN26T9L1ABz0eBl3jqB2SCvljwFFouLIIW5b2dbFHKHqMF7BJccLmdhA=='
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite song?')
		    , @UserProfileId_Linko
		)
	
	-- Linko Support SQs
    -- string: Test answer
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    'SjZF/gS1UraAPjQuf5QbbBsgf+TRXhVsNOmHE/mCfMlkGUp7egGFcLcb+jzHh1G82SA5NN/YmOOUWrczy9zc1luE+bO3UU0JijWTJHAl9+YnsHciLG86oVDVI/Uo/HC2'
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is the name of your favorite sports team?')
		    , @UserProfileId_Linko
		)

    -- string: Test answer
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    'SjZF/gS1UraAPjQuf5QbbBsgf+TRXhVsNOmHE/mCfMlkGUp7egGFcLcb+jzHh1G82SA5NN/YmOOUWrczy9zc1luE+bO3UU0JijWTJHAl9+YnsHciLG86oVDVI/Uo/HC2'
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite food?')
		    , @UserProfileId_Linko
		)
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tUserPasswordHistory)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tUserPasswordHistory'
    PRINT '-----------------------------------'
    
    INSERT INTO dbo.tUserPasswordHistory (PasswordHash, UserProfileId)
		VALUES 
		(
		    'APTVnusF39VRg9PHU64tpgbMY0lE09A8c7u+ZRbEHx/nvflMIFS04TAB3sG2H6q0WQ=='
		    , @UserProfileId_Linko
		)
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tAuditLogTemplate)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tAuditLogTemplate'
    PRINT '--------------------------------'
    
    --- UC-5.1 registration denied, for authority
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Registration_AuthorityRegistrationDenied', 
        'Email',
        'Registration',   
        'AuthorityRegistrationDenied', 
        
        'Registration Denied',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

Your LinkoExchange Registration has been denied for the following:
                 
    Authority:  {authorityOrganizationName} 

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}. 
                </pre>
            </body>
         </html>' 
    )

    --- UC-5.1 registration denied, for industry
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Registration_IndustryRegistrationDenied' ,
        'Email',
        'Registration',   
        'IndustryRegistrationDenied', 
        
        'Registration Denied for {organizationName} ({authorityOrganizationName})',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

Your LinkoExchange Registration has been denied for the following:
                 
    Authority:  {authorityOrganizationName}
     Facility:  {organizationName}
                {addressLine1}
                {cityName}, {stateName} 

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}. 
                </pre>
            </body>
        </html>' 
    )

    --- UC-5.1 registration approved, for industry
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Registration_IndustryRegistrationApproved' ,
        'Email',
        'Registration',
        'IndustryRegistrationApproved',
          
        'Registration Approved for {organizationName} ({authorityOrganizationName})',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName}, 

Your LinkoExchange Registration has been approved for the following: 

    Authority:  {authorityOrganizationName}
     Facility:  {organizationName}
                {addressLine1}
                {cityName}, {stateName} 
                     
You can log in here: {link} 
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
         </html>'
    ) 

    --- UC-5.1 registration approved, for authority
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Registration_AuthorityRegistrationApproved' , 
        'Email',
        'Registration',
        'AuthorityRegistrationApproved', 
         
        'Registration Approved',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName}, 

Your LinkoExchange Registration has been approved for the following: 

    Authority:  {authorityOrganizationName} 
                     
You can log in here: {link} 
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'
    ) 

    -- UC-5.5 Lock/UnLock  (for authority user account / and IU User Account)
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    ( 
        'Email_UserAccess_AccountLockout' , 
        'Email',
        'UserAccess',  
        'AccountLockout',
         
        'LinkoExchange Account Lockout', 
        '<html>
            <body> 
                <pre>
Hello,

For security reasons, your account has been locked by the Authority. Please contact your Authority for assistance unlocking your account.

    {authorityName} at {authoritySupportEmail} or {authoritySupportPhoneNumber}  

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {authoritySupportEmail} or {authoritySupportPhoneNumber}.
                 </pre>
            </body>
        </html>' 
    )

    -- UC-5.5 Lock/UnLock (for Authority Sys Admin / IU Authority sys Admin)
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    ( 
        'Email_UserAccess_LockoutToSysAdmins' , 
        'Email',
        'UserAccess',  
        'LockoutToSysAdmins', 
        
        'LinkoExchange Account Lockout', 
        '<html>
            <body> 
                <pre>
Hello,

For security reasons, the following account has been locked by the Authority. Check the logs for more details about this user.

   First Name: {firstName}
    Last Name: {lastName}
    User Name: {userName}
        Email: {email}
                </pre>
            </body>
        </html>' 
    )

    -- UC-5.6 UC-7.6 Reset Authority User Account/Reset IU User Account
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Registration_ResetRequired' , 
        'Email',  
        'Registration', 
        'ResetRequired',
        
        'LinkoExchange Registration Reset Required', 
        '<html>
            <body> 
                <pre>
Hello,

For security reasons, you must reset your Registration Profile including Knowledge Based Questions and Security Questions.
                     
Please click the link below or copy and paste the link into your web browser.
                      
    {link}
                        
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'  
    )
      
    -- UC-5.7  Authority Invites Authority User
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Registration_InviteAuthorityUser' , 
        'Email',
        'Registration',   
        'InviteAuthorityUser', 
        
        'Invitation to {authorityOrganizationName}',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

You''ve been invited to be a user of LinkoExchange:
                        
    Authority:  {authorityOrganizationName} 
                     
To accept the invitation, please click here or copy and paste the link below into your web browser.
                      
    {link}
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'
    ) 

    -- UC-7.1 Authority Approves/Denies Industry User registration
    -- Save as 5.1 


    -- UC-7.5 Lock/Unlock IU User Account 
    -- Save as 5.5


    -- UC-7.6 Reset IU User Account 
    -- Save as 5.6 

    -- UC-7.7  Authority Invites Industry User 
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Registration_AuthorityInviteIndustryUser' , 
        'Email',
        'Registration',   
        'AuthorityInviteIndustryUser', 
        
        'Invitation to {organizationName} ({authorityOrganizationName})',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

You''ve been invited to be a user of LinkoExchange:
                        
    Authority:  {authorityOrganizationName}
     Facility:  {organizationName}
                {addressLine1}
                {cityName}, {stateName} 
                     
To accept the invitation, please click here or copy and paste the link below into your web browser.
                      
    {link}
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'
    ) 

    -- UC-7.8 Grant/Remove Signatory 
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Signature_SignatoryGranted' , 
        'Email',
        'Signature',
        'SignatoryGranted',
         
        'Signatory Rights Granted for {organizationName} ({authorityOrganizationName})',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

Your signatory rights have been granted for 

    Authority:  {authorityOrganizationName}
     Facility:  {organizationName}
                {addressLine1}
                {cityName}, {stateName} 
                     
You are now able to electronically sign report submissions. 
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'
    ) 
     
    -- UC-7.8 Grant/Remove Signatory To Admin (2.3)
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Signature_SignatoryGrantedToAdmin' , 
        'Email',
        'Signature',
        'SignatoryGrantedToAdmin',
         
        'Signatory Rights Granted for {organizationName} ({authorityOrganizationName})',
        '<html>
         <body> 
         <pre>
Hello {adminFirstName} {adminLastName},

Signatory rights have been granted for:

         User: {firstName} {lastName}
    Authority: {authorityOrganizationName}
     Facility: {organizationName}
               {addressLine1}
               {cityName}, {stateName} 
    
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
            </pre>
            </body>
</html>'
    ) 

    -- UC-7.8 Grant/Revoke Signatory 
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    ( 
        'Email_Signature_SignatoryRevoked' , 
        'Email',
        'Signature',
        'SignatoryRevoked', 
        
        'Signatory Rights Revoked for {organizationName} ({authorityOrganizationName})',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

Your signatory rights have been removed for 

    Authority: {authorityOrganizationName}
     Facility: {organizationName}
               {addressLine1}
               {cityName}, {stateName} 
                     
You are no longer able to electronically sign report submissions.
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'  
    )

    -- UC-7.8 Grant/Revoke Signatory (2.3)
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    ( 
        'Email_Signature_SignatoryRevokedToAdmin' , 
        'Email',
        'Signature',
        'SignatoryRevokedToAdmin', 
        
        'Signatory Rights Revoked for {organizationName} ({authorityOrganizationName})',
        '<html>
         <body> 
             <pre>
Hello {adminFirstName} {adminLastName},

Signatory rights have been revoked for:

         User: {firstName} {lastName}
    Authority: {authorityOrganizationName}
     Facility: {organizationName}
               {addressLine1}
               {cityName}, {stateName} 

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
              </pre>
           </body>
</html>'  
    )

    --UC-13.1 Industry Approves/Denies Industry User Registration
    --See 5.1

    --UC-30 Manage My Profile-Profile LockOut
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Profile_KBQFailedLockout' , 
        'Email',
        'Profile',
        'KBQFailedLockout', 
        'LinkoExchange Account Lockout',
        
        '<html>
            <body> 
                <pre>
Hello,

Your account has been locked due to an incorrect Knowledge Based Question answer attempting to access your profile.
Please contact your authority for assistance unlocking your account.

    {authorityList}
                           
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your authority or Linko Technology Inc at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'
    ) 

    --UC-30 Manage My Profile-Profile Changed
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Profile_ProfileChanged' , 
        'Email',
        'Profile',
        'ProfileChanged', 
        
        'LinkoExchange Profile Changed',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

For security reasons, we wanted to let you know your LinkoExchange Profile was changed.

If you did not make this change, please contact your Authority immediately to report a possible security breach.

    {authorityList}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your Authority or Linko Technology Inc at {supportPhoneNumber} or {supportEmail}.
                </pre>
            </body>
        </html>' 
    ) 

    --UC-30 Manage My Profile-Email Changed
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Profile_EmailChanged' , 
        'Email',
        'Profile',
        'EmailChanged', 
        
        'LinkoExchange Profile Changed',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

For security reasons, we wanted to let you know the Email Address on your LinkoExchange Profile was changed.

    Old Email Address:  {oldEmail}
    New Email Address:  {newEmail}

If you did not make this change, please contact your Authority immediately to report a possible security breach.

    {authorityList}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your Authority or Linko Technology Inc at {supportPhoneNumber} or {supportEmail}.
                </pre>
            </body>
        </html>' 
    ) 

    --UC-30 Manage My Profile-KBQs Changed
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Profile_KBQChanged' , 
        'Email',
        'Profile',
        'KBQChanged',
         
        'LinkoExchange Profile Changed',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

For security reasons, we wanted to let you know Knowledge Based Questions on your LinkoExchange Profile were changed. 

If you did not make this change, please contact your Authority immediately to report a possible security breach.

    {authorityList}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your Authority or Linko Technology Inc at {supportPhoneNumber} or {supportEmail}.
                </pre>
            </body>
        </html>' 
    ) 

    --UC-30 Manage My Profile-SQs Changed
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Profile_SecurityQuestionsChanged' , 
        'Email',
        'Profile',
        'SecurityQuestionsChanged', 
        
        'LinkoExchange Profile Changed',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

For security reasons, we wanted to let you know Security Questions on your LinkoExchange Profile were changed. 

If you did not make this change, please contact your Authority immediately to report a possible security breach.

    {authorityList}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your Authority or Linko Technology Inc at {supportPhoneNumber} or {supportEmail}.
                </pre>
            </body>
        </html>' 
    ) 

    --UC-30 Manage My Profile-Password Changed
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Profile_PasswordChanged' , 
        'Email',
        'Profile',
        'PasswordChanged', 
        'LinkoExchange Profile Changed',

        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

For security reasons, we wanted to let you know the password on your LinkoExchange Profile was changed. 

If you did not make this change, please contact your Authority immediately to report a possible security breach.

    {authorityList}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your Authority or Linko Technology Inc at {supportPhoneNumber} or {supportEmail}.
                </pre>
            </body>
        </html>' 
    ) 

    -- UC-31 Password Change 
    -- Save as UC-30

    -- UC-33 Forgot Password 
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_ForgotPassword_ForgotPassword' ,
        'Email',
        'ForgotPassword',
        'ForgotPassword', 
        
        'LinkoExchange Forgot Password',
        '<html>
            <body> 
                <pre>
Hello,

To reset your password, please click the link below or copy and paste the link into your web browser. 

    {link}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your Authority or Linko Technology Inc at {supportPhoneNumber} or {supportEmail}.
                </pre>
            </body>
        </html>' 
    ) 

    -- UC-35 Reset Profile from Account Reset
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Profile_ResetProfileRequired' ,
        'Email',
        'Profile',
        'ResetProfileRequired', 
        
        'LinkoExchange Registration Reset Required',
        '<html>
            <body> 
                <pre>
Hello,

For security reasons, you must reset your Registration Profile including Password, Knowledge Based Questions and Security Questions.

Please click the link below or copy and paste the link into your web browser.

    {link}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your Authority or Linko Technology Inc at {supportPhoneNumber} or {supportEmail}.
                </pre>
            </body>
        </html>' 
    ) 

    -- UC-42.2.a Registration From invitation -- Email to Registration Approvers, industry user
    -- Industry user 
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Registration_IndustryUserRegistrationPendingToApprovers' ,
        'Email',
        'Registration',
        'IndustryUserRegistrationPendingToApprovers', 

        'Registration Pending for {organizationName} ({authorityOrganizationName})' ,
        '<html>
            <body> 
                <pre>
The following user has registered at LinkoExchange and requires action:

       Registrant:  {firstName} {lastName}
        Authority:  {authorityOrganizationName}
         Facility:  {organizationName}
                    {addressLine1}
                    {cityName}, {stateName}
                      
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>' 
    ) 
    -- UC-42.2.a, Registration From invitation -- Email to Registration Approvers, authority user 
    -- authority User 

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Registration_AuthorityUserRegistrationPendingToApprovers' ,
        'Email',
        'Registration',
        'AuthorityUserRegistrationPendingToApprovers', 

        'Registration Pending for {organizationName} ({authorityOrganizationName})' ,
        '<html>
            <body> 
                <pre>
The following user has registered at LinkoExchange and requires action:

    Registrant:  {firstName} {lastName}
     Authority:  {authorityOrganizationName} 
                      
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>' 
    ) 

    -- UC-34 ForgotUserName
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_ForgotUserName_ForgotUserName' ,
        'Email',
        'ForgotUserName', 
        'ForgotUserName', 
        
        'LinkoExchange Forgot Username', 
        '<html>
            <body> 
                <pre>
Hello, 

Your username is: {userName}

You can login at {link}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your Authority or Linko Technology Inc at {supportPhoneNumber} or {supportEmail}.
                </pre>
            </body>
        </html>' 
    )


    -- UC-35 RegistrationResetPending
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Registration_RegistrationResetPending' ,
        'Email',
        'Registration',
        'RegistrationResetPending',  
        
        'Registration Reset Pending',
        '<html
            <body> 
                <pre>
The following user has completed a registration reset and requires action:

    Registrant:  {firstName} {lastName}
                      
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'  
    ) 

    -- UC-5.7  Industry Invites Industry User
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Registration_IndustryInviteIndustryUser' ,
        'Email',
        'Registration',   
        'IndustryInviteIndustryUser', 
        
        'Invitation to {organizationName} ({authorityOrganizationName})',
        '<html>
            <body> 
                <pre>
Hello {firstName} {lastName},

You''ve been invited to be a user of LinkoExchange:
                        
    Authority:  {authorityOrganizationName}
     Facility:  {organizationName}
                {addressLine1}
                {cityName}, {stateName} 
                     
To accept the invitation, please click here or copy and paste the link below into your web browser.
                      
    {link}
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'
    )
	
	--UC-54 4.3 Sign and submit report package, email to IU signatories
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Report_Submission_IU' , 
        'Email',  
        'Report', 
        'SignAndSubmissionToIU',
        
        '{reportPackageName} Submission Received', 
        ' <html>
            <body> 
                <pre>
Your report was received. Please keep this email as proof of your report submission.

Report Details:
	Report Name: {reportPackageName}
	Period Start: {periodStartDate}
	Period End: {periodEndDate}
	Submission Date: {submissionDateTime}
	LinkoExchange COR Signature:
	{corSignature}

Submitted To:
	{recipientOrganizationName}
	{recipientOrganizationAddressLine1} {recipientOrganizationAddressLine2}
	{recipientOrganizationCityName}, {recipientOrganizationJurisdictionName} {recipientOrganizationZipCode}

Submitted By:
	{submitterFirstName} {submitterLastName}
	{submitterTitle}
	{iuOrganizationName} 
	Permit #: {permitNumber} 
	{organizationAddressLine1} {organizationAddressLine2}
	{organizationCityName}, {organizationJurisdictionName} {organizationZipCode} 

	User Name: {userName}

To view the report in LinkoExchange, click the link below or copy and paste the link into your web browser. Login is required to view the report.

	{corViewLink}


This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'
    )
	
	--UC-54 4.4 Sign and submit report package, email to authority
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Report_Submission_AU' , 
        'Email',  
        'Report', 
        'SignAndSubmissionAU',
        
        '{iuOrganizationName} {reportPackageName} Received', 
        ' <html>
            <body> 
                <pre>
The following report was received.

Report Details:
	Report Name: {reportPackageName}
	Period Start: {periodStartDate}
	Period End: {periodEndDate}
	Submission Date: {submissionDateTime}
	LinkoExchange COR Signature: 
	{corSignature}

Submitted By:
	{submitterFirstName} {submitterLastName}
	{submitterTitle}
	{iuOrganizationName} 
	Permit #: {permitNumber} 
	{organizationAddressLine1} {organizationAddressLine2}
	{organizationCityName}, {organizationJurisdictionName} {organizationZipCode} 

	User Name: {userName}

To view the report in LinkoExchange, click the link below or copy and paste the link into your web browser. Login is required to view the report.

	{corViewLink}


                </pre>
            </body>
        </html>'
    )
	
	
	--UC-54 3.b Sign and submit-Incorrect Password 
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_COR_PasswordFailedLockout' , 
        'Email',  
        'COR', 
        'PasswordFailedUserLock',        
        'LinkoExchange Account Locked', 
        ' <html>
            <body> 
                <pre>
Hello,
Your account has been locked due to exceeding Password attempts during the signature ceremony.

Please contact your authority for assistance unlocking your account.

        {authorityList}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your authority or Linko Technology Inc at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'
    )
	
	-- repudiation -Incorrect Password 
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Repudiation_PasswordFailedLockout' , 
        'Email',  
        'Repudiation', 
        'ReportPasswordFailedUserLock',        
        'LinkoExchange Account Locked', 
        ' <html>
            <body> 
                <pre>
Hello,
Your account has been locked due to exceeding Password attempts during repudiation.

Please contact your authority for assistance unlocking your account.

        {authorityList}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your authority or Linko Technology Inc at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'
    )
	
	
	--UC-54 4.b Sign and submit-Incorrect KBQ 
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_COR_KBQFailedLockout' , 
        'Email',  
        'COR', 
        'KBQFailedUserLock',        
        'LinkoExchange Account Locked', 
        ' <html>
            <body> 
                <pre>
Hello,
Your account has been locked due to exceeding Knowledge Based Question attempts during the signature ceremony.

Please contact your authority for assistance unlocking your account.

        {authorityList}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your authority or Linko Technology Inc at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'
    )	
	
	--repudiation -Incorrect KBQ 
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Repudiation_KBQFailedLockout' , 
        'Email',  
        'Repudiation', 
        'RepudiationKBQFailedLockout',        
        'LinkoExchange Account Locked', 
        ' <html>
            <body> 
                <pre>
Hello,
Your account has been locked due to exceeding Knowledge Based Question attempts during repudiation.

Please contact your authority for assistance unlocking your account.

        {authorityList}

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact your authority or Linko Technology Inc at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'
    )
	
	
	
	--UC-19 Report Repudiation sent to standard users of Authority (UC-19 8.3.)
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Report_Repudiation_IU' , 
        'Email',  
        'Report', 
        'RepudiationToIU',
        
        '{reportPackageName} Repudiation Received', 
        ' <html>
            <body> 
                <pre>
Your report repudiation was received.  Please keep this email as proof of your report repudiation.

Report Details:
	Report Name: {reportPackageName}
	Period Start: {periodStartDate}
	Period End: {periodEndDate}
	Submission Date: {submissionDateTime}
	LinkoExchange COR Signature:
	{corSignature}  
	Repudiated Date: {repudiatedDateTime}

Repudiated To:
	{authOrganizationName}
	{authOrganizationAddressLine1} {authOrganizationAddressLine2}
	{authOrganizationCityName} {authOrganizationJurisdictionName} {authOrganizationZipCode} 

Repudiated By:
	{submitterFirstName} {submitterLastName}
	{submitterTitle}
	{iuOrganizationName} 
	Permit #: {permitNumber} 
	{organizationAddressLine1} {organizationAddressLine2}
	{organizationCityName} {organizationJurisdictionName} {organizationZipCode} 
	User Name: {userName}
	
To view the report in LinkoExchange, click the link below or copy and paste the link into your web browser.  Login is required to view the report.

	{corViewLink}
	
                            
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {supportEmail} or {supportPhoneNumber}.
                </pre>
            </body>
        </html>'  
    )


	--UC-19 Report Repudiation sent to standard users of Authority (UC-19 8.4.)
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'Email_Report_Repudiation_AU' , 
        'Email',  
        'Report', 
        'RepudiationToAU',
        
        '{iuOrganizationName} {reportPackageName} Repudiated', 
        ' <html>
            <body> 
                <pre>
The following report was repudiated.

Report Details:
	Report Name: {reportPackageName}
	Period Start: {periodStartDate}
	Period End: {periodEndDate}
	Original Submission Date: {submissionDateTime}
	LinkoExchange COR Signature:
	{corSignature}
	Repudiated Date: {repudiatedDateTime}
	Reason: {repudiationReason}
	Comments: {repudiationReasonComments}

Repudiated By:
	{submitterFirstName} {submitterLastName}
	{submitterTitle}
	{iuOrganizationName} 
	Permit #: {permitNumber} 
	{organizationAddressLine1} {organizationAddressLine2}
	{organizationCityName} {organizationJurisdictionName} {organizationZipCode} 
	User Name: {userName}
	
To view the report in LinkoExchange, click the link below or copy and paste the link into your web browser.  Login is required to view the report.

	{corViewLink}
                </pre>
            </body>
        </html>'  
    )

	-- Cromerr Event Log Templates
	
	INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Registration_InviteSent', 
        'CromerrEvent',
        'Registration',   
        'InviteSent', 
        '',
        'An invitation was sent to:
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

For:
{authorityName}
{organizationName}
{regulatoryProgram}

By:
{inviterFirstName} {inviterLastName} 
User Name: {inviterUserName}
Email:  {inviterEmailAddress}
' 
    )

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Registration_InviteDeleted', 
        'CromerrEvent',
        'Registration',   
        'InviteDeleted', 
        '',
        'An invitation was deleted for:
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

For:
{authorityName}
{organizationName}
{regulatoryProgram}

By:
{actorFirstName} {actorLastName} 
User Name: {actorUserName}
Email:  {actorEmailAddress}
' 
    )
    
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Registration_RegistrationPending', 
        'CromerrEvent',
        'Registration',   
        'RegistrationPending', 
        '',
        'A registration request was received.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

For:
{authorityName}
{organizationName}
{regulatoryProgram}
' 
    )

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Registration_RegistrationApproved', 
        'CromerrEvent',
        'Registration',   
        'RegistrationApproved', 
        '',
        'A registration request was approved.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

For:
{authorityName}
{organizationName}
{regulatoryProgram}

By:
{actorFirstName} {actorLastName} 
Approver User Name: {actorUserName}
Approver Email:  {actorEmailAddress}
' 
    )
    
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Registration_RegistrationDenied', 
        'CromerrEvent',
        'Registration',   
        'RegistrationDenied', 
        '',
        'A registration request was denied.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

For:
{authorityName}
{organizationName}
{regulatoryProgram}

By:
{actorFirstName} {actorLastName} 
Denier User Name: {actorUserName}
Denier Email:  {actorEmailAddress}
' 
    )
    
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_UserAccess_Disabled', 
        'CromerrEvent',
        'UserAccess',   
        'Disabled', 
        '',
        'Account access to {organizationName} was disabled.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

By:
{actorFirstName} {actorLastName} 
User Name: {actorUserName}
' 
    )

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_UserAccess_Enabled', 
        'CromerrEvent',
        'UserAccess',   
        'Enabled', 
        '',
        'Account access to {organizationName} was enabled.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

By:
{actorFirstName} {actorLastName} 
User Name: {actorUserName}
' 
    )

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_UserAccess_Removed', 
        'CromerrEvent',
        'UserAccess',   
        'Removed', 
        '',
        'User was removed from {organizationName}.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

By:
{actorFirstName} {actorLastName} 
User Name: {actorUserName}
' 
    )

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_UserAccess_RoleChange', 
        'CromerrEvent',
        'UserAccess',   
        'RoleChange', 
        '',
        'User roles changed for {organizationName}.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

Old Roles:
{oldRole}

New Roles:
{newRole}

By:
{actorOrganizationName}
{actorFirstName} {actorLastName} 
User Name: {actorUserName}
' 
    )

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_UserAccess_ManualAccountLock', 
        'CromerrEvent',
        'UserAccess',   
        'ManualAccountLock', 
        '',
        'Account was manually locked.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

By:
{authorityName}
{authorityFirstName} {authorityLastName} 
User Name: {authorityUserName}
Email:  {authorityEmailaddress}
' 
    )

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_UserAccess_ManualAccountUnlock', 
        'CromerrEvent',
        'UserAccess',   
        'ManualAccountUnlock', 
        '',
        'Account was manually unlocked.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

By:
{authorityName}
{authorityFirstName} {authorityLastName} 
User Name: {authorityUserName}
Email:  {authorityEmailaddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_UserAccess_AccountResetInitiated', 
        'CromerrEvent',
        'UserAccess',   
        'AccountResetInitiated', 
        '',
        'Account reset initiated.  Re-registration pending.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

By:
{authorityName}
{authorityFirstName} {authorityLastName} 
User Name: {authorityUserName}
Email:  {authorityEmailaddress}
' 
)
    
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_UserAccess_AccountResetExpired', 
        'CromerrEvent',
        'UserAccess',   
        'AccountResetExpired', 
        '',
        'An attempt to reset an account failed because the reset link expired.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_UserAccess_AccountResetSuccessful', 
        'CromerrEvent',
        'UserAccess',   
        'AccountResetSuccessful', 
        '',
        'Account re-registration from reset complete.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Login_Success', 
        'CromerrEvent',
        'Login',   
        'Success', 
        '',
        'Login successful to {organizationName}
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Login_PasswordLockout', 
        'CromerrEvent',
        'Login',   
        'PasswordLockout', 
        '',
        'Login Failed. Consecutive invalid password attempts resulted in a password lock.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Login_AccountLocked', 
        'CromerrEvent',
        'Login',   
        'AccountLocked', 
        '',
        'Login failed.  User attempted to log into a Locked Account.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Login_AccountResetRequired', 
        'CromerrEvent',
        'Login',   
        'AccountResetRequired', 
        '',
        'Login failed.  User attempted to log in when an Account Reset was required.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Login_UserDisabled', 
        'CromerrEvent',
        'Login',   
        'UserDisabled', 
        '',
        'Login Failed.  User account is disabled.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Login_RegistrationPending', 
        'CromerrEvent',
        'Login',   
        'RegistrationPending', 
        '',
        'Login failed. User registration is pending.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Login_NoAssociation', 
        'CromerrEvent',
        'Login',   
        'NoAssociation', 
        '',
        'Login failed.  User is not associated with an enabled account.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_ForgotPassword_Success', 
        'CromerrEvent',
        'ForgotPassword',   
        'Success', 
        '',
        'Successful password reset.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_ForgotPassword_PasswordResetExpired', 
        'CromerrEvent',
        'ForgotPassword',   
        'PasswordResetExpired', 
        '',
        'Forgot Password reset attempted on an expired link.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_ForgotPassword_AccountLocked', 
        'CromerrEvent',
        'ForgotPassword',   
        'AccountLocked', 
        '',
        'Login failed.  Account locked for exceeding Knowledge Based Question attempts during password reset.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Profile_AccountLocked', 
        'CromerrEvent',
        'Profile',   
        'AccountLocked', 
        '',
        'Account locked for exceeding Knowledge Based Question attempts during profile access.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Profile_PasswordChanged', 
        'CromerrEvent',
        'Profile',   
        'PasswordChanged', 
        '',
        'Password was changed.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Profile_EmailChanged', 
        'CromerrEvent',
        'Profile',   
        'EmailChanged', 
        '',
        'Email was changed from {oldEmail} to {newEmail}
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Profile_KBQChanged', 
        'CromerrEvent',
        'Profile',   
        'KBQChanged', 
        '',
        'Knowledge Based Questions were changed
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)


    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Profile_SQChanged', 
        'CromerrEvent',
        'Profile',   
        'SQChanged', 
        '',
        'Security Questions were changed
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)


    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Signature_IdentityProofed', 
        'CromerrEvent',
        'Signature',   
        'IdentityProofed', 
        '',
        'The user has successfully been identity proofed.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Signature_SignatoryPending', 
        'CromerrEvent',
        'Signature',   
        'SignatoryPending', 
        '',
        'Signatory rights have been requested for {organizationName}.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)


	INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Signature_SignatoryGranted', 
        'CromerrEvent',
        'Signature',   
        'SignatoryGranted', 
        '',
        'Signatory rights granted to {organizationName}.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Signature_SignatoryRevoked', 
        'CromerrEvent',
        'Signature',   
        'SignatoryRevoked', 
        '',
        'Signatory rights revoked for {organizationName}.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Signature_SignFailed', 
        'CromerrEvent',
        'Signature',   
        'SignFailed', 
        '',
        'Signature ceremony failed because the user entered and incorrect KBQ answer for {organizationName}.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Signature_AccountLockedKBQSigning', 
        'CromerrEvent',
        'Signature',   
        'AccountLockedKBQSigning', 
        '',
        'Account locked for exceeding Knowledge Based Question attempts during signature ceremony.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

For:
Organization: {organizationName}
Report: {reportPackageName}
Report Period: {periodStartDate} - {periodEndDate}
' 
)
   
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Signature_AccountLockedPasswordSigning', 
        'CromerrEvent',
        'Signature',   
        'AccountLockedPasswordSigning', 
        '',
        'Account locked for exceeding Password attempts during signature ceremony.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

For:
Organization: {organizationName}
Report: {reportPackageName}
Report Period: {periodStartDate} - {periodEndDate}
' 
)
   
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Signature_AccountLockedKBQRepudiation', 
        'CromerrEvent',
        'Signature',   
        'AccountLockedKBQRepudiation', 
        '',
        'Account locked for exceeding Knowledge Based Question attempts during repudiation.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

For:
Organization: {organizationName}
Report: {reportPackageName}
Report Period: {periodStartDate} - {periodEndDate}
' 
)
   
    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Signature_AccountLockedPasswordRepudiation', 
        'CromerrEvent',
        'Signature',   
        'AccountLockedPasswordRepudiation', 
        '',
        'Account locked for exceeding Password attempts during repudiation.
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}

For:
Organization: {organizationName}
Report: {reportPackageName}
Report Period: {periodStartDate} - {periodEndDate}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Report_Submitted', 
        'CromerrEvent',
        'Report',   
        'Submitted', 
        '',
        'Report submitted for:
Organization: {organizationName}
Report: {reportPackageName}
Report Period: {periodStartDate} - {periodEndDate}
LinkoExchange COR Signature:
{corSignature}

By:
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

    INSERT INTO dbo.tAuditLogTemplate (Name, TemplateType, EventCategory, EventType, SubjectTemplate, MessageTemplate)
    VALUES 
    (
        'CromerrEvent_Report_Repudiated', 
        'CromerrEvent',
        'Report',   
        'Repudiated', 
        '',
        'Report repudiated for:
Organization: {organizationName}
Report: {reportPackageName}
Report Period: {periodStartDate} - {periodEndDate}
LinkoExchange COR Signature:
{corSignature}

By:
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}
' 
)

END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tPermissionGroupTemplate)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tPermissionGroupTemplate'
    PRINT '---------------------------------------'
    
    INSERT INTO dbo.tPermissionGroupTemplate (Name, Description, OrganizationTypeRegulatoryProgramId)
		VALUES 
		(
		    'Administrator'
		    , 'Authority Administrator User'
		    , (SELECT OrganizationTypeRegulatoryProgramId FROM dbo.tOrganizationTypeRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationTypeId = @OrganizationTypeId_Authority)
		)
	INSERT INTO dbo.tPermissionGroupTemplate (Name, Description, OrganizationTypeRegulatoryProgramId)
		VALUES 
		(
		    'Standard'
		    , 'Authority Standard User'
		    , (SELECT OrganizationTypeRegulatoryProgramId FROM dbo.tOrganizationTypeRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationTypeId = @OrganizationTypeId_Authority)
		)
	INSERT INTO dbo.tPermissionGroupTemplate (Name, Description, OrganizationTypeRegulatoryProgramId)
		VALUES 
		(
		    'Administrator'
		    , 'Industry Administrator User'
		    , (SELECT OrganizationTypeRegulatoryProgramId FROM dbo.tOrganizationTypeRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationTypeId = @OrganizationTypeId_Industry)
		)
	INSERT INTO dbo.tPermissionGroupTemplate (Name, Description, OrganizationTypeRegulatoryProgramId)
		VALUES 
		(
		    'Standard'
		    , 'Industry Standard User'
		    , (SELECT OrganizationTypeRegulatoryProgramId FROM dbo.tOrganizationTypeRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationTypeId = @OrganizationTypeId_Industry)
		)
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tPermissionGroup)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tPermissionGroup'
    PRINT '-------------------------------'
    
    -- GRESD
    INSERT INTO dbo.tPermissionGroup (Name, Description, OrganizationRegulatoryProgramId)
		VALUES 
		(
		    'Administrator'
		    , 'Authority Administrator User'
		    , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
		)
	INSERT INTO dbo.tPermissionGroup (Name, Description, OrganizationRegulatoryProgramId)
		VALUES 
		(
		    'Standard'
		    , 'Authority Standard User'
		    , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
		)
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganizationRegulatoryProgramUser)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganizationRegulatoryProgramUser'
    PRINT '-------------------------------------------------'
    
    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    @UserProfileId_Linko
		    , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
		    , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
            , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD))
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tSignatoryRequestStatus)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tSignatoryRequestStatus'
    PRINT '--------------------------------------'
    
    INSERT INTO dbo.tSignatoryRequestStatus (Name, Description)
		VALUES ('New', 'New signatory rights request')
	INSERT INTO dbo.tSignatoryRequestStatus (Name, Description)
		VALUES ('Granted', 'Signatory rights request granted')
	INSERT INTO dbo.tSignatoryRequestStatus (Name, Description)
		VALUES ('Denied', 'Signatory rights request denied')
	INSERT INTO dbo.tSignatoryRequestStatus (Name, Description)
		VALUES ('Revoked', 'Signatory rights request revoked')
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tCollectionMethodType)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tCollectionMethodType'
    PRINT '------------------------------------'
    
    INSERT INTO dbo.tCollectionMethodType (Name, Description)
		VALUES ('Grab', 'Grab')
    INSERT INTO dbo.tCollectionMethodType (Name, Description)
		VALUES ('Composite', 'Composite')
    INSERT INTO dbo.tCollectionMethodType (Name, Description)
		VALUES ('Field\Meter', 'Field\Meter')
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tLimitType)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tLimitType'
    PRINT '-------------------------'
    
    INSERT INTO dbo.tLimitType (Name, Description)
		VALUES ('Daily', 'Daily')
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tLimitBasis)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tLimitBasis'
    PRINT '--------------------------'
    
    INSERT INTO dbo.tLimitBasis (Name, Description)
		VALUES ('Concentration', 'Concentration')
    INSERT INTO dbo.tLimitBasis (Name, Description)
		VALUES ('MassLoading', 'Mass Loading')
    INSERT INTO dbo.tLimitBasis (Name, Description)
		VALUES ('VolumeFlowRate', 'Volume Flow Rate')
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tFileType)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tFileType'
    PRINT '------------------------'
    
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.docx', 'application/msword')
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.doc', 'application/msword')
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.xls', 'application/excel')
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.xlsx', 'application/excel')
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.pdf', 'application/pdf')
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.tif', 'image/tiff')
	INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.tiff', 'image/tiff')
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.jpg', 'image/jpeg')
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.jpeg', 'image/pjpeg')
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.bmp', 'image/bmp')
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.png', 'image/png')
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.txt', 'text/plain')
    INSERT INTO dbo.tFileType (Extension, Description)
		VALUES ('.csv', 'text/csv')
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tReportElementCategory)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tReportElementCategory'
    PRINT '-------------------------------------'
    
    INSERT INTO dbo.tReportElementCategory (Name, Description)
		VALUES ('SamplesAndResults', 'Samples and results data')
    INSERT INTO dbo.tReportElementCategory (Name, Description)
		VALUES ('Attachments', 'File attachments')
    INSERT INTO dbo.tReportElementCategory (Name, Description)
		VALUES ('Certifications', 'Certification statements')
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tReportElementType)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tReportElementType'
    PRINT '---------------------------------'
    
    INSERT INTO dbo.tReportElementType (Name, Description, ReportElementCategoryId, OrganizationRegulatoryProgramId)
		VALUES 
        (
            'Samples and Results'
            , 'Sample results'
            , 1
            , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
        )
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tReportStatus)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tReportStatus'
    PRINT '----------------------------'
    
    INSERT INTO dbo.tReportStatus (Name, Description)
		VALUES ('Draft', 'Draft')
    INSERT INTO dbo.tReportStatus (Name, Description)
		VALUES ('ReadyToSubmit', 'Ready To Submit')
    INSERT INTO dbo.tReportStatus (Name, Description)
		VALUES ('Submitted', 'Submitted')
    INSERT INTO dbo.tReportStatus (Name, Description)
		VALUES ('Repudiated', 'Repudiated')
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tRepudiationReason)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tRepudiationReason'
    PRINT '---------------------------------'
    
    INSERT INTO dbo.tRepudiationReason (Name, Description, OrganizationRegulatoryProgramId)
		VALUES 
        (
            'I did not submit this report'
            , 'I did not submit this report'
            , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
        )
    INSERT INTO dbo.tRepudiationReason (Name, Description, OrganizationRegulatoryProgramId)
		VALUES 
        (
            'Report is missing a sample'
            , 'Report is missing a sample'
            , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
        )
    INSERT INTO dbo.tRepudiationReason (Name, Description, OrganizationRegulatoryProgramId)
		VALUES 
        (
            'Report is missing a parameter'
            , 'Report is missing a parameter'
            , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
        )
    INSERT INTO dbo.tRepudiationReason (Name, Description, OrganizationRegulatoryProgramId)
		VALUES 
        (
            'Report has errors'
            , 'Report has errors'
            , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
        )
    INSERT INTO dbo.tRepudiationReason (Name, Description, OrganizationRegulatoryProgramId)
		VALUES 
        (
            'A hold time was exceeded'
            , 'A hold time was exceeded'
            , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
        )
    INSERT INTO dbo.tRepudiationReason (Name, Description, OrganizationRegulatoryProgramId)
		VALUES 
        (
            'Other (please comment)'
            , 'Other (please comment)'
            , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
        )
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'db_ctssync' AND type = 'R')
BEGIN
	-- Create new role specific for LinkoCTS sync
	CREATE ROLE db_ctssync

    GRANT SELECT ON dbo.tOrganization TO db_ctssync
	GRANT INSERT ON dbo.tOrganization TO db_ctssync
	GRANT UPDATE ON dbo.tOrganization TO db_ctssync

    GRANT SELECT ON dbo.tOrganizationSetting TO db_ctssync
	GRANT INSERT ON dbo.tOrganizationSetting TO db_ctssync
	GRANT UPDATE ON dbo.tOrganizationSetting TO db_ctssync

    GRANT SELECT ON dbo.tOrganizationRegulatoryProgram TO db_ctssync
	GRANT INSERT ON dbo.tOrganizationRegulatoryProgram TO db_ctssync
	GRANT UPDATE ON dbo.tOrganizationRegulatoryProgram TO db_ctssync

    GRANT SELECT ON dbo.tOrganizationRegulatoryProgramSetting TO db_ctssync
	GRANT INSERT ON dbo.tOrganizationRegulatoryProgramSetting TO db_ctssync
	GRANT UPDATE ON dbo.tOrganizationRegulatoryProgramSetting TO db_ctssync

    GRANT SELECT ON dbo.tPermissionGroup TO db_ctssync
	GRANT INSERT ON dbo.tPermissionGroup TO db_ctssync

    GRANT SELECT ON dbo.tCtsEventType TO db_ctssync
	GRANT INSERT ON dbo.tCtsEventType TO db_ctssync
	GRANT UPDATE ON dbo.tCtsEventType TO db_ctssync

    GRANT SELECT ON dbo.tCollectionMethod TO db_ctssync
	GRANT INSERT ON dbo.tCollectionMethod TO db_ctssync
	GRANT UPDATE ON dbo.tCollectionMethod TO db_ctssync

    GRANT SELECT ON dbo.tUnit TO db_ctssync
	GRANT INSERT ON dbo.tUnit TO db_ctssync
	GRANT UPDATE ON dbo.tUnit TO db_ctssync

    GRANT SELECT ON dbo.tMonitoringPoint TO db_ctssync
	GRANT INSERT ON dbo.tMonitoringPoint TO db_ctssync
	GRANT UPDATE ON dbo.tMonitoringPoint TO db_ctssync

    GRANT SELECT ON dbo.tParameter TO db_ctssync
	GRANT INSERT ON dbo.tParameter TO db_ctssync
	GRANT UPDATE ON dbo.tParameter TO db_ctssync

    GRANT SELECT ON dbo.tMonitoringPointParameter TO db_ctssync
	GRANT INSERT ON dbo.tMonitoringPointParameter TO db_ctssync
	GRANT DELETE ON dbo.tMonitoringPointParameter TO db_ctssync

    GRANT SELECT ON dbo.tSampleFrequency TO db_ctssync
	GRANT INSERT ON dbo.tSampleFrequency TO db_ctssync
	GRANT DELETE ON dbo.tSampleFrequency TO db_ctssync

    GRANT SELECT ON dbo.tSampleRequirement TO db_ctssync
	GRANT INSERT ON dbo.tSampleRequirement TO db_ctssync
	GRANT DELETE ON dbo.tSampleRequirement TO db_ctssync

    GRANT SELECT ON dbo.tMonitoringPointParameterLimit TO db_ctssync
	GRANT INSERT ON dbo.tMonitoringPointParameterLimit TO db_ctssync
	GRANT DELETE ON dbo.tMonitoringPointParameterLimit TO db_ctssync

    -- Requires only SELECT because they are involved in SELECT statements
    GRANT SELECT ON dbo.tOrganizationTypeRegulatoryProgram TO db_ctssync
    GRANT SELECT ON dbo.tOrganizationType TO db_ctssync
    GRANT SELECT ON dbo.tRegulatoryProgram TO db_ctssync
    GRANT SELECT ON dbo.tSettingTemplate TO db_ctssync
    GRANT SELECT ON dbo.tSystemSetting TO db_ctssync
    GRANT SELECT ON dbo.tUserProfile TO db_ctssync
    GRANT SELECT ON dbo.tJurisdiction TO db_ctssync
    GRANT SELECT ON dbo.tPermissionGroupTemplate TO db_ctssync
    GRANT SELECT ON dbo.tCollectionMethodType TO db_ctssync
    GRANT SELECT ON dbo.tLimitBasis TO db_ctssync
    GRANT SELECT ON dbo.tLimitType TO db_ctssync
END
GO

PRINT CHAR(13)
PRINT '---------------------------'
PRINT 'END OF: LinkoExchange Setup'
PRINT '---------------------------'