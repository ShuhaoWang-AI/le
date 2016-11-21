PRINT CHAR(13)
PRINT CHAR(13)
PRINT CHAR(13)
PRINT CHAR(13)
PRINT '---------------------------------------'
PRINT 'START OF: LinkoExchange Test Data Setup'
PRINT '---------------------------------------'
PRINT CHAR(13)

USE [LinkoExchange]
GO

DECLARE @JurisdictionId_MI int
SELECT @JurisdictionId_MI = JurisdictionId 
FROM dbo.tJurisdiction 
WHERE Code = 'MI'


IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tUserProfile WHERE UserName <> 'Linko')
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tUserProfile'
    PRINT '---------------------------'
    
    UPDATE dbo.tUserProfile
    SET Email = 'linkoqa@linkotechnology.com'
    WHERE UserName = 'Linko'

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
        , 'Kurt'
        , 'Anderson'
        , NULL
        , 'Grand Rapids'
        , '1300 Market Ave SW'
        , NULL
        , 'Grand Rapids'
        , '49503'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa1@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-456-3261'
        , 0
        , 0
        , 1
        , 0 
        , 'kanderson'
    )

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
        , 'Paul'
        , 'Kuklewski'
        , NULL
        , 'Grand Rapids'
        , '1300 Market Ave SW'
        , NULL
        , 'Grand Rapids'
        , '49503'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa2@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-125-4554'
        , 0
        , 0
        , 1
        , 0 
        , 'pkuklewski'
    )

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
        , 'David'
        , 'Pelletier'
        , 'Environmental Compliance Officer'
        , 'Valley City Plating'
        , '3353 Eastern SE'
        , NULL
        , 'Grand Rapids'
        , '49508'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa3@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-245-1223'
        , 0
        , 0
        , 1
        , 0 
        , 'dpelletier'
    )

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
        , 'Robert'
        , 'Davis'
        , 'Safety/Environmental Engineer'
        , 'Kerry Sweet Ingredients'
        , '4444 52nd Street SE'
        , NULL
        , 'Kentwood'
        , '49512'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa4@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-871-9299'
        , 0
        , 0
        , 1
        , 0 
        , 'rdavis'
    )

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
        , 'Nayana'
        , 'Bora'
        , 'Chemist'
        , 'Allied Finishing'
        , '4100 Broadmoor SE'
        , NULL
        , 'Kentwood'
        , '49512'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa5@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-871-6123'
        , 0
        , 0
        , 1
        , 0 
        , 'nbora'
    )

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
        , 'Jon'
        , 'Rasche'
        , NULL
        , 'Valley City Plating'
        , '3353 Eastern SE'
        , NULL
        , 'Grand Rapids'
        , '49508'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa6@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-154-5669'
        , 0
        , 0
        , 1
        , 0 
        , 'jrasche'
    )

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
        , 'Brian'
        , 'Skip'
        , NULL
        , 'Kerry Sweet Ingredients'
        , '4444 52nd Street SE'
        , NULL
        , 'Kentwood'
        , '49512'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa7@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-871-9299'
        , 0
        , 0
        , 1
        , 0 
        , 'bskip'
    )

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
        , 'Brad'
        , 'Hirdes'
        , 'Mgr Technical Operations'
        , 'Allied Finishing'
        , '4100 Broadmoor SE'
        , NULL
        , 'Kentwood'
        , '49512'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa8@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-460-0576'
        , 0
        , 0
        , 1
        , 0 
        , 'bhirdes'
    )

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
        , 'Brian'
        , 'Frazier'
        , NULL
        , 'Grand Rapids'
        , '1300 Market Ave SW'
        , NULL
        , 'Grand Rapids'
        , '49503'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa9@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-178-8999'
        , 0
        , 0
        , 1
        , 0 
        , 'bfrazier'
    )

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
        , 'Jim'
        , 'Soper'
        , NULL
        , 'Grand Rapids'
        , '1300 Market Ave SW'
        , NULL
        , 'Grand Rapids'
        , '49503'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa10@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-458-1565'
        , 0
        , 0
        , 1
        , 0 
        , 'jsoper'
    )

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
        , 'Curt'
        , 'Norberg'
        , 'Maintenance Manager'
        , 'Kerry Sweet Ingredients'
        , '4444 52nd Street SE'
        , NULL
        , 'Kentwood'
        , '49512'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa11@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-871-1257'
        , 0
        , 0
        , 1
        , 0 
        , 'cnorberg'
    )

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
        , 'Scott'
        , 'Schnau'
        , 'Process Engineer'
        , 'Allied Finishing'
        , '3353 Eastern SE'
        , NULL
        , 'Kentwood'
        , '49508'
        , @JurisdictionId_MI
        , 1
        , 0
        , 'linkoqa12@linkotechnology.com'
        , 1
        , 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , '8599e4f3-53e5-4ee8-bcc5-907d4f08ee95'
        , '(616)-698-7550'
        , 0
        , 0
        , 1
        , 0 
        , 'sschnau'
    )
