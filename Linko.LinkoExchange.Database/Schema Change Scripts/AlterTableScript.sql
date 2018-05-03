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

-- END: User Story 8206:Authority Portal - Add new setting to save default Report Package Element Attachment

  PRINT 'END of ALTER TABLE SCRIPT';
