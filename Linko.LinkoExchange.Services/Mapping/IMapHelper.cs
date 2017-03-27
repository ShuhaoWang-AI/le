using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Mapping
{
    public interface IMapHelper
    {
        OrganizationDto GetOrganizationDtoFromOrganization(Core.Domain.Organization organization, OrganizationDto dto = null);
        AuthorityDto GetAuthorityDtoFromOrganization(Core.Domain.Organization organization, AuthorityDto dto = null);
        UserDto GetUserDtoFromUserProfile(UserProfile userProfile, UserDto dto = null);
        UserProfile GetUserProfileFromUserDto(UserDto dto, UserProfile userProfile = null);
        OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(OrganizationRegulatoryProgramUser user, OrganizationRegulatoryProgramUserDto dto = null);
        OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(OrganizationRegulatoryProgram org, OrganizationRegulatoryProgramDto dto = null);
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
        UnitDto GetUnitDtoFromUnit(Unit unit);
        Core.Domain.ReportElementCategory GetReportElementCategoryFromReportElementCategoryDto(ReportElementCategoryDto cat);
        ReportElementCategoryDto GetReportElementCategoryDtoFromReportElementCategory(Core.Domain.ReportElementCategory cat);

        ReportPackageTemplateElementCategoryDto GetReportPackageTemplateElementCategoryDtoFromReportPackageTemplateElementCategory(ReportPackageTemplateElementCategory cat);
        ReportPackageTemplateElementCategory GetReportPackageTemplateElementCategoryFromReportPackageTemplateElementCategoryDto(ReportPackageTemplateElementCategoryDto cat);

        ReportPackageTemplateElementTypeDto GetReportPackageTemplateElememtTypeDtoFromReportPackageTemplateElementType(ReportPackageTemplateElementType rptet);
        ReportPackageTemplateElementType GetReportPackageTemplateElememtTypeFromReportPackageTemplateElementTypeDto(ReportPackageTemplateElementTypeDto rptetDto);

        CtsEventTypeDto GetCtsEventTypeDtoFromEventType(CtsEventType ctsEventType);
        CtsEventType GetCtsEventTypeFromEventTypeDto(CtsEventTypeDto ctsEventTypeDto);

        ReportPackageTemplateDto GetReportPackageTemplateDtoFromReportPackageTemplate(ReportPackageTemplate reportPackageTemplate);
        ReportPackageTemplate GetReportPackageTemplateFromReportPackageTemplateDto(ReportPackageTemplateDto reportPackageTemplateDto);

        ReportPackageTemplateElementTypeDto GetReportPackageTemplateElmentTypeDtoFromReportPackageTemplateType(ReportPackageTemplateElementType rptet);
        ReportPackageTemplateElementType GetReportPackageTemplateElmentTypeFromReportPackageTemplateTypeDto(ReportPackageTemplateElementTypeDto rptet);

        ReportElementTypeDto GetReportElementTypeDtoFromReportElementType(ReportElementType reportElementType);
        ReportElementType GetReportElementTypeFromReportElementTypeDto(ReportElementTypeDto reportElementTypeDto, ReportElementType existingReportElementType = null);

        ReportPackageTemplateAssignment GetReportPackageTemplateAssignmentFromReportPackageTemplateAssignmentDto(ReportPackageTemplateAssignmentDto rptDto);

        ReportPackageTemplateAssignmentDto GetReportPackageTemplateAssignmentDtoFromReportPackageTemplateAssignment(ReportPackageTemplateAssignment rpt);
        FileStoreDto GetFileStoreDtoFromFileStore(Core.Domain.FileStore fileStore);
        Core.Domain.FileStore GetFileStoreFromFileStoreDto(FileStoreDto fileStoreDto);
    }
}
