using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Mapping
{
    public class MapHelper : IMapHelper
    {
        public OrganizationDto GetOrganizationDtoFromOrganization(Core.Domain.Organization organization, OrganizationDto dto = null)
        {
            if (dto == null)
            {
                dto = new OrganizationDto();
                dto.OrganizationType = GetOrganizationTypeDtoFromOrganizationType(organization.OrganizationType);
            }
            else
            {
                dto.OrganizationType = GetOrganizationTypeDtoFromOrganizationType(organization.OrganizationType, dto.OrganizationType);
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
                dto.OrganizationType = new OrganizationTypeDto();
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
            userDto.OrganizationRegulatoryProgramDto = GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(user.OrganizationRegulatoryProgram);

            //Map InviterOrganizationRegulatoryProgramDto
            userDto.InviterOrganizationRegulatoryProgramDto = GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(user.InviterOrganizationRegulatoryProgram);

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
            dto.RegulatoryProgramDto = GetProgramDtoFromOrganizationRegulatoryProgram(org.RegulatoryProgram);
            dto.OrganizationId = org.OrganizationId;
            dto.RegulatorOrganizationId = org.RegulatorOrganizationId;

            dto.OrganizationDto = GetOrganizationDtoFromOrganization(org.Organization);

            if (org.RegulatorOrganization != null)
            {
                dto.RegulatorOrganization = GetOrganizationDtoFromOrganization(org.RegulatorOrganization);
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
            dto.TemplateName = (SettingType)Enum.Parse(typeof(SettingType), setting.SettingTemplate.Name);
            dto.OrgTypeName = (OrganizationTypeName)Enum.Parse(typeof(OrganizationTypeName), setting.OrganizationRegulatoryProgram.Organization.OrganizationType.Name);
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
            dto.TemplateName = (SettingType)Enum.Parse(typeof(SettingType), setting.SettingTemplate.Name);
            dto.OrgTypeName = (OrganizationTypeName)Enum.Parse(typeof(OrganizationTypeName), setting.SettingTemplate.OrganizationType.Name);
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
            if (parameter.DefaultUnit != null)
            {
                paramDto.DefaultUnit = GetUnitDtoFromUnit(parameter.DefaultUnit);
            }
            paramDto.TrcFactor = parameter.TrcFactor;
            paramDto.OrganizationRegulatoryProgramId = parameter.OrganizationRegulatoryProgramId;
            paramDto.IsRemoved = parameter.IsRemoved;

            return paramDto;
        }

        public ParameterGroupDto GetParameterGroupDtoFromParameterGroup(ParameterGroup parameterGroup)
        {
            var paramGroupDto = new ParameterGroupDto();
            paramGroupDto.ParameterGroupId = parameterGroup.ParameterGroupId;
            paramGroupDto.Name = parameterGroup.Name;
            paramGroupDto.Description = parameterGroup.Description;
            paramGroupDto.OrganizationRegulatoryProgramId = parameterGroup.OrganizationRegulatoryProgramId;
            paramGroupDto.IsActive = parameterGroup.IsActive;

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
                parameterGroup.ParameterGroupParameters = new List<ParameterGroupParameter>();
            }

            parameterGroup.Name = parameterGroupDto.Name;
            parameterGroup.Description = parameterGroupDto.Description;
            parameterGroup.IsActive = parameterGroupDto.IsActive;
            foreach (var param in parameterGroupDto.Parameters)
            {
                parameterGroup.ParameterGroupParameters.Add(new ParameterGroupParameter() { ParameterId = param.ParameterId });
            }

            return parameterGroup;
        }

        public UnitDto GetUnitDtoFromUnit(Core.Domain.Unit unit)
        {
            var unitDto = new UnitDto();
            unitDto.UnitId = unit.UnitId;
            unitDto.Name = unit.Name;
            unitDto.Description = unit.Description;
            unitDto.IsFlowUnit = unit.IsFlowUnit;
            unitDto.OrganizationId = unit.OrganizationId;
            unitDto.IsRemoved = unit.IsRemoved;
            unitDto.CreationDateTimeUtc = unit.CreationDateTimeUtc;
            unitDto.LastModificationDateTimeUtc = unit.LastModificationDateTimeUtc;
            unitDto.LastModifierUserId = unit.LastModifierUserId;

            return unitDto;
        }


        private CtsEventTypeDto GetCtsEventTypeDtoFromCtsEventType(CtsEventType ctsEventType)
        {
            if (ctsEventType == null)
            {
                return null;
            }

            var mappedDto = new CtsEventTypeDto
            {
                CtsEventTypeId = ctsEventType.CtsEventTypeId,
                Name = ctsEventType.Name,
                CtsEventCategoryName = ctsEventType.CtsEventCategoryName,
                Description = ctsEventType.Description
            };
            return mappedDto;
        }

        public ReportElementCategoryDto GetReportElementCategoryDtoFromReportElementCategory(ReportElementCategory cat)
        {
            if (cat == null)
            {
                return null;
            }

            var reportElementCategory = new ReportElementCategoryDto();
            reportElementCategory.ReportElementCategoryId = cat.ReportElementCategoryId;
            reportElementCategory.Name = cat.Name;
            reportElementCategory.Description = cat.Description;
            reportElementCategory.CreationDateTimeUtc = cat.CreationDateTimeUtc;
            reportElementCategory.LastModificationDateTimeUtc = cat.LastModificationDateTimeUtc;
            reportElementCategory.LastModifierUserId = cat.LastModifierUserId;

            return reportElementCategory;
        }

        public ReportPackageTemplateElementCategoryDto GetReportPackageTemplateElementCategoryDtoFromReportPackageTemplateElementCategory(ReportPackageTemplateElementCategory cat)
        {
            if (cat == null)
            {
                return null;
            }

            var reportPackageTemplateElementCategory = new ReportPackageTemplateElementCategoryDto();
            reportPackageTemplateElementCategory.ReportPackageTemplateElementCategoryId = cat.ReportPackageTemplateElementCategoryId;

            reportPackageTemplateElementCategory.ReportPackageTemplateId = cat.ReportPackageTemplateId;

            reportPackageTemplateElementCategory.ReportElementCategoryId = cat.ReportElementCategoryId;
            reportPackageTemplateElementCategory.ReportElementCategory = GetReportElementCategoryDtoFromReportElementCategory(cat.ReportElementCategory);

            reportPackageTemplateElementCategory.SortOrder = cat.SortOrder;

            //// deep naviagation cause error  
            //var dtos = new List<ReportPackageTemplateElementTypeDto>();
            //foreach (var c in cat.ReportPackageTemplateElementTypes)
            //{
            //    dtos.Add(this.GetReportPackageTemplateElememtTypeDtoFromReportPackageTemplateElementType(c));
            //}
            //reportPackageTemplateElementCategory.ReportPackageTemplateElementTypes = dtos;

            return reportPackageTemplateElementCategory;
        }
        public ReportPackageTemplateElementTypeDto GetReportPackageTemplateElememtTypeDtoFromReportPackageTemplateElementType(ReportPackageTemplateElementType rptet)
        {
            if (rptet == null)
            {
                return null;
            }

            var rptetDto = new ReportPackageTemplateElementTypeDto();
            rptetDto.ReportPackageTemplateElementTypeId = rptet.ReportPackageTemplateElementTypeId;
            rptetDto.ReportPackageTemplateElementCategoryId = rptet.ReportPackageTemplateElementCategoryId;
            rptetDto.ReportPackageTemplateElementCategory = GetReportPackageTemplateElementCategoryDtoFromReportPackageTemplateElementCategory(rptet.ReportPackageTemplateElementCategory);
            rptetDto.ReportElementTypeId = rptet.ReportElementTypeId;
            rptetDto.ReportElementType = GetReportElementTypeDtoFromReportElementType(rptet.ReportElementType);
            rptetDto.IsRequired = rptet.IsRequired;
            rptetDto.SortOrder = rptet.SortOrder;
            return rptetDto;
        }

        public ReportPackageTemplateElementType GetReportPackageTemplateElememtTypeFromReportPackageTemplateElementTypeDto(ReportPackageTemplateElementTypeDto rptetDto)
        {
            if (rptetDto == null)
            {
                return null;
            }

            var rptet = new ReportPackageTemplateElementType();
            rptet.ReportPackageTemplateElementTypeId = rptetDto.ReportPackageTemplateElementTypeId;
            rptet.ReportPackageTemplateElementCategoryId = rptetDto.ReportPackageTemplateElementCategoryId;
            rptet.ReportPackageTemplateElementCategory = GetReportPackageTemplateElementCategoryFromReportPackageTemplateElementCategoryDto(rptetDto.ReportPackageTemplateElementCategory);
            rptet.ReportElementTypeId = rptetDto.ReportElementTypeId;
            rptet.ReportElementType = GetReportElementTypeFromReportElementTypeDto(rptetDto.ReportElementType);
            rptet.IsRequired = rptetDto.IsRequired;
            rptet.SortOrder = rptetDto.SortOrder;
            return rptet;
        }

        public ReportPackageTemplateElementCategory GetReportPackageTemplateElementCategoryFromReportPackageTemplateElementCategoryDto(ReportPackageTemplateElementCategoryDto cat)
        {
            if (cat == null)
            {
                return null;
            }

            var reportPackageTemplateElementCategory = new ReportPackageTemplateElementCategory
            {
                ReportPackageTemplateElementCategoryId = cat.ReportPackageTemplateElementCategoryId,
                ReportPackageTemplateId = cat.ReportPackageTemplateId,
                ReportPackageTemplate =
                    GetReportPackageTemplateFromReportPackageTemplateDto(cat.ReportPackageTemplate),
                ReportElementCategoryId = cat.ReportElementCategoryId,
                ReportElementCategory =
                    GetReportElementCategoryFromReportElementCategoryDto(cat.ReportElementCategory),
                SortOrder = cat.SortOrder
            };

            //var rptetps = new List<ReportPackageTemplateElementType>();
            //foreach (var rptetDto in cat.ReportPackageTemplateElementTypes)
            //{
            //    rptetps.Add(this.GetReportPackageTemplateElememtTypeFromReportPackageTemplateElementTypeDto(rptetDto));
            //}

            //reportPackageTemplateElementCategory.ReportPackageTemplateElementTypes = rptetps;

            return reportPackageTemplateElementCategory;
        }

        public ReportElementCategory GetReportElementCategoryFromReportElementCategoryDto(ReportElementCategoryDto cat)
        {
            if (cat == null)
            {
                return null;
            }

            var reportElementCategory = new ReportElementCategory
            {
                ReportElementCategoryId = cat.ReportElementCategoryId,
                Name = cat.Name,
                Description = cat.Description,
                CreationDateTimeUtc = cat.CreationDateTimeUtc,
                LastModificationDateTimeUtc = cat.LastModificationDateTimeUtc,
                LastModifierUserId = cat.LastModifierUserId
            };

            return reportElementCategory;
        }

        public CtsEventTypeDto GetCtsEventTypeDtoFromEventType(CtsEventType ctsEventType)
        {
            if (ctsEventType == null)
            {
                return null;
            }

            var ctsEventTypeDto = new CtsEventTypeDto
            {
                CtsEventTypeId = ctsEventType.CtsEventTypeId,
                Name = ctsEventType.Name,
                CtsEventCategoryName = ctsEventType.CtsEventCategoryName,
                Description = ctsEventType.Description
            };

            return ctsEventTypeDto;
        }

        public CtsEventType GetCtsEventTypeFromEventTypeDto(CtsEventTypeDto ctsEventTypeDto)
        {
            if (ctsEventTypeDto == null)
            {
                return null;
            }

            var ctsEventType = new CtsEventType
            {
                CtsEventTypeId = ctsEventTypeDto.CtsEventTypeId,
                Name = ctsEventTypeDto.Name,
                CtsEventCategoryName = ctsEventTypeDto.CtsEventCategoryName,
                Description = ctsEventTypeDto.Description
            };

            return ctsEventType;
        }

        public ReportPackageTemplateDto GetReportPackageTemplateDtoFromReportPackageTemplate(ReportPackageTemplate reportPackageTemplate)
        {
            if (reportPackageTemplate == null)
            {
                return null;
            }

            var rpt = new ReportPackageTemplateDto
            {
                ReportPackageTemplateId = reportPackageTemplate.ReportPackageTemplateId,
                Name = reportPackageTemplate.Name,
                Description = reportPackageTemplate.Description,
                RetirementDateTimeUtc = reportPackageTemplate.RetirementDateTimeUtc,
                IsSubmissionBySignatoryRequired = reportPackageTemplate.IsSubmissionBySignatoryRequired,
                CtsEventTypeId = reportPackageTemplate.CtsEventTypeId,
                CtsEventType = GetCtsEventTypeDtoFromEventType(reportPackageTemplate.CtsEventType),
                OrganizationRegulatoryProgramId = reportPackageTemplate.OrganizationRegulatoryProgramId,
                IsActive = reportPackageTemplate.IsActive,
                LastModifierUserId = reportPackageTemplate.LastModifierUserId,

                ReportPackageTemplateElementCategories = new List<ReportElementCategoryName>()
            };

            // This will include attachment, TTO, comments etc categories
            foreach (var cat in reportPackageTemplate.ReportPackageTemplateElementCategories)
            {
                var reportElementCategoryName = (ReportElementCategoryName)Enum.Parse(typeof(ReportElementCategoryName), cat.ReportElementCategory.Name);
                rpt.ReportPackageTemplateElementCategories.Add(reportElementCategoryName);
            }

            // For ReportPackageTemplateAssignments fields  
            rpt.ReportPackageTemplateAssignments = new List<OrganizationRegulatoryProgramDto>();
            foreach (var rpta in reportPackageTemplate.ReportPackageTemplateAssignments)
            {
                rpt.ReportPackageTemplateAssignments.Add(GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(rpta.OrganizationRegulatoryProgram));
            }

            return rpt;
        }

        public ReportPackageTemplate GetReportPackageTemplateFromReportPackageTemplateDto(ReportPackageTemplateDto reportPackageTemplateDto)
        {
            if (reportPackageTemplateDto == null)
            {
                return null;
            }

            var rpt = new ReportPackageTemplate
            {
                Name = reportPackageTemplateDto.Name,
                Description = reportPackageTemplateDto.Description,
                RetirementDateTimeUtc = reportPackageTemplateDto.RetirementDateTimeUtc,
                IsSubmissionBySignatoryRequired = reportPackageTemplateDto.IsSubmissionBySignatoryRequired,
                CtsEventTypeId = reportPackageTemplateDto.CtsEventTypeId,
                //OrganizationRegulatoryProgramId = reportPackageTemplateDto.OrganizationRegulatoryProgramId,
                IsActive = reportPackageTemplateDto.IsActive,
                LastModifierUserId = reportPackageTemplateDto.LastModifierUserId,
                CtsEventType = GetCtsEventTypeFromEventTypeDto(reportPackageTemplateDto.CtsEventType),
                ReportPackageTemplateAssignments = new List<ReportPackageTemplateAssignment>(),
                ReportPackageTemplateElementCategories = new List<ReportPackageTemplateElementCategory>()
            };

            if (reportPackageTemplateDto.ReportPackageTemplateId.HasValue)
            {
                rpt.ReportPackageTemplateId = reportPackageTemplateDto.ReportPackageTemplateId.Value;
            }

            return rpt;
        }

        public ReportPackageTemplateElementTypeDto GetReportPackageTemplateElmentTypeDtoFromReportPackageTemplateType(ReportPackageTemplateElementType rptet)
        {
            if (rptet == null)
            {
                return null;
            }

            var dto = new ReportPackageTemplateElementTypeDto();
            dto.ReportPackageTemplateElementTypeId = rptet.ReportPackageTemplateElementTypeId;
            dto.ReportPackageTemplateElementCategoryId = rptet.ReportPackageTemplateElementCategoryId;
            dto.ReportPackageTemplateElementCategory = GetReportPackageTemplateElementCategoryDtoFromReportPackageTemplateElementCategory(rptet.ReportPackageTemplateElementCategory);
            dto.ReportElementTypeId = rptet.ReportElementTypeId;
            dto.ReportElementType = GetReportElementTypeDtoFromReportElementType(rptet.ReportElementType);

            return dto;
        }

        public ReportElementTypeDto GetReportElementTypeDtoFromReportElementType(ReportElementType reportElementType)
        {
            if (reportElementType == null)
            {
                return null;
            }

            var mappedReportElementType = new ReportElementTypeDto();
            if (reportElementType.ReportElementCategory != null)
            {
                mappedReportElementType.ReportElementCategory = (ReportElementCategoryName)(Enum.Parse(typeof(ReportElementCategoryName), reportElementType.ReportElementCategory.Name));
            }
            mappedReportElementType.ReportElementTypeId = reportElementType.ReportElementTypeId;
            mappedReportElementType.Name = reportElementType.Name;
            mappedReportElementType.Description = reportElementType.Description;
            mappedReportElementType.Content = reportElementType.Content;
            mappedReportElementType.IsContentProvided = reportElementType.IsContentProvided;
            mappedReportElementType.CtsEventType = GetCtsEventTypeDtoFromCtsEventType(reportElementType.CtsEventType);
            mappedReportElementType.OrganizationRegulatoryProgramId = reportElementType.OrganizationRegulatoryProgramId;
            return mappedReportElementType;
        }

        public ReportElementType GetReportElementTypeFromReportElementTypeDto(ReportElementTypeDto reportElementTypeDto, ReportElementType reportElementType = null)
        {
            if (reportElementType == null)
            {
                reportElementType = new ReportElementType();
            }

            reportElementType.Name = reportElementTypeDto.Name;
            reportElementType.Description = reportElementTypeDto.Description;
            reportElementType.Content = reportElementTypeDto.Content;
            reportElementType.IsContentProvided = reportElementTypeDto.IsContentProvided;
            reportElementType.CtsEventTypeId = reportElementTypeDto.CtsEventType?.CtsEventTypeId;
            //IGNORE reportElementType.CtsEventType
            //reportElementType.ReportElementCategoryId = reportElementTypeDto.ReportElementCategoryId;
            //IGNORE reportElementType.ReportElementCategory
            //IGNORE reportElementType.OrganizationRegulatoryProgram
            //IGNORE reportElementType.CreationDateTimeUtc
            //IGNORE reportElementType.LastModificationDateTimeUtc
            //IGNORE reportElementType.LastModifierUserId

            return reportElementType;
        }

        public ReportPackageTemplateElementType GetReportPackageTemplateElmentTypeFromReportPackageTemplateTypeDto(ReportPackageTemplateElementTypeDto rptet)
        {
            throw new NotImplementedException();
        }

        //public ReportPackageTemplateAssignmentDto GetReportPackageTemplateAssignmentDtoFromReportPackageTemplateAssignment(ReportPackageTemplateAssignment rpt)
        //{
        //    if (rpt == null)
        //    {
        //        return null;
        //    }

        //    var rptaDto = new ReportPackageTemplateAssignmentDto
        //    {
        //        ReportPackageTemplateAssignmentId = rpt.ReportPackageTemplateAssignmentId,
        //        ReportPackageTemplateId = rpt.ReportPackageTemplateId,
        //        OrganizationRegulatoryProgramId = rpt.OrganizationRegulatoryProgramId,
        //        OrganizationRegulatoryProgram = GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(rpt.OrganizationRegulatoryProgram)
        //    };
        //    return rptaDto;
        //}

        //public ReportPackageTemplateAssignment GetReportPackageTemplateAssignmentFromReportPackageTemplateAssignmentDto(ReportPackageTemplateAssignmentDto rptDto)
        //{
        //    if (rptDto == null)
        //    {
        //        return null;
        //    }

        //    var rpta = new ReportPackageTemplateAssignment
        //    {
        //        ReportPackageTemplateAssignmentId = rptDto.ReportPackageTemplateAssignmentId,
        //        ReportPackageTemplateId = rptDto.ReportPackageTemplateId,
        //        ReportPackageTemplate =
        //            GetReportPackageTemplateFromReportPackageTemplateDto(rptDto.ReportPackageTemplate),
        //        OrganizationRegulatoryProgramId = rptDto.OrganizationRegulatoryProgramId
        //    };
        //    return rpta;

        //}

        public FileStoreDto GetFileStoreDtoFromFileStore(Core.Domain.FileStore fileStore)
        {
            if (fileStore == null)
            {
                return null;
            }

            return new FileStoreDto
            {
                FileStoreId = fileStore.FileStoreId,
                Name = fileStore.Name,
                Description = fileStore.Description,
                OriginalFileName = fileStore.OriginalName,
                SizeByte = fileStore.SizeByte,
                ReportElementTypeId = fileStore.ReportElementTypeId,
                ReportElementTypeName = fileStore.ReportElementTypeName,
                OrganizationRegulatoryProgramId = fileStore.OrganizationRegulatoryProgramId,
                OrganizationRegulatoryProgram =
                    GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(
                        fileStore.OrganizationRegulatoryProgram),
                UploalDateTimeLocal = fileStore.UploadDateTimeUtc,
                UploaderUserId = fileStore.UploaderUserId,

                //TODO: wating for poco is finished
                //LastModifierUserId = fileStore.LastModifierUserId; 
                //LastModificationDateTimeLocal = fileStore.LastModificationDateTimeUtc;

            };
        }

        public Core.Domain.FileStore GetFileStoreFromFileStoreDto(FileStoreDto fileStoreDto)
        {
            if (fileStoreDto == null)
            {
                return null;
            }

            var fileStore = new Core.Domain.FileStore
            {
                Name = fileStoreDto.Name,
                Description = fileStoreDto.Description,
                OriginalName = fileStoreDto.OriginalFileName,
                SizeByte = fileStoreDto.SizeByte,
                ReportElementTypeId = fileStoreDto.ReportElementTypeId,
                ReportElementTypeName = fileStoreDto.ReportElementTypeName,
                OrganizationRegulatoryProgramId = fileStoreDto.OrganizationRegulatoryProgramId,
                UploaderUserId = fileStoreDto.UploaderUserId

                //TODO: wating for poco is finished
                //LastModifierUserId = fileStoreDto.LastModifierUserId;  
            };

            if (fileStoreDto.FileStoreId.HasValue)
            {
                fileStore.FileStoreId = fileStoreDto.FileStoreId.Value;
            }

            return fileStore;
        }

        public MonitoringPointDto GetMonitoringPointDtoFromMonitoringPoint(Core.Domain.MonitoringPoint mp)
        {
            if (mp == null)
            {
                return null;
            }

            var dto = new MonitoringPointDto();
            dto.MonitoringPointId = mp.MonitoringPointId;
            dto.Name = mp.Name;
            dto.Description = mp.Description;
            dto.OrganizationRegulatoryProgramId = mp.OrganizationRegulatoryProgramId;
            dto.IsEnabled = mp.IsEnabled;
            dto.IsRemoved = mp.IsRemoved;
            return dto;
        }

        public SampleDto GetSampleDtoFromSample(Core.Domain.Sample sample)
        {
            if (sample == null)
            {
                return null;
            }

            var dto = new SampleDto();
            dto.SampleId = sample.SampleId;
            dto.Name = sample.Name;
            dto.MonitoringPointId = sample.MonitoringPointId;
            dto.MonitoringPointName = sample.MonitoringPointName;
            dto.CtsEventTypeId = sample.CtsEventTypeId;
            dto.CtsEventTypeName = sample.CtsEventTypeName;
            dto.CtsEventCategoryName = sample.CtsEventCategoryName;

            dto.CollectionMethodId = sample.CollectionMethodId;
            dto.CollectionMethodName = sample.CollectionMethodName;
            dto.SampleStatusId = sample.SampleStatusId;
            dto.SampleStatusName = sample.SampleStatus.Name;
            dto.LabSampleIdentifier = sample.LabSampleIdentifier;

            //Handle this in calling code
            //var resultDtos = new List<SampleResultDto>();
            //foreach (var sampleResult in sample.SampleResults)
            //{
            //    resultDtos.Add(this.GetSampleResultDtoFromSampleResult(sampleResult));
            //}
            //dto.SampleResults = resultDtos;

            return dto;
        }
        public Core.Domain.Sample GetSampleFromSampleDto(SampleDto sampleDto, Core.Domain.Sample existingSample = null)
        {
            if (existingSample == null)
            {
                existingSample = new Core.Domain.Sample();
            }

            existingSample.Name = sampleDto.Name;
            existingSample.MonitoringPointId = sampleDto.MonitoringPointId;
            existingSample.MonitoringPointName = sampleDto.MonitoringPointName;
            existingSample.CtsEventTypeId = sampleDto.CtsEventTypeId;
            existingSample.CtsEventTypeName = sampleDto.CtsEventTypeName;
            existingSample.CtsEventCategoryName = sampleDto.CtsEventCategoryName;
            existingSample.CollectionMethodId = sampleDto.CollectionMethodId;
            existingSample.CollectionMethodName = sampleDto.CollectionMethodName;
            existingSample.LabSampleIdentifier = sampleDto.LabSampleIdentifier;
            //existingSample.StartDateTimeUtc = sampleDto.StartDateTimeUtc;
            //existingSample.EndDateTimeUtc = sampleDto.EndDateTimeUtc;
            existingSample.IsCalculated = sampleDto.IsCalculated;
            existingSample.SampleStatusId = sampleDto.SampleStatusId;
            existingSample.OrganizationTypeId = sampleDto.OrganizationTypeId;
            existingSample.OrganizationRegulatoryProgramId = sampleDto.OrganizationRegulatoryProgramId;

            return existingSample;
        }

        public SampleResultDto GetSampleResultDtoFromSampleResult(SampleResult sampleResult)
        {
            if (sampleResult == null)
                return null;

            var dto = new SampleResultDto();
            dto.SampleResultId = sampleResult.SampleResultId;
            dto.SampleId = sampleResult.SampleId;
            dto.ParameterId = sampleResult.ParameterId;
            dto.ParameterName = sampleResult.ParameterName;
            dto.Qualifier = sampleResult.Qualifier;
            dto.Value = sampleResult.Value;
            dto.UnitId = sampleResult.UnitId;
            dto.UnitName = sampleResult.UnitName;
            dto.MethodDetectionLimit = sampleResult.MethodDetectionLimit;
            dto.AnalysisMethod = sampleResult.AnalysisMethod;
            //dto.AnalysisDateTimeLocal = set outside MapHelper
            dto.IsApprovedEPAMethod = sampleResult.IsApprovedEPAMethod;
            dto.IsCalculated = sampleResult.IsCalculated;
            dto.LimitTypeId = sampleResult.LimitTypeId;
            dto.LimitBasisId = sampleResult.LimitBasisId;
            //dto.LastModificationDateTimeLocal = set outside MapHelper
            //dto.LastModifierUserFullName = set outside MapHelper

            return dto;
        }

        public SampleResult GetConcentrationSampleResultFromSampleResultDto(SampleResultDto dto)
        {
             var concentrationResult = new SampleResult()
            {
                SampleId = dto.SampleId
                ,ParameterId = dto.ParameterId
                ,ParameterName = dto.ParameterName
                ,Qualifier = dto.Qualifier
                ,Value = dto.Value
                ,DecimalPlaces = dto.DecimalPlaces
                ,UnitId = dto.UnitId
                ,UnitName = dto.UnitName
                ,MethodDetectionLimit = dto.MethodDetectionLimit
                //,AnalysisDateTimeUtc = set outside after calling line
                ,IsApprovedEPAMethod = dto.IsApprovedEPAMethod
                ,IsMassLoadingCalculationRequired = false
                ,IsFlowForMassLoadingCalculation = false
                ,IsCalculated = false
                 //,LimitTypeId = set outside after calling line
                 //,LimitBasisId = set outside after calling line
                 //,CreationDateTimeUtc = set outside after calling line
                 //,LastModificationDateTimeUtc = set outside after calling line
                 //,LastModifierUserId = set outside after calling line

             };

            return concentrationResult;
        }

        public SampleResult GetMassSampleResultFromSampleResultDto(SampleResultDto dto)
        {
            var massResult = new SampleResult()
            {
                SampleId = dto.SampleId
                ,ParameterId = dto.ParameterId
                ,ParameterName = dto.ParameterName
                ,Qualifier = dto.MassLoadingQualifier
                ,Value = dto.MassLoadingValue
                ,DecimalPlaces = dto.MassLoadingDecimalPlaces
                ,UnitId = dto.MassLoadingUnitId
                ,UnitName = dto.MassLoadingUnitName
                ,MethodDetectionLimit = dto.MethodDetectionLimit
                //,AnalysisDateTimeUtc = set outside after calling line
                ,IsApprovedEPAMethod = dto.IsApprovedEPAMethod
                ,IsMassLoadingCalculationRequired = true
                ,IsFlowForMassLoadingCalculation = false
                ,IsCalculated = false
                 //,LimitTypeId = set outside after calling line
                 //,LimitBasisId = set outside after calling line
                 //,CreationDateTimeUtc = set outside after calling line
                 //,LastModificationDateTimeUtc = set outside after calling line
                 //,LastModifierUserId = set outside after calling line
            };

            

            return massResult;
        }

    }
}
