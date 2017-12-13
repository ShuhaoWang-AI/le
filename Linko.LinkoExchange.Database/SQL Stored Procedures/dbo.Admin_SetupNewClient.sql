
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'dbo.Admin_SetupNewClient') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE dbo.Admin_SetupNewClient
GO

-- ==========================================================================================
-- Author:		Sundoro S
-- Create date: 2017-06-23
-- Description:	sets up a new client, usually a regulator,
--              and other related info.
-- This script is the first step during the onboarding process.
-- Separate synching process must be done later to bring other
--  related data from LinkoCTS.
-- 
-- Changes:		SS: 20170818 - Added DB user creation for LinkoCTS sync
-- ==========================================================================================
CREATE PROCEDURE Admin_SetupNewClient
    (
		@ExistingServerLoginName		varchar(50)
		, @NewDatabaseUserNameForSync	varchar(50)
	    , @RegulatoryProgramName        varchar(100)
        , @OrganizationTypeName         varchar(100)
        , @OrganizationName             varchar(254)
        , @OrganizationAddressLine1     varchar(100)
        , @OrganizationAddressLine2     varchar(100)
        , @OrganizationCityName         varchar(100)
        , @OrganizationZipCode          varchar(50)
        , @OrganizationJurisdictionCode varchar(100)
        , @OrganizationPhoneNumber      varchar(25)
        , @OrganizationFaxNumber        varchar(25)
        , @OrganizationWebsiteURL       varchar(256)
        , @OrganizationRegulatoryProgramReferenceNumber varchar(50)
    )
AS

-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY

	DECLARE @SQL VARCHAR(4000)
    SET @SQL = 'CREATE USER '+ QUOTENAME(@NewDatabaseUserNameForSync,'[') +' FOR LOGIN '+ QUOTENAME(@ExistingServerLoginName,'[')
	EXEC(@SQL)
    SET @SQL = 'ALTER ROLE [db_ctssync]' +' ADD MEMBER '+ QUOTENAME(@NewDatabaseUserNameForSync,'[')
    EXEC(@SQL)

