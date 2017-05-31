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


UPDATE dbo.tSystemSetting
SET Value = 'linkoqa@linkotechnology.com'
WHERE Name = 'SupportEmailAddress'

UPDATE dbo.tUserProfile
SET Email = 'linkoqa@linkotechnology.com'
WHERE UserName = 'Linko'


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
        , 1
        , SYSDATETIMEOFFSET()
        , 'linkoqa01@linkotechnology.com'
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
        , 1
        , SYSDATETIMEOFFSET()
        , 'linkoqa02@linkotechnology.com'
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
        , 1
        , SYSDATETIMEOFFSET()
        , 'linkoqa03@linkotechnology.com'
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
        , 1
        , SYSDATETIMEOFFSET()
        , 'linkoqa04@linkotechnology.com'
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
        , 1
        , SYSDATETIMEOFFSET()
        , 'linkoqa05@linkotechnology.com'
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
        , 1
        , SYSDATETIMEOFFSET()
        , 'linkoqa06@linkotechnology.com'
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
        , 1
        , SYSDATETIMEOFFSET()
        , 'linkoqa07@linkotechnology.com'
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
        , 1
        , SYSDATETIMEOFFSET()
        , 'linkoqa08@linkotechnology.com'
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
        , 1
        , SYSDATETIMEOFFSET()
        , 'linkoqa09@linkotechnology.com'
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
        , 1
        , SYSDATETIMEOFFSET()
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
        , 1
        , SYSDATETIMEOFFSET()
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
        , 1
        , SYSDATETIMEOFFSET()
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

-- Get the LE account
DECLARE @LEAccountID int
SET @LEAccountID = 1;

-- Get the last time sync was done
DECLARE @LELastSyncDateTimeUtc datetime
SET @LELastSyncDateTimeUtc = '2017-01-01 00:00:00'

-- Get the Linko user id
DECLARE @LELinkoUserProfileId int
SELECT @LELinkoUserProfileId = UserProfileId
FROM LinkoExchange.dbo.tUserProfile
WHERE UserName = 'Linko';

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tOrganization WHERE OrganizationTypeId = @OrganizationTypeId_Industry)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tOrganization'
    PRINT '----------------------------'
    
