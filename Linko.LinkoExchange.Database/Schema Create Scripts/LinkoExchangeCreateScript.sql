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
	( NAME = N'LinkoExchange_Data', FILENAME = N'C:\mssql\data\LinkoExchange.mdf' , SIZE = 4096KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
	 LOG ON 
	( NAME = N'LinkoExchange_Log', FILENAME = N'C:\mssql\logs\LinkoExchange_log.ldf' , SIZE = 1024KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
	
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

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT 1 FROM master.sys.server_principals WHERE name = 'exnet')
BEGIN
    CREATE LOGIN exnet WITH PASSWORD = N'test'
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'exnet')
BEGIN
	CREATE USER exnet FOR LOGIN exnet
	EXEC sp_addrolemember 'db_datareader', 'exnet'
	EXEC sp_addrolemember 'db_datawriter', 'exnet'
	EXEC sp_addrolemember 'db_owner', 'exnet'
END
GO


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
        , Abbreviation                  varchar(10) NOT NULL  
        , Name                          varchar(100) NOT NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tTimeZone PRIMARY KEY CLUSTERED 
        (
	        TimeZoneId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tTimeZone_Abbreviation UNIQUE 
        (
            Abbreviation ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
    ) ON [PRIMARY]
    
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY] 
    ) ON [PRIMARY]
    
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tRegulatoryProgram_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
    ) ON [PRIMARY]
    
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tOrganizationType_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tOrganizationType ADD CONSTRAINT DF_tOrganizationType_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
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
        UserProfileId                   int IDENTITY(1,1) NOT NULL  
        , FirstName                     varchar(50) NOT NULL  
        , LastName                      varchar(50) NOT NULL  
        , TitleRole                     varchar(250) NULL  
        , BusinessName                  varchar(100) NULL  
        , AddressLine1                  varchar(100) NOT NULL  
        , AddressLine2                  varchar(100) NULL  
        , CityName                      varchar(100) NOT NULL  
        , ZipCode                       varchar(50) NOT NULL  
        , JurisdictionId                int NULL    
        , PhoneExt                      int NULL  
        , IsAccountLocked               bit NOT NULL  
        , IsAccountResetRequired        bit NOT NULL  
        , IsIdentityProofed             bit NOT NULL  
        , IsInternalAccount             bit NOT NULL  
        , OldEmailAddress               nvarchar(256) NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , Id                            nvarchar(128) NOT NULL  
        , Email                         nvarchar(256) NOT NULL  
        , EmailConfirmed                bit NOT NULL  
        , PasswordHash                  nvarchar(max) NULL  
        , SecurityStamp                 nvarchar(max) NULL
        , PhoneNumber                   nvarchar(max) NULL  
        , PhoneNumberConfirmed          bit NOT NULL  
        , TwoFactorEnabled              bit NOT NULL  
        , LockoutEndDateUtc             datetime NULL  
        , LockoutEnabled                bit NOT NULL  
        , AccessFailedCount             int NOT NULL  
        , UserName                      nvarchar(256) NOT NULL  
        
        CONSTRAINT PK_tUserProfile PRIMARY KEY NONCLUSTERED 
        (
	        Id ASC
        ) WITH FILLFACTOR = 90 ON [PRIMARY]
        , CONSTRAINT AK_tUserProfile_UserProfileId UNIQUE 
        (
            UserProfileId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tUserProfile_tJurisdiction FOREIGN KEY 
		(
			JurisdictionId
		) REFERENCES dbo.tJurisdiction(JurisdictionId)
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tUserProfile ADD CONSTRAINT DF_tUserProfile_IsAccountLocked DEFAULT 0 FOR IsAccountLocked
    ALTER TABLE dbo.tUserProfile ADD CONSTRAINT DF_tUserProfile_IsAccountResetRequired DEFAULT 0 FOR IsAccountResetRequired
    ALTER TABLE dbo.tUserProfile ADD CONSTRAINT DF_tUserProfile_IsIdentityProofed DEFAULT 0 FOR IsIdentityProofed
    ALTER TABLE dbo.tUserProfile ADD CONSTRAINT DF_tUserProfile_IsInternalAccount DEFAULT 0 FOR IsInternalAccount
    ALTER TABLE dbo.tUserProfile ADD CONSTRAINT DF_tUserProfile_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tUserProfile_JurisdictionId ON dbo.tUserProfile 
    (
	    JurisdictionId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        , AddressLine1                  varchar(100) NOT NULL  
        , AddressLine2                  varchar(100) NULL  
        , CityName                      varchar(100) NOT NULL  
        , ZipCode                       varchar(50) NOT NULL  
        , JurisdictionId                int NULL  
        , PhoneNumber                   varchar(25) NULL  
        , PhoneExt                      int NULL  
        , FaxNumber                     varchar(25) NULL  
        , WebsiteURL                    varchar(256) NULL  
        , PermitNumber                  varchar(50) NULL  
        , Signer                        varchar(250) NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
        
        CONSTRAINT PK_tOrganization PRIMARY KEY CLUSTERED 
        (
	        OrganizationId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tOrganization ADD CONSTRAINT DF_tOrganization_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganization_OrganizationTypeId ON dbo.tOrganization 
    (
	    OrganizationTypeId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tOrganization_JurisdictionId ON dbo.tOrganization 
    (
	    JurisdictionId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        , AssignedTo                        varchar(50) NULL  
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
        
        CONSTRAINT PK_tOrganizationTypeRegulatoryProgram PRIMARY KEY CLUSTERED 
        (
	        OrganizationTypeRegulatoryProgramId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tOrganizationTypeRegulatoryProgram_RegulatoryProgramId_OrganizationTypeId UNIQUE 
        (
            RegulatoryProgramId ASC
            , OrganizationTypeId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tOrganizationTypeRegulatoryProgram ADD CONSTRAINT DF_tOrganizationTypeRegulatoryProgram_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationTypeRegulatoryProgram_RegulatoryProgramId ON dbo.tOrganizationTypeRegulatoryProgram 
    (
	    RegulatoryProgramId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationTypeRegulatoryProgram_OrganizationTypeId ON dbo.tOrganizationTypeRegulatoryProgram 
    (
	    OrganizationTypeId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        , IsEnabled                     bit NOT NULL  
        , IsRemoved                     bit NOT NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL
        
        CONSTRAINT PK_tOrganizationRegulatoryProgram PRIMARY KEY CLUSTERED 
        (
	        OrganizationRegulatoryProgramId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tOrganizationRegulatoryProgram_RegulatoryProgramId_OrganizationId UNIQUE 
        (
            RegulatoryProgramId ASC
            , OrganizationId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tOrganizationRegulatoryProgram ADD CONSTRAINT DF_tOrganizationRegulatoryProgram_IsEnabled DEFAULT 0 FOR IsEnabled
    ALTER TABLE dbo.tOrganizationRegulatoryProgram ADD CONSTRAINT DF_tOrganizationRegulatoryProgram_IsRemoved DEFAULT 0 FOR IsRemoved
    ALTER TABLE dbo.tOrganizationRegulatoryProgram ADD CONSTRAINT DF_tOrganizationRegulatoryProgram_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgram_RegulatoryProgramId ON dbo.tOrganizationRegulatoryProgram
	(
		RegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgram_OrganizationId ON dbo.tOrganizationRegulatoryProgram
	(
		OrganizationId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgram_RegulatorOrganizationId ON dbo.tOrganizationRegulatoryProgram
	(
		RegulatorOrganizationId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tSettingTemplate ADD CONSTRAINT DF_tSettingTemplate_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tSettingTemplate_Name ON dbo.tSettingTemplate 
    (
	    Name ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]

    CREATE NONCLUSTERED INDEX IX_tSettingTemplate_OrganizationTypeId ON dbo.tSettingTemplate 
    (
	    OrganizationTypeId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tSettingTemplate_RegulatoryProgramId ON dbo.tSettingTemplate 
    (
	    RegulatoryProgramId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY] 
        , CONSTRAINT AK_tSystemSetting_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
    ) ON [PRIMARY]
    
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tOrganizationSetting_OrganizationId_SettingTemplateId UNIQUE 
        (
            OrganizationId ASC
            , SettingTemplateId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tOrganizationSetting_tOrganization FOREIGN KEY 
		(
			OrganizationId
		) REFERENCES dbo.tOrganization(OrganizationId)
		, CONSTRAINT FK_tOrganizationSetting_tSettingTemplate FOREIGN KEY 
		(
			SettingTemplateId
		) REFERENCES dbo.tSettingTemplate(SettingTemplateId)
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tOrganizationSetting ADD CONSTRAINT DF_tOrganizationSetting_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationSetting_OrganizationId ON dbo.tOrganizationSetting
	(
		OrganizationId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationSetting_SettingTemplateId ON dbo.tOrganizationSetting
	(
		SettingTemplateId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tOrganizationRegulatoryProgramSetting_OrganizationRegulatoryProgramId_SettingTemplateId UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , SettingTemplateId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tOrganizationRegulatoryProgramSetting_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
		, CONSTRAINT FK_tOrganizationRegulatoryProgramSetting_tSettingTemplate FOREIGN KEY 
		(
			SettingTemplateId
		) REFERENCES dbo.tSettingTemplate(SettingTemplateId)  
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tOrganizationRegulatoryProgramSetting ADD CONSTRAINT DF_tOrganizationRegulatoryProgramSetting_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramSetting_OrganizationRegulatoryProgramId ON dbo.tOrganizationRegulatoryProgramSetting
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramSetting_SettingTemplateId ON dbo.tOrganizationRegulatoryProgramSetting
	(
		SettingTemplateId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        
        CONSTRAINT PK_tInvitation PRIMARY KEY NONCLUSTERED 
        (
	        InvitationId ASC
        ) WITH FILLFACTOR = 90 ON [PRIMARY]
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
	) ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_tInvitation_SenderOrganizationRegulatoryProgramId ON dbo.tInvitation 
    (
	    SenderOrganizationRegulatoryProgramId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tInvitation_RecipientOrganizationRegulatoryProgramId ON dbo.tInvitation 
    (
	    RecipientOrganizationRegulatoryProgramId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
		) WITH FILLFACTOR = 100 ON [PRIMARY]
		, CONSTRAINT FK_AspNetUserClaims_tUserProfile FOREIGN KEY 
		(
			UserId
		) REFERENCES dbo.tUserProfile(Id) 
		ON DELETE CASCADE
    ) ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_AspNetUserClaims_UserId ON dbo.AspNetUserClaims
	(
		UserId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
		) WITH FILLFACTOR = 100 ON [PRIMARY]
		, CONSTRAINT FK_AspNetUserLogins_tUserProfile FOREIGN KEY 
		(
			UserId
		) REFERENCES dbo.tUserProfile(Id) 
		ON DELETE CASCADE  
    ) ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_AspNetUserLogins_UserId ON dbo.AspNetUserLogins
	(
		UserId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_AspNetRoles_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
    ) ON [PRIMARY]
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
		) WITH FILLFACTOR = 100 ON [PRIMARY]
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
    ) ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_AspNetUserRoles_UserId ON dbo.AspNetUserRoles
	(
		UserId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]

	CREATE NONCLUSTERED INDEX IX_AspNetUserRoles_RoleId ON dbo.AspNetUserRoles
	(
		RoleId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tQuestionType_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY] 
    ) ON [PRIMARY]
    
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tQuestion_tQuestionType FOREIGN KEY 
		(
			QuestionTypeId
		) REFERENCES dbo.tQuestionType(QuestionTypeId)  
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tQuestion ADD CONSTRAINT DF_tQuestion_IsActive DEFAULT 1 FOR IsActive
    ALTER TABLE dbo.tQuestion ADD CONSTRAINT DF_tQuestion_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tQuestion_QuestionTypeId ON dbo.tQuestion
	(
		QuestionTypeId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tUserQuestionAnswer_QuestionId_UserProfileId UNIQUE 
        (
            QuestionId ASC
            , UserProfileId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY] 
        , CONSTRAINT FK_tUserQuestionAnswer_tQuestion FOREIGN KEY 
		(
			QuestionId
		) REFERENCES dbo.tQuestion(QuestionId)
        , CONSTRAINT FK_tUserQuestionAnswer_tUserProfile FOREIGN KEY 
		(
			UserProfileId
		) REFERENCES dbo.tUserProfile(UserProfileId)
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tUserQuestionAnswer ADD CONSTRAINT DF_tUserQuestionAnswer_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tUserQuestionAnswer_QuestionId ON dbo.tUserQuestionAnswer
	(
		QuestionId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tUserQuestionAnswer_UserProfileId ON dbo.tUserQuestionAnswer
	(
		UserProfileId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
		) WITH FILLFACTOR = 100 ON [PRIMARY]
		, CONSTRAINT FK_tUserPasswordHistory_tUserProfile FOREIGN KEY 
		(
			UserProfileId
		) REFERENCES dbo.tUserProfile(UserProfileId)   
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tUserPasswordHistory ADD CONSTRAINT DF_tUserPasswordHistory_LastModificationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR LastModificationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tUserPasswordHistory_UserProfileId ON dbo.tUserPasswordHistory
	(
		UserProfileId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tAuditLogTemplate_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tAuditLogTemplate_TemplateType_EventCategory_EventType UNIQUE 
        (
            TemplateType ASC
            , EventCategory ASC
            , EventType ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]  
    ) ON [PRIMARY]
    
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tEmailAuditLog_tAuditLogTemplate FOREIGN KEY 
		(
			AuditLogTemplateId
		) REFERENCES dbo.tAuditLogTemplate(AuditLogTemplateId)  
    ) ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tEmailAuditLog_AuditLogTemplateId ON dbo.tEmailAuditLog
	(
		AuditLogTemplateId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_tEmailAuditLog_Token ON dbo.tEmailAuditLog
	(
		Token ASC
	) WITH FILLFACTOR = 90 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tCromerrAuditLog_tAuditLogTemplate FOREIGN KEY 
		(
			AuditLogTemplateId
		) REFERENCES dbo.tAuditLogTemplate(AuditLogTemplateId)  
    ) ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tCromerrAuditLog_AuditLogTemplateId ON dbo.tCromerrAuditLog
	(
		AuditLogTemplateId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tModule_Name_OrganizationTypeRegulatoryProgramId UNIQUE 
        (
            Name ASC
            , OrganizationTypeRegulatoryProgramId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tModule_tOrganizationTypeRegulatoryProgram FOREIGN KEY 
		(
			OrganizationTypeRegulatoryProgramId
		) REFERENCES dbo.tOrganizationTypeRegulatoryProgram(OrganizationTypeRegulatoryProgramId)
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tModule ADD CONSTRAINT DF_tModule_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tModule_Name ON dbo.tModule
	(
		Name ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tModule_OrganizationTypeRegulatoryProgramId ON dbo.tModule
	(
		OrganizationTypeRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tOrganizationRegulatoryProgramModule_OrganizationRegulatoryProgramId_ModuleId UNIQUE 
        (
            OrganizationRegulatoryProgramId ASC
            , ModuleId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tOrganizationRegulatoryProgramModule_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
		, CONSTRAINT FK_tOrganizationRegulatoryProgramModule_tModule FOREIGN KEY 
		(
			ModuleId
		) REFERENCES dbo.tModule(ModuleId)
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tOrganizationRegulatoryProgramModule ADD CONSTRAINT DF_tOrganizationRegulatoryProgramModule_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramModule_OrganizationRegulatoryProgramId ON dbo.tOrganizationRegulatoryProgramModule
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramModule_ModuleId ON dbo.tOrganizationRegulatoryProgramModule
	(
		ModuleId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tPermission_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tPermission_tModule FOREIGN KEY 
		(
			ModuleId
		) REFERENCES dbo.tModule(ModuleId)
		NOT FOR REPLICATION
	) ON [PRIMARY]
	
	ALTER TABLE dbo.tPermission ADD CONSTRAINT DF_tPermission_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

	CREATE NONCLUSTERED INDEX IX_tPermission_ModuleId ON dbo.tPermission 
    (
	    ModuleId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tPermissionGroupTemplate_Name_OrganizationTypeRegulatoryProgramId UNIQUE 
        (
            Name ASC
            , OrganizationTypeRegulatoryProgramId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tPermissionGroupTemplate_tOrganizationTypeRegulatoryProgram FOREIGN KEY 
		(
			OrganizationTypeRegulatoryProgramId
		) REFERENCES dbo.tOrganizationTypeRegulatoryProgram(OrganizationTypeRegulatoryProgramId)
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tPermissionGroupTemplate ADD CONSTRAINT DF_tPermissionGroupTemplate_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroupTemplate_Name ON dbo.tPermissionGroupTemplate
	(
		Name ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroupTemplate_OrganizationTypeRegulatoryProgramId ON dbo.tPermissionGroupTemplate
	(
		OrganizationTypeRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tPermissionGroupTemplatePermission_PermissionGroupTemplateId_PermissionId UNIQUE 
        (
            PermissionGroupTemplateId ASC
            , PermissionId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
    ) ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroupTemplatePermission_PermissionGroupTemplateId ON dbo.tPermissionGroupTemplatePermission 
    (
	    PermissionGroupTemplateId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroupTemplatePermission_PermissionId ON dbo.tPermissionGroupTemplatePermission 
    (
	    PermissionId ASC
    ) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tPermissionGroup_Name_OrganizationRegulatoryProgramId UNIQUE 
        (
            Name ASC
            , OrganizationRegulatoryProgramId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tPermissionGroup_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)  
    ) ON [PRIMARY]
    
    ALTER TABLE dbo.tPermissionGroup ADD CONSTRAINT DF_tPermissionGroup_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroup_Name ON dbo.tPermissionGroup
	(
		Name ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroup_OrganizationRegulatoryProgramId ON dbo.tPermissionGroup
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tPermissionGroupPermission_PermissionGroupId_PermissionId UNIQUE 
        (
            PermissionGroupId ASC
            , PermissionId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tPermissionGroupPermission_tPermissionGroup FOREIGN KEY 
		(
			PermissionGroupId
		) REFERENCES dbo.tPermissionGroup(PermissionGroupId)
		, CONSTRAINT FK_tPermissionGroupPermission_tPermission FOREIGN KEY 
		(
			PermissionId
		) REFERENCES dbo.tPermission(PermissionId)
    ) ON [PRIMARY]
    
    CREATE NONCLUSTERED INDEX IX_tPermissionGroupPermission_PermissionGroupId ON dbo.tPermissionGroupPermission
	(
		PermissionGroupId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_tPermissionGroupPermission_PermissionId ON dbo.tPermissionGroupPermission
	(
		PermissionId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        OrganizationRegulatoryProgramUserId int IDENTITY(1,1) NOT NULL
        , UserProfileId                     int NOT NULL    
        , OrganizationRegulatoryProgramId   int NOT NULL  
        , PermissionGroupId                 int NULL  
        , RegistrationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , IsRegistrationApproved            bit NOT NULL  
        , IsRegistrationDenied              bit NOT NULL  
        , IsEnabled                         bit NOT NULL  
        , IsRemoved                         bit NOT NULL  
        , IsSignatory                       bit NOT NULL  
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tOrganizationRegulatoryProgramUser PRIMARY KEY CLUSTERED 
        (
	        OrganizationRegulatoryProgramUserId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tOrganizationRegulatoryProgramUser_UserProfileId_OrganizationRegulatoryProgramId UNIQUE 
        (
            UserProfileId ASC
            , OrganizationRegulatoryProgramId ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tOrganizationRegulatoryProgramUser_tUserProfile FOREIGN KEY 
		(
			UserProfileId
		) REFERENCES dbo.tUserProfile(UserProfileId)
		, CONSTRAINT FK_tOrganizationRegulatoryProgramUser_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
		, CONSTRAINT FK_tOrganizationRegulatoryProgramUser_tPermissionGroup FOREIGN KEY 
		(
			PermissionGroupId
		) REFERENCES dbo.tPermissionGroup(PermissionGroupId) 
    ) ON [PRIMARY]
    
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
	) WITH FILLFACTOR = 100 ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramUser_OrganizationRegulatoryProgramId ON dbo.tOrganizationRegulatoryProgramUser
	(
		OrganizationRegulatoryProgramId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_tOrganizationRegulatoryProgramUser_PermissionGroupId ON dbo.tOrganizationRegulatoryProgramUser
	(
		PermissionGroupId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT AK_tSignatoryRequestStatus_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
    ) ON [PRIMARY]
    
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
        ) WITH FILLFACTOR = 100 ON [PRIMARY]
        , CONSTRAINT FK_tSignatoryRequest_tOrganizationRegulatoryProgramUser FOREIGN KEY 
		(
			OrganizationRegulatoryProgramUserId
		) REFERENCES dbo.tOrganizationRegulatoryProgramUser(OrganizationRegulatoryProgramUserId)
		, CONSTRAINT FK_tSignatoryRequest_tSignatoryRequestStatus FOREIGN KEY 
		(
			SignatoryRequestStatusId
		) REFERENCES dbo.tSignatoryRequestStatus(SignatoryRequestStatusId)
    ) ON [PRIMARY]
     
    CREATE NONCLUSTERED INDEX IX_tSignatoryRequest_OrganizationRegulatoryProgramUserId ON dbo.tSignatoryRequest
	(
		OrganizationRegulatoryProgramUserId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
	
	CREATE NONCLUSTERED INDEX IX_tSignatoryRequest_SignatoryRequestStatusId ON dbo.tSignatoryRequest
	(
		SignatoryRequestStatusId ASC
	) WITH FILLFACTOR = 100 ON [PRIMARY]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tTimeZone)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tTimeZone'
    PRINT '------------------------'
    
  --  INSERT INTO dbo.tTimeZone (Abbreviation, Name)
		--VALUES (0, 0)  
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
		VALUES ('Industry', 'A company that discharges industrial process waters to the City�s sewers, permitted by Authority. Industries will be using LinkoExchange to submit reports electronically to Authority.') 
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
        , BusinessName
        , LastName
        , AddressLine1
        , AddressLine2
        , CityName
        , ZipCode
        , JurisdictionId
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
        newid()
        , 'Linko'
        , 'Support'
        , 'Linko Technology Inc'
        , '4251 Kipling Street'
        , 'Suite 220'
        , 'Wheat Ridge'
        , '80033'
        , (SELECT JurisdictionId FROM dbo.tJurisdiction WHERE Code = 'CO')
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
        , PermitNumber
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
	    , 'MI0026069'
	) 

 -- GRESD Industries
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'American Seating, North Bldg.'
     , '401 American Seating Center'
     , NULL
     , 'Grand Rapids'
     , '49504'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0004'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Spectrum Industries Wealthy'
     , '700 Wealthy St., S.W.'
     , NULL
     , 'Grand Rapids'
     , '49504'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0006'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Di-Anodic Finishing'
     , '736 Ottawa Ave., N.W.'
     , NULL
     , 'Grand Rapids'
     , '49503'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0007'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Grand Rapids Stripping Co.'
     , '1933 Will Ave, N.W.'
     , NULL
     , 'Grand Rapids'
     , '49504'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0012'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'H. R. Terryberry Company'
     , '2033 Oak Industrial Dr., N.E.'
     , NULL
     , 'Grand Rapids'
     , '49505'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0013'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Haviland Products Co (East)'
     , '421 Ann St. NW'
     , NULL
     , 'Grand Rapids'
     , '49504'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0015'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Knape & Vogt Manufacturing Co.'
     , '2700 Oak Industrial Dr., N.E.'
     , NULL
     , 'Grand Rapids'
     , '49505'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0021'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Lacks Enterprises, Inc.'
     , '4260 Airlane, S.E.'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0022'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Master Finish Company'
     , '1160 Burton St.'
     , NULL
     , 'Grand Rapids'
     , '49507'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0025'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Midwest Plating Company, Inc.'
     , '613 North Ave., N.E.'
     , NULL
     , 'Grand Rapids'
     , '49503'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0028'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Lacks Enterprises, Inc.'
     , '1648 Monroe Ave., N.W.'
     , NULL
     , 'Grand Rapids'
     , '49505'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0032'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Valley City Plating'
     , '3353 Eastern, S.E.'
     , NULL
     , 'Grand Rapids'
     , '49508'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0040'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'The DECC Company'
     , '1266 Wallen, S.W.'
     , NULL
     , 'Grand Rapids'
     , '49507'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0052'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'CSX Transportation, Inc.'
     , '945 Freeman St., S.W.'
     , NULL
     , 'Grand Rapids, MI'
     , '49503'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0056'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Access Business Group L.L.C.'
     , '7575 Fulton, E.'
     , NULL
     , 'Ada'
     , '49355'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0062'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Dematic Corp. (0077)'
     , '507 Plymouth,N.E.'
     , NULL
     , 'Grand Rapids'
     , '49505'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0077'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Knoll, Inc.'
     , '4300 36th St., S.E.'
     , NULL
     , 'Grand Rapids'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0107'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Irwin Seating'
     , '3251 Fruitridge N.W.'
     , NULL
     , 'Grand Rapids'
     , '49544'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0136'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Kentwood Powder Coat Inc.'
     , '3900 Swank, S.E.'
     , NULL
     , 'Grand Rapids'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0458'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Lacks Enterprises, Inc.'
     , '4375 52nd St. SE'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0469'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Steelcase Inc. (Kentwood West)'
     , '4360 52nd St. SE'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0479'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Controlled Plating Tech.'
     , '1100 Godfrey Ave., S.W.'
     , NULL
     , 'Grand Rapids'
     , '49503'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0508'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Michigan Medical P.C.'
     , '4069 Lake Dr. SE'
     , 'Suite 313'
     , 'Grand Rapids'
     , '49546'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0605'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Midwest Plating Plant II'
     , '738 Lafayette,  N.E.'
     , NULL
     , 'Grand Rapids'
     , '49503'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0636'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Industrial Stripping Services'
     , '2235 29th St. SE'
     , NULL
     , 'Grand Rapids'
     , '49508'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0649'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Professional Metal Finishers'
     , '2474 Turner St., N.W.'
     , 'Section F'
     , 'Grand Rapids'
     , '49544'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0652'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Pro-Finish Powder Coating'
     , '1000 Ken-O-Sha Industrial Dr.'
     , NULL
     , 'Grand Rapids'
     , '49508'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0655'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Premier Finishing'
     , '3180 Fruitridge Ave NW'
     , NULL
     , 'Walker'
     , '49544'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0659'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'G.E. Aviation'
     , '3290 Patterson Ave., S.E.'
     , NULL
     , 'Grand Rapids'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0660'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Haviland Products Co (West)'
     , '525 Ann St. NW'
     , NULL
     , 'Grand Rapids'
     , '49504'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0670'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'West Michigan Coating'
     , '3150 Fruitridge Ave., N.W.'
     , NULL
     , 'Walker'
     , '49544'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0672'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'El Matador Tortilla Factory'
     , '45 Franklin, S.W.'
     , NULL
     , 'Grand Rapids'
     , '49507'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8277'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Keebler Company'
     , '310 28th St. SE'
     , NULL
     , 'Grand Rapids'
     , '49548'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8283'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Eurasia Feather'
     , '655 Evergreen, S.E.'
     , NULL
     , 'Grand Rapids'
     , '49507'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8290'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Valley City Linen'
     , '10 Diamond SE'
     , NULL
     , 'Grand Rapids'
     , '49506'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8292'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Arnie''s Bakery, Div of Arnie''s Inc'
     , '815 Leonard St., N.W.'
     , NULL
     , 'Grand Rapids'
     , '49504'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8293'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Spectrum Cubic McConnell'
     , '13 McConnell, S.W.'
     , NULL
     , 'Grand Rapids'
     , '49504'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0642'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Arkema Coating Resins'
     , '1415 Steele Ave., S.W.'
     , NULL
     , 'Grand Rapids'
     , '49507'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0260'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Southern Lithoplate DBA American Litho, Inc.'
     , '4150 Danvers Ct., S.E.'
     , NULL
     , 'Grand Rapids'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0697'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Costco Wholesale  #784'
     , '5100 28th St. SE'
     , NULL
     , 'Grand Rapids'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0708'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Lacks Enterprises, Inc.'
     , '4090 Barden S.E.'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0711'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Cintas 301'
     , '3149 Wilson Dr. NW'
     , NULL
     , 'Walker'
     , '49544'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8294'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Kerry, Inc.'
     , '4444 52nd St. SE'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8296'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Keeler Brass Company - Kentwood'
     , '2929 32nd St.'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0714'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Model Coverall Services Inc.'
     , '100 28th St. SE'
     , NULL
     , 'Grand Rapids'
     , '49548'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8289'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Advance Plating and Finishing'
     , '840 Cottage Grove St. SE'
     , NULL
     , 'Grand Rapids'
     , '49507'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0718'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Accurate Coating Inc.'
     , '955 Godfrey Ave SW'
     , 'Unit A'
     , 'Grand Rapids'
     , '49503'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0726'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Hearthside Food Solutions LLC'
     , '3061 Shaffer Ave'
     , 'P.O. Box H'
     , 'Kentwood'
     , '49502'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8297'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Yamaha Corporation of America'
     , '3050 Breton Ave SE'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0753'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Ventra Grand Rapids 29, LLC'
     , '2890 29th st. SE'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0755'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Cheeze Kurls'
     , '2915 Walkent Dr. N.W.'
     , NULL
     , 'Grand Rapids'
     , '49544'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8298'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Roskam Broadmoor'
     , '5353 Broadmoor Ave.'
     , 'P.O. 202'
     , 'Grand Rapids'
     , '49501'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8299'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Hearthside Oak Industrial'
     , '2455 Oak Industrial Dr'
     , NULL
     , 'Grand Rapids'
     , '49505'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8300'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Hearthside 44th St.'
     , '4185 44th St.'
     , NULL
     , 'Grand Rapids'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8301'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Roskam 4855 52nd St'
     , '4855 52nd St.'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8304'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Hearthside 3225 32nd St'
     , '3225 32nd St.'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8305'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Roskam 3035 32nd St.'
     , '3035 32nd St.'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8306'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Haviland Products Co (North)'
     , '2168 Avastar Parkway'
     , NULL
     , 'Grand Rapids'
     , '49544'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0760'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Roskam 2600 29th'
     , '2600 29th St.'
     , NULL
     , 'Grand Rapids'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8303'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Advanced Food Technologies'
     , '1140 Butterworth S.W.'
     , NULL
     , 'Grand Rapids'
     , '49504'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8310'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Festida Foods Ltd'
     , '219 Canton SW'
     , NULL
     , 'Grand Rapids'
     , '49507'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8308'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Coatings Plus, Inc.'
     , '675 Chestnut St. SW'
     , NULL
     , 'Grand Rapids'
     , '49503'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0762'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Lacks Enterprises, Inc.'
     , '4245 52nd St.'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0764'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Surefil'
     , '4560 Danvers Dr.'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8309'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Autocam Medical, LLC.'
     , '4162 East Paris Ave, SE'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0767'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Jedco, Inc.'
     , '1615 Broadway, NW'
     , NULL
     , 'Grand Rapids'
     , '49504'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0768'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Allied Finishing Inc'
     , '4100 Broadmoor Ave SE'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0002'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Kent County Waste to Energy Facility'
     , '950 Market Ave SW'
     , NULL
     , 'Grand Rapids'
     , '49503'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0769'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Coca-Cola Refreshments USA, Inc.'
     , '1440 Butterworth'
     , NULL
     , 'Grand Rapids'
     , '49504'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8311'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Ottery Brothers, LLC'
     , '4647 50th st SE'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8312'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Lacks Enterprises, Inc.'
     , '5675 Kraft Ave SE'
     , NULL
     , 'Kentwood'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0770'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Scott Group Custom Carpets, LLC'
     , '3232 Kraft Ave. SE'
     , NULL
     , 'Grand Rapids'
     , '49512'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0771'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Access Business Group LLC'
     , '5101 Spaulding Plaza SE'
     , NULL
     , 'Ada'
     , '49355'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8313'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Butterball Farms, Inc.'
     , '1435 Buchanan SW'
     , NULL
     , 'Grand Rapids'
     , '49507'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8317'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'SET Environmental, Inc.'
     , '1040 Market Ave'
     , NULL
     , 'Grand Rapids'
     , '49503'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0772'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Bimbo Bakeries USA, Inc'
     , '210 28th St. S.E.'
     , NULL
     , 'Grand Rapids'
     , '49548'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8315'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Founders Brewing'
     , '235 Grandville'
     , NULL
     , 'Grand Rapids'
     , '49503'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8316'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Spectrum Health Medical Group Urology Physicians'
     , '4069 Lake Drive SE'
     , 'Suite 313'
     , 'Grand Rapids'
     , '49546'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0773'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Premier Finishing, Inc.'
     , '3682 Northridge Dr NW ste 10'
     , NULL
     , 'Walker'
     , '49544'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '0774'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Bissell Homecare, Inc.'
     , '2345 Walker Ave NW'
     , NULL
     , 'Grand Rapids'
     , '49544'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8318'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Kent Quality Foods'
     , '703 Leonard'
     , NULL
     , 'Grand Rapids'
     , '49504'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , '8319'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Baker Tent Rental'
     , '201 Matilda ST NE'
     , NULL
     , 'Grand Rapids'
     , '49503'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , 'PTWDAHW101'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Chris Test IU'
     , '100 E. 26th Street'
     , NULL
     , 'Grand Rapids'
     , '80000'
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , 'IPP-TEST-01'
 ) 
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
        , PermitNumber
    )
 VALUES 
 (
    @OrganizationTypeId_Industry
     , 'Central Land Fill Test by Chris'
     , ''
     , NULL
     , ''
     , ''
     , @JurisdictionId_MI
     , NULL
     , NULL
     , NULL
     , 'PTX-01330-04'
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
    INSERT INTO dbo.tOrganizationRegulatoryProgram (RegulatoryProgramId, OrganizationId, RegulatorOrganizationId, IsEnabled)
		VALUES 
		(
		    @RegulatoryProgramId_IPP
		    , @OrganizationId_GRESD
		    , NULL
		    , 1
		)

    -- GRESD Industries
    INSERT INTO dbo.tOrganizationRegulatoryProgram (RegulatoryProgramId, OrganizationId, RegulatorOrganizationId, IsEnabled)
	SELECT @RegulatoryProgramId_IPP, OrganizationId, @OrganizationId_GRESD, 1 
    FROM dbo.tOrganization
    WHERE OrganizationTypeId = @OrganizationTypeId_Industry
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
            , ''
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
		    , '6'
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
		    'SuppportEmailAddress'
		    , 'support@linkoexchange.com'
		    , ''
		)
	INSERT INTO dbo.tSystemSetting (Name, Value, Description)
		VALUES 
		(
		    'EmailServer'
		    , 'wtraxadc2.watertrax.local'
		    , ''
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
    INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    ''
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite vacation destination?')
		    , @UserProfileId_Linko
		)
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    ''
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What was your first pet''s name?')
		    , @UserProfileId_Linko
		)
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    ''
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is the name of your home town newspaper?')
		    , @UserProfileId_Linko
		)
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    ''
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite hobby?')
		    , @UserProfileId_Linko
		)
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    ''
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite song?')
		    , @UserProfileId_Linko
		)
	
	-- Linko Support SQs
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    ''
		    , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is the name of your favorite sports team?')
		    , @UserProfileId_Linko
		)
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
		VALUES 
		(
		    ''
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
Hello {userName},
Your LinkoExchange Registration has been denied for the following:
                 
    Authority:  {authorityName} 

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}. 
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
        
        'Registration Denied for {organizationName}({authorityName})',
        '<html>
            <body> 
                <pre>
Hello {userName},
Your LinkoExchange Registration has been denied for the following:
                 
    Authority:  {authorityName}
     Facility:  {organizationName}
                {addressLine1}
                {cityName}, {stateName} 

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}. 
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
          
        'Registration Approved for {organizationName}({authorityName})',
        '<html>
            <body> 
                <pre>
Hello {userName}, 

Your LinkoExchange Registration has been approved for the following: 

    Authority:  {authorityName}
     Facility:  {organizationName}
                {addressLine1}
                {cityName}, {stateName} 
                     
You can log in here: {link} 
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
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
Hello {userName}, 

Your LinkoExchange Registration has been approved for the following: 

    Authority:  {authorityName} 
                     
You can log in here: {link} 
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
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

    {authorityName} at {emailAddress} or {phoneNumber}  

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
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

    Fist Name: {firstName}
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
        ' <html>
            <body> 
                <pre>
Hello,
For security reasons, you must reset your Registration Profile including Knowledge Based Questions and Security Questions.
                     
Please click the link below or copy and paste the link into your web browser.
                      
    {link}
                        
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
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
        
        'Invitation to {authorityName}',
        '<html>
            <body> 
                <pre>
Hello {userName},

You''ve been invited to be a user of LinkoExchange:
                        
    Authority:  {authorityName} 
                     
To accept the invitation, please click here or copy and paste the link below into your web browser.
                      
    {link}
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
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
        
        'Invitation to {organizationName}({authorityName})',
        '<html>
            <body> 
                <pre>
Hello {userName},

You''ve been invited to be a user of LinkoExchange:
                        
    Authority:  {authorityName}
     Facility:  {organizationName}
                {addressLine1}
                {cityName}, {stateName} 
                     
To accept the invitation, please click here or copy and paste the link below into your web browser.
                      
    {link}
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
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
         
        'Signatory Rights Granted for {organizationName}({authorityName})',
        '<html>
            <body> 
                <pre>
Hello {userName},

Your signatory rights have been granted for 

    Authority:  {authorityName}
     Facility:  {organizationName}
                {addressLine1}
                {cityName}, {stateName} 
                     
You are now able to electronically sign report submissions. 
                     
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
        
        'Signatory Rights Revoked for {organizationName}({authorityName})',
        '<html>
            <body> 
                <pre>
Hello {userName},

Your signatory rights have been granted for 

    Authority:  {authorityName}
     Facility:  {organizationName}
                {addressLine1}
                {cityName}, {stateName} 
                     
To accept the invitation, please click here or copy and paste the link below into your web browser. 
                     
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

Your account has been locked due to an incorrect Knowledge Based Question answer attempting to
access your profile.
Please contact your authority for assistance unlocking your account.

    {authorityList}
                           
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
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
Hello {userName},

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
Hello {userName},

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
Hello {userName},

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
Hello {userName},

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
Hello {userName},

For security reasons, we wanted to let you know the password on your LinkoExchange Profile were changed. 

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

For security reasons, you must reset your Registration Profile including Password, Knowledge Based Questions
and Security Questions.

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

        'Registration Pending for {organizationName} ({authorityName})' ,
        '<html>
            <body> 
                <pre>
The following user has registered at LinkoExchange and requires action:

       Registrant:  {firstName} {lastName}
        Authority:  {authorityName}
         Facility:  {organizationName}
                    {addressLine1}
                    {cityName}, {stateName}
                      
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
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

        'Registration Pending for {organizationName} ({authorityName})' ,
        '<html>
            <body> 
                <pre>
The following user has registered at LinkoExchange and requires action:

    Registrant:  {firstName} {lastName}
     Authority:  {authorityName} 
                      
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
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
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
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
        
        'Invitation to {organizationName}({authorityName})',
        '<html>
            <body> 
                <pre>
Hello {userName},

You''ve been invited to be a user of LinkoExchange:
                        
    Authority:  {authorityName}
     Facility:  {organizationName}
                {addressLine1}
                {cityName}, {stateName} 
                     
To accept the invitation, please click here or copy and paste the link below into your web browser.
                      
    {link}
                     
This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
If you have questions or concerns, please contact {authorityName} at {emailAddress} or {phoneNumber}.
                </pre>
            </body>
        </html>'
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

    -- GRESD Industries
    INSERT INTO dbo.tPermissionGroup (Name, Description, OrganizationRegulatoryProgramId)
	SELECT pgt.Name, pgt.Description, orp.OrganizationRegulatoryProgramId
    FROM dbo.tPermissionGroupTemplate pgt
        CROSS JOIN dbo.tOrganizationRegulatoryProgram orp
        INNER JOIN dbo.tOrganizationTypeRegulatoryProgram otrp ON otrp.OrganizationTypeRegulatoryProgramId = pgt.OrganizationTypeRegulatoryProgramId
    WHERE orp.RegulatorOrganizationId = @OrganizationId_GRESD
        AND otrp.OrganizationTypeId = @OrganizationTypeId_Industry
        AND otrp.RegulatoryProgramId = @RegulatoryProgramId_IPP
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganizationRegulatoryProgramUser)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganizationRegulatoryProgramUser'
    PRINT '-------------------------------------------------'
    
    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    @UserProfileId_Linko
		    , (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD)
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = (SELECT OrganizationRegulatoryProgramId FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP AND OrganizationId = @OrganizationId_GRESD))
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1)
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

PRINT CHAR(13)
PRINT '---------------------------'
PRINT 'END OF: LinkoExchange Setup'
PRINT '---------------------------'