BEGIN TRANSACTION

    DECLARE @RegulatoryProgramId int
    SELECT @RegulatoryProgramId = RegulatoryProgramId 
    FROM dbo.tRegulatoryProgram 
    WHERE Name = @RegulatoryProgramName;

    DECLARE @OrganizationTypeId int
    SELECT @OrganizationTypeId = OrganizationTypeId 
    FROM dbo.tOrganizationType 
    WHERE Name = @OrganizationTypeName;

    DECLARE @OrganizationJurisdictionId int
    SELECT @OrganizationJurisdictionId = JurisdictionId 
    FROM dbo.tJurisdiction 
    WHERE Code = @OrganizationJurisdictionCode;

    DECLARE @UserProfileId_Linko int
    SELECT @UserProfileId_Linko = UserProfileId 
    FROM dbo.tUserProfile 
    WHERE UserName = 'Linko';


    -- Add record to tOrganization
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
        @OrganizationTypeId
	    , @OrganizationName
	    , @OrganizationAddressLine1
	    , @OrganizationAddressLine2
	    , @OrganizationCityName
	    , @OrganizationZipCode
	    , @OrganizationJurisdictionId
	    , @OrganizationPhoneNumber
	    , @OrganizationFaxNumber
        , @OrganizationWebsiteURL
	); 

    -- Get the last identity value inserted
    DECLARE @OrganizationId int;
    SELECT @OrganizationId = SCOPE_IDENTITY();


    -- Add record to tOrganizationRegulatoryProgram
    INSERT INTO dbo.tOrganizationRegulatoryProgram (RegulatoryProgramId, OrganizationId, RegulatorOrganizationId, ReferenceNumber, IsEnabled)
		VALUES 
		(
		    @RegulatoryProgramId
		    , @OrganizationId
		    , NULL
            , @OrganizationRegulatoryProgramReferenceNumber
		    , 1
		);


    -- Add records to tOrganizationSetting
    INSERT INTO dbo.tOrganizationSetting (OrganizationId, SettingTemplateId, Value)
	SELECT @OrganizationId, SettingTemplateId, DefaultValue
    FROM dbo.tSettingTemplate
    WHERE OrganizationTypeId = @OrganizationTypeId 
        AND RegulatoryProgramId IS NULL;

    -- Add records to tOrganizationRegulatoryProgramSetting
    INSERT INTO dbo.tOrganizationRegulatoryProgramSetting (OrganizationRegulatoryProgramId, SettingTemplateId, Value)
	SELECT OrganizationRegulatoryProgramId, SettingTemplateId, DefaultValue
    FROM dbo.tSettingTemplate st
        INNER JOIN dbo.tOrganizationRegulatoryProgram orp ON orp.RegulatoryProgramId = st.RegulatoryProgramId
    WHERE st.OrganizationTypeId = @OrganizationTypeId 
        AND st.RegulatoryProgramId = @RegulatoryProgramId
        AND orp.OrganizationId = @OrganizationId;

    
    -- Add records to tPermissionGroup
    INSERT INTO dbo.tPermissionGroup (Name, Description, OrganizationRegulatoryProgramId)
	SELECT pgt.Name, pgt.Description, orp.OrganizationRegulatoryProgramId
    FROM dbo.tPermissionGroupTemplate pgt
        CROSS JOIN dbo.tOrganizationRegulatoryProgram orp
        INNER JOIN dbo.tOrganizationTypeRegulatoryProgram otrp ON otrp.OrganizationTypeRegulatoryProgramId = pgt.OrganizationTypeRegulatoryProgramId
    WHERE orp.OrganizationId = @OrganizationId
        AND orp.RegulatoryProgramId = otrp.RegulatoryProgramId
        AND otrp.OrganizationTypeId = @OrganizationTypeId
        AND otrp.RegulatoryProgramId = @RegulatoryProgramId;


    -- Add records to tOrganizationRegulatoryProgramModule
    INSERT INTO dbo.tOrganizationRegulatoryProgramModule (OrganizationRegulatoryProgramId, ModuleId)
	SELECT orp.OrganizationRegulatoryProgramId, m.ModuleId
    FROM dbo.tModule m
        CROSS JOIN dbo.tOrganizationRegulatoryProgram orp
        INNER JOIN dbo.tOrganizationTypeRegulatoryProgram otrp ON otrp.OrganizationTypeRegulatoryProgramId = m.OrganizationTypeRegulatoryProgramId
    WHERE orp.OrganizationId = @OrganizationId
        AND orp.RegulatoryProgramId = otrp.RegulatoryProgramId
        AND otrp.OrganizationTypeId = @OrganizationTypeId
        AND otrp.RegulatoryProgramId = @RegulatoryProgramId;


    -- Add records to tOrganizationRegulatoryProgramUser
    DECLARE @OrganizationRegulatoryProgramId int;
    SELECT @OrganizationRegulatoryProgramId = OrganizationRegulatoryProgramId 
    FROM dbo.tOrganizationRegulatoryProgram 
    WHERE RegulatoryProgramId = @RegulatoryProgramId 
        AND OrganizationId = @OrganizationId;
    
    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    @UserProfileId_Linko
		    , @OrganizationRegulatoryProgramId
		    , @OrganizationRegulatoryProgramId
            , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        );
    

    -- Add 'Samples and Results' to tReportElementType
    INSERT INTO dbo.tReportElementType (Name, Description, ReportElementCategoryId, OrganizationRegulatoryProgramId)
		VALUES 
        (
            'Samples and Results'
            , 'Samples and Results'
            , 1
            , @OrganizationRegulatoryProgramId
        );

COMMIT TRANSACTION

END TRY

BEGIN CATCH

    DECLARE @ErrorMessage nvarchar(4000);
    DECLARE @ErrorSeverity int;
    DECLARE @ErrorState int;
    DECLARE @ErrorProcedure nvarchar(1000);
    DECLARE @ErrorNumber int;
    DECLARE @ErrorLine int;

	--if using XACT_ABORT ON setting, need to check xact_state
	IF (XACT_STATE() = -1) 
    BEGIN 
        ROLLBACK TRAN 
    END


    SELECT @ErrorNumber = ERROR_NUMBER()
        , @ErrorSeverity = ERROR_SEVERITY()
        , @ErrorState = ERROR_STATE()
        , @ErrorProcedure = ERROR_PROCEDURE()
        , @ErrorLine = ERROR_LINE()
        , @ErrorMessage = ERROR_MESSAGE() + ' in LinkoExchange procedure ' + CONVERT(varchar(100), ISNULL(ERROR_PROCEDURE(), '-none-')) + ' at line ' + CONVERT(varchar(5), ERROR_LINE()); 
				
	RAISERROR (@ErrorMessage, @ErrorSeverity, 1)

END CATCH

GO