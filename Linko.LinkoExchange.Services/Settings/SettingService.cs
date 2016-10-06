using System.Collections.Generic; 
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Settings
{
    public class SettingService : ISettingService
    {
        /// <summary>
        /// Get organization settings for a collection of organization Ids
        /// </summary>
        /// <param name="organizationIds">The organization Ids.</param>
        /// <returns>Collection of organization settings</returns>
        public IEnumerable<OrganizationSettingDto> GetOrganizationSettingsByIds(IEnumerable<int> organizationIds)
        {
            return new[]
            {
                new OrganizationSettingDto
                {
                    OrganizationId = 100,
                    Settings = GetMockData()
                }
            };
        }

        /// <summary>
        /// Get the organization settings by organization Id
        /// </summary>
        /// <param name="organizationId"></param>
        /// <returns>OrganizationSettingDto object</returns>
        public OrganizationSettingDto GetOrganizationSettingsById(int organizationId)
        {
            return new OrganizationSettingDto
            {
                OrganizationId = 100,
                Settings = GetMockData()
            };
        }

        /// <summary>
        /// Get settings for one program
        /// </summary>
        /// <param name="programId">The program Id to get for</param>
        /// <returns>The PrrogramSetting object</returns>
        public ProgramSettingDto GetProgramSettingsById(int programId)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ProgramSettingDto> GetProgramSettingsByIds(IEnumerable<int> programIds)
        {
            return new[]
            {
                new ProgramSettingDto
                {
                    ProgramId = 100,
                    Settings = GetMockData()
                }
            };
        }

        /// <summary>
        /// Get application global settings.
        /// </summary>
        /// <returns>The settings for dictionary object</returns>
        public IDictionary<string, string> GetGlobalSettings()
        {
            //TODO: get system global settings
            var globalSetting = new Dictionary<string, string>();
            globalSetting.Add("PasswordRequireLength", "6");
            globalSetting.Add("PasswordRequireDigit", "true");
            return globalSetting; 
        }

        /// <summary>
        /// Get all organization settings for one user identified by userId.
        /// </summary>
        /// <param name="userId">The user id to get organization setting for.</param>
        /// <returns>A collection of organization settings</returns>
        public IEnumerable<OrganizationSettingDto> GetOrganizationSettingsByUserId(int userId)
        {
            return new[]
            {
                 new OrganizationSettingDto
                 {
                     OrganizationId= 100,
                     Settings =  GetMockData()
                 }
             };
        }
        
        private SettingDto[] GetMockData()
        {
            return new[] {
                         new SettingDto
                         {
                             Name="PasswordRequireLength",
                             Value="6"
                         },

                         new SettingDto
                         {
                             Name="PasswordRequireDigit",
                             Value = "true"
                         },

                         new SettingDto
                         {
                             Name="PasswordRequireLowerCase",
                             Value = "true"
                         },

                         new SettingDto
                         {
                             Name="PasswordRequireUpperCase",
                             Value = "true"
                         },
                         new SettingDto
                         {
                             Name="UserLockoutEnabledByDefault",
                             Value = "true"
                         }, 
                         
                         new SettingDto
                         {
                             Name = "DefaultAccountLockoutTimeSpan",   // lock out days
                             Value = "1"
                         }, 

                         new SettingDto
                         {
                             Name = "MaxFailedAccessAttemptsBeforeLockout",
                             Value = "2"
                         },
                };
        } 
    }
}
