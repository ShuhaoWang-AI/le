PRINT CHAR(13)
PRINT CHAR(13)
PRINT CHAR(13)
PRINT CHAR(13)
PRINT '---------------------------------------'
PRINT 'START OF: LinkoExchange - Phase 2 Setup'
PRINT '---------------------------------------'
PRINT CHAR(13)


-------------------------------- Create new tables for LinkoExchange - Phase 2 --------------------------------
PRINT CHAR(13)
PRINT CHAR(13)
PRINT 'Create new tables for LinkoExchange - Phase 2'

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tUnitDimension') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tUnitDimension'
    PRINT '---------------------'

    CREATE TABLE dbo.tUnitDimension
    (
        UnitDimensionId                 int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(50) NOT NULL  
        , Description                   varchar(500) NULL 
        --, ConversionSystemUnitId        int NULL
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tUnitDimension PRIMARY KEY CLUSTERED 
        (
	        UnitDimensionId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tUnitDimension_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tUnitDimension ADD CONSTRAINT DF_tUnitDimension_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    --CREATE NONCLUSTERED INDEX IX_tUnitDimension_ConversionSystemUnitId ON dbo.tUnitDimension 
    --(
	   -- ConversionSystemUnitId ASC
    --) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tSystemUnit') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tSystemUnit'
    PRINT '------------------'

    CREATE TABLE dbo.tSystemUnit
    (
        SystemUnitId                    int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(50) NOT NULL  
        , Description                   varchar(500) NOT NULL
        , UnitDimensionId               int NOT NULL
        , ConversionFactor              float NOT NULL
        , AdditiveFactor                float NOT NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tSystemUnit PRIMARY KEY CLUSTERED 
        (
	        SystemUnitId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tSystemUnit_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tSystemUnit_tUnitDimension FOREIGN KEY 
		(
			UnitDimensionId
		) REFERENCES dbo.tUnitDimension(UnitDimensionId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tSystemUnit ADD CONSTRAINT DF_tSystemUnit_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    -- This is done here because it requires the existence of tSystemUnit
 --   ALTER TABLE dbo.tUnitDimension ADD CONSTRAINT FK_tUnitDimension_tSystemUnit FOREIGN KEY 
	--(
	--	ConversionSystemUnitId
	--) REFERENCES dbo.tSystemUnit(SystemUnitId)

    CREATE NONCLUSTERED INDEX IX_tSystemUnit_UnitDimensionId ON dbo.tSystemUnit 
    (
	    UnitDimensionId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tDataOptionality') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tDataOptionality'
    PRINT '-----------------------'

    CREATE TABLE dbo.tDataOptionality
    (
        DataOptionalityId               int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(50) NOT NULL  
        , Description                   varchar(500) NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tDataOptionality PRIMARY KEY CLUSTERED 
        (
	        DataOptionalityId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tDataOptionality_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tDataOptionality ADD CONSTRAINT DF_tDataOptionality_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tDataFormat') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tDataFormat'
    PRINT '------------------'

    CREATE TABLE dbo.tDataFormat
    (
        DataFormatId                    int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(50) NOT NULL  
        , Description                   varchar(500) NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tDataFormat PRIMARY KEY CLUSTERED 
        (
	        DataFormatId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tDataFormat_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tDataFormat ADD CONSTRAINT DF_tDataFormat_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tSystemField') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tSystemField'
    PRINT '-------------------'

    CREATE TABLE dbo.tSystemField
    (
        SystemFieldId                   int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(50) NOT NULL  
        , Description                   varchar(500) NULL
        , DataFormatId                  int NOT NULL
        , IsRequired                    bit NOT NULL
        , Size                          int NULL
        , ExampleData                   varchar(500) NULL
        , AdditionalComments            varchar(500) NULL
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tSystemField PRIMARY KEY CLUSTERED 
        (
	        SystemFieldId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tSystemField_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tSystemField_tDataFormat FOREIGN KEY 
		(
			DataFormatId
		) REFERENCES dbo.tDataFormat(DataFormatId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tSystemField ADD CONSTRAINT DF_tSystemField_IsRequired DEFAULT 0 FOR IsRequired
    ALTER TABLE dbo.tSystemField ADD CONSTRAINT DF_tSystemField_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tSystemField_DataFormatId ON dbo.tSystemField 
    (
	    DataFormatId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tFileVersionTemplate') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tFileVersionTemplate'
    PRINT '---------------------------'

    CREATE TABLE dbo.tFileVersionTemplate
    (
        FileVersionTemplateId           int IDENTITY(1,1) NOT NULL  
        , Name                          varchar(50) NOT NULL  
        , Description                   varchar(500) NULL  
        , CreationDateTimeUtc           datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc   datetimeoffset(0) NULL  
        , LastModifierUserId            int NULL  
    
        CONSTRAINT PK_tFileVersionTemplate PRIMARY KEY CLUSTERED 
        (
	        FileVersionTemplateId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tFileVersionTemplate_Name UNIQUE 
        (
            Name ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tFileVersionTemplate ADD CONSTRAINT DF_tFileVersionTemplate_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tFileVersionTemplateField') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tFileVersionTemplateField'
    PRINT '--------------------------------'

    CREATE TABLE dbo.tFileVersionTemplateField
    (
        FileVersionTemplateFieldId      int IDENTITY(1,1) NOT NULL  
        , FileVersionTemplateId         int NOT NULL  
        , SystemFieldId                 int NOT NULL  
    
        CONSTRAINT PK_tFileVersionTemplateField PRIMARY KEY CLUSTERED 
        (
	        FileVersionTemplateFieldId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tFileVersionTemplateField_FileVersionTemplateId_SystemFieldId UNIQUE 
        (
            FileVersionTemplateId ASC
            , SystemFieldId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tFileVersionTemplateField_tFileVersionTemplate FOREIGN KEY 
		(
			FileVersionTemplateId
		) REFERENCES dbo.tFileVersionTemplate(FileVersionTemplateId)
        , CONSTRAINT FK_tFileVersionTemplateField_tSystemField FOREIGN KEY 
		(
			SystemFieldId
		) REFERENCES dbo.tSystemField(SystemFieldId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tFileVersionTemplateField_FileVersionTemplateId ON dbo.tFileVersionTemplateField 
    (
	    FileVersionTemplateId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tFileVersionTemplateField_SystemFieldId ON dbo.tFileVersionTemplateField 
    (
	    SystemFieldId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tFileVersion') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tFileVersion'
    PRINT '-------------------'

    CREATE TABLE dbo.tFileVersion
    (
        FileVersionId                       int IDENTITY(1,1) NOT NULL  
        , Name                              varchar(50) NOT NULL  
        , Description                       varchar(500) NULL
        , OrganizationRegulatoryProgramId   int NOT NULL  
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tFileVersion PRIMARY KEY CLUSTERED 
        (
	        FileVersionId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tFileVersion_Name_OrganizationRegulatoryProgramId UNIQUE 
        (
            Name ASC
            , OrganizationRegulatoryProgramId ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tFileVersion_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tFileVersion ADD CONSTRAINT DF_tFileVersion_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tFileVersion_OrganizationRegulatoryProgramId ON dbo.tFileVersion 
    (
	    OrganizationRegulatoryProgramId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tFileVersionField') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tFileVersionField'
    PRINT '------------------------'

    CREATE TABLE dbo.tFileVersionField
    (
        FileVersionFieldId      int IDENTITY(1,1) NOT NULL  
        , FileVersionId         int NOT NULL  
        , SystemFieldId         int NOT NULL
        , Name                  varchar(50) NOT NULL  
        , Description           varchar(500) NULL
        , DataOptionalityId     int NOT NULL
        , Size                  int NULL
        , ExampleData           varchar(500) NULL
        , AdditionalComments    varchar(500) NULL  
    
        CONSTRAINT PK_tFileVersionField PRIMARY KEY CLUSTERED 
        (
	        FileVersionFieldId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tFileVersionField_FileVersionId_SystemFieldId_Name UNIQUE 
        (
            FileVersionId ASC
            , SystemFieldId ASC
            , Name ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tFileVersionField_tFileVersion FOREIGN KEY 
		(
			FileVersionId
		) REFERENCES dbo.tFileVersion(FileVersionId)
        , CONSTRAINT FK_tFileVersionField_tSystemField FOREIGN KEY 
		(
			SystemFieldId
		) REFERENCES dbo.tSystemField(SystemFieldId)
        , CONSTRAINT FK_tFileVersionField_tDataOptionality FOREIGN KEY 
		(
			DataOptionalityId
		) REFERENCES dbo.tDataOptionality(DataOptionalityId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tFileVersionField_FileVersionId ON dbo.tFileVersionField 
    (
	    FileVersionId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tFileVersionField_SystemFieldId ON dbo.tFileVersionField 
    (
	    SystemFieldId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tFileVersionField_DataOptionalityId ON dbo.tFileVersionField 
    (
	    DataOptionalityId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tImportTempFile') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tImportTempFile'
    PRINT '----------------------'

    CREATE TABLE dbo.tImportTempFile
    (
        ImportTempFileId                    int IDENTITY(1,1) NOT NULL  
        , OriginalName                      varchar(256) NOT NULL  
        , SizeByte                          float NOT NULL
        , MediaType                         varchar(100) NULL
        , FileTypeId                        int NOT NULL
        , OrganizationRegulatoryProgramId   int NOT NULL
        , UploadDateTimeUtc                 datetimeoffset(0) NOT NULL  
        , UploaderUserId                    int NOT NULL  
        , RawFile                           varbinary(max) NOT NULL
    
        CONSTRAINT PK_tImportTempFile PRIMARY KEY CLUSTERED 
        (
	        ImportTempFileId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG3_LOB]
        , CONSTRAINT FK_tImportTempFile_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG3_LOB]
    
    ALTER TABLE dbo.tImportTempFile ADD CONSTRAINT DF_tImportTempFile_UploadDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR UploadDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tImportTempFile_OrganizationRegulatoryProgramId ON dbo.tImportTempFile 
    (
	    OrganizationRegulatoryProgramId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG3_LOB]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tDataSource') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tDataSource'
    PRINT '------------------'

    CREATE TABLE dbo.tDataSource
    (
        DataSourceId                       int IDENTITY(1,1) NOT NULL  
        , Name                              varchar(50) NOT NULL  
        , Description                       varchar(500) NULL
        , OrganizationRegulatoryProgramId   int NOT NULL  
        , CreationDateTimeUtc               datetimeoffset(0) NOT NULL  
        , LastModificationDateTimeUtc       datetimeoffset(0) NULL  
        , LastModifierUserId                int NULL  
    
        CONSTRAINT PK_tDataSource PRIMARY KEY CLUSTERED 
        (
	        DataSourceId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tDataSource_Name_OrganizationRegulatoryProgramId UNIQUE 
        (
            Name ASC
            , OrganizationRegulatoryProgramId ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tDataSource_tOrganizationRegulatoryProgram FOREIGN KEY 
		(
			OrganizationRegulatoryProgramId
		) REFERENCES dbo.tOrganizationRegulatoryProgram(OrganizationRegulatoryProgramId)
    ) ON [LinkoExchange_FG1_Data]
    
    ALTER TABLE dbo.tDataSource ADD CONSTRAINT DF_tDataSource_CreationDateTimeUtc DEFAULT SYSDATETIMEOFFSET() FOR CreationDateTimeUtc

    CREATE NONCLUSTERED INDEX IX_tDataSource_OrganizationRegulatoryProgramId ON dbo.tDataSource 
    (
	    OrganizationRegulatoryProgramId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tDataSourceMonitoringPoint') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tDataSourceMonitoringPoint'
    PRINT '---------------------------------'

    CREATE TABLE dbo.tDataSourceMonitoringPoint
    (
        DataSourceMonitoringPointId     int IDENTITY(1,1) NOT NULL  
        , DataSourceId                  int NOT NULL  
        , DataSourceTerm                varchar(50) NOT NULL
        , MonitoringPointId             int NOT NULL  
    
        CONSTRAINT PK_tDataSourceMonitoringPoint PRIMARY KEY CLUSTERED 
        (
	        DataSourceMonitoringPointId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tDataSourceMonitoringPoint_DataSourceId_DataSourceTerm UNIQUE 
        (
            DataSourceId ASC
            , DataSourceTerm ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tDataSourceMonitoringPoint_tDataSource FOREIGN KEY 
		(
			DataSourceId
		) REFERENCES dbo.tDataSource(DataSourceId)
        , CONSTRAINT FK_tDataSourceMonitoringPoint_tMonitoringPoint FOREIGN KEY 
		(
			MonitoringPointId
		) REFERENCES dbo.tMonitoringPoint(MonitoringPointId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tDataSourceMonitoringPoint_DataSourceId ON dbo.tDataSourceMonitoringPoint 
    (
	    DataSourceId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tDataSourceMonitoringPoint_MonitoringPointId ON dbo.tDataSourceMonitoringPoint 
    (
	    MonitoringPointId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tDataSourceCtsEventType') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tDataSourceCtsEventType'
    PRINT '------------------------------'

    CREATE TABLE dbo.tDataSourceCtsEventType
    (
        DataSourceCtsEventTypeId    int IDENTITY(1,1) NOT NULL  
        , DataSourceId              int NOT NULL  
        , DataSourceTerm            varchar(50) NOT NULL
        , CtsEventTypeId            int NOT NULL  
    
        CONSTRAINT PK_tDataSourceCtsEventType PRIMARY KEY CLUSTERED 
        (
	        DataSourceCtsEventTypeId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tDataSourceCtsEventType_DataSourceId_DataSourceTerm UNIQUE 
        (
            DataSourceId ASC
            , DataSourceTerm ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tDataSourceCtsEventType_tDataSource FOREIGN KEY 
		(
			DataSourceId
		) REFERENCES dbo.tDataSource(DataSourceId)
        , CONSTRAINT FK_tDataSourceCtsEventType_tCtsEventType FOREIGN KEY 
		(
			CtsEventTypeId
		) REFERENCES dbo.tCtsEventType(CtsEventTypeId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tDataSourceCtsEventType_DataSourceId ON dbo.tDataSourceCtsEventType 
    (
	    DataSourceId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tDataSourceCtsEventType_CtsEventTypeId ON dbo.tDataSourceCtsEventType 
    (
	    CtsEventTypeId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tDataSourceParameter') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tDataSourceParameter'
    PRINT '---------------------------'

    CREATE TABLE dbo.tDataSourceParameter
    (
        DataSourceParameterId   int IDENTITY(1,1) NOT NULL  
        , DataSourceId          int NOT NULL  
        , DataSourceTerm        varchar(254) NOT NULL
        , ParameterId           int NOT NULL  
    
        CONSTRAINT PK_tDataSourceParameter PRIMARY KEY CLUSTERED 
        (
	        DataSourceParameterId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tDataSourceParameter_DataSourceId_DataSourceTerm UNIQUE 
        (
            DataSourceId ASC
            , DataSourceTerm ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tDataSourceParameter_tDataSource FOREIGN KEY 
		(
			DataSourceId
		) REFERENCES dbo.tDataSource(DataSourceId)
        , CONSTRAINT FK_tDataSourceParameter_tParameter FOREIGN KEY 
		(
			ParameterId
		) REFERENCES dbo.tParameter(ParameterId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tDataSourceParameter_DataSourceId ON dbo.tDataSourceParameter 
    (
	    DataSourceId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tDataSourceParameter_ParameterId ON dbo.tDataSourceParameter 
    (
	    ParameterId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tDataSourceCollectionMethod') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tDataSourceCollectionMethod'
    PRINT '----------------------------------'

    CREATE TABLE dbo.tDataSourceCollectionMethod
    (
        DataSourceCollectionMethodId    int IDENTITY(1,1) NOT NULL  
        , DataSourceId                  int NOT NULL  
        , DataSourceTerm                varchar(254) NOT NULL
        , CollectionMethodId            int NOT NULL  
    
        CONSTRAINT PK_tDataSourceCollectionMethod PRIMARY KEY CLUSTERED 
        (
	        DataSourceCollectionMethodId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tDataSourceCollectionMethod_DataSourceId_DataSourceTerm UNIQUE 
        (
            DataSourceId ASC
            , DataSourceTerm ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tDataSourceCollectionMethod_tDataSource FOREIGN KEY 
		(
			DataSourceId
		) REFERENCES dbo.tDataSource(DataSourceId)
        , CONSTRAINT FK_tDataSourceCollectionMethod_tCollectionMethod FOREIGN KEY 
		(
			CollectionMethodId
		) REFERENCES dbo.tCollectionMethod(CollectionMethodId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tDataSourceCollectionMethod_DataSourceId ON dbo.tDataSourceCollectionMethod 
    (
	    DataSourceId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tDataSourceCollectionMethod_CollectionMethodId ON dbo.tDataSourceCollectionMethod 
    (
	    CollectionMethodId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM sys.objects WHERE object_id = OBJECT_ID(N'dbo.tDataSourceUnit') AND OBJECTPROPERTY(object_id, N'IsUserTable') = 1)
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Create tDataSourceUnit'
    PRINT '----------------------'

    CREATE TABLE dbo.tDataSourceUnit
    (
        DataSourceUnitId    int IDENTITY(1,1) NOT NULL  
        , DataSourceId      int NOT NULL  
        , DataSourceTerm    varchar(254) NOT NULL
        , UnitId            int NOT NULL  
    
        CONSTRAINT PK_tDataSourceUnit PRIMARY KEY CLUSTERED 
        (
	        DataSourceUnitId ASC
        ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT AK_tDataSourceUnit_DataSourceId_DataSourceTerm UNIQUE 
        (
            DataSourceId ASC
            , DataSourceTerm ASC
        ) WITH FILLFACTOR = 90 ON [LinkoExchange_FG1_Data]
        , CONSTRAINT FK_tDataSourceUnit_tDataSource FOREIGN KEY 
		(
			DataSourceId
		) REFERENCES dbo.tDataSource(DataSourceId)
        , CONSTRAINT FK_tDataSourceUnit_tUnit FOREIGN KEY 
		(
			UnitId
		) REFERENCES dbo.tUnit(UnitId)
    ) ON [LinkoExchange_FG1_Data]
    
    CREATE NONCLUSTERED INDEX IX_tDataSourceUnit_DataSourceId ON dbo.tDataSourceUnit 
    (
	    DataSourceId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]

    CREATE NONCLUSTERED INDEX IX_tDataSourceUnit_UnitId ON dbo.tDataSourceUnit 
    (
	    UnitId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END
GO

IF DB_NAME() = 'LinkoExchange' AND COL_LENGTH('dbo.tUnit', 'SystemUnitId') IS NULL
BEGIN
	PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add columns to tUnit'
    PRINT '--------------------'

    ALTER TABLE dbo.tUnit ADD SystemUnitId int NULL
    ALTER TABLE dbo.tUnit ADD IsAvailableToRegulatee bit NOT NULL CONSTRAINT DF_tUnit_IsAvailableToRegulatee DEFAULT 0
    ALTER TABLE dbo.tUnit ADD IsReviewed bit NOT NULL CONSTRAINT DF_tUnit_IsReviewed DEFAULT 0

    ALTER TABLE dbo.tUnit
    ADD CONSTRAINT FK_tUnit_tSystemUnit FOREIGN KEY
    (
        SystemUnitId
    )
    REFERENCES tSystemUnit (SystemUnitId)

    CREATE NONCLUSTERED INDEX IX_tUnit_SystemUnitId ON dbo.tUnit
    (
	    SystemUnitId ASC
    ) WITH FILLFACTOR = 100 ON [LinkoExchange_FG1_Data]
END


IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tDataFormat)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tDataFormat'
    PRINT '--------------------------'

	INSERT INTO dbo.tDataFormat (Name, Description)
		VALUES ('Text', 'Text')
    INSERT INTO dbo.tDataFormat (Name, Description)
		VALUES ('Float', 'Float')
    INSERT INTO dbo.tDataFormat (Name, Description)
		VALUES ('DateTime', 'Date/Time')
    INSERT INTO dbo.tDataFormat (Name, Description)
		VALUES ('Bit', 'Bit')
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tDataOptionality)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tDataOptionality'
    PRINT '-------------------------------'

	INSERT INTO dbo.tDataOptionality (Name, Description)
		VALUES ('Required', 'Required')
    INSERT INTO dbo.tDataOptionality (Name, Description)
		VALUES ('Optional', 'Optional')
    INSERT INTO dbo.tDataOptionality (Name, Description)
		VALUES ('Recommended', 'Recommended')
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tSystemField)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tSystemField'
    PRINT '---------------------------'

    DECLARE @DataFormatId_Text int
    SELECT @DataFormatId_Text = DataFormatId
    FROM dbo.tDataFormat
    WHERE Name = 'Text'

    DECLARE @DataFormatId_Float int
    SELECT @DataFormatId_Float = DataFormatId
    FROM dbo.tDataFormat
    WHERE Name = 'Float'

    DECLARE @DataFormatId_DateTime int
    SELECT @DataFormatId_DateTime = DataFormatId
    FROM dbo.tDataFormat
    WHERE Name = 'DateTime'

    DECLARE @DataFormatId_Bit int
    SELECT @DataFormatId_Bit = DataFormatId
    FROM dbo.tDataFormat
    WHERE Name = 'Bit'

	INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'MonitoringPoint'
            , 'This identifies the location where the sample was taken.'
            , @DataFormatId_Text
            , 1
            , 25
            , '001'
            , 'See Data Rules tab for more details.

This field is used to identify a unique sample in LinkoExchange.'
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'SampleType'
            , 'The type of sample to be created in LinkoExchange; such IU_SAMPLE, COMPLIANCE_SAMPLE, RESAMPLE.'
            , @DataFormatId_Text
            , 1
            , 25
            , 'COMPLIANCE_SAMPLE'
            , 'See Data Rules tab for more details.

This field is used to identify a unique sample in LinkoExchange.'
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'CollectionMethod'
            , 'The method used for collecting the sample. E.g. Composite or Grab'
            , @DataFormatId_Text
            , 1
            , 25
            , 'COMPOSITE'
            , 'This field is used to identify a unique sample in LinkoExchange.'
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'SampleStartDateTime'
            , 'Date and/or date and time sample collection was started. 
Format should be mm/dd/yyyy hh:mm:ss AM/PM'
            , @DataFormatId_DateTime
            , 1
            , NULL
            , '12/4/2017
-or-
12/4/2017  3:00:00 PM'
            , 'LinkoExchange can accept most U.S. date and datetime formats. The formats shown here are recommended.

This field is used to identify a unique sample in LinkoExchange.'
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'SampleEndDateTime'
            , 'Date and/or date and time sample collection ended. 
Format should be mm/dd/yyyy hh:mm:ss AM/PM'
            , @DataFormatId_DateTime
            , 1
            , NULL
            , '12/5/2017
-or-
12/5/2017  3:00:00 PM'
            , 'LinkoExchange can accept most U.S. date and datetime formats. The formats shown here are recommended.

This field is used to identify a unique sample in LinkoExchange.'
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'ParameterName'
            , 'Analyte/Pollutant compound name'
            , @DataFormatId_Text
            , 1
            , 60
            , 'Copper'
            , NULL
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'ResultQualifier'
            , 'If the result is not detactable, valid values here are ND, >#, <#. The qualifier must match one accepted by the Authority.'
            , @DataFormatId_Text
            , 1
            , 2
            , '<'
            , 'See Valid Result and Result Qualifier Values in Data Rules tab.'
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'Result'
            , 'Numeric value of the result.'
            , @DataFormatId_Float
            , 1
            , NULL
            , '0.1'
            , 'See Valid Result and Result Qualifier Values in Data Rules tab.'
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'ResultUnits'
            , 'Unit of measurement for Result, Method Detection Limit and Method Reporting Limit.'
            , @DataFormatId_Text
            , 1
            , 10
            , 'mg/L'
            , NULL
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'LabSampleID'
            , 'The labs sample identifier, typically the SampleID from LIMS.'
            , @DataFormatId_Text
            , 0
            , 25
            , '20131130-001'
            , 'This field is used to identify a unique sample in LinkoExchange.'
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'MethodDetectionLimit'
            , 'The Method Detection Limit.'
            , @DataFormatId_Float
            , 0
            , 10
            , '0.0005'
            , NULL
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'AnalysisDateTime'
            , 'Date and/or date and time sample was analyzed.'
            , @DataFormatId_DateTime
            , 0
            , NULL
            , '12/11/2017
  - or -
12/11/2017 3:00:00 PM'
            , 'LinkoExchange can accept most U.S. date and datetime formats. The formats shown here are recommended.'
        )

    INSERT INTO dbo.tSystemField (Name, Description, DataFormatId, IsRequired, Size, ExampleData, AdditionalComments)
		VALUES 
        (
            'AnalysisMethod'
            , 'The analysis method used to perform the analysis.'
            , @DataFormatId_Text
            , 0
            , 15
            , 'EPA200.7'
            , NULL
        )
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tFileVersionTemplate)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tFileVersionTemplate'
    PRINT '-----------------------------------'

    -- This is temporary naming until it is approved/changed by Chris
	INSERT INTO dbo.tFileVersionTemplate (Name, Description)
		VALUES ('SampleImport', 'Sample Import')
END


DECLARE @FileVersionTemplateId_SampleImport int
SELECT @FileVersionTemplateId_SampleImport = FileVersionTemplateId
FROM dbo.tFileVersionTemplate
WHERE Name = 'SampleImport'

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tFileVersionTemplateField)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tFileVersionTemplateField'
    PRINT '----------------------------------------'

    -- Sample Import will currently include every field in tSystemField
    INSERT INTO dbo.tFileVersionTemplateField (FileVersionTemplateId, SystemFieldId)
	SELECT @FileVersionTemplateId_SampleImport
        , SystemFieldId 
    FROM dbo.tSystemField 
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tFileVersion)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tFileVersion'
    PRINT '---------------------------'

    DECLARE @RegulatoryProgramId_IPP int
    SELECT @RegulatoryProgramId_IPP = RegulatoryProgramId 
    FROM dbo.tRegulatoryProgram 
    WHERE Name = 'IPP'

    DECLARE @OrganizationTypeId_Authority int
    SELECT @OrganizationTypeId_Authority = OrganizationTypeId 
    FROM dbo.tOrganizationType 
    WHERE Name = 'Authority'

    -- Copy file versions from the templates for existing Authority IPP programs
	INSERT INTO dbo.tFileVersion (Name, Description, OrganizationRegulatoryProgramId)
	SELECT fvt.Name
        , fvt.Description
        , orp.OrganizationRegulatoryProgramId
    FROM dbo.tFileVersionTemplate fvt
        CROSS JOIN dbo.tOrganizationRegulatoryProgram orp
        INNER JOIN dbo.tOrganization o ON o.OrganizationId = orp.OrganizationId
        INNER JOIN dbo.tRegulatoryProgram rp ON rp.RegulatoryProgramId = orp.RegulatoryProgramId
    WHERE o.OrganizationTypeId = @OrganizationTypeId_Authority
        AND rp.RegulatoryProgramId = @RegulatoryProgramId_IPP
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tFileVersionField)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tFileVersionField'
    PRINT '--------------------------------'

    DECLARE @DataOptionalityId_Required int
    SELECT @DataOptionalityId_Required = DataOptionalityId
    FROM dbo.tDataOptionality
    WHERE Name = 'Required'

    DECLARE @DataOptionalityId_Optional int
    SELECT @DataOptionalityId_Optional = DataOptionalityId
    FROM dbo.tDataOptionality
    WHERE Name = 'Optional'

    DECLARE @DataOptionalityId_Recommended int
    SELECT @DataOptionalityId_Recommended = DataOptionalityId
    FROM dbo.tDataOptionality
    WHERE Name = 'Recommended'

    -- Copy system fields from the template fields for the related file versions
    INSERT INTO dbo.tFileVersionField (FileVersionId, SystemFieldId, Name, Description, DataOptionalityId, Size, ExampleData, AdditionalComments)
	SELECT fv.FileVersionId
        , fvtf.SystemFieldId
        , sf.Name
        , sf.Description
        , CASE
            WHEN sf.IsRequired = 1 THEN
                CASE 
                    -- Taken from Data Rules tab in LinkoExchange_SampleDataImportExample_20180131.xlsx
                    -- Recommended fields
                    -- These fields are not required but are strongly recommended to simplify data import.  
                    -- Sample results cannot be imported into LinkoExchange without this data.
                    -- If it is not included in the file, the LinkoExchange user will be prompted to choose this data during the import process.  
                    -- If the data can be included in the file, then the file can contain sample results for multiple Monitoring Points, Collection Methods and Sample Types.  
                    -- If the data cannot be included in the file, then the file must only contain data for a single Monitoring Point, Collection Method and Sample Type.
                    WHEN fv.FileVersionId = @FileVersionTemplateId_SampleImport AND sf.Name = 'MonitoringPoint' THEN 
                        @DataOptionalityId_Recommended
                    WHEN fv.FileVersionId = @FileVersionTemplateId_SampleImport AND sf.Name = 'SampleType' THEN 
                        @DataOptionalityId_Recommended
                    WHEN fv.FileVersionId = @FileVersionTemplateId_SampleImport AND sf.Name = 'CollectionMethod' THEN 
                        @DataOptionalityId_Recommended
                    ELSE
                        @DataOptionalityId_Required
                END
            WHEN sf.IsRequired = 0 THEN 
                @DataOptionalityId_Optional
          END
        , sf.Size
        , sf.ExampleData
        , sf.AdditionalComments
    FROM dbo.tFileVersion fv 
        INNER JOIN dbo.tFileVersionTemplate fvt ON fvt.Name = fv.Name
        INNER JOIN dbo.tFileVersionTemplateField fvtf ON fvtf.FileVersionTemplateId = fvt.FileVersionTemplateId
        INNER JOIN dbo.tSystemField sf ON sf.SystemFieldId = fvtf.SystemFieldId
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tFileVersionTemplateField)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tLimit'
    PRINT '-------------------------------'

    -- Sample Import will currently include every field in tSystemField
    INSERT INTO dbo.tLimitType(Name, Description)
	    VALUES('FourDay', 'Four Day')
    INSERT INTO dbo.tLimitType(Name, Description)
	    VALUES('Monthly', 'Monthly')
END

PRINT CHAR(13)
PRINT '-------------------------------------'
PRINT 'END OF: LinkoExchange - Phase 2 Setup'
PRINT '-------------------------------------'