END


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


IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganization WHERE OrganizationTypeId = @OrganizationTypeId_Industry)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganization'
    PRINT '----------------------------'
    
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


IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganizationRegulatoryProgram WHERE RegulatorOrganizationId = @OrganizationId_GRESD)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganizationRegulatoryProgram'
    PRINT '---------------------------------------------'
    
    -- GRESD Industries
    -- AssignedTo: TM
    INSERT INTO dbo.tOrganizationRegulatoryProgram (RegulatoryProgramId, OrganizationId, RegulatorOrganizationId, AssignedTo, IsEnabled)
	SELECT @RegulatoryProgramId_IPP, OrganizationId, @OrganizationId_GRESD, 'TM', 1 
    FROM dbo.tOrganization
    WHERE OrganizationTypeId = @OrganizationTypeId_Industry
        AND PermitNumber IN 
        (
            '0006'
            , '0007'
            , '0012'
            , '0013'
            , '0015'
            , '0022'
            , '0025'
            , '0028'
            , '0032'
            , '0040'
            , '0052'
            , '0056'
            , '0062'
            , '0077'
            , '0107'
            , '0136'
            , '0458'
            , '0469'
            , '0479'
            , '0605'
            , '0636'
            , '0649'
            , '0655'
            , '0659'
            , '0660'
            , '0670'
            , '8277'
            , '8283'
            , '8290'
            , '8292'
            , '8293'
            , '0002'
        )

    -- AssignedTo: HB
    INSERT INTO dbo.tOrganizationRegulatoryProgram (RegulatoryProgramId, OrganizationId, RegulatorOrganizationId, AssignedTo, IsEnabled)
	SELECT @RegulatoryProgramId_IPP, OrganizationId, @OrganizationId_GRESD, 'HB', 1 
    FROM dbo.tOrganization
    WHERE OrganizationTypeId = @OrganizationTypeId_Industry
        AND PermitNumber IN 
        (
            '0021'
            , '0508'
            , '0652'
            , '0672'
        )

    -- AssignedTo: KA
    INSERT INTO dbo.tOrganizationRegulatoryProgram (RegulatoryProgramId, OrganizationId, RegulatorOrganizationId, AssignedTo, IsEnabled)
	SELECT @RegulatoryProgramId_IPP, OrganizationId, @OrganizationId_GRESD, 'KA', 1 
    FROM dbo.tOrganization
    WHERE OrganizationTypeId = @OrganizationTypeId_Industry
        AND PermitNumber IN 
        (
            'IPP-TEST-01'
        )

    -- AssignedTo: MB
    INSERT INTO dbo.tOrganizationRegulatoryProgram (RegulatoryProgramId, OrganizationId, RegulatorOrganizationId, AssignedTo, IsEnabled)
	SELECT @RegulatoryProgramId_IPP, OrganizationId, @OrganizationId_GRESD, 'KA', 1 
    FROM dbo.tOrganization
    WHERE OrganizationTypeId = @OrganizationTypeId_Industry
        AND PermitNumber IN 
        (
            '0697'
        )

    -- AssignedTo: PK
    INSERT INTO dbo.tOrganizationRegulatoryProgram (RegulatoryProgramId, OrganizationId, RegulatorOrganizationId, AssignedTo, IsEnabled)
	SELECT @RegulatoryProgramId_IPP, OrganizationId, @OrganizationId_GRESD, 'PK', 1 
    FROM dbo.tOrganization
    WHERE OrganizationTypeId = @OrganizationTypeId_Industry
        AND PermitNumber IN 
        (
            '8315'
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgram (RegulatoryProgramId, OrganizationId, RegulatorOrganizationId, IsEnabled)
	SELECT @RegulatoryProgramId_IPP, OrganizationId, @OrganizationId_GRESD, 1 
    FROM dbo.tOrganization
    WHERE OrganizationTypeId = @OrganizationTypeId_Industry
        AND PermitNumber NOT IN
        (
            '0006'
            , '0007'
            , '0012'
            , '0013'
            , '0015'
            , '0022'
            , '0025'
            , '0028'
            , '0032'
            , '0040'
            , '0052'
            , '0056'
            , '0062'
            , '0077'
            , '0107'
            , '0136'
            , '0458'
            , '0469'
            , '0479'
            , '0605'
            , '0636'
            , '0649'
            , '0655'
            , '0659'
            , '0660'
            , '0670'
            , '8277'
            , '8283'
            , '8290'
            , '8292'
            , '8293'
            , '0002'
            , '0021'
            , '0508'
            , '0652'
            , '0672'
            , 'IPP-TEST-01'
            , '0697'
            , '8315'
        )
END


DECLARE @UserProfileId_Kurt int
SELECT @UserProfileId_Kurt = UserProfileId 
FROM dbo.tUserProfile 
WHERE UserName = 'kanderson'


IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tUserQuestionAnswer WHERE UserProfileId = @UserProfileId_Kurt)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tUserQuestionAnswer'
    PRINT '----------------------------------'
    
    -- KBQs
    -- string: Hash tHiS answer
    INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
	SELECT 'AKuOPfPEFZoHj9FLjgGatC34IIgOfou3ImkGJSew5HNRmJpgHWpG20VkoY/mU0kpVw=='
        , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite TV show?')
		, UserProfileId
	FROM dbo.tUserProfile
    WHERE UserName <> 'Linko'

    -- string: Hash tHiS answer 2
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
	SELECT 'AE+w46NmQpmYTdIShLn6Kt5m97tLl/iaAAMXO5KBm9QaqPRurxHOWxlYHrDcyJO+Tg=='
        , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite book?')
		, UserProfileId
	FROM dbo.tUserProfile
    WHERE UserName <> 'Linko'

    -- string: Hash tHiS answer 3
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
	SELECT 'APkqDsEGZEKJAUkZ/0jxHAPFeYszYflXH8QYTgkRAAQt4BHmugJXGMV+PQXfDwQ47A=='
        , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite hobby?')
		, UserProfileId
	FROM dbo.tUserProfile
    WHERE UserName <> 'Linko'

    -- string: Hash tHiS answer 4
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
	SELECT 'AEAsu7xLtE5tAiJLa6ljvx+INXQMjV2n4Nv2xpTdw7LCUGTZMOjC/SA8UlDVcJCcgw=='
        , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is the last name of your favorite teacher?')
		, UserProfileId
	FROM dbo.tUserProfile
    WHERE UserName <> 'Linko'

    -- string: Hash tHiS answer 5
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
	SELECT 'ANiN5YMCvnaN26T9L1ABz0eBl3jqB2SCvljwFFouLIIW5b2dbFHKHqMF7BJccLmdhA=='
        , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite pet''s name?')
		, UserProfileId
	FROM dbo.tUserProfile
    WHERE UserName <> 'Linko'
	
	-- SQs
    -- string: Test answer
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
	SELECT 'bTnDtaQz5oHY/cbKJbvAOLjvCEegDzjjRGIpIoJlOZ63oRl/Qa5qf4iMTZnoFm3GhKC20ZQn0HP6uO22EaIYPaW55QmdDG3U/VxbJLZQF3jRiwEtHPuNk8+OiKZeLFGZ'
        , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite color?')
		, UserProfileId
	FROM dbo.tUserProfile
    WHERE UserName <> 'Linko'

    -- string: Test answer
	INSERT INTO dbo.tUserQuestionAnswer (Content, QuestionId, UserProfileId)
	SELECT 'bTnDtaQz5oHY/cbKJbvAOLjvCEegDzjjRGIpIoJlOZ63oRl/Qa5qf4iMTZnoFm3GhKC20ZQn0HP6uO22EaIYPaW55QmdDG3U/VxbJLZQF3jRiwEtHPuNk8+OiKZeLFGZ'
        , (SELECT QuestionID FROM dbo.tQuestion WHERE Content = 'What is your favorite food?')
		, UserProfileId
	FROM dbo.tUserProfile
    WHERE UserName <> 'Linko'
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tUserPasswordHistory WHERE UserProfileId = @UserProfileId_Kurt)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tUserPasswordHistory'
    PRINT '-----------------------------------'
    
    INSERT INTO dbo.tUserPasswordHistory (PasswordHash, UserProfileId)
	SELECT 'AOrF2kSIYL8ig2Dw6gVncnX+YkYvoGRdg6lYySQo+CU1HYWBphkgsQ8Imf1Ga+GTUA=='
        , UserProfileId
    FROM dbo.tUserProfile
    WHERE UserName <> 'Linko'