-- GRESD Industries
-- Sync: tOrganization AND ts_LE_CTSOrganizations

    -- Create temp table to hold merge rslts
    CREATE TABLE #OrganizationMergeResult
    (
        Action      nvarchar(10) NOT NULL
        , CTSId     int NOT NULL
        , LEId      int NOT NULL
    );

    -- Merge LE table with staging table
    WITH src 
    AS
    (
        SELECT ot.OrganizationTypeId AS LEOrganizationTypeID
                , j.JurisdictionId AS LEJurisdictionID
                , stgo.*
        FROM dbo.ts_LE_CTSOrganizations stgo
            INNER JOIN LinkoExchange.dbo.tOrganizationType ot ON ot.Name = stgo.CTSOrgType
            LEFT JOIN LinkoExchange.dbo.tJurisdiction j ON j.Code = stgo.CTSSiteState
        WHERE stgo.LEOrganizationRegulatoryProgramID = @LEAccountID
    )
    MERGE LinkoExchange.dbo.tOrganization AS tgt
    USING src
    ON (tgt.OrganizationId = src.LEOrganizationID)
    WHEN MATCHED AND (src.LastUpdatedDateTimeUtc > @LELastSyncDateTimeUtc OR src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc) THEN
        -- Update or soft-delete
        UPDATE 
        SET tgt.OrganizationTypeId              = src.LEOrganizationTypeID
            , tgt.Name                          = src.CTSPermittee
            , tgt.AddressLine1                  = src.CTSSiteAddr1
            , tgt.AddressLine2                  = src.CTSSiteAddr2
            , tgt.CityName                      = src.CTSSiteCity
            , tgt.ZipCode                       = src.CTSSiteZipCode
            , tgt.JurisdictionId                = src.LEJurisdictionID
            , tgt.Classification                = src.CTSClassCode
            , tgt.LastModificationDateTimeUtc   = SYSDATETIMEOFFSET()
            , tgt.LastModifierUserId            = @LELinkoUserProfileId
    WHEN NOT MATCHED AND src.InsertedDateTimeUtc > @LELastSyncDateTimeUtc THEN
        -- New
        INSERT  (OrganizationTypeId
                , Name
                , AddressLine1
                , AddressLine2
                , CityName
                , ZipCode
                , JurisdictionId
                , Classification
                , LastModifierUserId)
        VALUES 
                (src.LEOrganizationTypeID
                , src.CTSPermittee
                , src.CTSSiteAddr1
                , src.CTSSiteAddr2
                , src.CTSSiteCity
                , src.CTSSiteZipCode
                , src.LEJurisdictionID
                , src.CTSClassCode
                , @LELinkoUserProfileId)
    OUTPUT $action 
            , src.CTSPermitID
            , INSERTED.OrganizationId
    INTO #OrganizationMergeResult;

    -- Update the staging table with the new LE table Ids
    UPDATE stg
    SET LEOrganizationID = rslt.LEId
    FROM #OrganizationMergeResult rslt
        INNER JOIN dbo.ts_LE_CTSOrganizations stg ON stg.CTSPermitID = rslt.CTSId
    WHERE rslt.Action = 'INSERT';

    DROP TABLE #OrganizationMergeResult;

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
    
    -- Sync: tOrganizationRegulatoryProgram AND ts_LE_CTSOrganizations

    -- Create temp table to hold merge rslts
    CREATE TABLE #OrganizationRegulatoryProgramMergeResult
    (
        Action      nvarchar(10) NOT NULL
        , CTSId     int NOT NULL
        , LEId      int NOT NULL
    );

    -- Merge LE table with staging table
    WITH src 
    AS
    (
        SELECT  authorp.OrganizationId AS LERegulatorOrganizationID
                , rp.RegulatoryProgramId AS LERegulatoryProgramID
                , stgo.*
        FROM dbo.ts_LE_CTSOrganizations stgo
            INNER JOIN LinkoExchange.dbo.tRegulatoryProgram rp ON rp.Name = stgo.CTSRegulatoryProgram
            INNER JOIN LinkoExchange.dbo.tOrganizationRegulatoryProgram authorp ON authorp.OrganizationRegulatoryProgramId = stgo.LEOrganizationRegulatoryProgramID
        WHERE stgo.LEOrganizationRegulatoryProgramID = @LEAccountID
    )
    MERGE LinkoExchange.dbo.tOrganizationRegulatoryProgram WITH (HOLDLOCK) AS tgt
    USING src
    ON (tgt.OrganizationId = src.LEOrganizationID AND tgt.RegulatoryProgramId = src.LERegulatoryProgramID)
    WHEN MATCHED AND (src.LastUpdatedDateTimeUtc > @LELastSyncDateTimeUtc OR src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc) THEN
        -- Update or soft-delete
        UPDATE 
        SET tgt.RegulatoryProgramId             = src.LERegulatoryProgramID
            , tgt.OrganizationId                = src.LEOrganizationID
            , tgt.RegulatorOrganizationId       = src.LERegulatorOrganizationID
            , tgt.AssignedTo                    = src.CTSUserAbbrv
            , tgt.ReferenceNumber               = src.CTSPermitNo
            , tgt.IsEnabled                     = ~src.CTSRemoved
            , tgt.IsRemoved                     = IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
            , tgt.LastModificationDateTimeUtc   = SYSDATETIMEOFFSET()
            , tgt.LastModifierUserId            = @LELinkoUserProfileId
    WHEN NOT MATCHED AND src.InsertedDateTimeUtc > @LELastSyncDateTimeUtc THEN
        -- New
        INSERT  (RegulatoryProgramId
                , OrganizationId
                , RegulatorOrganizationId
                , AssignedTo
                , ReferenceNumber
                , IsEnabled
                , IsRemoved
                , LastModifierUserId)
        VALUES 
                (src.LERegulatoryProgramID
                , src.LEOrganizationID
                , src.LERegulatorOrganizationID
                , src.CTSUserAbbrv
                , src.CTSPermitNo
                , ~src.CTSRemoved
                , IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
                , @LELinkoUserProfileId)
    OUTPUT $action 
            , src.CTSPermitID
            , INSERTED.OrganizationRegulatoryProgramId
    INTO #OrganizationRegulatoryProgramMergeResult;

    DROP TABLE #OrganizationRegulatoryProgramMergeResult;


    -- GRESD Industries
    -- AssignedTo: TM
    UPDATE dbo.tOrganizationRegulatoryProgram
	SET AssignedTo = 'TM'
    WHERE ReferenceNumber IN 
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
    UPDATE dbo.tOrganizationRegulatoryProgram
	SET AssignedTo = 'HB'
    WHERE ReferenceNumber IN 
        (
            '0021'
            , '0508'
            , '0652'
            , '0672'
        )

    -- AssignedTo: KA
    UPDATE dbo.tOrganizationRegulatoryProgram
	SET AssignedTo = 'KA'
    WHERE ReferenceNumber IN 
        (
            'IPP-TEST-01'
        )

    -- AssignedTo: MB
    UPDATE dbo.tOrganizationRegulatoryProgram
	SET AssignedTo = 'MB'
    WHERE ReferenceNumber IN 
        (
            '0697'
        )

    -- AssignedTo: PK
    UPDATE dbo.tOrganizationRegulatoryProgram
	SET AssignedTo = 'PK'
    WHERE ReferenceNumber IN 
        (
            '8315'
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

DECLARE @OrganizationRegulatoryProgramId_Spectrum int
SELECT @OrganizationRegulatoryProgramId_Spectrum = orp.OrganizationRegulatoryProgramId
FROM dbo.tOrganizationRegulatoryProgram orp
    INNER JOIN dbo.tOrganization o ON o.OrganizationId = orp.OrganizationId
WHERE RegulatoryProgramId = @RegulatoryProgramId_IPP
    AND o.Name = 'Spectrum Industries Wealthy'

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
    
    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'Linko')
		    , @OrganizationRegulatoryProgramId_Spectrum
            , @OrganizationRegulatoryProgramId_GRESD
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Spectrum)
		    , SYSDATETIMEOFFSET()
		    , 0
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'kanderson')
		    , @OrganizationRegulatoryProgramId_GRESD
            , @OrganizationRegulatoryProgramId_GRESD
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_GRESD)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'pkuklewski')
		    , @OrganizationRegulatoryProgramId_GRESD
            , @OrganizationRegulatoryProgramId_GRESD
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Standard' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_GRESD)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'dpelletier')
		    , @OrganizationRegulatoryProgramId_Valley
            , @OrganizationRegulatoryProgramId_GRESD
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Valley)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'rdavis')
		    , @OrganizationRegulatoryProgramId_Kerry
            , @OrganizationRegulatoryProgramId_GRESD
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Kerry)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'nbora')
		    , @OrganizationRegulatoryProgramId_Allied
            , @OrganizationRegulatoryProgramId_GRESD
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Administrator' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Allied)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'jrasche')
		    , @OrganizationRegulatoryProgramId_Valley
            , @OrganizationRegulatoryProgramId_Valley
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Standard' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Valley)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'bskip')
		    , @OrganizationRegulatoryProgramId_Kerry
            , @OrganizationRegulatoryProgramId_Kerry
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Standard' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Kerry)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'bhirdes')
		    , @OrganizationRegulatoryProgramId_Allied
            , @OrganizationRegulatoryProgramId_Allied
		    , (SELECT PermissionGroupId FROM dbo.tPermissionGroup WHERE Name = 'Standard' AND OrganizationRegulatoryProgramId = @OrganizationRegulatoryProgramId_Allied)
		    , SYSDATETIMEOFFSET()
		    , 1
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'bfrazier')
		    , @OrganizationRegulatoryProgramId_GRESD
            , @OrganizationRegulatoryProgramId_GRESD
		    , NULL
		    , SYSDATETIMEOFFSET()
		    , 0
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'jsoper')
		    , @OrganizationRegulatoryProgramId_GRESD
            , @OrganizationRegulatoryProgramId_GRESD
		    , NULL
		    , SYSDATETIMEOFFSET()
		    , 0
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'cnorberg')
		    , @OrganizationRegulatoryProgramId_Kerry
            , @OrganizationRegulatoryProgramId_Kerry
		    , NULL
		    , SYSDATETIMEOFFSET()
		    , 0
		    , 1
        )

    INSERT INTO dbo.tOrganizationRegulatoryProgramUser (UserProfileId, OrganizationRegulatoryProgramId, InviterOrganizationRegulatoryProgramId, PermissionGroupId, RegistrationDateTimeUtc, IsRegistrationApproved, IsEnabled)
		VALUES 
		(
		    (SELECT UserProfileId FROM dbo.tUserProfile WHERE UserName = 'sschnau')
		    , @OrganizationRegulatoryProgramId_Allied
            , @OrganizationRegulatoryProgramId_Allied
		    , NULL
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

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tMonitoringPoint)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tMonitoringPoint'
    PRINT '-------------------------------'
    
  
	-- Sync: tMonitoringPoint AND ts_LE_CTSMonitoringPoints

    -- Create temp table to hold merge rslts
    CREATE TABLE #MonitoringPointMergeResult
    (
        Action      nvarchar(10) NOT NULL
        , CTSId     int NOT NULL
        , LEId      int NOT NULL
    );

    -- Merge LE table with staging table
    WITH src
    AS
    (
        SELECT iuorp.OrganizationRegulatoryProgramId AS LEIUOrganizationRegulatoryProgramID
                , stgmp.* 
        FROM dbo.ts_LE_CTSMonitoringPoints stgmp
            INNER JOIN ts_LE_CTSOrganizations stgorg ON stgorg.CTSPermitID = stgmp.CTSPermitID
            INNER JOIN LinkoExchange.dbo.tOrganizationRegulatoryProgram regorp ON regorp.OrganizationRegulatoryProgramId = stgmp.LEOrganizationRegulatoryProgramID
            INNER JOIN LinkoExchange.dbo.tOrganizationRegulatoryProgram iuorp ON iuorp.RegulatorOrganizationId = regorp.OrganizationId
                                                                                        AND iuorp.RegulatoryProgramId = regorp.RegulatoryProgramId
                                                                                        AND iuorp.ReferenceNumber = stgorg.CTSPermitNo
        WHERE stgmp.LEOrganizationRegulatoryProgramID = @LEAccountID
    )
    MERGE LinkoExchange.dbo.tMonitoringPoint WITH (HOLDLOCK) AS tgt
    USING src
    ON (tgt.MonitoringPointId = src.LEMonitoringPointID)
    WHEN MATCHED AND (src.LastUpdatedDateTimeUtc > @LELastSyncDateTimeUtc OR src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc) THEN
        -- Update or soft-delete
        UPDATE 
        SET tgt.Name                            = src.CTSMonPointAbbrv
            , tgt.Description                   = src.CTSMonPointDesc
            , tgt.IsEnabled                     = ~src.CTSRemoved
            , tgt.IsRemoved                     = IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
            , tgt.LastModificationDateTimeUtc   = SYSDATETIMEOFFSET()
            , tgt.LastModifierUserId            = @LELinkoUserProfileId
    WHEN NOT MATCHED AND src.InsertedDateTimeUtc > @LELastSyncDateTimeUtc THEN
        -- New
        INSERT  (Name
                , Description
                , OrganizationRegulatoryProgramId
                , IsEnabled
                , IsRemoved
                , LastModifierUserId)
        VALUES 
                (src.CTSMonPointAbbrv
                , src.CTSMonPointDesc
                , src.LEIUOrganizationRegulatoryProgramID
                , ~src.CTSRemoved
                , IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
                , @LELinkoUserProfileId)
    OUTPUT $action 
            , src.CTSMonPointID
            , INSERTED.MonitoringPointId
    INTO #MonitoringPointMergeResult;

    -- Update the staging table with the new LE table Ids
    UPDATE stg
    SET LEMonitoringPointID = rslt.LEId
    FROM #MonitoringPointMergeResult rslt
        INNER JOIN dbo.ts_LE_CTSMonitoringPoints stg ON stg.CTSMonPointID = rslt.CTSId
    WHERE rslt.Action = 'INSERT';

    DROP TABLE #MonitoringPointMergeResult;
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tCtsEventType)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tCtsEventType'
    PRINT '----------------------------'
    
    -- Sync: tCtsEventType AND ts_LE_CTSEventTypes

    -- Create temp table to hold merge rslts
    CREATE TABLE #EventTypeMergeResult
    (
        Action      nvarchar(10) NOT NULL
        , CTSId     int NOT NULL
        , LEId      int NOT NULL
    );

    -- Merge LE table with staging table
    WITH src 
    AS
    (
        SELECT stgct.*
        FROM dbo.ts_LE_CTSEventTypes stgct
        WHERE stgct.LEOrganizationRegulatoryProgramID = @LEAccountID
    )
    MERGE LinkoExchange.dbo.tCtsEventType WITH (HOLDLOCK) AS tgt
    USING src
    ON (tgt.CtsEventTypeId = src.LEEventTypeID)
    WHEN MATCHED AND (src.LastUpdatedDateTimeUtc > @LELastSyncDateTimeUtc OR src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc) THEN
        -- Update or soft-delete
        UPDATE 
        SET tgt.Name                            = src.CTSEventTypeAbbrv
            , tgt.CtsEventCategoryName          = src.CTSEventCatCode
            , tgt.IsEnabled                     = ~src.CTSRemoved
            , tgt.IsRemoved                     = IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
            , tgt.LastModificationDateTimeUtc   = SYSDATETIMEOFFSET()
            , tgt.LastModifierUserId            = @LELinkoUserProfileId
    WHEN NOT MATCHED AND src.InsertedDateTimeUtc > @LELastSyncDateTimeUtc THEN
        -- New
        INSERT  (Name
                , CtsEventCategoryName
                , OrganizationRegulatoryProgramId
                , IsEnabled
                , IsRemoved
                , LastModifierUserId)
        VALUES 
                (src.CTSEventTypeAbbrv
                , src.CTSEventCatCode
                , src.LEOrganizationRegulatoryProgramID
                , ~src.CTSRemoved
                , IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
                , @LELinkoUserProfileId)
    OUTPUT $action 
            , src.CTSEventTypeID
            , INSERTED.CtsEventTypeId
    INTO #EventTypeMergeResult;

    -- Update the staging table with the new LE table Ids
    UPDATE stg
    SET LEEventTypeID = rslt.LEId
    FROM #EventTypeMergeResult rslt
        INNER JOIN dbo.ts_LE_CTSEventTypes stg ON stg.CTSEventTypeID = rslt.CTSId
    WHERE rslt.Action = 'INSERT';

    DROP TABLE #EventTypeMergeResult;
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tCollectionMethod)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tCollectionMethod'
    PRINT '--------------------------------'
    
    -- Sync: tCollectionMethod AND ts_LE_CTSCollectionMethods

    -- Create temp table to hold merge rslts
    CREATE TABLE #CollectionMethodMergeResult
    (
        Action                  nvarchar(10) NOT NULL
        , CTSCollectMethCode    varchar(50) NOT NULL
        , LEId                  int NOT NULL
    );

    -- Merge LE table with staging table
    WITH src
    AS
    (
        SELECT orp.OrganizationId AS LEOrganizationID
                , cmt.CollectionMethodTypeId AS LECollectionMethodTypeID
                , stgcm.*
        FROM dbo.ts_LE_CTSCollectionMethods stgcm
            INNER JOIN LinkoExchange.dbo.tOrganizationRegulatoryProgram orp ON orp.OrganizationRegulatoryProgramId = stgcm.LEOrganizationRegulatoryProgramID
            INNER JOIN LinkoExchange.dbo.tCollectionMethodType cmt ON cmt.Name = stgcm.CTSCollectMethType
        WHERE stgcm.LEOrganizationRegulatoryProgramID = @LEAccountID
    )
    MERGE LinkoExchange.dbo.tCollectionMethod WITH (HOLDLOCK) AS tgt
    USING src
    ON (tgt.CollectionMethodId = src.LECollectionMethodID)
    WHEN MATCHED AND (src.LastUpdatedDateTimeUtc > @LELastSyncDateTimeUtc OR src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc) THEN
        -- Update or soft-delete
        UPDATE 
        SET tgt.CollectionMethodTypeId          = src.LECollectionMethodTypeID
            , tgt.IsEnabled                     = ~src.CTSRemoved
            , tgt.IsRemoved                     = IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
            , tgt.LastModificationDateTimeUtc   = SYSDATETIMEOFFSET()
            , tgt.LastModifierUserId            = @LELinkoUserProfileId
    WHEN NOT MATCHED AND src.InsertedDateTimeUtc > @LELastSyncDateTimeUtc THEN
        -- New
        INSERT (Name
                , OrganizationId
                , CollectionMethodTypeId
                , IsEnabled
                , IsRemoved
                , LastModifierUserId)
        VALUES 
                (src.CTSCollectMethCode
                , src.LEOrganizationID
                , src.LECollectionMethodTypeID
                , ~src.CTSRemoved
                , IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
                , @LELinkoUserProfileId)
    OUTPUT $action 
            , src.CTSCollectMethCode
            , INSERTED.CollectionMethodId
    INTO #CollectionMethodMergeResult;

    -- Update the staging table with the new LE table Ids
    UPDATE stg
    SET LECollectionMethodID = rslt.LEId
    FROM #CollectionMethodMergeResult rslt
        INNER JOIN dbo.ts_LE_CTSCollectionMethods stg ON stg.CTSCollectMethCode = rslt.CTSCollectMethCode
    WHERE rslt.Action = 'INSERT';
     
    DROP TABLE #CollectionMethodMergeResult;
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tUnit)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tUnit'
    PRINT '--------------------'
    
    -- Sync: tUnit AND ts_LE_CTSUnits

    -- Create temp table to hold merge rslts
    CREATE TABLE #UnitMergeResult
    (
        Action          nvarchar(10) NOT NULL
        , CTSUnitCode   varchar(50) NOT NULL
        , LEId          int NOT NULL
    );

    -- Merge LE table with staging table
    WITH src
    AS
    (
        SELECT orp.OrganizationId AS LEOrganizationID
                , stgu.*
        FROM dbo.ts_LE_CTSUnits stgu
            INNER JOIN LinkoExchange.dbo.tOrganizationRegulatoryProgram orp ON orp.OrganizationRegulatoryProgramId = stgu.LEOrganizationRegulatoryProgramID
        WHERE stgu.LEOrganizationRegulatoryProgramID = @LEAccountID
    )
    MERGE LinkoExchange.dbo.tUnit WITH (HOLDLOCK) AS tgt
    USING src
    ON (tgt.UnitId = src.LEUnitID)
    WHEN MATCHED AND (src.LastUpdatedDateTimeUtc > @LELastSyncDateTimeUtc OR src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc) THEN
        -- Update or soft-delete
        UPDATE 
        SET tgt.Description                     = src.CTSUnitDesc
            , tgt.IsFlowUnit                    = src.CTSIsFlowUnit
            , tgt.IsRemoved                     = IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
            , tgt.LastModificationDateTimeUtc   = SYSDATETIMEOFFSET()
            , tgt.LastModifierUserId            = @LELinkoUserProfileId
    WHEN NOT MATCHED AND src.InsertedDateTimeUtc > @LELastSyncDateTimeUtc THEN
        -- New
        INSERT (Name
                , Description
                , IsFlowUnit
                , OrganizationId
                , IsRemoved
                , LastModifierUserId)
        VALUES 
                (src.CTSUnitCode
                , src.CTSUnitDesc
                , src.CTSIsFlowUnit
                , src.LEOrganizationID
                , IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
                , @LELinkoUserProfileId)
    OUTPUT $action 
            , src.CTSUnitCode
            , INSERTED.UnitId
    INTO #UnitMergeResult;

    -- Update the staging table with the new LE table Ids
    UPDATE stg
    SET LEUnitID = rslt.LEId
    FROM #UnitMergeResult rslt
        INNER JOIN dbo.ts_LE_CTSUnits stg ON stg.CTSUnitCode = rslt.CTSUnitCode
    WHERE rslt.Action = 'INSERT';
     
    DROP TABLE #UnitMergeResult;
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tParameter)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tParameter'
    PRINT '-------------------------'
    
    -- Sync: tParameter AND ts_LE_CTSParameters

    -- Create temp table to hold merge rslts
    CREATE TABLE #ParameterMergeResult
    (
        Action      nvarchar(10) NOT NULL
        , CTSId     int NOT NULL
        , LEId      int NOT NULL
    );

    -- Merge LE table with staging table
    WITH src
    AS
    (
        SELECT stgu.LEUnitID
                , stgp.*
        FROM dbo.ts_LE_CTSParameters stgp
            INNER JOIN dbo.ts_LE_CTSUnits stgu ON stgu.CTSUnitCode = stgp.CTSDefaultUnitsCode
            INNER JOIN LinkoExchange.dbo.tOrganizationRegulatoryProgram orp ON orp.OrganizationRegulatoryProgramId = stgp.LEOrganizationRegulatoryProgramID
        WHERE stgp.LEOrganizationRegulatoryProgramID = @LEAccountID
    )
    MERGE LinkoExchange.dbo.tParameter WITH (HOLDLOCK) AS tgt
    USING src
    ON (tgt.ParameterId = src.LEParameterID)
    WHEN MATCHED AND (src.LastUpdatedDateTimeUtc > @LELastSyncDateTimeUtc OR src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc) THEN
        -- Update or soft-delete
        UPDATE 
        SET tgt.Name                                = src.CTSParamName
            , tgt.DefaultUnitId                     = src.LEUnitID
            , tgt.TrcFactor                         = src.CTSTRCFactor
            , tgt.IsFlowForMassLoadingCalculation   = src.IsFlowForMassCalcs
            , tgt.IsRemoved                         = IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
            , tgt.LastModificationDateTimeUtc       = SYSDATETIMEOFFSET()
            , tgt.LastModifierUserId                = @LELinkoUserProfileId
    WHEN NOT MATCHED AND src.InsertedDateTimeUtc > @LELastSyncDateTimeUtc THEN
        -- New
        INSERT  (Name
                , DefaultUnitId
                , TrcFactor
                , IsFlowForMassLoadingCalculation
                , OrganizationRegulatoryProgramId
                , IsRemoved
                , LastModifierUserId)
        VALUES 
                (src.CTSParamName
                , src.LEUnitID
                , src.CTSTRCFactor
                , src.IsFlowForMassCalcs
                , src.LEOrganizationRegulatoryProgramID
                , IIF(src.DeletedDateTimeUtc > @LELastSyncDateTimeUtc, 1, 0)
                , @LELinkoUserProfileId)
    OUTPUT $action 
            , src.CTSParamID
            , INSERTED.ParameterId
    INTO #ParameterMergeResult;

    -- Update the staging table with the new LE table Ids
    UPDATE stg
    SET LEParameterID = rslt.LEId
    FROM #ParameterMergeResult rslt
        INNER JOIN dbo.ts_LE_CTSParameters stg ON stg.CTSParamID = rslt.CTSId
    WHERE rslt.Action = 'INSERT';

    DROP TABLE #ParameterMergeResult;
