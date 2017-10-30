PRINT 'START of ALTER TABLE SCRIPT'

-- change email template for lock manual lock

 UPDATE [DBO].[tAuditLogTemplate]
 SET MessageTemplate = '<html>
            <body> 
                <pre>
Hello,

For security reasons, your account has been locked by the Authority. Please contact your Authority for assistance unlocking your account.

    {authorityName} at {authoritySupportEmail} or {authoritySupportPhoneNumber}  

This email was sent from an unmonitored account. Do not reply to this email because it will not be received.
                 </pre>
            </body>
        </html>'

 WHERE NAME = 'Email_UserAccess_AccountLockout'  

 -- change email template for Email_Report_Submission_AU (bug 4382)

 UPDATE [DBO].[tAuditLogTemplate]
 SET MessageTemplate = '<html>
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

 WHERE NAME = 'Email_Report_Submission_AU'  

-- change script for bug 5463 to added IsIncluded column to "tReportPackageElementType" table
IF EXISTS(SELECT 1 FROM sys.columns 
          WHERE Name = N'IsIncluded'
          AND Object_ID = Object_ID(N'dbo.tReportPackageElementType'))
BEGIN
    -- Column exists
	PRINT 'IsIncluded column already exists in dbo.tReportPackageElementType'
END
ELSE
BEGIN
    -- Column does not exists
	PRINT 'IsIncluded column does not exist in dbo.tReportPackageElementTypeDoes. Adding now...'

	ALTER TABLE dbo.tReportPackageElementType
	ADD IsIncluded bit NOT NULL;

	ALTER TABLE dbo.tReportPackageElementType 
	ADD CONSTRAINT DF_tReportPackageElementType_IsIncluded DEFAULT 0 FOR IsIncluded

END



PRINT 'END of ALTER TABLE SCRIPT';