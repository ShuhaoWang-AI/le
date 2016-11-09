using System.Collections.Generic;
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
        string GetSettingTemplateValue(SettingType settingType);

        string GetOrganizationSettingValueByUserId(int userProfileId, SettingType settingType, bool? isChooseMin, bool? isChooseMax);

        string GetOrganizationSettingValue(int organizationId, SettingType settingType);

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
        
        int PasswordLockoutHours();

        void CreateOrUpdateProgramSettings(int orgRegProgId, IEnumerable<SettingDto> settingDtos);
        void CreateOrUpdateProgramSetting(int orgRegProgId, SettingDto settingDto);
        void CreateOrUpdateOrganizationSettings(int organizationId, IEnumerable<SettingDto> settingDtos);
        void CreateOrUpdateOrganizationSetting(int organizationId, SettingDto settingDto);
    }
}