END

    -- Sync: tMonitoringPointParameter, tMonitoringPointParameterLimit, tSampleRequreirement, tSampleFrequency AND ts_LE_CTSMonPointLimits
    -- No updates occur. Always delete and insert the data.

    -- Save all MonitoringPointParameterId first before deleting them
    SELECT MonitoringPointParameterId
    INTO #DeletedMonitoringPointParameter
    FROM LinkoExchange.dbo.tMonitoringPointParameter mpp
        INNER JOIN LinkoExchange.dbo.tMonitoringPoint mp ON mp.MonitoringPointId = mpp.MonitoringPointId
        INNER JOIN LinkoExchange.dbo.tOrganizationRegulatoryProgram iuorp ON iuorp.OrganizationRegulatoryProgramId = mp.OrganizationRegulatoryProgramId
        INNER JOIN LinkoExchange.dbo.tOrganizationRegulatoryProgram authorp ON authorp.OrganizationId = iuorp.RegulatorOrganizationId
                                                                                AND authorp.RegulatoryProgramId = iuorp.RegulatoryProgramId
    WHERE authorp.OrganizationRegulatoryProgramId = @LEAccountID;

    -- Delete data from tMonitoringPointParameter and related tables (tMonitoringPointParameterLimit, tSampleRequreirement, tSampleFrequency)
    DELETE mppl
    FROM LinkoExchange.dbo.tMonitoringPointParameterLimit mppl
        INNER JOIN #DeletedMonitoringPointParameter dmpp ON dmpp.MonitoringPointParameterId = mppl.MonitoringPointParameterId;
    
    DELETE sr
    FROM LinkoExchange.dbo.tSampleRequirement sr
        INNER JOIN #DeletedMonitoringPointParameter dmpp ON dmpp.MonitoringPointParameterId = sr.MonitoringPointParameterId;

    DELETE sf
    FROM LinkoExchange.dbo.tSampleFrequency sf
        INNER JOIN #DeletedMonitoringPointParameter dmpp ON dmpp.MonitoringPointParameterId = sf.MonitoringPointParameterId;

    DELETE FROM LinkoExchange.dbo.tMonitoringPointParameter
     FROM LinkoExchange.dbo.tMonitoringPointParameter mpp
        INNER JOIN #DeletedMonitoringPointParameter dmpp ON dmpp.MonitoringPointParameterId = mpp.MonitoringPointParameterId;

    DROP TABLE #DeletedMonitoringPointParameter


    -- Insert data into tMonitoringPointParameter and related tables (tMonitoringPointParameterLimit, tSampleFrequency, tSampleRequirement)
    
    -- tMonitoringPointParameter
    INSERT INTO LinkoExchange.dbo.tMonitoringPointParameter
    (
        MonitoringPointId
        , ParameterId
        , DefaultUnitId
        , EffectiveDateTimeUtc
        , RetirementDateTimeUtc
    )
    -- Monitoring point limits and sample frequencies
    SELECT stgmp.LEMonitoringPointID
            , stgp.LEParameterID
            , stgu.LEUnitID
            , stgmpl.CTSDateEffective
            , CASE
                    WHEN stgmpl.CTSDateRetired IS NULL THEN
                        CONVERT(datetimeoffset(0), '9999-12-31 00:00:00')
                    ELSE
                        stgmpl.CTSDateRetired
              END AS CTSDateRetired
    FROM dbo.ts_LE_CTSMonPointLimits stgmpl
        INNER JOIN dbo.ts_LE_CTSMonitoringPoints stgmp ON stgmp.CTSMonPointID = stgmpl.CTSMonPointID
        INNER JOIN dbo.ts_LE_CTSParameters stgp ON stgp.CTSParamName = stgmpl.CTSParamName 
                                                    AND stgp.LEOrganizationRegulatoryProgramID = stgmpl.LEOrganizationRegulatoryProgramID
        INNER JOIN dbo.ts_LE_CTSUnits stgu ON stgu.CTSUnitCode = stgmpl.CTSDailyLimitUnitsCode
    WHERE stgmpl.LEOrganizationRegulatoryProgramID = @LEAccountID
        AND stgmpl.InsertedDateTimeUtc > @LELastSyncDateTimeUtc
    UNION
    -- IU sample requirements
    SELECT stgmp.LEMonitoringPointID
            , stgp.LEParameterID
            , NULL
            , stgsr.CTSLimitEffectiveDate
            , CASE
                    WHEN stgsr.CTSLimitRetireDate IS NULL THEN
                        CONVERT(datetimeoffset(0), '9999-12-31 00:00:00')
                    ELSE
                        stgsr.CTSLimitRetireDate
              END AS CTSLimitRetireDate
    FROM dbo.ts_LE_CTSIUSampleRequirements stgsr
        LEFT JOIN dbo.ts_LE_CTSMonPointLimits stgmpl ON stgmpl.CTSMonPointID = stgsr.CTSMonPointID
                                                        AND stgmpl.CTSParamName = stgsr.CTSParamName
                                                        AND stgmpl.CTSDateEffective = stgsr.CTSLimitEffectiveDate
        INNER JOIN dbo.ts_LE_CTSMonitoringPoints stgmp ON stgmp.CTSMonPointID = stgsr.CTSMonPointID
        INNER JOIN dbo.ts_LE_CTSParameters stgp ON stgp.CTSParamName = stgsr.CTSParamName 
                                                    AND stgp.LEOrganizationRegulatoryProgramID = stgsr.LEOrganizationRegulatoryProgramID
    WHERE stgmpl.CTSMonPointLimID IS NULL
        AND stgsr.LEOrganizationRegulatoryProgramID = @LEAccountID
        AND stgsr.InsertedDateTimeUtc > @LELastSyncDateTimeUtc;

    -- Assumption: no negative values allowed in LinkoCTS (confirmed by CW). Otherwise, a more sophisticated limit value parsing is required.
    -- tMonitoringPointParameterLimit
    WITH src
    AS
    (
        -- Concentration Daily
        SELECT stgmpl.*
                , lb.LimitBasisId AS LELimitBasisID
                , lb.Description AS LELimitBasisDescription
                , lt.LimitTypeId AS LELimitTypeID
                , lt.Description AS LELimitTypeDescription
                , CASE
                        WHEN  CHARINDEX('-', stgmpl.CTSDailyLimit) = 0 THEN
                            NULL
                        ELSE
                            CONVERT(float, REPLACE(SUBSTRING(stgmpl.CTSDailyLimit, 1, CHARINDEX('-', stgmpl.CTSDailyLimit) - 1), ',', ''))
                  END AS MinimumValue
                , CONVERT(float, REPLACE(SUBSTRING(stgmpl.CTSDailyLimit, CHARINDEX('-', stgmpl.CTSDailyLimit) + 1, LEN(stgmpl.CTSDailyLimit) - CHARINDEX('-', stgmpl.CTSDailyLimit)), ',', '')) AS MaximumValue
                , stgu.LEUnitID
                , cmt.CollectionMethodTypeId AS LECollectionMethodTypeID
        FROM dbo.ts_LE_CTSMonPointLimits stgmpl
            INNER JOIN dbo.ts_LE_CTSUnits stgu ON stgu.CTSUnitCode = stgmpl.CTSDailyLimitUnitsCode
            INNER JOIN dbo.ts_LE_CTSCollectionMethods stgcm ON stgcm.CTSCollectMethCode = stgmpl.CTSCollectMethCode
            INNER JOIN LinkoExchange.dbo.tCollectionMethodType cmt ON cmt.Name = stgcm.CTSCollectMethType
            INNER JOIN LinkoExchange.dbo.tLimitBasis lb ON lb.Name = 'Concentration'
            INNER JOIN LinkoExchange.dbo.tLimitType lt ON lt.Name = 'Daily'
        WHERE stgmpl.CTSDailyLimit IS NOT NULL
            AND stgmpl.LEOrganizationRegulatoryProgramID = @LEAccountID
            AND stgmpl.InsertedDateTimeUtc > @LELastSyncDateTimeUtc
        UNION
        -- MassLoading Daily
        SELECT stgmpl.*
                , lb.LimitBasisId AS LELimitBasisID
                , lb.Description AS LELimitBasisDescription
                , lt.LimitTypeId AS LELimitTypeID
                , lt.Description AS LELimitTypeDescription
                , CASE
                        WHEN  CHARINDEX('-', stgmpl.CTSMassDailyLimit) = 0 THEN
                            NULL
                        ELSE
                            CONVERT(float, REPLACE(SUBSTRING(stgmpl.CTSMassDailyLimit, 1, CHARINDEX('-', stgmpl.CTSMassDailyLimit) - 1), ',', ''))
                  END AS MinimumValue
                , CONVERT(float, REPLACE(SUBSTRING(stgmpl.CTSMassDailyLimit, CHARINDEX('-', stgmpl.CTSMassDailyLimit) + 1, LEN(stgmpl.CTSMassDailyLimit) - CHARINDEX('-', stgmpl.CTSMassDailyLimit)), ',', '')) AS MaximumValue
                , stgu.LEUnitID
                , cmt.CollectionMethodTypeId AS LECollectionMethodTypeID
        FROM dbo.ts_LE_CTSMonPointLimits stgmpl
            INNER JOIN dbo.ts_LE_CTSUnits stgu ON stgu.CTSUnitCode = stgmpl.CTSMassDailyLimitUnitsCode
            INNER JOIN dbo.ts_LE_CTSCollectionMethods stgcm ON stgcm.CTSCollectMethCode = stgmpl.CTSCollectMethCode
            INNER JOIN LinkoExchange.dbo.tCollectionMethodType cmt ON cmt.Name = stgcm.CTSCollectMethType
            INNER JOIN LinkoExchange.dbo.tLimitBasis lb ON lb.Name = 'MassLoading'
            INNER JOIN LinkoExchange.dbo.tLimitType lt ON lt.Name = 'Daily'
        WHERE stgmpl.CTSMassDailyLimit IS NOT NULL
            AND stgmpl.LEOrganizationRegulatoryProgramID = @LEAccountID
            AND stgmpl.InsertedDateTimeUtc > @LELastSyncDateTimeUtc
    )
    INSERT INTO LinkoExchange.dbo.tMonitoringPointParameterLimit
    (
        MonitoringPointParameterId
        , Name
        , MinimumValue
        , MaximumValue
        , BaseUnitId
        , CollectionMethodTypeId
        , LimitTypeId
        , LimitBasisId
    )
    SELECT mpp.MonitoringPointParameterId
            , CONCAT(stgp.CTSParamName, ' ', src.LELimitBasisDescription, ' ', src.LELimitTypeDescription)
            , src.MinimumValue
            , src.MaximumValue
            , src.LEUnitID
            , src.LECollectionMethodTypeID
            , src.LELimitTypeID
            , src.LELimitBasisID
    FROM src
        INNER JOIN dbo.ts_LE_CTSMonitoringPoints stgmp ON stgmp.CTSMonPointID = src.CTSMonPointID
        INNER JOIN dbo.ts_LE_CTSParameters stgp ON stgp.CTSParamName = src.CTSParamName
                                                    AND stgp.LEOrganizationRegulatoryProgramID = src.LEOrganizationRegulatoryProgramID
        INNER JOIN LinkoExchange.dbo.tMonitoringPointParameter mpp ON mpp.MonitoringPointId = stgmp.LEMonitoringPointID
                                                                        AND mpp.ParameterId = stgp.LEParameterID
                                                                        AND mpp.EffectiveDateTimeUtc = src.CTSDateEffective;

    -- tSampleFrequency
    INSERT INTO LinkoExchange.dbo.tSampleFrequency
    (
        MonitoringPointParameterId
        , CollectionMethodId
        , IUSampleFrequency
        , AuthoritySampleFrequency
    )
    SELECT mpp.MonitoringPointParameterId
            , cm.CollectionMethodId
            , src.CTSIUSampleFrequency
            , src.CTSAuthSampleFrequency
    FROM dbo.ts_LE_CTSMonPointLimits src
        INNER JOIN dbo.ts_LE_CTSMonitoringPoints stgmp ON stgmp.CTSMonPointID = src.CTSMonPointID
        INNER JOIN dbo.ts_LE_CTSParameters stgp ON stgp.CTSParamName = src.CTSParamName
                                                    AND stgp.LEOrganizationRegulatoryProgramID = src.LEOrganizationRegulatoryProgramID
        INNER JOIN LinkoExchange.dbo.tMonitoringPointParameter mpp ON mpp.MonitoringPointId = stgmp.LEMonitoringPointID
                                                                        AND mpp.ParameterId = stgp.LEParameterID
                                                                        AND mpp.EffectiveDateTimeUtc = src.CTSDateEffective
        INNER JOIN LinkoExchange.dbo.tCollectionMethod cm ON cm.Name = src.CTSCollectMethCode
    WHERE src.LEOrganizationRegulatoryProgramID = @LEAccountID
        AND src.InsertedDateTimeUtc > @LELastSyncDateTimeUtc;


    -- tSampleRequirement
    WITH src
    AS
    (
        -- IU sample requirements
        SELECT mp.OrganizationRegulatoryProgramId AS ByOrganizationRegulatoryProgramId
                , stgmp.LEMonitoringPointID
                , stgsr.*
        FROM dbo.ts_LE_CTSIUSampleRequirements stgsr
            INNER JOIN dbo.ts_LE_CTSMonitoringPoints stgmp ON stgmp.CTSMonPointID = stgsr.CTSMonPointID
            INNER JOIN LinkoExchange.dbo.tMonitoringPoint mp ON mp.MonitoringPointId = stgmp.LEMonitoringPointID
        WHERE stgsr.LEOrganizationRegulatoryProgramID = @LEAccountID
            AND stgsr.InsertedDateTimeUtc > @LELastSyncDateTimeUtc
        -- UNION
        -- Future: Authority sample requirements
    )
    INSERT INTO LinkoExchange.dbo.tSampleRequirement
    (
        MonitoringPointParameterId
        , PeriodStartDateTimeUtc
        , PeriodEndDateTimeUtc
        , SamplesRequired
        , ByOrganizationRegulatoryProgramId
    )
    SELECT mpp.MonitoringPointParameterId
            , src.CTSPeriodStart
            , src.CTSPeriodEnd
            , src.CTSSampsRequired
            , src.ByOrganizationRegulatoryProgramId
    FROM src
        INNER JOIN dbo.ts_LE_CTSParameters stgp ON stgp.CTSParamName = src.CTSParamName
                                                    AND stgp.LEOrganizationRegulatoryProgramID = src.LEOrganizationRegulatoryProgramID
        INNER JOIN LinkoExchange.dbo.tMonitoringPointParameter mpp ON mpp.MonitoringPointId = src.LEMonitoringPointID
                                                                        AND mpp.ParameterId = stgp.LEParameterID
                                                                        AND mpp.EffectiveDateTimeUtc = src.CTSLimitEffectiveDate;

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tParameterGroup)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tParameterGroup'
    PRINT '------------------------------'
    
    INSERT INTO dbo.tParameterGroup (Name, Description, OrganizationRegulatoryProgramId, IsActive)
		VALUES
        (
            'TotalMetals'
            , 'All Metals'
            , @OrganizationRegulatoryProgramId_GRESD
            , 1
        )
        ,
        (
            'TTO'
            , 'Total Toxic Organics'
            , @OrganizationRegulatoryProgramId_GRESD
            , 1
        )
        ,
        (
            'Surcharge'
            , 'Surcharge Parameters'
            , @OrganizationRegulatoryProgramId_GRESD
            , 1
        )
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tParameterGroupParameter)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tParameterGroupParameter'
    PRINT '---------------------------------------'
    
    INSERT INTO dbo.tParameterGroupParameter (ParameterGroupId, ParameterId)
		VALUES
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TotalMetals')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = 'Arsenic, total')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TotalMetals')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = 'Boron, total')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TotalMetals')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = 'Cadmium, total')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TotalMetals')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = 'Chromium, total')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TotalMetals')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = 'Lead, total')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TotalMetals')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = 'Nickel, total')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TotalMetals')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = 'Zinc, total')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,1,1,2-Tetrachloroethane')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,1,1-Trichloroethane')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,1,1-Trichloroethene')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,1,2,2-Tetrachloroethane')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,1,2-Trichloroethane')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,1,2-Trichloroethene')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,1-Dichloroethane')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,1-Dichloroethene')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,1-Dichlorpropene')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,2,3-Trichlorobenzene')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,2,3-Trichlorpropane')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,2,4-Trichlorobenzene')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,2,4-Trimethylbenzene')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'TTO')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = '1,2-Dibromoethane')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'Surcharge')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = 'Biochemical Oxygen Demand')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'Surcharge')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = 'Total Suspended Solids')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'Surcharge')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = 'Ammonia')
        )
        ,
        (
            (SELECT ParameterGroupId FROM dbo.tParameterGroup WHERE Name = 'Surcharge')
            , (SELECT ParameterId FROM dbo.tParameter WHERE Name = 'Chemical Oxygen Demand')
        )
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tReportElementType WHERE Name <> 'Samples and Results')
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tReportElementType'
    PRINT '---------------------------------'
    
    INSERT INTO dbo.tReportElementType (Name, Description, Content, IsContentProvided, CtsEventTypeId, ReportElementCategoryId, OrganizationRegulatoryProgramId)
		VALUES
        (
            'Lab Analysis Report'
            , 'Lab Analysis Report'
            , NULL
            , 0
            , (SELECT CtsEventTypeId FROM dbo.tCtsEventType WHERE Name = 'P2 Certification')
            , (SELECT ReportElementCategoryId FROM dbo.tReportElementCategory WHERE Name = 'Attachments')
            , @OrganizationRegulatoryProgramId_GRESD
        )
        ,
        (
            'TTO Certification'
            , 'TTO Certification'
            , 'Based on my inquiry of the person or persons directly responsible for managing compliance with the pretreatment standard for total toxic organics (T.T.O.), I certify that to the best of my knowledge and belief, no dumping of concentrated toxic organics into the wastewater has occurred since the filing of the last analysis report.  I further certify that this company is implementing the solvent management plan submitted to the Control Authority.'
            , 1
            , NULL
            , (SELECT ReportElementCategoryId FROM dbo.tReportElementCategory WHERE Name = 'Certifications')
            , @OrganizationRegulatoryProgramId_GRESD
        )
        ,
        (
            'Signature Certification'
            , 'Signature Certification'
            , 'I certify under penalty of law that this document and all attachments were prepared under my direction or supervision in accordance with a system designed to assure that qualified personnel properly gathered and evaluated the information submitted. Based on my inquiry of the person or persons who managed the system, or those persons directly responsible for gathering information, the information submitted is, to the best of my knowledge and belief, true, accurate and complete. I am aware that there are significant penalties for submitting false information, including the possibility of fines and imprisonment for a knowing violation.'
            , 1
            , NULL
            , (SELECT ReportElementCategoryId FROM dbo.tReportElementCategory WHERE Name = 'Certifications')
            , @OrganizationRegulatoryProgramId_GRESD
        )
        ,
        (
            'Violation Response Letter'
            , 'Violation Response Letter'
            , NULL
            , 0
            , (SELECT CtsEventTypeId FROM dbo.tCtsEventType WHERE Name = 'IURESP')
            , (SELECT ReportElementCategoryId FROM dbo.tReportElementCategory WHERE Name = 'Attachments')
            , @OrganizationRegulatoryProgramId_GRESD
        )
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tReportPackageTemplate)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tReportPackageTemplate'
    PRINT '-------------------------------------'
    
    INSERT INTO dbo.tReportPackageTemplate (Name, Description, EffectiveDateTimeUtc, IsSubmissionBySignatoryRequired, CtsEventTypeId, OrganizationRegulatoryProgramId, IsActive)
		VALUES
        (
            '1st Quarter PCR'
            , 'Submit 1st Quarter PCR including resubmission of a repudiated report'
            , '2017-01-01 00:00:00 -07:00'
            , 1
            , (SELECT CtsEventTypeId FROM dbo.tCtsEventType WHERE Name = '1st quarter report')
            , @OrganizationRegulatoryProgramId_GRESD
            , 1
        )
        ,
        (
            '1st Quarter PCR'
            , 'Submit 1st Quarter PCR including resubmission of a repudiated report'
            , '2017-04-13 00:00:00 -07:00'
            , 1
            , (SELECT CtsEventTypeId FROM dbo.tCtsEventType WHERE Name = '1st quarter report')
            , @OrganizationRegulatoryProgramId_GRESD
            , 1
        )
        ,
        (
            '1st Quarter PCR'
            , 'Submit 1st Quarter PCR including resubmission of a repudiated report'
            , '2017-08-01 00:00:00 -07:00'
            , 1
            , (SELECT CtsEventTypeId FROM dbo.tCtsEventType WHERE Name = '1st quarter report')
            , @OrganizationRegulatoryProgramId_GRESD
            , 1
        )
        ,
        (
            'Semi Annual Flow'
            , 'Submit Monthly Daily Maximum Flow and Average Daily Flow'
            , '2017-01-01 00:00:00 -07:00'
            , 1
            , (SELECT CtsEventTypeId FROM dbo.tCtsEventType WHERE Name = 'Flow Report')
            , @OrganizationRegulatoryProgramId_GRESD
            , 1
        )
        ,
        (
            'Resample'
            , 'Submit the results of a resampling event'
            , '2017-01-01 00:00:00 -07:00'
            , 1
            , (SELECT CtsEventTypeId FROM dbo.tCtsEventType WHERE Name = 'Resample' AND CtsEventCategoryName = 'GENERIC')
            , @OrganizationRegulatoryProgramId_GRESD
            , 1
        )
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tReportPackageTemplateAssignment)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tReportPackageTemplateAssignment'
    PRINT '-----------------------------------------------'
    
    -- Assign Report Package Template 5 to all Industries
    INSERT INTO dbo.tReportPackageTemplateAssignment (ReportPackageTemplateId, OrganizationRegulatoryProgramId)
		SELECT 5, OrganizationRegulatoryProgramId
        FROM dbo.tOrganizationRegulatoryProgram
        WHERE RegulatorOrganizationId = @OrganizationId_GRESD

    -- Assign Report Package Template 4 to all Industries except Di-Anodic
    INSERT INTO dbo.tReportPackageTemplateAssignment (ReportPackageTemplateId, OrganizationRegulatoryProgramId)
		SELECT 4, orp.OrganizationRegulatoryProgramId
        FROM dbo.tOrganizationRegulatoryProgram orp
            INNER JOIN dbo.tOrganization o ON o.OrganizationId = orp.OrganizationId
        WHERE orp.RegulatorOrganizationId = @OrganizationId_GRESD 
            AND o.Name <> 'Di-Anodic Finishing'

    INSERT INTO dbo.tReportPackageTemplateAssignment (ReportPackageTemplateId, OrganizationRegulatoryProgramId)
		SELECT 1, orp.OrganizationRegulatoryProgramId 
        FROM dbo.tOrganizationRegulatoryProgram orp 
        WHERE orp.ReferenceNumber IN ('0006', '0040', '0002', '0022', '0032', '0469', '0711', '0764', '0770')

    INSERT INTO dbo.tReportPackageTemplateAssignment (ReportPackageTemplateId, OrganizationRegulatoryProgramId)
		SELECT 2, orp.OrganizationRegulatoryProgramId 
        FROM dbo.tOrganizationRegulatoryProgram orp 
        WHERE orp.ReferenceNumber = '0006'
    
    INSERT INTO dbo.tReportPackageTemplateAssignment (ReportPackageTemplateId, OrganizationRegulatoryProgramId)
		SELECT 3, orp.OrganizationRegulatoryProgramId 
        FROM dbo.tOrganizationRegulatoryProgram orp 
        WHERE orp.ReferenceNumber = '0002'       
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tReportPackageTemplateElementCategory)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tReportPackageTemplateElementCategory'
    PRINT '----------------------------------------------------'
    
    INSERT INTO dbo.tReportPackageTemplateElementCategory (ReportPackageTemplateId, ReportElementCategoryId, SortOrder)
		VALUES (1, 1, 1)
            , (1, 2, 2)
            , (1, 3, 3)
            , (2, 1, 1)
            , (2, 2, 2)
            , (2, 3, 3)
            , (3, 1, 1)
            , (3, 2, 2)
            , (3, 3, 3)
            , (4, 1, 1)
            , (4, 3, 2)
            , (5, 1, 1)
            , (5, 2, 2)
            , (5, 3, 3)