END


DECLARE @OrganizationRegulatoryProgramId_GRESD int
SELECT @OrganizationRegulatoryProgramId_GRESD = OrganizationRegulatoryProgramId
FROM dbo.tOrganizationRegulatoryProgram
WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP
    AND OrganizationId = @OrganizationId_GRESD


DECLARE @OrganizationRegulatoryProgramId_Valley int
SELECT @OrganizationRegulatoryProgramId_Valley = orp.OrganizationRegulatoryProgramId
FROM dbo.tOrganizationRegulatoryProgram orp
    INNER JOIN dbo.tOrganization o ON o.OrganizationId = orp.OrganizationId
WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP
    AND o.Name = 'Valley City Plating'


DECLARE @OrganizationRegulatoryProgramId_Kerry int
SELECT @OrganizationRegulatoryProgramId_Kerry = orp.OrganizationRegulatoryProgramId
FROM dbo.tOrganizationRegulatoryProgram orp
    INNER JOIN dbo.tOrganization o ON o.OrganizationId = orp.OrganizationId
WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP
    AND o.Name = 'Kerry, Inc.'


DECLARE @OrganizationRegulatoryProgramId_Allied int
SELECT @OrganizationRegulatoryProgramId_Allied = orp.OrganizationRegulatoryProgramId
FROM dbo.tOrganizationRegulatoryProgram orp
    INNER JOIN dbo.tOrganization o ON o.OrganizationId = orp.OrganizationId
WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP
    AND o.Name = 'Allied Finishing Inc'


IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tPermissionGroup WHERE OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Valley)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tPermissionGroup'
    PRINT '-------------------------------'
    
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

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganizationRegulatoryProgramUser WHERE OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Valley)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganizationRegulatoryProgramUser'
    PRINT '-------------------------------------------------'
    
    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'Linko')
		    , @OrganizationRegulatoryProgramId_Valley
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Valley)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'kanderson')
		    , @OrganizationRegulatoryProgramId_GRESD
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_GRESD)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'pkuklewski')
		    , @OrganizationRegulatoryProgramId_GRESD
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Standard' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_GRESD)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'dpelletier')
		    , @OrganizationRegulatoryProgramId_Valley
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Valley)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'rdavis')
		    , @OrganizationRegulatoryProgramId_Kerry
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Kerry)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'nbora')
		    , @OrganizationRegulatoryProgramId_Allied
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Allied)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'jrasche')
		    , @OrganizationRegulatoryProgramId_Valley
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Standard' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Valley)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'bskip')
		    , @OrganizationRegulatoryProgramId_Kerry
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Standard' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Kerry)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'bhirdes')
		    , @OrganizationRegulatoryProgramId_Allied
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Standard' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Allied)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'bfrazier')
		    , @OrganizationRegulatoryProgramId_GRESD
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_GRESD)
		    , SYSDATETIMEOFFSET()
		    , 0
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'jsoper')
		    , @OrganizationRegulatoryProgramId_GRESD
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Standard' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_GRESD)
		    , SYSDATETIMEOFFSET()
		    , 0
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'cnorberg')
		    , @OrganizationRegulatoryProgramId_Kerry
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Kerry)
		    , SYSDATETIMEOFFSET()
		    , 0
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'sschnau')
		    , @OrganizationRegulatoryProgramId_Allied
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Standard' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Allied)
		    , SYSDATETIMEOFFSET()
		    , 0
		    , 1
        )
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tInvitation)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tInvitation'
    PRINT '--------------------------'
    
    INSERT INTO dbo.tInvitation (InvitationId, FirstName, LastName, EmailAddress, InvitationDateTimeUtc, SenderOrganizationRegulatoryProgramId, RecipientOrganizationRegulatoryProgramId)
		VALUES 
		(
            NEWID()
		    , 'Marc'
            , 'Barton'
            , 'linkoqa13@linkotechnology.com'
            , SYSDATETIMEOFFSET()
            , @OrganizationRegulatoryProgramId_GRESD
            , @OrganizationRegulatoryProgramId_GRESD
        )

    INSERT INTO dbo.tInvitation (InvitationId,FirstName, LastName, EmailAddress, InvitationDateTimeUtc, SenderOrganizationRegulatoryProgramId, RecipientOrganizationRegulatoryProgramId)
		VALUES 
		(
		    NEWID()
		    , 'Sara'
            , 'Youngman'
            , 'linkoqa14@linkotechnology.com'
            , SYSDATETIMEOFFSET()
            , @OrganizationRegulatoryProgramId_GRESD
            , @OrganizationRegulatoryProgramId_GRESD
        )

    INSERT INTO dbo.tInvitation (InvitationId, FirstName, LastName, EmailAddress, InvitationDateTimeUtc, SenderOrganizationRegulatoryProgramId, RecipientOrganizationRegulatoryProgramId)
		VALUES 
		(
		    NEWID()
		    , 'Bruce'
            , 'Stone'
            , 'linkoqa15@linkotechnology.com'
            , SYSDATETIMEOFFSET()
            , @OrganizationRegulatoryProgramId_Allied
            , @OrganizationRegulatoryProgramId_Allied
        )
END


PRINT CHAR(13)
PRINT '-------------------------------------'
PRINT 'END OF: LinkoExchange Test Data Setup'
PRINT '-------------------------------------'