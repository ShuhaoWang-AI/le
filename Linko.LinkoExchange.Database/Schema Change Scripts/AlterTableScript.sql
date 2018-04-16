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

  PRINT 'END of ALTER TABLE SCRIPT';
