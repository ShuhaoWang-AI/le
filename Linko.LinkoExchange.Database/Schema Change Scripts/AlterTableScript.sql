PRINT 'START of ALTER TABLE SCRIPT'
 -- Scripts for 2018.2.0 

 -- START: Bug 6275 - Update email templates
 
IF EXISTS (SELECT * FROM tAuditLogTemplate WHERE MessageTemplate LIKE '%{authorityName} at {supportEmail%' OR MessageTemplate LIKE '%{authorityName} at {emailAddress%')
BEGIN
	UPDATE tAuditLogTemplate
	SET MessageTemplate = REPLACE(at.MessageTemplate, '{supportEmail}','{authoritySupportEmail}')
	FROM tAuditLogTemplate at  
	WHERE Name in ( 'Email_Registration_AuthorityRegistrationDenied',
					'Email_Registration_IndustryRegistrationDenied',
					'Email_Registration_IndustryRegistrationApproved',
					'Email_Registration_AuthorityRegistrationApproved',
					'Email_Registration_ResetRequired',
					'Email_Registration_InviteAuthorityUser',
					'Email_Registration_AuthorityInviteIndustryUser',
					'Email_Signature_SignatoryGranted',
					'Email_Signature_SignatoryRevoked',
					'Email_Registration_IndustryUserRegistrationPendingToApprovers',
					'Email_Registration_AuthorityUserRegistrationPendingToApprovers',
					'Email_Registration_RegistrationResetPending',
					'Email_Registration_IndustryInviteIndustryUser',
					'Email_Report_Submission_IU',
					'Email_Report_Repudiation_IU'
	)

	UPDATE tAuditLogTemplate
	SET MessageTemplate = REPLACE(at.MessageTemplate, '{emailAddress}','{authoritySupportEmail}')
	FROM tAuditLogTemplate at  
	WHERE Name in ( 'Email_Signature_SignatoryGrantedToAdmin',
					'Email_Signature_SignatoryRevokedToAdmin'
	)
		
	UPDATE tAuditLogTemplate
	SET MessageTemplate = REPLACE(at.MessageTemplate, '{supportPhoneNumber}','{authoritySupportPhoneNumber}')
	FROM tAuditLogTemplate at  
	WHERE Name in ( 'Email_Registration_AuthorityRegistrationDenied',
					'Email_Registration_IndustryRegistrationDenied',
					'Email_Registration_IndustryRegistrationApproved',
					'Email_Registration_AuthorityRegistrationApproved',
					'Email_Registration_ResetRequired',
					'Email_Registration_InviteAuthorityUser',
					'Email_Registration_AuthorityInviteIndustryUser',
					'Email_Signature_SignatoryGranted',
					'Email_Signature_SignatoryRevoked',
					'Email_Registration_IndustryUserRegistrationPendingToApprovers',
					'Email_Registration_AuthorityUserRegistrationPendingToApprovers',
					'Email_Registration_RegistrationResetPending',
					'Email_Registration_IndustryInviteIndustryUser',
					'Email_Report_Submission_IU',
					'Email_Report_Repudiation_IU'
	)
		
	UPDATE tAuditLogTemplate
	SET MessageTemplate = REPLACE(at.MessageTemplate, '{phoneNumber}','{authoritySupportPhoneNumber}')
	FROM tAuditLogTemplate at  
	WHERE Name in ( 'Email_Signature_SignatoryGrantedToAdmin',
					'Email_Signature_SignatoryRevokedToAdmin'
	)
END

 -- END: Bug 6275 - Update email templates

 
-- START: User Story 8206:Authority Portal - Add new setting to save default Report Package Element Attachment
 
 IF NOT EXISTS (SELECT st.SettingTemplateId FROM tSettingTemplate st WHERE st.Name = 'ReportElementTypeIdForIndustryFileUpload')
 BEGIN
    INSERT INTO [dbo].[tSettingTemplate]
               ([Name]
               ,[Description]
               ,[DefaultValue]
               ,[OrganizationTypeId]
               ,[RegulatoryProgramId])
         VALUES
               ('ReportElementTypeIdForIndustryFileUpload'
               ,'Attachment Type for Industry File Upload'
               ,''
               ,1
               ,1)
END

IF NOT EXISTS(
    SELECT 1 FROM [tOrganizationRegulatoryProgramSetting] orps 
    WHERE orps.SettingTemplateId = (SELECT st.SettingTemplateId FROM tSettingTemplate st WHERE st.Name = 'ReportElementTypeIdForIndustryFileUpload') 
)
BEGIN
    INSERT INTO [dbo].[tOrganizationRegulatoryProgramSetting] ([OrganizationRegulatoryProgramId], [SettingTemplateId], [Value]) 
    SELECT  t.OrganizationRegulatoryProgramId, st.SettingTemplateId, t.ReportElementTypeId
    FROM (
       SELECT *, ROW_NUMBER() OVER (PARTITION BY OrganizationRegulatoryProgramId ORDER BY ReportElementTypeId) AS rowNumber
       FROM tReportElementType ret
       WHERE ret.ReportElementCategoryId = 2 
    ) t
    JOIN tSettingTemplate st on st.Name = 'ReportElementTypeIdForIndustryFileUpload'
    WHERE t.rowNumber = 1
END

GO
-- END: User Story 8206:Authority Portal - Add new setting to save default Report Package Element Attachment


-- START: ADD SYSTEM UNITS
-- adding the store procedure here also as need to run before followed by codes
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM [dbo].[sysobjects] WHERE id = OBJECT_ID(N'dbo.Admin_AddSystemUnit') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[Admin_AddSystemUnit]
GO

