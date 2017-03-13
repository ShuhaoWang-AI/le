using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Parameter;
using Linko.LinkoExchange.Services.Report;
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

        public OrganizationDto GetOrganizationDtoFromOrganization(Core.Domain.Organization organization, OrganizationDto dto = null)
        {
            if (dto == null)
            {
                dto = new OrganizationDto();
                dto.OrganizationType = this.GetOrganizationTypeDtoFromOrganizationType(organization.OrganizationType);
            }
            else
            {
                dto.OrganizationType = this.GetOrganizationTypeDtoFromOrganizationType(organization.OrganizationType, dto.OrganizationType);
            }

            dto.AddressLine1 = organization.AddressLine1;
            dto.AddressLine2 = organization.AddressLine2;
            dto.CityName = organization.CityName;
            dto.State = organization.Jurisdiction.Code;
            dto.OrganizationId = organization.OrganizationId;
            dto.OrganizationTypeId = organization.OrganizationTypeId;

            dto.OrganizationName = organization.Name;
            dto.ZipCode = organization.ZipCode;
            dto.PhoneNumber = organization.PhoneNumber;
            if (organization.PhoneExt.HasValue)
                dto.PhoneExt = organization.PhoneExt.Value.ToString();
            dto.FaxNumber = organization.FaxNumber;
            dto.WebsiteURL = organization.WebsiteUrl;
            dto.PermitNumber = organization.PermitNumber;
            dto.Signer = organization.Signer;
            dto.Classification = organization.Classification;
            return dto;
        }

        public AuthorityDto GetAuthorityDtoFromOrganization(Core.Domain.Organization organization, AuthorityDto dto = null)
        {
            if (dto == null)
            {
                dto = new AuthorityDto();
                dto.OrganizationType = new Dto.OrganizationTypeDto();
            }
            dto.AddressLine1 = organization.AddressLine1;
            dto.AddressLine2 = organization.AddressLine2;
            dto.CityName = organization.CityName;
            //dto.State = organization.Jurisdiction.Code;
            dto.OrganizationId = organization.OrganizationId;
            dto.OrganizationTypeId = organization.OrganizationTypeId;

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

        public UserDto GetUserDtoFromUserProfile(UserProfile userProfile, UserDto dto = null)
        {
            if (dto == null)
            {
                dto = new UserDto();
            }
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

        public UserProfile GetUserProfileFromUserDto(UserDto dto, UserProfile userProfile = null)
        {
            if (userProfile == null)
            {
                userProfile = new UserProfile();
            }
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

        public OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser
            (OrganizationRegulatoryProgramUser user, OrganizationRegulatoryProgramUserDto userDto = null)
        {
            if (userDto == null)
            {
                userDto = new OrganizationRegulatoryProgramUserDto();
                userDto.PermissionGroup = new PermissionGroupDto();
            }

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
            if (user.PermissionGroup != null)
            {
                userDto.PermissionGroup.PermissionGroupId = user.PermissionGroup.PermissionGroupId;
                userDto.PermissionGroup.Name = user.PermissionGroup.Name;
                userDto.PermissionGroup.Description = user.PermissionGroup.Description;
                userDto.PermissionGroup.OrganizationRegulatoryProgramId = user.PermissionGroup.OrganizationRegulatoryProgramId;
                userDto.PermissionGroup.CreationDateTimeUtc = user.PermissionGroup.CreationDateTimeUtc;
                userDto.PermissionGroup.LastModificationDateTimeUtc = user.PermissionGroup.LastModificationDateTimeUtc;
            }

            //Map OrganizationRegulatoryProgramDto
            userDto.OrganizationRegulatoryProgramDto = this.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(user.OrganizationRegulatoryProgram);

            //Map InviterOrganizationRegulatoryProgramDto
            userDto.InviterOrganizationRegulatoryProgramDto = this.GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(user.InviterOrganizationRegulatoryProgram);

            return userDto;

        }

        public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram
            (OrganizationRegulatoryProgram org, OrganizationRegulatoryProgramDto dto = null)
        {
            if (dto == null)
            {
                dto = new OrganizationRegulatoryProgramDto();
            }
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

        public ProgramDto GetProgramDtoFromOrganizationRegulatoryProgram(RegulatoryProgram org, ProgramDto dto = null)
        {
            if (dto == null)
            {
                dto = new ProgramDto();
            }
            dto.RegulatoryProgramId = org.RegulatoryProgramId;
            dto.Name = org.Name;
            dto.Description = org.Description;
            return dto;
        }

        public OrganizationTypeDto GetOrganizationTypeDtoFromOrganizationType(OrganizationType orgType, OrganizationTypeDto dto = null)
        {
            if (dto == null)
            {
                dto = new OrganizationTypeDto();
            }
            dto.Name = orgType.Name;
            dto.Description = orgType.Description;
            return dto;
        }

        public SettingDto GetSettingDtoFromOrganizationRegulatoryProgramSetting(OrganizationRegulatoryProgramSetting setting, SettingDto dto = null)
        {
            if (dto == null)
            {
                dto = new SettingDto();
            }
            dto.TemplateName = (SettingType) Enum.Parse(typeof(SettingType), setting.SettingTemplate.Name);
            dto.OrgTypeName = (OrganizationTypeName) Enum.Parse(typeof(OrganizationTypeName), setting.OrganizationRegulatoryProgram.Organization.OrganizationType.Name);
            dto.Value = setting.Value;
            dto.DefaultValue = setting.SettingTemplate.DefaultValue;
            return dto;
        }

        public SettingDto GetSettingDtoFromOrganizationSetting(OrganizationSetting setting, SettingDto dto = null)
        {
            if (dto == null)
            {
                dto = new SettingDto();
            }
            dto.TemplateName = (SettingType) Enum.Parse(typeof(SettingType), setting.SettingTemplate.Name);
            dto.OrgTypeName = (OrganizationTypeName) Enum.Parse(typeof(OrganizationTypeName), setting.SettingTemplate.OrganizationType.Name);
            dto.Value = setting.Value;
            dto.DefaultValue = setting.SettingTemplate.DefaultValue;
            return dto;
        }

        public InvitationDto GetInvitationDtoFromInvitation(Core.Domain.Invitation invitation, InvitationDto dto = null)
        {
            if (dto == null)
            {
                dto = new InvitationDto();
            }
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

        public PermissionGroupDto GetPermissionGroupDtoFromPermissionGroup(PermissionGroup permissionGroup, PermissionGroupDto dto = null)
        {
            if (dto == null)
            {
                dto = new PermissionGroupDto();
            }
            dto.PermissionGroupId = permissionGroup.PermissionGroupId;
            dto.Name = permissionGroup.Name;
            dto.Description = permissionGroup.Description;
            dto.OrganizationRegulatoryProgramId = permissionGroup.OrganizationRegulatoryProgramId;
            dto.CreationDateTimeUtc = permissionGroup.CreationDateTimeUtc;
            dto.LastModificationDateTimeUtc = permissionGroup.LastModificationDateTimeUtc;

            return dto;
        }

        public TimeZoneDto GetTimeZoneDtoFromTimeZone(Core.Domain.TimeZone timeZone, TimeZoneDto dto = null)
        {
            if (dto == null)
            {
                dto = new TimeZoneDto();
            }
            dto.TimeZoneId = timeZone.TimeZoneId;
            dto.Name = timeZone.Name;
            return dto;
        }

        public EmailAuditLog GetEmailAuditLogFromEmailAuditLogEntryDto(EmailAuditLogEntryDto dto, EmailAuditLog emailAuditLog = null)
        {
            if (emailAuditLog == null)
            {
                emailAuditLog = new EmailAuditLog();
            }
            emailAuditLog.EmailAuditLogId = dto.EmailAuditLogId;
            emailAuditLog.AuditLogTemplateId = dto.AuditLogTemplateId;
            //IGNORE AuditLogTemplate
            emailAuditLog.SenderRegulatoryProgramId = dto.SenderRegulatoryProgramId;
            emailAuditLog.SenderOrganizationId = dto.SenderOrganizationId;
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

        public JurisdictionDto GetJurisdictionDtoFromJurisdiction(Core.Domain.Jurisdiction jurisdiction, JurisdictionDto dto = null)
        {
            if (dto == null)
            {
                dto = new JurisdictionDto();
            }
            dto.JurisdictionId = jurisdiction.JurisdictionId;
            dto.CountryId = jurisdiction.JurisdictionId;
            dto.StateId = jurisdiction.JurisdictionId;
            dto.Code = jurisdiction.Code;
            dto.Name = jurisdiction.Name;
            if (jurisdiction.ParentId.HasValue)
                dto.ParentId = jurisdiction.ParentId.Value;

            return dto;
        }

        public CromerrAuditLog GetCromerrAuditLogFromCromerrAuditLogEntryDto(CromerrAuditLogEntryDto dto, CromerrAuditLog cromerrAuditLog = null)
        {
            if (cromerrAuditLog == null)
            {
                cromerrAuditLog = new CromerrAuditLog();
            }

            cromerrAuditLog.AuditLogTemplateId = dto.AuditLogTemplateId;
            cromerrAuditLog.RegulatoryProgramId = dto.RegulatoryProgramId;
            cromerrAuditLog.OrganizationId = dto.OrganizationId;
            cromerrAuditLog.RegulatorOrganizationId = dto.RegulatorOrganizationId;
            cromerrAuditLog.UserProfileId = dto.UserProfileId;
            cromerrAuditLog.UserName = dto.UserName;
            cromerrAuditLog.UserFirstName = dto.UserFirstName;
            cromerrAuditLog.UserLastName = dto.UserLastName;
            cromerrAuditLog.UserEmailAddress = dto.UserEmailAddress;
            cromerrAuditLog.IPAddress = dto.IPAddress;
            cromerrAuditLog.HostName = dto.HostName;
            cromerrAuditLog.Comment = dto.Comment;
            cromerrAuditLog.LogDateTimeUtc = dto.LogDateTimeUtc;

            return cromerrAuditLog;
        }

        public CromerrAuditLogEntryDto GetCromerrAuditLogDtoFromCromerrAuditLog(CromerrAuditLog cromerrAuditLog, CromerrAuditLogEntryDto dto = null)
        {
            if (dto == null)
            {
                dto = new CromerrAuditLogEntryDto();
            }

            dto.CromerrAuditLogId = cromerrAuditLog.CromerrAuditLogId;
            dto.AuditLogTemplateId = cromerrAuditLog.AuditLogTemplateId;
            dto.RegulatoryProgramId = cromerrAuditLog.RegulatoryProgramId;
            dto.OrganizationId = cromerrAuditLog.OrganizationId;
            dto.RegulatorOrganizationId = cromerrAuditLog.RegulatorOrganizationId;
            dto.UserProfileId = cromerrAuditLog.UserProfileId;
            dto.EventCategory = cromerrAuditLog.AuditLogTemplate.EventCategory;
            dto.EventType = cromerrAuditLog.AuditLogTemplate.EventType;
            dto.UserName = cromerrAuditLog.UserName;
            dto.UserFirstName = cromerrAuditLog.UserFirstName;
            dto.UserLastName = cromerrAuditLog.UserLastName;
            dto.UserEmailAddress = cromerrAuditLog.UserEmailAddress;
            dto.IPAddress = cromerrAuditLog.IPAddress;
            dto.HostName = cromerrAuditLog.HostName;
            dto.Comment = cromerrAuditLog.Comment;
            dto.LogDateTimeUtc = cromerrAuditLog.LogDateTimeUtc;

            return dto;
        }

        public ParameterDto GetParameterDtoFromParameter(Core.Domain.Parameter parameter)
        {
            var paramDto = new ParameterDto();
            paramDto.ParameterId = parameter.ParameterId;
            paramDto.Name = parameter.Name;
            paramDto.Description = parameter.Description;
            paramDto.DefaultUnit = GetUnitDtoFromUnit(parameter.DefaultUnit);
            paramDto.TrcFactor = parameter.TrcFactor;
            paramDto.OrganizationRegulatoryProgram = GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(parameter.OrganizationRegulatoryProgram);
            paramDto.IsRemoved = parameter.IsRemoved;
            paramDto.CreationDateTimeUtc = parameter.CreationDateTimeUtc;
            paramDto.LastModificationDateTimeUtc = parameter.LastModificationDateTimeUtc;
            paramDto.LastModifierUserId = parameter.LastModifierUserId;

            return paramDto;
        }

        public ParameterGroupDto GetParameterGroupDtoFromParameterGroup(ParameterGroup parameterGroup)
        {
            var paramGroupDto = new ParameterGroupDto();
            paramGroupDto.ParameterGroupId = parameterGroup.ParameterGroupId;
            paramGroupDto.Name = parameterGroup.Name;
            paramGroupDto.Description = parameterGroup.Description;
            paramGroupDto.OrganizationRegulatoryProgram = GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(parameterGroup.OrganizationRegulatoryProgram);
            paramGroupDto.IsActive = parameterGroup.IsActive;
            paramGroupDto.CreationDateTimeUtc = parameterGroup.CreationDateTimeUtc;
            paramGroupDto.LastModificationDateTimeUtc = parameterGroup.LastModificationDateTimeUtc;
            paramGroupDto.LastModifierUserId = parameterGroup.LastModifierUserId;

            paramGroupDto.Parameters = new List<ParameterDto>();
            foreach (var paramAssocation in parameterGroup.ParameterGroupParameters)
            {
                paramGroupDto.Parameters.Add(GetParameterDtoFromParameter(paramAssocation.Parameter));
            }

            return paramGroupDto;

        }

        public ParameterGroup GetParameterGroupFromParameterGroupDto(ParameterGroupDto parameterGroupDto, ParameterGroup parameterGroup = null)
        {
            if (parameterGroup == null)
            {
                parameterGroup = new ParameterGroup();
            }

            //TO-DO: make sure the right properties are being set
            parameterGroup.Name = parameterGroupDto.Name;
            parameterGroup.Description = parameterGroupDto.Description;
            //OrganizationRegulatoryProgramId
            parameterGroup.IsActive = parameterGroupDto.IsActive;
            //CreationDateTimeUtc
            parameterGroup.LastModificationDateTimeUtc = parameterGroupDto.LastModificationDateTimeUtc;
            parameterGroup.LastModifierUserId = parameterGroupDto.LastModifierUserId;

            parameterGroup.ParameterGroupParameters = new List<ParameterGroupParameter>();
            foreach (var param in parameterGroupDto.Parameters)
            {
                parameterGroup.ParameterGroupParameters.Add(new ParameterGroupParameter() { ParameterId = param.ParameterId });
            }

            return parameterGroup;
        }

        private UnitDto GetUnitDtoFromUnit(Unit unit)
        {
            var unitDto = new UnitDto();
            unitDto.UnitId = unit.UnitId;
            unitDto.Name = unit.Name;
            unitDto.Description = unit.Description;
            unitDto.IsFlow = unit.IsFlow;
            unitDto.OrganizationRegulatoryProgram = GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(unit.OrganizationRegulatoryProgram);
            unitDto.IsRemoved = unit.IsRemoved;
            unitDto.CreationDateTimeUtc = unit.CreationDateTimeUtc;
            unitDto.LastModificationDateTimeUtc = unit.LastModificationDateTimeUtc;
            unitDto.LastModifierUserId = unit.LastModifierUserId;

            return unitDto;
        }

        public ReportElementType GetReportElementTypeFromCertificationTypeDto(CertificationTypeDto certificationType, ReportElementType reportElementType = null)
        {
            if (reportElementType == null)
            {
                reportElementType = new ReportElementType();
            }

            reportElementType.Name  = certificationType.CertificationTypeName;
            reportElementType.Description  = certificationType.CertificationTypeDescription;
            reportElementType.Content  = certificationType.CertificationText;
            reportElementType.IsContentProvided = true;
            reportElementType.CtsEventTypeId  = certificationType.CtsEventType.CtsEventTypeId;
            //IGNORE reportElementType.CtsEventType
            reportElementType.ReportElementCategoryId = certificationType.ReportElementCategoryId;
            //IGNORE reportElementType.ReportElementCategory
            reportElementType.OrganizationRegulatoryProgramId = certificationType.OrganizationRegulatoryProgramId;
            //IGNORE reportElementType.OrganizationRegulatoryProgram
            reportElementType.CreationDateTimeUtc = certificationType.CreationDateTimeUtc;
            reportElementType.LastModificationDateTimeUtc  = certificationType.LastModificationDateTimeUtc;
            reportElementType.LastModifierUserId  = certificationType.LastModifierUserId;

            return reportElementType;
        }

        private CtsEventTypeDto GetCtsEventTypeDtoFromCtsEventType(CtsEventType ctsEventType)
        {
            var mappedDto = new CtsEventTypeDto();
            mappedDto.CtsEventTypeId = ctsEventType.CtsEventTypeId;
            mappedDto.Name = ctsEventType.Name;
            mappedDto.Description = ctsEventType.Description;
            return mappedDto;
        }

        public CertificationTypeDto GetCertificationTypeDtoFromReportElementType(ReportElementType reportElementType)
        {
            var mappedCertType = new CertificationTypeDto();
            mappedCertType.ReportElementCategoryId = reportElementType.ReportElementCategoryId;
            mappedCertType.CertificationTypeID = reportElementType.ReportElementTypeId;
            mappedCertType.CertificationTypeName = reportElementType.Name;
            mappedCertType.CertificationTypeDescription = reportElementType.Description;
            mappedCertType.CtsEventType = GetCtsEventTypeDtoFromCtsEventType(reportElementType.CtsEventType);
            mappedCertType.CertificationText = reportElementType.Content;
            //mappedCertType.IsDeleted = row removed via hard delete? (TO-DO: confirm)
            mappedCertType.OrganizationRegulatoryProgramId = reportElementType.OrganizationRegulatoryProgramId;
            mappedCertType.CreationDateTimeUtc = reportElementType.CreationDateTimeUtc;
            mappedCertType.LastModificationDateTimeUtc = reportElementType.LastModificationDateTimeUtc;
            mappedCertType.LastModifierUserId = reportElementType.LastModifierUserId;
            return mappedCertType;
        }

        public AttachmentTypeDto GetAttachmentTypeDtoFromReportElementType(ReportElementType reportElementType)
        {
            var mappedAttachmentType = new AttachmentTypeDto();
            mappedAttachmentType.ReportElementCategoryId = reportElementType.ReportElementCategoryId;
            mappedAttachmentType.AttachmentTypeID = reportElementType.ReportElementTypeId;
            mappedAttachmentType.AttachmentTypeName = reportElementType.Name;
            mappedAttachmentType.AttachmentTypeDescription = reportElementType.Description;
            mappedAttachmentType.CtsEventType = GetCtsEventTypeDtoFromCtsEventType(reportElementType.CtsEventType);
            //mappedCertType.IsDeleted = row removed via hard delete? (TO-DO: confirm)
            mappedAttachmentType.OrganizationRegulatoryProgramId = reportElementType.OrganizationRegulatoryProgramId;
            mappedAttachmentType.CreationDateTimeUtc = reportElementType.CreationDateTimeUtc;
            mappedAttachmentType.LastModificationDateTimeUtc = reportElementType.LastModificationDateTimeUtc;
            mappedAttachmentType.LastModifierUserId = reportElementType.LastModifierUserId;
            return mappedAttachmentType;
        }

        public ReportElementCategoryDto GetReportElementCategoryDtoFromReportElementCategory(Core.Domain.ReportPackageTemplateElementCategory reportPackageTmeplateElementCategory)
        {
            //TODO
            return new ReportElementCategoryDto();
        }

        public CtsEventTypeDto GetEventTypeDtoFromEventType(CtsEventType ctsEventType)
        {
            //TODO 
            if (ctsEventType == null)
            {
                return null;
            }

            var ctsEventTypeDto = new CtsEventTypeDto();
            return ctsEventTypeDto;
        }

        public CtsEventType GetEventTypeFromEventTypeDto(CtsEventTypeDto ctsEventTypeDto)
        {
            //TODO 
            if (ctsEventTypeDto == null)
            {
                return null;
            }

            var ctsEventType = new CtsEventType();
            return ctsEventType;
        }

        public ReportPackageTemplateDto GetReportPackageTemplateDtoFromReportPackageTemplate(ReportPackageTemplate reportPackageTemplate)
        {
            if (reportPackageTemplate == null)
            {
                return null;
            }

            var rpt = new Dto.ReportPackageTemplateDto();
            // TODO:  Add other fields that can be convert here

            rpt.TemplateName = reportPackageTemplate.Name;
            rpt.LastModifiedDate = reportPackageTemplate.LastModificationDateTimeUtc;
            rpt.CtsEventType = GetEventTypeDtoFromEventType(reportPackageTemplate.CtsEventType);
            rpt.Description = reportPackageTemplate.Description;
            rpt.EffectiveDate = reportPackageTemplate.EffectiveDateTimeUtc;
            rpt.Id = reportPackageTemplate.ReportPackageTemplateId;

            return rpt;
        }

        public ReportPackageTemplate GetReportPackageTemplateFromReportPackageTemplateDto(ReportPackageTemplateDto reportPackageTemplateDto)
        {
            if (reportPackageTemplateDto == null)
            {
                return null;
            }

            var rpt = new ReportPackageTemplate();
            // TODO:  Add other fields that can be convert here

            rpt.Name = reportPackageTemplateDto.TemplateName;
            rpt.LastModificationDateTimeUtc = reportPackageTemplateDto.LastModifiedDate;

            rpt.CtsEventType = GetEventTypeFromEventTypeDto(reportPackageTemplateDto.CtsEventType);
            rpt.Description = reportPackageTemplateDto.Description;
            rpt.EffectiveDateTimeUtc = reportPackageTemplateDto.EffectiveDate;
            rpt.ReportPackageTemplateId = reportPackageTemplateDto.Id;

            return rpt;
        }

    }
}
