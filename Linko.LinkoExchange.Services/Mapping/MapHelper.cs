using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Mapping
{
	public class MapHelper : IMapHelper
	{
		#region interface implementations

		public OrganizationDto GetOrganizationDtoFromOrganization(Core.Domain.Organization organization, OrganizationDto dto = null)
		{
			if (dto == null)
			{
				dto = new OrganizationDto
				      {
					      OrganizationType = GetOrganizationTypeDtoFromOrganizationType(orgType:organization.OrganizationType)
				      };
			}
			else
			{
				dto.OrganizationType = GetOrganizationTypeDtoFromOrganizationType(orgType:organization.OrganizationType, dto:dto.OrganizationType);
			}

			dto.AddressLine1 = organization.AddressLine1;
			dto.AddressLine2 = organization.AddressLine2;
			dto.CityName = organization.CityName;
			dto.State = organization.Jurisdiction?.Code ?? "";
			dto.OrganizationId = organization.OrganizationId;
			dto.OrganizationTypeId = organization.OrganizationTypeId;

			dto.OrganizationName = organization.Name;
			dto.ZipCode = organization.ZipCode;
			dto.PhoneNumber = organization.PhoneNumber;
			if (organization.PhoneExt.HasValue)
			{
				dto.PhoneExt = organization.PhoneExt.Value.ToString();
			}
			dto.FaxNumber = organization.FaxNumber;
			dto.WebsiteURL = organization.WebsiteUrl;
			dto.Signer = organization.Signer;
			dto.Classification = organization.Classification;
			return dto;
		}

		public AuthorityDto GetAuthorityDtoFromOrganization(Core.Domain.Organization organization, AuthorityDto dto = null)
		{
			if (dto == null)
			{
				dto = new AuthorityDto
				      {
					      OrganizationType = new OrganizationTypeDto()
				      };
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
			{
				dto.PhoneExt = organization.PhoneExt.Value.ToString();
			}
			dto.FaxNumber = organization.FaxNumber;
			dto.WebsiteURL = organization.WebsiteUrl;
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
			{
				dto.JurisdictionId = userProfile.JurisdictionId.Value;
			}
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
			{
				dto.LockoutEndDateUtc = userProfile.LockoutEndDateUtc.Value;
			}
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
				userDto = new OrganizationRegulatoryProgramUserDto
				          {
					          PermissionGroup = new PermissionGroupDto()
				          };
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
			userDto.OrganizationRegulatoryProgramDto = GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:user.OrganizationRegulatoryProgram);

			//Map InviterOrganizationRegulatoryProgramDto
			userDto.InviterOrganizationRegulatoryProgramDto =
				GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:user.InviterOrganizationRegulatoryProgram);

			return userDto;
		}

		public OrganizationRegulatoryProgramDto GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram
			(OrganizationRegulatoryProgram orgRegProgram, OrganizationRegulatoryProgramDto dto = null)
		{
			if (dto == null)
			{
				dto = new OrganizationRegulatoryProgramDto();
			}
			dto.OrganizationRegulatoryProgramId = orgRegProgram.OrganizationRegulatoryProgramId;
			dto.RegulatoryProgramId = orgRegProgram.RegulatoryProgramId;
			dto.RegulatoryProgramDto = GetProgramDtoFromOrganizationRegulatoryProgram(org:orgRegProgram.RegulatoryProgram);
			dto.OrganizationId = orgRegProgram.OrganizationId;
			dto.RegulatorOrganizationId = orgRegProgram.RegulatorOrganizationId;

			dto.OrganizationDto = GetOrganizationDtoFromOrganization(organization:orgRegProgram.Organization);

			if (orgRegProgram.RegulatorOrganization != null)
			{
				dto.RegulatorOrganization = GetOrganizationDtoFromOrganization(organization:orgRegProgram.RegulatorOrganization);
			}

			dto.IsEnabled = orgRegProgram.IsEnabled;
			dto.IsRemoved = orgRegProgram.IsRemoved;
			dto.AssignedTo = orgRegProgram.AssignedTo;
			dto.ReferenceNumber = orgRegProgram.ReferenceNumber;

			//IGNORE HasSignatory
			//IGNORE HasAdmin

			return dto;
		}

		public SettingDto GetSettingDtoFromOrganizationRegulatoryProgramSetting(OrganizationRegulatoryProgramSetting setting, SettingDto dto = null)
		{
			if (dto == null)
			{
				dto = new SettingDto();
			}
			dto.TemplateName = (SettingType) Enum.Parse(enumType:typeof(SettingType), value:setting.SettingTemplate.Name);
			dto.OrgTypeName = (OrganizationTypeName) Enum.Parse(enumType:typeof(OrganizationTypeName),
			                                                    value:setting.OrganizationRegulatoryProgram.Organization.OrganizationType.Name);
			dto.Value = setting.Value;
			dto.DefaultValue = setting.SettingTemplate.DefaultValue;
			dto.Description = setting.SettingTemplate.Description;
			return dto;
		}

		public SettingDto GetSettingDtoFromOrganizationSetting(OrganizationSetting setting, SettingDto dto = null)
		{
			if (dto == null)
			{
				dto = new SettingDto();
			}
			dto.TemplateName = (SettingType) Enum.Parse(enumType:typeof(SettingType), value:setting.SettingTemplate.Name);
			dto.OrgTypeName = (OrganizationTypeName) Enum.Parse(enumType:typeof(OrganizationTypeName), value:setting.SettingTemplate.OrganizationType.Name);
			dto.Value = setting.Value;
			dto.DefaultValue = setting.SettingTemplate.DefaultValue;
			dto.Description = setting.SettingTemplate.Description;
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
			dto.IsResetInvitation = invitation.IsResetInvitation;

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
			if (jurisdiction == null)
			{
				return null;
			}

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
			{
				dto.ParentId = jurisdiction.ParentId.Value;
			}

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
			var paramDto = new ParameterDto
			               {
				               ParameterId = parameter.ParameterId,
				               Name = parameter.Name,
				               Description = parameter.Description,
				               DefaultUnit = ToDto(fromDomainObject:parameter.DefaultUnit),
				               TrcFactor = parameter.TrcFactor,
				               OrganizationRegulatoryProgramId = parameter.OrganizationRegulatoryProgramId,
				               IsRemoved = parameter.IsRemoved
			               };

			return paramDto;
		}

		public ParameterGroupDto GetParameterGroupDtoFromParameterGroup(ParameterGroup parameterGroup)
		{
			var paramGroupDto = new ParameterGroupDto
			                    {
				                    ParameterGroupId = parameterGroup.ParameterGroupId,
				                    Name = parameterGroup.Name,
				                    Description = parameterGroup.Description,
				                    OrganizationRegulatoryProgramId = parameterGroup.OrganizationRegulatoryProgramId,
				                    IsActive = parameterGroup.IsActive,
				                    Parameters = new List<ParameterDto>()
			                    };

			foreach (var paramAssocation in parameterGroup.ParameterGroupParameters)
			{
				if (!paramAssocation.Parameter.IsRemoved)
				{
					paramGroupDto.Parameters.Add(item:GetParameterDtoFromParameter(parameter:paramAssocation.Parameter));
				}
			}

			return paramGroupDto;
		}

		public ParameterGroup GetParameterGroupFromParameterGroupDto(ParameterGroupDto parameterGroupDto, ParameterGroup parameterGroup = null)
		{
			if (parameterGroup == null)
			{
				parameterGroup = new ParameterGroup
				                 {
					                 ParameterGroupParameters = new List<ParameterGroupParameter>()
				                 };
			}

			parameterGroup.Name = parameterGroupDto.Name.Trim();
			parameterGroup.Description = parameterGroupDto.Description;
			parameterGroup.IsActive = parameterGroupDto.IsActive;

			//Handle updating parameters outside this method after calling code

			return parameterGroup;
		}

		public UnitDto ToDto(Core.Domain.Unit fromDomainObject)
		{
			var unitDto = new UnitDto
			              {
				              UnitId = fromDomainObject.UnitId,
				              Name = fromDomainObject.Name,
				              Description = fromDomainObject.Description,
				              IsFlowUnit = fromDomainObject.IsFlowUnit,
				              OrganizationId = fromDomainObject.OrganizationId,
				              IsRemoved = fromDomainObject.IsRemoved,
				              CreationDateTimeUtc = fromDomainObject.CreationDateTimeUtc,
				              LastModificationDateTimeUtc = fromDomainObject.LastModificationDateTimeUtc,
				              LastModifierUserId = fromDomainObject.LastModifierUserId,
							  SystemUnitId = fromDomainObject.SystemUnitId,
				              SystemUnit = ToDto(fromDomainObject:fromDomainObject.SystemUnit),
							  IsAvailableToRegulatee = fromDomainObject.IsAvailableToRegulatee,
				              IsReviewed = fromDomainObject.IsReviewed
			              };

			return unitDto;
		}

		/// <inheritdoc />
		public Core.Domain.Unit ToDomainObject(UnitDto fromDto, Core.Domain.Unit existingDomainObject = null)
		{
			if (fromDto == null)
			{
				return null;
			}

			if (existingDomainObject == null)
			{
				existingDomainObject = new Core.Domain.Unit
				                       {
					                       UnitId = fromDto.UnitId,
					                       OrganizationId = fromDto.OrganizationId
				                       };
			}

			existingDomainObject.Name = fromDto.Name;
			existingDomainObject.Description = fromDto.Description;
			existingDomainObject.IsFlowUnit = fromDto.IsFlowUnit;
			existingDomainObject.IsRemoved = fromDto.IsRemoved;
			existingDomainObject.SystemUnitId = fromDto.SystemUnitId;
			existingDomainObject.IsAvailableToRegulatee = fromDto.IsAvailableToRegulatee;
			existingDomainObject.IsReviewed = fromDto.IsReviewed;

			return existingDomainObject;
		}

		public ReportElementCategoryDto GetReportElementCategoryDtoFromReportElementCategory(ReportElementCategory cat)
		{
			if (cat == null)
			{
				return null;
			}

			var reportElementCategory = new ReportElementCategoryDto
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

		public ReportPackageTemplateElementCategoryDto GetReportPackageTemplateElementCategoryDtoFromReportPackageTemplateElementCategory(ReportPackageTemplateElementCategory cat)
		{
			if (cat == null)
			{
				return null;
			}

			var reportPackageTemplateElementCategory =
				new ReportPackageTemplateElementCategoryDto
				{
					ReportPackageTemplateElementCategoryId = cat.ReportPackageTemplateElementCategoryId,
					ReportPackageTemplateId = cat.ReportPackageTemplateId,
					ReportElementCategoryId = cat.ReportElementCategoryId,
					ReportElementCategory = GetReportElementCategoryDtoFromReportElementCategory(cat:cat.ReportElementCategory),
					SortOrder = cat.SortOrder
				};




			//// deep navigation cause error  
			//var dtos = new List<ReportPackageTemplateElementTypeDto>();
			//foreach (var c in cat.ReportPackageTemplateElementTypes)
			//{
			//    dtos.Add(this.GetReportPackageTemplateElementTypeDtoFromReportPackageTemplateElementType(c));
			//}
			//reportPackageTemplateElementCategory.ReportPackageTemplateElementTypes = dtos;

			return reportPackageTemplateElementCategory;
		}

		public ReportPackageTemplateElementTypeDto GetReportPackageTemplateElementTypeDtoFromReportPackageTemplateElementType(ReportPackageTemplateElementType rptet)
		{
			if (rptet == null)
			{
				return null;
			}

			var rptetDto = new ReportPackageTemplateElementTypeDto
			               {
				               ReportPackageTemplateElementTypeId = rptet.ReportPackageTemplateElementTypeId,
				               ReportPackageTemplateElementCategoryId = rptet.ReportPackageTemplateElementCategoryId,
				               ReportPackageTemplateElementCategory =
					               GetReportPackageTemplateElementCategoryDtoFromReportPackageTemplateElementCategory(cat:rptet.ReportPackageTemplateElementCategory),
				               ReportElementTypeId = rptet.ReportElementTypeId,
				               ReportElementType = GetReportElementTypeDtoFromReportElementType(reportElementType:rptet.ReportElementType),
				               IsRequired = rptet.IsRequired,
				               SortOrder = rptet.SortOrder
			               };
			return rptetDto;
		}

		public ReportPackageTemplateElementType GetReportPackageTemplateElementTypeFromReportPackageTemplateElementTypeDto(ReportPackageTemplateElementTypeDto rptetDto)
		{
			if (rptetDto == null)
			{
				return null;
			}

			var rptet = new ReportPackageTemplateElementType
			            {
				            ReportPackageTemplateElementTypeId = rptetDto.ReportPackageTemplateElementTypeId,
				            ReportPackageTemplateElementCategoryId = rptetDto.ReportPackageTemplateElementCategoryId,
				            ReportPackageTemplateElementCategory =
					            GetReportPackageTemplateElementCategoryFromReportPackageTemplateElementCategoryDto(cat:rptetDto.ReportPackageTemplateElementCategory),
				            ReportElementTypeId = rptetDto.ReportElementTypeId,
				            ReportElementType = GetReportElementTypeFromReportElementTypeDto(reportElementTypeDto:rptetDto.ReportElementType),
				            IsRequired = rptetDto.IsRequired,
				            SortOrder = rptetDto.SortOrder
			            };
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
					                                           GetReportPackageTemplateFromReportPackageTemplateDto(reportPackageTemplateDto:cat.ReportPackageTemplate),
				                                           ReportElementCategoryId = cat.ReportElementCategoryId,
				                                           ReportElementCategory =
					                                           GetReportElementCategoryFromReportElementCategoryDto(cat:cat.ReportElementCategory),
				                                           SortOrder = cat.SortOrder
			                                           };

			//var rptetps = new List<ReportPackageTemplateElementType>();
			//foreach (var rptetDto in cat.ReportPackageTemplateElementTypes)
			//{
			//    rptetps.Add(this.GetReportPackageTemplateElementTypeFromReportPackageTemplateElementTypeDto(rptetDto));
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
				          CtsEventType = GetCtsEventTypeDtoFromEventType(ctsEventType:reportPackageTemplate.CtsEventType),
				          OrganizationRegulatoryProgramId = reportPackageTemplate.OrganizationRegulatoryProgramId,
				          IsActive = reportPackageTemplate.IsActive,
				          LastModifierUserId = reportPackageTemplate.LastModifierUserId,

				          ReportPackageTemplateElementCategories = new List<ReportElementCategoryName>()
			          };

			// This will include attachment, TTO, comments etc categories
			foreach (var cat in reportPackageTemplate.ReportPackageTemplateElementCategories)
			{
				var reportElementCategoryName = (ReportElementCategoryName) Enum.Parse(enumType:typeof(ReportElementCategoryName), value:cat.ReportElementCategory.Name);
				rpt.ReportPackageTemplateElementCategories.Add(item:reportElementCategoryName);
			}

			// For ReportPackageTemplateAssignments fields  
			rpt.ReportPackageTemplateAssignments = new List<OrganizationRegulatoryProgramDto>();
			foreach (var rpta in reportPackageTemplate.ReportPackageTemplateAssignments)
			{
				rpt.ReportPackageTemplateAssignments.Add(item:
				                                         GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:rpta.OrganizationRegulatoryProgram));
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

				          //CtsEventType = GetCtsEventTypeFromEventTypeDto(reportPackageTemplateDto.CtsEventType),
				          ReportPackageTemplateAssignments = new List<ReportPackageTemplateAssignment>(),
				          ReportPackageTemplateElementCategories = new List<ReportPackageTemplateElementCategory>()
			          };

			if (reportPackageTemplateDto.ReportPackageTemplateId.HasValue)
			{
				rpt.ReportPackageTemplateId = reportPackageTemplateDto.ReportPackageTemplateId.Value;
			}

			return rpt;
		}

		public ReportPackageTemplateElementTypeDto GetReportPackageTemplateElementTypeDtoFromReportPackageTemplateType(ReportPackageTemplateElementType rptElementType)
		{
			if (rptElementType == null)
			{
				return null;
			}

			var dto = new ReportPackageTemplateElementTypeDto
			          {
				          ReportPackageTemplateElementTypeId = rptElementType.ReportPackageTemplateElementTypeId,
				          ReportPackageTemplateElementCategoryId = rptElementType.ReportPackageTemplateElementCategoryId,
				          ReportPackageTemplateElementCategory =
					          GetReportPackageTemplateElementCategoryDtoFromReportPackageTemplateElementCategory(cat:rptElementType.ReportPackageTemplateElementCategory),
				          ReportElementTypeId = rptElementType.ReportElementTypeId,
				          ReportElementType = GetReportElementTypeDtoFromReportElementType(reportElementType:rptElementType.ReportElementType)
			          };

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
				mappedReportElementType.ReportElementCategory =
					(ReportElementCategoryName) Enum.Parse(enumType:typeof(ReportElementCategoryName), value:reportElementType.ReportElementCategory.Name);
			}
			mappedReportElementType.ReportElementTypeId = reportElementType.ReportElementTypeId;
			mappedReportElementType.Name = reportElementType.Name;
			mappedReportElementType.Description = reportElementType.Description;
			mappedReportElementType.Content = reportElementType.Content;
			mappedReportElementType.IsContentProvided = reportElementType.IsContentProvided;
			mappedReportElementType.CtsEventType = GetCtsEventTypeDtoFromCtsEventType(ctsEventType:reportElementType.CtsEventType);
			mappedReportElementType.OrganizationRegulatoryProgramId = reportElementType.OrganizationRegulatoryProgramId;
			return mappedReportElementType;
		}

		public ReportElementType GetReportElementTypeFromReportElementTypeDto(ReportElementTypeDto reportElementTypeDto, ReportElementType reportElementType = null)
		{
			if (reportElementType == null)
			{
				reportElementType = new ReportElementType();
			}

			reportElementType.Name = reportElementTypeDto.Name.Trim();
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

		public ReportPackageTemplateElementType GetReportPackageTemplateElementTypeFromReportPackageTemplateTypeDto(ReportPackageTemplateElementTypeDto rptElementTypeDto)
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
				       MediaType = fileStore.MediaType,
				       OriginalFileName = fileStore.OriginalName,
				       FileTypeId = fileStore.FileTypeId,
				       SizeByte = fileStore.SizeByte,
				       ReportElementTypeId = fileStore.ReportElementTypeId,
				       ReportElementTypeName = fileStore.ReportElementTypeName,
				       OrganizationRegulatoryProgramId = fileStore.OrganizationRegulatoryProgramId,
				       OrganizationRegulatoryProgram = GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:fileStore.OrganizationRegulatoryProgram),
				       UploadDateTimeLocal = fileStore.UploadDateTimeUtc.UtcDateTime,
				       UploaderUserId = fileStore.UploaderUserId
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
				                MediaType = fileStoreDto.MediaType,
				                SizeByte = fileStoreDto.SizeByte,
				                FileTypeId = fileStoreDto.FileTypeId,
				                ReportElementTypeId = fileStoreDto.ReportElementTypeId,
				                ReportElementTypeName = fileStoreDto.ReportElementTypeName,
				                OrganizationRegulatoryProgramId = fileStoreDto.OrganizationRegulatoryProgramId,
				                UploaderUserId = fileStoreDto.UploaderUserId
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
			
			var dto = new MonitoringPointDto
			          {
				          MonitoringPointId = mp.MonitoringPointId,
				          Name = mp.Name,
				          Description = mp.Description,
				          OrganizationRegulatoryProgramId = mp.OrganizationRegulatoryProgramId,
				          IsEnabled = mp.IsEnabled,
				          IsRemoved = mp.IsRemoved,
						  MonitoringPointParameter = GetMonitoringPointParameterDtoFromMonitoringPointParameter(mp.MonitoringPointParameters)
			};
			return dto;
		}

		public SampleDto GetSampleDtoFromSample(Core.Domain.Sample sample)
		{
			if (sample == null)
			{
				return null;
			}

			var dto = new SampleDto
			          {
				          SampleId = sample.SampleId,
				          Name = sample.Name,
				          MonitoringPointId = sample.MonitoringPointId,
				          MonitoringPointName = sample.MonitoringPointName,
				          CtsEventTypeId = sample.CtsEventTypeId,
				          CtsEventTypeName = sample.CtsEventTypeName,
				          CtsEventCategoryName = sample.CtsEventCategoryName,
				          CollectionMethodId = sample.CollectionMethodId,
				          CollectionMethodName = sample.CollectionMethodName,
				          IsReadyToReport = sample.IsReadyToReport,
				          LabSampleIdentifier = sample.LabSampleIdentifier,
				          ResultQualifierValidValues = sample.ResultQualifierValidValues,
				          MassLoadingConversionFactorPounds = sample.MassLoadingConversionFactorPounds,
				          MassLoadingCalculationDecimalPlaces = sample.MassLoadingCalculationDecimalPlaces,
				          IsMassLoadingResultToUseLessThanSign = sample.IsMassLoadingResultToUseLessThanSign,
				          ByOrganizationTypeName = sample.ByOrganizationRegulatoryProgram.Organization.OrganizationType.Name
			          };


			//dto.SampleStatusName = set outside after exiting this method

			//dto.FlowUnitValidValues = sample.FlowUnitValidValues; // set outside after exiting this method

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

			//existingSample.Name = sampleDto.Name; //set after exit this method
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
			existingSample.IsSystemGenerated = false; //not currently used
			existingSample.IsReadyToReport = sampleDto.IsReadyToReport;

			//existingSample.FlowUnitValidValues = sampleDto.FlowUnitValidValues; //set after we exit this method
			existingSample.ResultQualifierValidValues = sampleDto.ResultQualifierValidValues;
			existingSample.MassLoadingConversionFactorPounds = sampleDto.MassLoadingConversionFactorPounds;
			existingSample.MassLoadingCalculationDecimalPlaces = sampleDto.MassLoadingCalculationDecimalPlaces;
			existingSample.IsMassLoadingResultToUseLessThanSign = sampleDto.IsMassLoadingResultToUseLessThanSign;

			//existingSample.OrganizationRegulatoryProgramId = sampleDto.OrganizationRegulatoryProgramId; //set after we exit this method

			return existingSample;
		}

		public SampleResultDto GetSampleResultDtoFromSampleResult(SampleResult sampleResult)
		{
			if (sampleResult == null)
			{
				return null;
			}

			var dto = new SampleResultDto
			          {
				          ConcentrationSampleResultId = sampleResult.SampleResultId,
				          ParameterId = sampleResult.ParameterId,
				          ParameterName = sampleResult.ParameterName,
				          Qualifier = sampleResult.Qualifier,
				          Value = sampleResult.Value,
				          EnteredValue = sampleResult.EnteredValue,
				          UnitId = sampleResult.UnitId,
				          UnitName = sampleResult.UnitName,
				          EnteredMethodDetectionLimit = sampleResult.EnteredMethodDetectionLimit,
				          MethodDetectionLimit = sampleResult.MethodDetectionLimit,
				          AnalysisMethod = sampleResult.AnalysisMethod,
				          IsApprovedEPAMethod = sampleResult.IsApprovedEPAMethod
			          };

			//dto.AnalysisDateTimeLocal = set outside MapHelper

			//dto.IsCalculated = sampleResult.IsCalculated;
			//dto.LimitTypeName = set outside MapHelper
			//dto.LimitBasisName = set outside MapHelper
			//dto.LastModificationDateTimeLocal = set outside MapHelper
			//dto.LastModifierUserFullName = set outside MapHelper

			return dto;
		}

		public SampleResult GetConcentrationSampleResultFromSampleResultDto(SampleResultDto dto, SampleResult existingSampleResult)
		{
			if (existingSampleResult == null)
			{
				existingSampleResult = new SampleResult();
			}

			existingSampleResult.ParameterId = dto.ParameterId;
			existingSampleResult.ParameterName = dto.ParameterName;
			existingSampleResult.Qualifier = dto.Qualifier;
			existingSampleResult.EnteredValue = dto.EnteredValue;

			//existingSampleResult.Value = Convert.ToDouble(dto.Value), //set outside after calling line
			existingSampleResult.UnitId = dto.UnitId;
			existingSampleResult.UnitName = dto.UnitName;
			existingSampleResult.EnteredMethodDetectionLimit = dto.EnteredMethodDetectionLimit;
			existingSampleResult.MethodDetectionLimit = dto.MethodDetectionLimit;
			existingSampleResult.AnalysisMethod = dto.AnalysisMethod;

			//existingSampleResult.AnalysisDateTimeUtc = set outside after calling line
			existingSampleResult.IsApprovedEPAMethod = dto.IsApprovedEPAMethod;
			existingSampleResult.IsCalculated = false;

			//existingSampleResult.LimitTypeId = set outside after calling line
			//existingSampleResult.LimitBasisId = set outside after calling line
			//existingSampleResult.CreationDateTimeUtc = set outside after calling line
			//existingSampleResult.LastModificationDateTimeUtc = set outside after calling line
			//existingSampleResult.LastModifierUserId = set outside after calling line

			return existingSampleResult;
		}

		public SampleResult GetMassSampleResultFromSampleResultDto(SampleResultDto dto, SampleResult existingSampleResult)
		{
			if (existingSampleResult == null)
			{
				existingSampleResult = new SampleResult();
			}

			existingSampleResult.ParameterId = dto.ParameterId;
			existingSampleResult.ParameterName = dto.ParameterName;
			existingSampleResult.Qualifier = dto.MassLoadingQualifier;
			existingSampleResult.EnteredValue = dto.MassLoadingValue;

			//existingSampleResult.Value = Convert.ToDouble(dto.MassLoadingValue), //set outside after calling line
			existingSampleResult.UnitId = dto.MassLoadingUnitId;
			existingSampleResult.UnitName = dto.MassLoadingUnitName;

			//existingSampleResult.EnteredMethodDetectionLimit  // do not save. see more details in #Bug 4316
			//existingSampleResult.AnalysisMethod // do not save. see more details in #Bug 4316
			//existingSampleResult.AnalysisDateTimeUtc  // do not save. see more details in #Bug 4316
			//existingSampleResult.IsApprovedEPAMethod // do not save. see more details in #Bug 4316
			existingSampleResult.IsCalculated = true;

			//existingSampleResult.LimitTypeId = set outside after calling line
			//existingSampleResult.LimitBasisId = set outside after calling line
			//existingSampleResult.CreationDateTimeUtc = set outside after calling line
			//existingSampleResult.LastModificationDateTimeUtc = set outside after calling line
			//existingSampleResult.LastModifierUserId = set outside after calling line

			return existingSampleResult;
		}

		public ReportPackageDto GetReportPackageDtoFromReportPackage(ReportPackage rpt)
		{
			if (rpt == null)
			{
				return null;
			}

			var reportPackageDto = new ReportPackageDto
			                       {
				                       ReportPackageId = rpt.ReportPackageId,
				                       Name = rpt.Name,
				                       Description = rpt.Description,
				                       PeriodStartDateTimeLocal = rpt.PeriodStartDateTimeUtc.UtcDateTime,
				                       PeriodEndDateTimeLocal = rpt.PeriodEndDateTimeUtc.UtcDateTime,
				                       CtsEventTypeId = rpt.CtsEventTypeId,
				                       CtsEventTypeName = rpt.CtsEventTypeName,
				                       CtsEventCategoryName = rpt.CtsEventCategoryName,
				                       Comments = rpt.Comments,
				                       IsSubmissionBySignatoryRequired = rpt.IsSubmissionBySignatoryRequired,
				                       OrganizationRegulatoryProgramId = rpt.OrganizationRegulatoryProgramId,
				                       OrganizationReferenceNumber = rpt.OrganizationReferenceNumber,
				                       OrganizationName = rpt.OrganizationName,
				                       OrganizationAddressLine1 = rpt.OrganizationAddressLine1,
				                       OrganizationAddressLine2 = rpt.OrganizationAddressLine2,
				                       OrganizationCityName = rpt.OrganizationCityName,
				                       OrganizationJurisdictionName = rpt.OrganizationJurisdictionName,
				                       OrganizationZipCode = rpt.OrganizationZipCode,
				                       RecipientOrganizationName = rpt.RecipientOrganizationName,
				                       RecipientOrganizationAddressLine1 = rpt.RecipientOrganizationAddressLine1,
				                       RecipientOrganizationAddressLine2 = rpt.RecipientOrganizationAddressLine2,
				                       RecipientOrganizationCityName = rpt.RecipientOrganizationCityName,
				                       RecipientOrganizationJurisdictionName = rpt.RecipientOrganizationJurisdictionName,
				                       RecipientOrganizationZipCode = rpt.RecipientOrganizationZipCode,
				                       CreationDateTimeLocal = rpt.CreationDateTimeUtc.UtcDateTime,
				                       SubmitterUserId = rpt.SubmitterUserId ?? -1,
				                       SubmitterUserName = rpt.SubmitterUserName,
				                       SubmitterFirstName = rpt.SubmitterFirstName,
				                       SubmitterLastName = rpt.SubmitterLastName,
				                       SubmitterTitleRole = rpt.SubmitterTitleRole,
				                       SubmitterIPAddress = rpt.SubmitterIPAddress,
				                       SubmissionDateTimeOffset = rpt.SubmissionDateTimeUtc
			                       };


			//reportPackageDto.ReportStatusName = set outside after calling line

			//reportPackageDto.RecipientOrgRegProgramId = set outside after calling code

			//Submission Review
			if (rpt.SubmissionReviewDateTimeUtc.HasValue)
			{
				//reportPackageDto.SubmissionReviewDateTimeLocal = set outside after calling code
				reportPackageDto.SubmissionReviewerFirstName = rpt.SubmissionReviewerFirstName;
				reportPackageDto.SubmissionReviewerLastName = rpt.SubmissionReviewerLastName;
				reportPackageDto.SubmissionReviewerTitleRole = rpt.SubmissionReviewerTitleRole;
				reportPackageDto.SubmissionReviewComments = rpt.SubmissionReviewComments;
			}

			if (rpt.RepudiationDateTimeUtc.HasValue
			    && rpt.RepudiatorUserId.HasValue
			    && rpt.RepudiationReasonId.HasValue)
			{
				//Map all Repudiation related fields

				//reportPackageDto.RepudiationDateTimeLocal = set outside after calling line
				reportPackageDto.RepudiatorUserId = rpt.RepudiatorUserId;
				reportPackageDto.RepudiatorFirstName = rpt.RepudiatorFirstName;
				reportPackageDto.RepudiatorLastName = rpt.RepudiatorLastName;
				reportPackageDto.RepudiatorTitleRole = rpt.RepudiatorTitleRole;
				reportPackageDto.RepudiationReasonId = rpt.RepudiationReasonId;
				reportPackageDto.RepudiationReasonName = rpt.RepudiationReasonName;
				reportPackageDto.RepudiationComments = rpt.RepudiationComments;
			}

			if (rpt.RepudiationReviewDateTimeUtc.HasValue
			    && rpt.RepudiationReviewerUserId.HasValue)
			{
				//Map all Repudiation Review related fields

				//reportPackageDto.RepudiationReviewDateTimeLocal = set outside after calling line
				reportPackageDto.RepudiationReviewerUserId = rpt.RepudiationReviewerUserId;
				reportPackageDto.RepudiationReviewerFirstName = rpt.RepudiationReviewerFirstName;
				reportPackageDto.RepudiationReviewerLastName = rpt.RepudiationReviewerLastName;
				reportPackageDto.RepudiationReviewerTitleRole = rpt.RepudiationReviewerTitleRole;
				reportPackageDto.RepudiationReviewComments = rpt.RepudiationReviewComments;
			}

			//Last Sent
			if (rpt.LastSentDateTimeUtc.HasValue)
			{
				//reportPackageDto.LastSentDateTimeLocal = set outside after calling line
				reportPackageDto.LastSenderFirstName = rpt.LastSenderFirstName;
				reportPackageDto.LastSenderLastName = rpt.LastSenderLastName;
			}

			reportPackageDto.ReportPackageElementCategories = new List<ReportElementCategoryName>();
			var sortedCategories = new List<ReportPackageElementCategory>();
			foreach (var rpec in rpt.ReportPackageElementCategories)
			{
				sortedCategories.Add(item:rpec);
			}

			foreach (var sortedCategoryValue in sortedCategories.OrderBy(item => item.SortOrder).ToList())
			{
				reportPackageDto.ReportPackageElementCategories.Add(item:(ReportElementCategoryName) Enum.Parse(enumType:typeof(ReportElementCategoryName),
				                                                                                                value:sortedCategoryValue.ReportElementCategory.Name));
			}

			return reportPackageDto;
		}

		public ReportPackage GetReportPackageFromReportPackageTemplate(ReportPackageTemplate rpt)
		{
			if (rpt == null)
			{
				return null;
			}

			return new ReportPackage
			       {
				       Name = rpt.Name,
				       Description = rpt.Description,

				       //PeriodStartDateTimeUtc = set outside after calling line
				       //PeriodEndDateTimeUtc = set outside after calling line
				       CtsEventTypeId = rpt.CtsEventTypeId,
				       CtsEventTypeName = rpt.CtsEventType?.Name,
				       CtsEventCategoryName = rpt.CtsEventType?.CtsEventCategoryName,
				       IsSubmissionBySignatoryRequired = rpt.IsSubmissionBySignatoryRequired,
				       ReportPackageTemplateId = rpt.ReportPackageTemplateId,

				       //ReportStatusId = set outside after calling line
				       //OrganizationRegulatoryProgramId = set outside after calling line
				       //OrganizationName = set outside after calling line
				       //OrganizationAddressLine1 = set outside after calling line
				       //OrganizationAddressLine2 = set outside after calling line
				       //OrganizationCityName = set outside after calling line
				       //OrganizationJurisdictionName = set outside after calling line
				       //OrganizationZipCode = set outside after calling line
				       RecipientOrganizationName = rpt.OrganizationRegulatoryProgram.Organization.Name,
				       RecipientOrganizationAddressLine1 = rpt.OrganizationRegulatoryProgram.Organization.AddressLine1,
				       RecipientOrganizationAddressLine2 = rpt.OrganizationRegulatoryProgram.Organization.AddressLine2,
				       RecipientOrganizationCityName = rpt.OrganizationRegulatoryProgram.Organization.CityName,
				       RecipientOrganizationJurisdictionName = rpt.OrganizationRegulatoryProgram.Organization.Jurisdiction?.Name,
				       RecipientOrganizationZipCode = rpt.OrganizationRegulatoryProgram.Organization.ZipCode,

				       //CreationDateTimeUtc = set outside after calling line
				       ReportPackageElementCategories = new List<ReportPackageElementCategory>()
			       };
		}

		public RepudiationReasonDto GetRepudiationReasonDtoFromRepudiationReason(RepudiationReason repudiationReason)
		{
			if (repudiationReason == null)
			{
				return null;
			}

			return new RepudiationReasonDto
			       {
				       RepudiationReasonId = repudiationReason.RepudiationReasonId,
				       Name = repudiationReason.Name,
				       Description = repudiationReason.Description

				       //CreationLocalDateTime = set outside after calling line
				       //LastModificationLocalDateTime = set outside after calling line
			       };
		}

		public ReportPackageElementTypeDto GetReportPackageElementTypeDtoFromReportPackageElementType(ReportPackageElementType reportPackageElementType)
		{
			if (reportPackageElementType == null)
			{
				return null;
			}

			return new ReportPackageElementTypeDto
			       {
				       ReportPackageElementTypeId = reportPackageElementType.ReportPackageElementTypeId,
				       ReportPackageElementCategoryId = reportPackageElementType.ReportPackageElementCategoryId,
				       ReportElementTypeId = reportPackageElementType.ReportElementTypeId,
				       ReportElementTypeName = reportPackageElementType.ReportElementTypeName,
				       ReportElementTypeContent = reportPackageElementType.ReportElementTypeContent,
				       ReportElementTypeIsContentProvided = reportPackageElementType.ReportElementTypeIsContentProvided,
				       CtsEventTypeId = reportPackageElementType.CtsEventTypeId,
				       CtsEventTypeName = reportPackageElementType.CtsEventTypeName,
				       CtsEventCategoryName = reportPackageElementType.CtsEventCategoryName,
				       IsRequired = reportPackageElementType.IsRequired,
				       SortOrder = reportPackageElementType.SortOrder,
				       IsIncluded = reportPackageElementType.IsIncluded
			       };
		}

        public DataSourceDto GetDataSourceDtoFroDataSource(Core.Domain.DataSource dataSource)
        {
            if (dataSource == null)
            {
                return null;
            }

            var dataSourceDto = new DataSourceDto
            {
                DataSourceId = dataSource.DataSourceId,
                Name = dataSource.Name,
                Description = dataSource.Description,
                OrganizationRegulatoryProgramId = dataSource.OrganizationRegulatoryProgramId
            };

	        if (dataSource.DataSourceMonitoringPoints != null)
	        {
		        dataSourceDto.DataSourceMonitoringPoints = dataSource.DataSourceMonitoringPoints.Select(selector:ToDataSourceMonitoringPointDto).ToList();
	        }
	        if (dataSource.DataSourceCollectionMethods != null)
	        {
		        dataSourceDto.DataSourceCollectionMethods = dataSource.DataSourceCollectionMethods.Select(selector:ToDataSourceCollectionMethodDto).ToList();
	        }
	        if (dataSource.DataSourceCtsEventTypes != null)
	        {
		        dataSourceDto.DataSourceSampleTypes = dataSource.DataSourceCtsEventTypes.Select(selector:ToDataSourceSampleTypeDto).ToList();
	        }
	        if (dataSource.DataSourceParameters != null)
	        {
		        dataSourceDto.DataSourceParameters = dataSource.DataSourceParameters.Select(selector:ToDataSourceParameterDto).ToList();
	        }
	        if (dataSource.DataSourceUnits != null)
	        {
		        dataSourceDto.DataSourceUnits = dataSource.DataSourceUnits.Select(selector:ToDataSourceUnitDto).ToList();
	        }
            return dataSourceDto;
        }

        public Core.Domain.DataSource GetDataSourceFromDataSourceDto(DataSourceDto dto, Core.Domain.DataSource existingDataSource)
        {
            if (existingDataSource == null)
            {
                existingDataSource = new Core.Domain.DataSource();
            }

            existingDataSource.Name = dto.Name;
            existingDataSource.Description = dto.Description;
            existingDataSource.OrganizationRegulatoryProgramId = dto.OrganizationRegulatoryProgramId;

            return existingDataSource;
        }

        public DataSourceTranslationDto ToDataSourceMonitoringPointDto(DataSourceMonitoringPoint @from)
        {
            if (from == null)
            {
                return null;
            }

            return new DataSourceTranslationDto
            {
                Id = from.DataSourceMonitoringPointId,
                DataSourceTerm = from.DataSourceTerm,
                DataSourceId = from.DataSourceId,
	            TranslationItem = new DataSourceTranslationItemDto
                {
                    TranslationId = from.MonitoringPointId,
					TranslationName = from.MonitoringPoint.Name,
	                TranslationType = DataSourceTranslationType.MonitoringPoint
                }
            };
        }

        public DataSourceMonitoringPoint ToDataSourceMonitoringPoint(DataSourceTranslationDto @from, DataSourceMonitoringPoint existingDomainObject)
        {
            if (from == null)
            {
                return null;
            }

	        if (existingDomainObject == null)
	        {
		        return new DataSourceMonitoringPoint
		               {
			               DataSourceMonitoringPointId = @from.Id.GetValueOrDefault(),
			               DataSourceTerm = @from.DataSourceTerm,
			               DataSourceId = @from.DataSourceId,
			               MonitoringPointId = @from.TranslationItem.TranslationId
		               };
	        }

	        existingDomainObject.MonitoringPointId = @from.TranslationItem.TranslationId;
	        return existingDomainObject;
        }

        public DataSourceTranslationDto ToDataSourceSampleTypeDto(DataSourceCtsEventType @from)
        {
            if (from == null)
            {
                return null;
            }

            return new DataSourceTranslationDto
            {
                Id = from.DataSourceCtsEventTypeId,
                DataSourceTerm = from.DataSourceTerm,
                DataSourceId = from.DataSourceId,
	            TranslationItem = new DataSourceTranslationItemDto
                {
                    TranslationId = from.CtsEventTypeId,
					TranslationName = from.CtsEventType.Name,
	                TranslationType = DataSourceTranslationType.SampleType
                }
            };
        }

        public DataSourceCtsEventType ToDataSourceSampleType(DataSourceTranslationDto @from, DataSourceCtsEventType existingDomainObject)
        {
            if (from == null)
            {
                return null;
            }

	        if (existingDomainObject == null)
	        {
		        return new DataSourceCtsEventType
		               {
			               DataSourceCtsEventTypeId = from.Id.GetValueOrDefault(),
			               DataSourceTerm = from.DataSourceTerm,
			               DataSourceId = from.DataSourceId,
			               CtsEventTypeId = from.TranslationItem.TranslationId
		               };
	        }

	        existingDomainObject.CtsEventTypeId = @from.TranslationItem.TranslationId;
	        return existingDomainObject;
        }

        public DataSourceTranslationDto ToDataSourceCollectionMethodDto(DataSourceCollectionMethod @from)
        {
            if (from == null)
            {
                return null;
            }

            return new DataSourceTranslationDto
            {
                Id = from.DataSourceCollectionMethodId,
                DataSourceTerm = from.DataSourceTerm,
                DataSourceId = from.DataSourceId,
	            TranslationItem = new DataSourceTranslationItemDto
                {
                    TranslationId = from.CollectionMethodId,
					TranslationName = from.CollectionMethod.Name,
	                TranslationType = DataSourceTranslationType.CollectionMethod
                }
            };
        }

        public DataSourceCollectionMethod ToDataSourceCollectionMethod(DataSourceTranslationDto @from, DataSourceCollectionMethod existingDomainObject)
        {
            if (from == null)
            {
                return null;
            }

	        if (existingDomainObject == null)
	        {
		        return new DataSourceCollectionMethod
		               {
			               DataSourceCollectionMethodId = from.Id.GetValueOrDefault(),
			               DataSourceTerm = from.DataSourceTerm,
			               DataSourceId = from.DataSourceId,
			               CollectionMethodId = from.TranslationItem.TranslationId
		               };
	        }

	        existingDomainObject.CollectionMethodId = @from.TranslationItem.TranslationId;
            return existingDomainObject;
        }

        public DataSourceTranslationDto ToDataSourceParameterDto(DataSourceParameter @from)
        {
            if (from == null)
            {
                return null;
            }

            return new DataSourceTranslationDto
            {
                Id = from.DataSourceParameterId,
                DataSourceTerm = from.DataSourceTerm,
                DataSourceId = from.DataSourceId,
                TranslationItem = new DataSourceTranslationItemDto
                {
                    TranslationId = from.ParameterId,
					TranslationName=from.Parameter.Name,
	                TranslationType = DataSourceTranslationType.Parameter
                }
            };
        }

        public DataSourceParameter ToDataSourceParameter(DataSourceTranslationDto @from, DataSourceParameter existingDomainObject)
        {
            if (from == null)
            {
                return null;
            }

	        if (existingDomainObject == null)
	        {
		        return new DataSourceParameter
		               {
			               DataSourceParameterId = from.Id.GetValueOrDefault(),
			               DataSourceTerm = from.DataSourceTerm,
			               DataSourceId = from.DataSourceId,
			               ParameterId = from.TranslationItem.TranslationId
		               };
	        }

	        existingDomainObject.ParameterId = @from.TranslationItem.TranslationId;
	        return existingDomainObject;
        }

        public DataSourceTranslationDto ToDataSourceUnitDto(DataSourceUnit @from)
        {
            if (from == null)
            {
                return null;
            }

            return new DataSourceTranslationDto
            {
                Id = from.DataSourceUnitId,
                DataSourceTerm = from.DataSourceTerm,
                DataSourceId = from.DataSourceId,
                TranslationItem = new DataSourceTranslationItemDto
                {
                    TranslationId = from.UnitId,
					TranslationName = from.Unit.Name,
					TranslationType = DataSourceTranslationType.Unit
                }
            };
        }

        public DataSourceUnit ToDataSourceUnit(DataSourceTranslationDto @from, DataSourceUnit existingDomainObject)
        {
            if (from == null)
            {
                return null;
            }

	        if (existingDomainObject == null)
	        {
		        return new DataSourceUnit
		               {
			               DataSourceUnitId = from.Id.GetValueOrDefault(),
			               DataSourceTerm = from.DataSourceTerm,
			               DataSourceId = from.DataSourceId,
			               UnitId = from.TranslationItem.TranslationId
		               };
	        }

	        existingDomainObject.UnitId = @from.TranslationItem.TranslationId;
	        return existingDomainObject;
        }

        /// <inheritdoc />
        public ImportTempFileDto ToDto(Core.Domain.ImportTempFile fromDomainObject)
		{
			if (fromDomainObject == null)
			{
				return null;
			}

			return new ImportTempFileDto
			       {
				       ImportTempFileId = fromDomainObject.ImportTempFileId,
				       MediaType = fromDomainObject.MediaType,
				       OriginalFileName = fromDomainObject.OriginalName,
				       FileTypeId = fromDomainObject.FileTypeId,
				       SizeByte = fromDomainObject.SizeByte,
				       OrganizationRegulatoryProgramId = fromDomainObject.OrganizationRegulatoryProgramId,
				       OrganizationRegulatoryProgram = GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:fromDomainObject.OrganizationRegulatoryProgram),
				       UploadDateTimeLocal = fromDomainObject.UploadDateTimeUtc.UtcDateTime,
				       UploaderUserId = fromDomainObject.UploaderUserId,
					   RawFile = fromDomainObject.RawFile
					   //FileExtension = implemented in service
			       };
		}

		/// <inheritdoc />
		public Core.Domain.ImportTempFile ToDomainObject(ImportTempFileDto fromDto, Core.Domain.ImportTempFile existingDomainObject = null)
		{
			if (fromDto == null)
			{
				return null;
			}

			if (existingDomainObject == null)
			{
				existingDomainObject = new Core.Domain.ImportTempFile();
			}

			if (fromDto.ImportTempFileId != null)
			{
				existingDomainObject.ImportTempFileId = fromDto.ImportTempFileId.Value;
			}

			existingDomainObject.OriginalName = fromDto.OriginalFileName;
			existingDomainObject.MediaType = fromDto.MediaType;
			existingDomainObject.SizeByte = fromDto.RawFile.Length;
			existingDomainObject.RawFile = fromDto.RawFile;

			return existingDomainObject;
		}

		/// <inheritdoc />
		public SystemUnitDto ToDto(Core.Domain.SystemUnit fromDomainObject)
		{
			if (fromDomainObject == null)
			{
				return null;
			}

			return new SystemUnitDto
			       {
				       SystemUnitId = fromDomainObject.SystemUnitId,
				       Name = fromDomainObject.Name,
				       Description = fromDomainObject.Description,
				       ConversionFactor = fromDomainObject.ConversionFactor,
				       AdditiveFactor = fromDomainObject.AdditiveFactor,
				       UnitDimensionId = fromDomainObject.UnitDimensionId,
					   UnitDimension = ToDto(fromDomainObject:fromDomainObject.UnitDimension),
				       //CreationDateTimeUtc = service will populate later,
				       //LastModificationDateTimeUtc = service will populate later,
				       LastModifierUserId = fromDomainObject.LastModifierUserId,
				       //LastModifierFullName = service will populate later
			       };
		}

		/// <inheritdoc />
		public Core.Domain.SystemUnit ToDomainObject(SystemUnitDto fromDto, Core.Domain.SystemUnit existingDomainObject = null)
		{
			if (fromDto == null)
			{
				return null;
			}

			if (existingDomainObject == null)
			{
				existingDomainObject = new Core.Domain.SystemUnit {SystemUnitId = fromDto.SystemUnitId};
			}

			existingDomainObject.Name = fromDto.Name;
			existingDomainObject.Description = fromDto.Description;
			existingDomainObject.ConversionFactor = fromDto.ConversionFactor;
			existingDomainObject.AdditiveFactor = fromDto.AdditiveFactor;
			existingDomainObject.UnitDimensionId = fromDto.UnitDimensionId;

			return existingDomainObject;
		}
		

		/// <inheritdoc />
		public UnitDimensionDto ToDto(Core.Domain.UnitDimension fromDomainObject)
		{
			if (fromDomainObject == null)
			{
				return null;
			}

			return new UnitDimensionDto
			       {
				       UnitDimensionId = fromDomainObject.UnitDimensionId,
				       Name = fromDomainObject.Name,
				       Description = fromDomainObject.Description,
				       //CreationDateTimeUtc = service will populate later,
				       //LastModificationDateTimeUtc = service will populate later,
				       LastModifierUserId = fromDomainObject.LastModifierUserId,
				       //LastModifierFullName = service will populate later
			       };
		}

		/// <inheritdoc />
		public Core.Domain.UnitDimension ToDomainObject(UnitDimensionDto fromDto, Core.Domain.UnitDimension existingDomainObject = null)
		{
			if (fromDto == null)
			{
				return null;
			}

			if (existingDomainObject == null)
			{
				existingDomainObject = new Core.Domain.UnitDimension {UnitDimensionId = fromDto.UnitDimensionId};
			}

			existingDomainObject.Name = fromDto.Name;
			existingDomainObject.Description = fromDto.Description;

			return existingDomainObject;
		}

		/// <inheritdoc />
		public FileVersionDto ToDto(FileVersion fromDomainObject)
		{
			if (fromDomainObject == null)
			{
				return null;
			}

			return new FileVersionDto
			       {
				       FileVersionId = fromDomainObject.FileVersionId,
				       Name = fromDomainObject.Name,
				       Description = fromDomainObject.Description,
				       OrganizationRegulatoryProgramId = fromDomainObject.OrganizationRegulatoryProgramId,
				       OrganizationRegulatoryProgram = GetOrganizationRegulatoryProgramDtoFromOrganizationRegulatoryProgram(orgRegProgram:fromDomainObject.OrganizationRegulatoryProgram),
				       //LastModificationDateTimeLocal = service will populate later,
				       LastModifierUserId = fromDomainObject.LastModifierUserId,
				       FileVersionFields = fromDomainObject.FileVersionFields.Select(selector:ToDto).ToList()
			       };
		}

		/// <inheritdoc />
		public FileVersionFieldDto ToDto(FileVersionField fromDomainObject)
		{
			if (fromDomainObject == null)
			{
				return null;
			}

			return new FileVersionFieldDto
			       {
				       FileVersionFieldId = fromDomainObject.FileVersionFieldId,
					   SystemFieldName = (SampleImportColumnName)fromDomainObject.SystemFieldId,
				       FileVersionFieldName = fromDomainObject.Name,
					   DataFormatName = (DataFormatName)fromDomainObject.SystemField.DataFormatId,
					   DataFormatDescription = fromDomainObject.SystemField.DataFormat.Description,
				       Description = fromDomainObject.Description,
				       DataOptionalityName = (DataOptionalityName)fromDomainObject.DataOptionalityId,
				       IsSystemRequired =fromDomainObject.SystemField.IsRequired,
				       Size = fromDomainObject.Size,
				       ExampleData = fromDomainObject.ExampleData,
				       AdditionalComments = fromDomainObject.AdditionalComments,
					   IsIncluded = true
			       };
		}


		/// <inheritdoc />
		public FileVersionDto MargeToFileVersionDto(FileVersionDto fileVersionDto, FileVersionTemplate fileVersionTemplate)
		{
			var dto = new FileVersionDto
			                        {
				                        Name = fileVersionTemplate.Name,
				                        Description = fileVersionTemplate.Description,
				                        FileVersionFields = fileVersionTemplate.FileVersionTemplateFields.Select(ToFileVersionFieldDto).ToList()
			                        };

			if (fileVersionDto != null)
			{
				fileVersionDto.FileVersionFields.ForEach(x => x.IsIncluded = true);

				dto.FileVersionId = fileVersionDto.FileVersionId;
				dto.Name = fileVersionDto.Name;
				dto.LastModifierUserId = fileVersionDto.LastModifierUserId;
				dto.Description = fileVersionDto.Description;
				dto.FileVersionFields.RemoveAll(x => fileVersionDto.FileVersionFields.Exists(y => y.SystemFieldName == x.SystemFieldName));
				dto.FileVersionFields.AddRange(collection:fileVersionDto.FileVersionFields);
				dto.OrganizationRegulatoryProgramId = fileVersionDto.OrganizationRegulatoryProgramId;
				dto.OrganizationRegulatoryProgram = fileVersionDto.OrganizationRegulatoryProgram;

				//dto.LastModificationDateTimeLocal = service will populate later,
			}

			return dto;
		}

		private FileVersionFieldDto ToFileVersionFieldDto(FileVersionTemplateField fromDomainObject)
		{
			return new FileVersionFieldDto
			       {
				       SystemFieldName = (SampleImportColumnName) fromDomainObject.SystemFieldId,
				       FileVersionFieldName = fromDomainObject.SystemField.Name,
				       DataFormatName = (DataFormatName) fromDomainObject.SystemField.DataFormatId,
					   DataFormatDescription = fromDomainObject.SystemField.DataFormat.Description,
				       Description = fromDomainObject.SystemField.Description,
				       DataOptionalityName = fromDomainObject.SystemField.IsRequired ? DataOptionalityName.Required : DataOptionalityName.Optional,
				       IsSystemRequired = fromDomainObject.SystemField.IsRequired,
				       Size = fromDomainObject.SystemField.Size,
				       ExampleData = fromDomainObject.SystemField.ExampleData,
				       AdditionalComments = fromDomainObject.SystemField.AdditionalComments,
				       IsIncluded = false
			       };
		}

		#endregion
		private List<MonitoringPointParameterDto> GetMonitoringPointParameterDtoFromMonitoringPointParameter(ICollection<MonitoringPointParameter> monitoringPointParameters)
		{
			return monitoringPointParameters?.Select(i => new MonitoringPointParameterDto
			                                             {
				                                             MonitoringPointId = i.MonitoringPointId,
				                                             MonitoringPointParameterId = i.MonitoringPointParameterId
			                                             }
			                                       ).ToList();
		}
		
		private ProgramDto GetProgramDtoFromOrganizationRegulatoryProgram(RegulatoryProgram org, ProgramDto dto = null)
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

		private OrganizationTypeDto GetOrganizationTypeDtoFromOrganizationType(OrganizationType orgType, OrganizationTypeDto dto = null)
		{
			if (dto == null)
			{
				dto = new OrganizationTypeDto();
			}
			dto.Name = orgType.Name;
			dto.Description = orgType.Description;
			return dto;
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
	}
}