-- =============================================
-- Author:		Rajeeb Saha
-- Create date: 2018-05-23
-- Description:	Add new system unit
-- =============================================
CREATE PROCEDURE Admin_AddSystemUnit
	(
		@UnitName varchar(50),
		@Description varchar(500),
		@UnitDimensionName varchar(50),
		@ConversionFactor float,
		@AdditiveFactor float
	)
AS
-- SET NOCOUNT ON added to prevent extra result sets from
-- interfering with SELECT statements.
SET NOCOUNT ON;
	
BEGIN TRY
	BEGIN TRANSACTION
	
	IF NOT EXISTS ( SELECT TOP 1 * FROM [dbo].[tUnitDimension] WHERE Name = @UnitDimensionName)
	BEGIN
		INSERT INTO [dbo].[tUnitDimension] ([Name],[Description]) VALUES(@UnitDimensionName, @Description )
	END

	DECLARE @UnitDimensionId int
    SELECT @UnitDimensionId = UnitDimensionId 
    FROM [dbo].[tUnitDimension]  
    WHERE Name LIKE @UnitDimensionName;

	IF NOT EXISTS ( SELECT TOP 1 * FROM [dbo].[tSystemUnit] WHERE Name = @UnitName)
	BEGIN
		INSERT INTO [dbo].[tSystemUnit]
				   (
						[Name]
					   ,[Description]
					   ,[UnitDimensionId]
					   ,[ConversionFactor]
					   ,[AdditiveFactor]
				   )
			 VALUES
				   (
					    @UnitName
					   ,@Description
					   ,@UnitDimensionId
					   ,@ConversionFactor
					   ,@AdditiveFactor
				   )
	END

	PRINT SCOPE_IDENTITY();
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

EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'ppm', @Description = N'parts per million', @UnitDimensionName = N'Concentration', @ConversionFactor = 0.001, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'%', @Description = N'percent', @UnitDimensionName = N'Concentration', @ConversionFactor = 1, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'mg/gal', @Description = N'milligrams per gallon', @UnitDimensionName = N'Concentration', @ConversionFactor = 0.0002641721, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'ppb', @Description = N'parts per billion', @UnitDimensionName = N'Concentration', @ConversionFactor = 1E-06, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'ug/L', @Description = N'micrograms per liter', @UnitDimensionName = N'Concentration', @ConversionFactor = 1E-06, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'ng/L', @Description = N'nanograms per liter', @UnitDimensionName = N'Concentration', @ConversionFactor = 1E-09, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'mg/L', @Description = N'milligrams per liter', @UnitDimensionName = N'Concentration', @ConversionFactor = 0.001, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'mg/kg', @Description = N'milligrams per kilogram', @UnitDimensionName = N'Concentration', @ConversionFactor = 0.001, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'ug/kg', @Description = N'micrograms per kilogram', @UnitDimensionName = N'Concentration', @ConversionFactor = 1E-06, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'S.U.', @Description = N'Standard pH Units', @UnitDimensionName = N'Dimensionless', @ConversionFactor = 1, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'kg/day', @Description = N'kilograms per day', @UnitDimensionName = N'Flow Rate (Mass Basis)', @ConversionFactor = 11.574074, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'lb/day', @Description = N'pounds per day', @UnitDimensionName = N'Flow Rate (Mass Basis)', @ConversionFactor = 5.2499117, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'1000 gpd', @Description = N'thousand gallons per day', @UnitDimensionName = N'Flow Rate (Volume Basis)', @ConversionFactor = 3.785412, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'gpd', @Description = N'gallons per day', @UnitDimensionName = N'Flow Rate (Volume Basis)', @ConversionFactor = 0.0037854118, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'MGD', @Description = N'million gallons per day', @UnitDimensionName = N'Flow Rate (Volume Basis)', @ConversionFactor = 3785.4118, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'gpm', @Description = N'gallon per minute', @UnitDimensionName = N'Flow Rate (Volume Basis)', @ConversionFactor = 5.450992992, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'MPN/100ml', @Description = N'Most Probable Number per 100 milliliters', @UnitDimensionName = N'Microbiological Concentration', @ConversionFactor = 1, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'degrees F', @Description = N'degrees Fahrenheit', @UnitDimensionName = N'Temperature', @ConversionFactor = 1, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'degrees C', @Description = N'degrees Celsius', @UnitDimensionName = N'Temperature', @ConversionFactor = 1.8, @AdditiveFactor = 32
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'ft³', @Description = N'cubic foot', @UnitDimensionName = N'Volume', @ConversionFactor = 0.02831685, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'gal', @Description = N'gallons', @UnitDimensionName = N'Volume', @ConversionFactor = 0.003785411784, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'L', @Description = N'liter', @UnitDimensionName = N'Volume', @ConversionFactor = 0.001, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'1000 ft³', @Description = N'thousand cubic feet', @UnitDimensionName = N'Volume', @ConversionFactor = 28.32058907, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'mgal', @Description = N'million gallons', @UnitDimensionName = N'Volume', @ConversionFactor = 3785.411784, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'1000 gal', @Description = N'thousand gallons', @UnitDimensionName = N'Volume', @ConversionFactor = 3.785411784, @AdditiveFactor = 0
EXEC [dbo].[Admin_AddSystemUnit] @UnitName = N'mg', @Description = N'milligram', @UnitDimensionName = N'Weight (Mass)', @ConversionFactor = 1E-06, @AdditiveFactor = 0


-- SELECT * FROM [tSystemUnit]
GO

-- END: ADD SYSTEM UNITS


PRINT 'END of ALTER TABLE SCRIPT';
