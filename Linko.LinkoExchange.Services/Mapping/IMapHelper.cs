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
        OrganizationDto GetOrganizationDtoFromOrganization(Core.Domain.Organization organization);
        AuthorityDto GetAuthorityDtoFromOrganization(Core.Domain.Organization organization);
        UserDto GetUserDtoFromUserProfile(UserProfile userProfile);
        UserProfile GetUserProfileFromUserDto(UserDto dto);
        OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(OrganizationRegulatoryProgramUser user);
        OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(OrganizationRegulatoryProgram org);
        SettingDto GetSettingDtoFromOrganizationRegulatoryProgramSetting(OrganizationRegulatoryProgramSetting setting);
        SettingDto GetSettingDtoFromOrganizationSetting(OrganizationSetting setting);
        InvitationDto GetInvitationDtoFromInvitation(Core.Domain.Invitation invitation);
        PermissionGroupDto GetPermissionGroupDtoFromPermissionGroup(PermissionGroup permissionGroup);
        TimeZoneDto GetTimeZoneDtoFromTimeZone(Core.Domain.TimeZone timeZone);
        EmailAuditLog GetEmailAuditLogFromEmailAuditLogEntryDto(EmailAuditLogEntryDto dto);
        JurisdictionDto GetJurisdictionDtoFromJurisdiction(Core.Domain.Jurisdiction jurisdiction);

    }
}
