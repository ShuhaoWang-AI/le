using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Mapping
{
    public interface IMapHelper
    {
        OrganizationDto GetOrganizationDtoFromOrganization(Core.Domain.Organization organization, OrganizationDto dto = null);
        AuthorityDto GetAuthorityDtoFromOrganization(Core.Domain.Organization organization, AuthorityDto dto = null);
        UserDto GetUserDtoFromUserProfile(UserProfile userProfile, UserDto dto = null);
        UserProfile GetUserProfileFromUserDto(UserDto dto, UserProfile userProfile = null);

        OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(
            OrganizationRegulatoryProgramUser user, OrganizationRegulatoryProgramUserDto dto = null);

        OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(
            OrganizationRegulatoryProgram orgRegProgram, OrganizationRegulatoryProgramDto dto = null);

        SettingDto GetSettingDtoFromOrganizationRegulatoryProgramSetting(OrganizationRegulatoryProgramSetting setting, SettingDto dto = null);
        SettingDto GetSettingDtoFromOrganizationSetting(OrganizationSetting setting, SettingDto dto = null);
        InvitationDto GetInvitationDtoFromInvitation(Core.Domain.Invitation invitation, InvitationDto dto = null);
        PermissionGroupDto GetPermissionGroupDtoFromPermissionGroup(PermissionGroup permissionGroup, PermissionGroupDto dto = null);
        TimeZoneDto GetTimeZoneDtoFromTimeZone(Core.Domain.TimeZone timeZone, TimeZoneDto dto = null);
        EmailAuditLog GetEmailAuditLogFromEmailAuditLogEntryDto(EmailAuditLogEntryDto dto, EmailAuditLog emailAuditLog = null);
        JurisdictionDto GetJurisdictionDtoFromJurisdiction(Core.Domain.Jurisdiction jurisdiction, JurisdictionDto dto = null);
        CromerrAuditLog GetCromerrAuditLogFromCromerrAuditLogEntryDto(CromerrAuditLogEntryDto dto, CromerrAuditLog cromerrAuditLog = null);
        CromerrAuditLogEntryDto GetCromerrAuditLogDtoFromCromerrAuditLog(CromerrAuditLog cromerrAuditLog, CromerrAuditLogEntryDto dto = null);
        ParameterDto GetParameterDtoFromParameter(Core.Domain.Parameter parameter);
        ParameterGroupDto GetParameterGroupDtoFromParameterGroup(ParameterGroup parameterGroup);
        ParameterGroup GetParameterGroupFromParameterGroupDto(ParameterGroupDto parameterGroupDto, ParameterGroup parameterGroup = null);
        UnitDto ToDto(Core.Domain.Unit fromDomainObject);
        Core.Domain.Unit ToDomainObject(UnitDto fromDto, Core.Domain.Unit existingDomainObject = null);
        ReportElementCategory GetReportElementCategoryFromReportElementCategoryDto(ReportElementCategoryDto cat);
        ReportElementCategoryDto GetReportElementCategoryDtoFromReportElementCategory(ReportElementCategory cat);

        ReportPackageTemplateElementCategoryDto GetReportPackageTemplateElementCategoryDtoFromReportPackageTemplateElementCategory(ReportPackageTemplateElementCategory cat);
        ReportPackageTemplateElementCategory GetReportPackageTemplateElementCategoryFromReportPackageTemplateElementCategoryDto(ReportPackageTemplateElementCategoryDto cat);

        ReportPackageTemplateElementTypeDto GetReportPackageTemplateElementTypeDtoFromReportPackageTemplateElementType(ReportPackageTemplateElementType rptet);
        ReportPackageTemplateElementType GetReportPackageTemplateElementTypeFromReportPackageTemplateElementTypeDto(ReportPackageTemplateElementTypeDto rptetDto);

        CtsEventTypeDto GetCtsEventTypeDtoFromEventType(CtsEventType ctsEventType);
        CtsEventType GetCtsEventTypeFromEventTypeDto(CtsEventTypeDto ctsEventTypeDto);

        ReportPackageTemplateDto GetReportPackageTemplateDtoFromReportPackageTemplate(ReportPackageTemplate reportPackageTemplate);
        ReportPackageTemplate GetReportPackageTemplateFromReportPackageTemplateDto(ReportPackageTemplateDto reportPackageTemplateDto);

        ReportPackageTemplateElementTypeDto GetReportPackageTemplateElementTypeDtoFromReportPackageTemplateType(ReportPackageTemplateElementType rptElementType);
        ReportPackageTemplateElementType GetReportPackageTemplateElementTypeFromReportPackageTemplateTypeDto(ReportPackageTemplateElementTypeDto rptElementTypeDto);

        ReportElementTypeDto GetReportElementTypeDtoFromReportElementType(ReportElementType reportElementType);
        ReportElementType GetReportElementTypeFromReportElementTypeDto(ReportElementTypeDto reportElementTypeDto, ReportElementType existingReportElementType = null);

        FileStoreDto GetFileStoreDtoFromFileStore(Core.Domain.FileStore fileStore);
        Core.Domain.FileStore GetFileStoreFromFileStoreDto(FileStoreDto fileStoreDto);

        MonitoringPointDto GetMonitoringPointDtoFromMonitoringPoint(Core.Domain.MonitoringPoint mp);

        SampleDto GetSampleDtoFromSample(Core.Domain.Sample sample);
        Core.Domain.Sample GetSampleFromSampleDto(SampleDto sampleDto, Core.Domain.Sample existingSample = null);
        SampleResultDto GetSampleResultDtoFromSampleResult(SampleResult sampleResult);
        SampleResult GetConcentrationSampleResultFromSampleResultDto(SampleResultDto dto, SampleResult existingSampleResult);
        SampleResult GetMassSampleResultFromSampleResultDto(SampleResultDto dto, SampleResult existingSampleResult);
        ReportPackageDto GetReportPackageDtoFromReportPackage(ReportPackage rpt);
        ReportPackage GetReportPackageFromReportPackageTemplate(ReportPackageTemplate rpt);
        RepudiationReasonDto GetRepudiationReasonDtoFromRepudiationReason(RepudiationReason repudiationReason);

        ReportPackageElementTypeDto GetReportPackageElementTypeDtoFromReportPackageElementType(ReportPackageElementType reportPackageElementType);

        DataSourceDto GetDataSourceDtoFroDataSource(Core.Domain.DataSource dataSource);
        Core.Domain.DataSource GetDataSourceFromDataSourceDto(DataSourceDto dto, Core.Domain.DataSource existingDataSource);
        DataSourceTranslationDto ToDataSourceMonitoringPointDto(DataSourceMonitoringPoint from);
        DataSourceMonitoringPoint ToDataSourceMonitoringPoint(DataSourceTranslationDto from, DataSourceMonitoringPoint existingDomainObject);
        DataSourceTranslationDto ToDataSourceSampleTypeDto(DataSourceCtsEventType from);
        DataSourceCtsEventType ToDataSourceSampleType(DataSourceTranslationDto from, DataSourceCtsEventType existingDomainObject);
        DataSourceTranslationDto ToDataSourceCollectionMethodDto(DataSourceCollectionMethod from);
        DataSourceCollectionMethod ToDataSourceCollectionMethod(DataSourceTranslationDto from, DataSourceCollectionMethod existingDomainObject);
        DataSourceTranslationDto ToDataSourceParameterDto(DataSourceParameter from);
        DataSourceParameter ToDataSourceParameter(DataSourceTranslationDto from, DataSourceParameter existingDomainObject);
        DataSourceTranslationDto ToDataSourceUnitDto(DataSourceUnit from);
        DataSourceUnit ToDataSourceUnit(DataSourceTranslationDto from, DataSourceUnit existingDomainObject);

        ImportTempFileDto ToDto(ImportTempFile fromDomainObject);
        ImportTempFile ToDomainObject(ImportTempFileDto fromDto, ImportTempFile existingDomainObject = null);
        SystemUnitDto ToDto(SystemUnit fromDomainObject);
        SystemUnit ToDomainObject(SystemUnitDto fromDto, SystemUnit existingDomainObject = null);


        UnitDimensionDto ToDto(UnitDimension fromDomainObject);
        UnitDimension ToDomainObject(UnitDimensionDto fromDto, UnitDimension existingDomainObject = null);
        FileVersionDto ToDto(FileVersion fromDomainObject);
        FileVersionFieldDto ToDto(FileVersionField fromDomainObject);
    }
}