END

IF DB_NAME() = 'LinkoExchange' AND NOT EXISTS (SELECT TOP 1 * FROM dbo.tReportPackageTemplateElementType)
BEGIN
    PRINT CHAR(13)
    PRINT CHAR(13)
    PRINT 'Add records to tReportPackageTemplateElementType'
    PRINT '------------------------------------------------'
    
    INSERT INTO dbo.tReportPackageTemplateElementType (ReportPackageTemplateElementCategoryId, ReportElementTypeId, IsRequired, SortOrder)
		VALUES (1, 1, 0, 1)
            , (2, 2, 0, 1)
            , (3, 3, 0, 1)
            , (3, 4, 0, 2)
            , (4, 1, 0, 1)
            , (5, 2, 0, 1)
            , (6, 3, 0, 1)
            , (6, 4, 0, 2)
            , (7, 1, 0, 1)
            , (8, 2, 0, 1)
            , (9, 4, 0, 1)
            , (10, 1, 0, 1)
            , (11, 4, 0, 1)
            , (12, 1, 0, 1)
            , (13, 2, 0, 1)
            , (13, 5, 0, 2)
            , (14, 4, 0, 1)            
END

PRINT CHAR(13)
PRINT '-------------------------------------'
PRINT 'END OF: LinkoExchange Test Data Setup'
PRINT '-------------------------------------'