PRINT 'START of ALTER TABLE SCRIPT'

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

-- change script to add new org reg program setting for "ComplianceDeterminationDate"

IF EXISTS(SELECT 1 FROM dbo.tSettingTemplate 
          WHERE Name = N'ComplianceDeterminationDate')
BEGIN
    -- Setting exists
	PRINT 'ComplianceDeterminationDate setting already exists in dbo.tSettingTemplate'
END
ELSE
BEGIN
	-- Setting does not exist. Need to create.

	PRINT 'ComplianceDeterminationDate setting does not exist in dbo.tSettingTemplate. Creating now...'

	DECLARE @RegulatoryProgramId_IPP int
	SELECT @RegulatoryProgramId_IPP = RegulatoryProgramId 
	FROM dbo.tRegulatoryProgram 
	WHERE Name = 'IPP'

	DECLARE @OrganizationTypeId_Authority int
	SELECT @OrganizationTypeId_Authority = OrganizationTypeId 
	FROM dbo.tOrganizationType 
	WHERE Name = 'Authority'

	INSERT INTO dbo.tSettingTemplate (Name, Description, DefaultValue, OrganizationTypeId, RegulatoryProgramId)
			VALUES 
			(
				'ComplianceDeterminationDate'
				, 'Use Start or End Date Sampled when calculating compliance and determining what parameters to include in a compliance period'
				, 'EndDateSampled'
				, @OrganizationTypeId_Authority
				, @RegulatoryProgramId_IPP
			)
END

-- change email template for CromerrEvent_Profile_KBQChanged (bug 6012)

 UPDATE [DBO].[tAuditLogTemplate]
 SET MessageTemplate = 'Knowledge Based Questions
User: {firstName} {lastName}
User Name: {userName}
Email: {emailAddress}'
WHERE Name='CromerrEvent_Profile_KBQChanged'


-- Wording change in Account Lockout email templates - "LinkoExchange Technology" -> "Linko Technology" (Bug 6076)

UPDATE [dbo].[tAuditLogTemplate]
SET [MessageTemplate] = REPLACE([MessageTemplate], 'LinkoExchange Technology', 'Linko Technology')
WHERE [MessageTemplate] LIKE '%LinkoExchange Technology%'

PRINT 'END of ALTER TABLE SCRIPT';