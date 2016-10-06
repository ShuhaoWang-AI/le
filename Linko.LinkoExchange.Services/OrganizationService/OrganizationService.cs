using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Services.Common;

namespace Linko.LinkoExchange.Services
{
    public class OrganizationService : IOrganizationService
    {
        public OrganizationSettings GetOrganizationSettingsById(string organizationid)
        {
            return new OrganizationSettings
            {
                OrganizationId = "abc",
                Settings = GetMockData()
            };
        }

        public IEnumerable<OrganizationSettings> GetOrganizationSettingsByUserId(string userId)
        {
            return new[]
            {
                 new OrganizationSettings
                 {
                     OrganizationId="abc",
                     Settings =  GetMockData()
                 }
             };
        }


        private Setting[] GetMockData()
        {
            return new[] {
                         new Setting
                         {
                             Name="PasswordRequireLength",
                             Value="6"
                         },

                         new Setting
                         {
                             Name="PasswordRequireDigit",
                             Value = "true"
                         },

                         new Setting
                         {
                             Name="PasswordRequireLowerCase",
                             Value = "true"
                         },

                         new Setting
                         {
                             Name="PasswordRequireUpperCase",
                             Value = "true"
                         },
                         new Setting
                         {
                             Name="UserLockoutEnabledByDefault",
                             Value = "true"
                         }, 
                         
                         new Setting
                         {
                             Name = "DefaultAccountLockoutTimeSpan",
                             Value = "1"
                         }, 

                         new Setting
                         {
                             Name = "MaxFailedAccessAttemptsBeforeLockout",
                             Value = "2"
                         },
                };
        } 
    }
}
