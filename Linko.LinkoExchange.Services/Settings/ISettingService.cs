using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Settings
{
    public interface ISettingService
    {
        /// <summary>
        /// Ge the system global settings;
        /// </summary>
        /// <returns></returns>
        IDictionary<SettingType, string > GetGlobalSettings(); 

        /// <summary>
        /// Get the organizationSetting by UserId
        /// </summary>
        /// <param name="userId">UserDto Id</param>
        /// <returns>Collection of organization settings</returns>
        IEnumerable<OrganizationSettingDto> GetOrganizationSettingsByUserId(int userId);

        /// <summary>
        /// Get organization settings for a collection of organization Ids
        /// </summary>
        /// <param name="organizationIds">The organization Ids.</param>
        /// <returns>Collection of organization settings</returns>
        IEnumerable<OrganizationSettingDto> GetOrganizationSettingsByIds(IEnumerable<int> organizationIds);

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
        ProgramSettingDto GetProgramSettingsById(int programId);

        /// <summary>
        /// Get the program settings for a collection of program Ids
        /// </summary>
        /// <param name="programIds">The program Ids</param>
        /// <returns></returns>
        IEnumerable<ProgramSettingDto> GetProgramSettingsByIds(IEnumerable<int> programIds);


        bool PasswordRequireDigital();
        bool PassowrdRequireLowerCase();
        bool PasswordRequireUpperCase();
        int PasswordRequireLength();

        int PasswordLockoutHours();


        void CreateOrUpdateProgramSettings(ProgramSettingDto settingDtos);
        void CreateOrUpdateProgramSettings(int orgRegProgId, IEnumerable<SettingDto> settingDtos);
        void CreateOrUpdateProgramSetting(int orgRegProgId, SettingDto settingDto);
        void CreateOrUpdateOrganizationSettings(OrganizationSettingDto settingDtos);
        void CreateOrUpdateOrganizationSettings(int organizationId, IEnumerable<SettingDto> settingDtos);
        void CreateOrUpdateOrganizationSetting(int organizationId, SettingDto settingDto);
    }
}
