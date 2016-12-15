using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Mapping
{
    public class MapHelper : IMapHelper
    {
        public MapHelper()
        {

        }

        public OrganizationDto GetOrganizationDtoFromOrganization(Core.Domain.Organization organization)
        {
            var dto = new OrganizationDto();
            dto.AddressLine1 = organization.AddressLine1;
            dto.AddressLine2 = organization.AddressLine2;
            dto.CityName = organization.CityName;
            dto.State = organization.Jurisdiction.Code;
            dto.OrganizationId = organization.OrganizationId;
            dto.OrganizationTypeId = organization.OrganizationTypeId;

            dto.OrganizationType = this.GetOrganizationTypeDtoFromOrganizationType(organization.OrganizationType);

            dto.OrganizationName = organization.Name;
            dto.ZipCode = organization.ZipCode;
            dto.PhoneNumber = organization.PhoneNumber;
            if (organization.PhoneExt.HasValue)
                dto.PhoneExt = organization.PhoneExt.Value.ToString();
            dto.FaxNumber = organization.FaxNumber;
            dto.WebsiteURL = organization.WebsiteUrl;
            dto.PermitNumber = organization.PermitNumber;
            dto.Signer = organization.Signer;

            return dto;
        }

        public AuthorityDto GetAuthorityDtoFromOrganization(Core.Domain.Organization organization)
        {
            var dto = new AuthorityDto();
            dto.AddressLine1 = organization.AddressLine1;
            dto.AddressLine2 = organization.AddressLine2;
            dto.CityName = organization.CityName;
            //dto.State = organization.Jurisdiction.Code;
            dto.OrganizationId = organization.OrganizationId;
            dto.OrganizationTypeId = organization.OrganizationTypeId;

            dto.OrganizationType = new Dto.OrganizationTypeDto();
            dto.OrganizationType.Name = organization.OrganizationType.Name;
            dto.OrganizationType.Description = organization.OrganizationType.Description;

            dto.OrganizationName = organization.Name;
            dto.ZipCode = organization.ZipCode;
            dto.PhoneNumber = organization.PhoneNumber;
            if (organization.PhoneExt.HasValue)
                dto.PhoneExt = organization.PhoneExt.Value.ToString();
            dto.FaxNumber = organization.FaxNumber;
            dto.WebsiteURL = organization.WebsiteUrl;
            dto.PermitNumber = organization.PermitNumber;
            dto.Signer = organization.Signer;

            return dto;
        }

        public UserDto GetUserDtoFromUserProfile(UserProfile userProfile)
        {
            var dto = new UserDto();
            dto.UserProfileId = userProfile.UserProfileId;
            dto.TitleRole = userProfile.TitleRole;
            dto.FirstName = userProfile.FirstName;
            dto.LastName = userProfile.LastName;
            dto.UserName = userProfile.UserName;
            dto.BusinessName = userProfile.BusinessName;
            dto.AddressLine1 = userProfile.AddressLine1;
            dto.AddressLine2 = userProfile.AddressLine2;
            dto.CityName = userProfile.CityName;
            dto.ZipCode = userProfile.ZipCode;
            if (userProfile.JurisdictionId.HasValue)
                dto.JurisdictionId = userProfile.JurisdictionId.Value;
            dto.PhoneExt = userProfile.PhoneExt;
            dto.IsAccountLocked = userProfile.IsAccountLocked;
            dto.IsAccountResetRequired = userProfile.IsAccountResetRequired;
            dto.IsIdentityProofed = userProfile.IsIdentityProofed;
            dto.IsInternalAccount = userProfile.IsInternalAccount;
            dto.CreationDateTimeUtc = userProfile.CreationDateTimeUtc;
            dto.LastModificationDateTimeUtc = userProfile.LastModificationDateTimeUtc;
            dto.Email = userProfile.Email;
            dto.OldEmailAddress = userProfile.OldEmailAddress;
            dto.EmailConfirmed = userProfile.EmailConfirmed;
            dto.PasswordHash = userProfile.PasswordHash;
            dto.PhoneNumber = userProfile.PhoneNumber;
            if (userProfile.LockoutEndDateUtc.HasValue)
                dto.LockoutEndDateUtc = userProfile.LockoutEndDateUtc.Value;
            dto.LockoutEnabled = userProfile.LockoutEnabled;

            return dto;
        }

        public UserProfile GetUserProfileFromUserDto(UserDto dto)
        {
            var userProfile = new UserProfile();
            //IGNORE userProfile.UserProfileId
            userProfile.FirstName = dto.FirstName;
            userProfile.LastName = dto.LastName;
            userProfile.TitleRole = dto.TitleRole;
            userProfile.BusinessName = dto.BusinessName;
            userProfile.AddressLine1 = dto.AddressLine1;
            userProfile.AddressLine2 = dto.AddressLine2;
            userProfile.CityName = dto.CityName;
            userProfile.ZipCode = dto.ZipCode;
            userProfile.JurisdictionId = dto.JurisdictionId;
            //IGNORE userProfile.Jurisdiction
            userProfile.PhoneNumber = dto.PhoneNumber;
            userProfile.PhoneExt = dto.PhoneExt;
            //IGNORE userProfile.IsAccountLocked
            //IGNORE userProfile.IsAccountResetRequired
            //IGNORE userProfile.IsIdentityProofed
            //IGNORE userProfile.IsInternalAccount
            //IGNORE userProfile.OldEmailAddress
            //IGNORE userProfile.CreationDateTimeUtc
            //IGNORE userProfile.LastModificationDateTimeUtc

            return userProfile;
        }

        public OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(OrganizationRegulatoryProgramUser user)
        {
            var userDto = new OrganizationRegulatoryProgramUserDto();

            userDto.OrganizationRegulatoryProgramUserId = user.OrganizationRegulatoryProgramUserId;
            userDto.UserProfileId = user.UserProfileId;
            userDto.OrganizationRegulatoryProgramId = user.OrganizationRegulatoryProgramId;
            userDto.InviterOrganizationRegulatoryProgramId = user.OrganizationRegulatoryProgramId;
            userDto.IsEnabled = user.IsEnabled;
            userDto.IsRegistrationApproved = user.IsRegistrationApproved;
            userDto.IsRegistrationDenied = user.IsRegistrationDenied;
            userDto.IsRemoved = user.IsRemoved;
            userDto.IsSignatory = user.IsSignatory;
            userDto.RegistrationDateTimeUtc = user.RegistrationDateTimeUtc;

            //Map PermissionGroupDto
            userDto.PermissionGroup = new PermissionGroupDto();
            userDto.PermissionGroup.PermissionGroupId = user.PermissionGroup.PermissionGroupId;
            userDto.PermissionGroup.Name = user.PermissionGroup.Name;
            userDto.PermissionGroup.Description = user.PermissionGroup.Description;
            userDto.PermissionGroup.OrganizationRegulatoryProgramId = user.PermissionGroup.OrganizationRegulatoryProgramId;
            userDto.PermissionGroup.CreationDateTimeUtc = user.PermissionGroup.CreationDateTimeUtc;
            userDto.PermissionGroup.LastModificationDateTimeUtc = user.PermissionGroup.LastModificationDateTimeUtc;

            //Map OrganizationRegulatoryProgramDto
            userDto.OrganizationRegulatoryProgramDto = this.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(user.OrganizationRegulatoryProgram);

            //Map InviterOrganizationRegulatoryProgramDto
            userDto.InviterOrganizationRegulatoryProgramDto = this.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(user.InviterOrganizationRegulatoryProgram);

            return userDto;

        }

        //OrganizationDto, Core.Domain.Organization
        public Core.Domain.Organization GetOrganizationFromOrganizationDto(OrganizationDto dto)
        {
            var org = new Core.Domain.Organization();
            org.OrganizationId = dto.OrganizationTypeId;
            org.OrganizationTypeId = dto.OrganizationTypeId;

            org.OrganizationType = new Core.Domain.OrganizationType();
            org.OrganizationType.Name = dto.OrganizationType.Name;
            org.OrganizationType.Description = dto.OrganizationType.Description;

            org.Name = dto.OrganizationName;
            org.AddressLine1 = dto.AddressLine1;
            org.AddressLine2 = dto.AddressLine2;
            org.CityName = dto.CityName;
            org.ZipCode = dto.ZipCode;
            //org.JurisdictionId = dto.JurisdictionId;
            //org.Jurisdiction = dto.Jurisdiction;
            org.PhoneNumber = dto.PhoneNumber;
            int phoneExtInt;
            if (Int32.TryParse(dto.PhoneExt, out phoneExtInt))
            {
                org.PhoneExt = phoneExtInt;
            }
            org.FaxNumber = dto.FaxNumber;
            //org.WebsiteUrl = dto.WebsiteUrl;
            org.PermitNumber = dto.PermitNumber;
            org.Signer = dto.Signer;
            //org.CreationDateTimeUtc = dto.CreationDateTimeUtc;
            //org.LastModificationDateTimeUtc = dto.LastModificationDateTimeUtc;
            //org.LastModifierUserId = dto.LastModifierUserId;
            //org.OrganizationRegulatoryPrograms = dto.OrganizationRegulatoryPrograms;
            //org.RegulatorOrganizationRegulatoryPrograms = dto.RegulatorOrganizationRegulatoryPrograms;
            //org.OrganizationSettings = dto.OrganizationSettings;

            return org;
        }

        public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(OrganizationRegulatoryProgram org)
        {
            //var dto = new OrganizationRegulatoryProgramDto();
            //dto.OrganizationRegulatoryProgramId = org.
            //dto.RegulatoryProgramId = org.
            //dto.RegulatoryProgramDto = org.
            //dto.OrganizationId = org.
            //dto.RegulatorOrganizationId = org.
            //dto.OrganizationDto = org.
            //dto.RegulatorOrganization = org.
            //dto.IsEnabled = org.
            //dto.IsRemoved = org.
            //dto.AssignedTo = org.
            //IGNORE dto.HasSignatory
            //IGNORE dto.HasAdmin

            var dto = new OrganizationRegulatoryProgramDto();
            dto.OrganizationRegulatoryProgramId = org.OrganizationRegulatoryProgramId;
            dto.RegulatoryProgramId = org.RegulatoryProgramId;

            dto.RegulatoryProgramDto = this.GetProgramDtoFromOrganizationRegulatoryProgram(org.RegulatoryProgram);

            dto.OrganizationId = org.OrganizationId;
            dto.RegulatorOrganizationId = org.RegulatorOrganizationId;

            dto.OrganizationDto = this.GetOrganizationDtoFromOrganization(org.Organization);

            if (org.RegulatorOrganization != null)
            {
                dto.RegulatorOrganization = this.GetOrganizationDtoFromOrganization(org.RegulatorOrganization);
            }

            dto.IsEnabled = org.IsEnabled;
            dto.IsRemoved = org.IsRemoved;
            dto.AssignedTo = org.AssignedTo;

            //IGNORE HasSignatory
            //IGNORE HasAdmin

            return dto;
        }

        public ProgramDto GetProgramDtoFromOrganizationRegulatoryProgram(RegulatoryProgram org)
        {
            var dto = new ProgramDto();
            dto.RegulatoryProgramId = org.RegulatoryProgramId;
            dto.Name = org.Name;
            dto.Description = org.Description;
            return dto;
        }

        public OrganizationTypeDto GetOrganizationTypeDtoFromOrganizationType(OrganizationType orgType)
        {
            var dto = new OrganizationTypeDto();
            dto.Name = orgType.Name;
            dto.Description = orgType.Description;
            return dto;
        }

        public SettingDto GetSettingDtoFromOrganizationRegulatoryProgramSetting(OrganizationRegulatoryProgramSetting setting)
        {
            var dto = new SettingDto();
            dto.TemplateName = (SettingType)Enum.Parse(typeof(SettingType), setting.SettingTemplate.Name);
            dto.OrgTypeName = (OrganizationTypeName)Enum.Parse(typeof(OrganizationTypeName), setting.OrganizationRegulatoryProgram.Organization.OrganizationType.Name);
            dto.Value = setting.Value;
            dto.DefaultValue = setting.SettingTemplate.DefaultValue;
            return dto;
        }

        public SettingDto GetSettingDtoFromOrganizationSetting(OrganizationSetting setting)
        {
            var dto = new SettingDto();
            dto.TemplateName = (SettingType)Enum.Parse(typeof(SettingType), setting.SettingTemplate.Name);
            dto.OrgTypeName = (OrganizationTypeName)Enum.Parse(typeof(OrganizationTypeName), setting.SettingTemplate.OrganizationType.Name);
            dto.Value = setting.Value;
            dto.DefaultValue = setting.SettingTemplate.DefaultValue;
            return dto;
        }

        public InvitationDto GetInvitationDtoFromInvitation(Core.Domain.Invitation invitation)
        {
            var dto = new InvitationDto();
            dto.InvitationId = invitation.InvitationId;
            dto.FirstName = invitation.FirstName;
            dto.LastName = invitation.LastName;
            dto.EmailAddress = invitation.EmailAddress;
            dto.InvitationDateTimeUtc = invitation.InvitationDateTimeUtc;
            //IGNORE ExpiryDateTimeUtc
            dto.SenderOrganizationRegulatoryProgramId = invitation.SenderOrganizationRegulatoryProgramId;
            dto.RecipientOrganizationRegulatoryProgramId = invitation.RecipientOrganizationRegulatoryProgramId;
            //IGNORE ProgramName
            //IGNORE AuthorityName
            //IGNORE IndustryName

            return dto;

        }

        public PermissionGroupDto GetPermissionGroupDtoFromPermissionGroup(PermissionGroup permissionGroup)
        {
            var dto = new PermissionGroupDto();
            dto.PermissionGroupId = permissionGroup.PermissionGroupId;
            dto.Name = permissionGroup.Name;
            dto.Description = permissionGroup.Description;
            dto.OrganizationRegulatoryProgramId = permissionGroup.OrganizationRegulatoryProgramId;
            dto.CreationDateTimeUtc = permissionGroup.CreationDateTimeUtc;
            dto.LastModificationDateTimeUtc = permissionGroup.LastModificationDateTimeUtc;

            return dto;
        }

        public TimeZoneDto GetTimeZoneDtoFromTimeZone(Core.Domain.TimeZone timeZone)
        {
            var dto = new TimeZoneDto();
            dto.TimeZoneId = timeZone.TimeZoneId;
            dto.Name = timeZone.Name;
            return dto;
        }

        public EmailAuditLog GetEmailAuditLogFromEmailAuditLogEntryDto(EmailAuditLogEntryDto dto)
        {
            var emailAuditLog = new EmailAuditLog();
            emailAuditLog.EmailAuditLogId = dto.EmailAuditLogId;
            emailAuditLog.AuditLogTemplateId = dto.AuditLogTemplateId;
            //IGNORE AuditLogTemplate
            emailAuditLog.SenderRegulatoryProgramId = dto.SenderRegulatoryProgramId;
            emailAuditLog.SenderOrganizationId = dto.SenderRegulatorOrganizationId;
            emailAuditLog.SenderRegulatorOrganizationId = dto.SenderRegulatorOrganizationId;
            emailAuditLog.SenderUserProfileId = dto.SenderUserProfileId;
            emailAuditLog.SenderUserName = dto.SenderUserName;
            emailAuditLog.SenderFirstName = dto.SenderFirstName;
            emailAuditLog.SenderLastName = dto.SenderLastName;
            emailAuditLog.SenderEmailAddress = dto.SenderEmailAddress;
            emailAuditLog.RecipientRegulatoryProgramId = dto.RecipientRegulatoryProgramId;
            emailAuditLog.RecipientOrganizationId = dto.RecipientOrganizationId;
            emailAuditLog.RecipientRegulatorOrganizationId = dto.RecipientRegulatorOrganizationId;
            emailAuditLog.RecipientUserProfileId = dto.RecipientUserProfileId;
            emailAuditLog.RecipientUserName = dto.RecipientUserName;
            emailAuditLog.RecipientFirstName = dto.RecipientFirstName;
            emailAuditLog.RecipientLastName = dto.RecipientLastName;
            emailAuditLog.RecipientEmailAddress = dto.RecipientEmailAddress;
            emailAuditLog.Subject = dto.Subject;
            emailAuditLog.Body = dto.Body;
            emailAuditLog.Token = dto.Token;
            emailAuditLog.SentDateTimeUtc = dto.SentDateTimeUtc;

            return emailAuditLog;
        }

        public JurisdictionDto GetJurisdictionDtoFromJurisdiction(Core.Domain.Jurisdiction jurisdiction)
        {
            var dto = new JurisdictionDto();
            dto.JurisdictionId = jurisdiction.JurisdictionId;
            dto.CountryId = jurisdiction.JurisdictionId;
            dto.StateId = jurisdiction.JurisdictionId;
            dto.Code = jurisdiction.Code;
            dto.Name = jurisdiction.Name;
            if (jurisdiction.ParentId.HasValue)
                dto.ParentId = jurisdiction.ParentId.Value;

            return dto;
        }

    }
}
