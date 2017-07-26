using System.Collections.Generic;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Settings
{
    public interface ISettingService
    {
        /// <summary>
        /// Ge the system global settings;
        /// </summary>
        /// <returns></returns>
        IDictionary<SystemSettingType, string > GetGlobalSettings();

        /// <summary>
        /// Get the default Setting template value
        /// </summary>
        /// <param name="settingType"></param>
        /// <returns></returns>
        string GetSettingTemplateValue(SettingType settingType, OrganizationTypeName? orgType = null);

        /// <summary>
        /// Finds all org reg programs associated with the user and their authorities. Compares each authority's setting of the
        /// requested setting type (found in the tOrganizationSetting table) and returns either the first (or only value) 
        /// or optionally returns the minimum or maximum value.
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <param name="settingType"></param>
        /// <param name="isChooseMin"></param>
        /// <param name="isChooseMax"></param>
        /// <returns></returns>
        string GetOrganizationSettingValueByUserId(int userProfileId, SettingType settingType, bool? isChooseMin, bool? isChooseMax);

        /// <summary>
        /// Finds the authority of the passed in org reg program (if applicable) and returns the
        /// setting from the tOrganizationSetting table associated with the authority. If the
        /// org reg program is itself the authority, the setting will be associated with this org reg program.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <param name="settingType"></param>
        /// <returns></returns>
        string GetOrganizationSettingValue(int orgRegProgramId, SettingType settingType);

        /// <summary>
        /// Finds the authority of the organization within the regulatory program (if applicable) and returns the
        /// setting from the tOrganizatioSetting table associated with the authority. If the
        /// organization is itself the authority, the setting will be associated with this organization.
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="regProgramId"></param>
        /// <param name="settingType"></param>
        /// <returns></returns>
        string GetOrganizationSettingValue(int organizationId, int regProgramId, SettingType settingType);

        /// <summary>
        /// Finds the authority of the passed in org reg program (if applicable) and returns the
        /// setting from the tOrganizatioRegulatoryProgramSetting table associated with the authority. If the
        /// org reg program is itself the authority, the setting will be associated with this org reg program.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <param name="settingType"></param>
        /// <returns></returns>
        string GetOrgRegProgramSettingValue(int orgRegProgramId, SettingType settingType);

        /// <summary>
        /// Get organization settings for a collection of organization Ids
        /// </summary>
        /// <param name="organizationIds">The organization Ids.</param>
        /// <returns>Collection of organization settings</returns>
        ICollection<OrganizationSettingDto> GetOrganizationSettingsByIds(IEnumerable<int> organizationIds);

        /// <summary>
        /// Get settings for one organization
        /// </summary>
        /// <param name="organizationId">OrganizationDto Id</param>
        /// <returns>OrganizationSettingDto object</returns>
        OrganizationSettingDto GetOrganizationSettingsById(int organizationId);

        /// <summary>
        /// Get settings for one program
        /// </summary>
        /// <param name="programId">The program Id to get for</param>
        /// <returns>The PrrogramSetting object</returns>
        ProgramSettingDto GetProgramSettingsById(int orgRegProgramId);

        ProgramSettingDto GetAuthorityProgramSettingsById(int orgRegProgramId);
        int PasswordLockoutHours();

        void CreateOrUpdateProgramSettings(int orgRegProgId, IEnumerable<SettingDto> settingDtos);
        void CreateOrUpdateProgramSetting(int orgRegProgId, SettingDto settingDto);
        void CreateOrUpdateOrganizationSettings(int organizationId, IEnumerable<SettingDto> settingDtos);
        void CreateOrUpdateOrganizationSetting(int organizationId, SettingDto settingDto);
        OrganizationRegulatoryProgram GetAuthority(int? organizationId = null, int? regProgramId = null, int? orgRegProgramId = null);
    }
}
