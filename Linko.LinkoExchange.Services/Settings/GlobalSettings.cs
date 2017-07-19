using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Settings
{
    public class GlobalSettings : IGlobalSettings
    {
        private static Dictionary<SystemSettingType, string> _globalSettings = new Dictionary<SystemSettingType, string>();

        //Only cache those settings that appear here and for these number of hours
        //  (Populated from Resource Manager)
        private Dictionary<SettingType, int> _settingsCacheDurationHours = new Dictionary<SettingType, int>();

        public GlobalSettings(LinkoExchangeContext dbContext)
        {
            _globalSettings = new Dictionary<SystemSettingType, string>();

            var systemSettings = dbContext.SystemSettings.ToList();

            _globalSettings.Add(SystemSettingType.EmailServer, systemSettings.First(i => i.Name == SystemSettingType.EmailServer.ToString()).Value);
            _globalSettings.Add(SystemSettingType.PasswordExpiredDays, systemSettings.First(i => i.Name == SystemSettingType.PasswordExpiredDays.ToString()).Value);
            _globalSettings.Add(SystemSettingType.SupportPhoneNumber, systemSettings.First(i => i.Name == SystemSettingType.SupportPhoneNumber.ToString()).Value);
            _globalSettings.Add(SystemSettingType.SupportEmailAddress, systemSettings.First(i => i.Name == SystemSettingType.SupportEmailAddress.ToString()).Value);
            _globalSettings.Add(SystemSettingType.SystemEmailEmailAddress, systemSettings.First(i => i.Name == SystemSettingType.SystemEmailEmailAddress.ToString()).Value);
            _globalSettings.Add(SystemSettingType.SystemEmailFirstName, systemSettings.First(i => i.Name == SystemSettingType.SystemEmailFirstName.ToString()).Value);
            _globalSettings.Add(SystemSettingType.SystemEmailLastName, systemSettings.First(i => i.Name == SystemSettingType.SystemEmailLastName.ToString()).Value);

            _globalSettings.Add(SystemSettingType.PasswordRequireDigit, systemSettings.First(i => i.Name == SystemSettingType.PasswordRequireDigit.ToString()).Value);
            _globalSettings.Add(SystemSettingType.PasswordRequiredLength, systemSettings.First(i => i.Name == SystemSettingType.PasswordRequiredLength.ToString()).Value);
            _globalSettings.Add(SystemSettingType.PasswordRequiredMaxLength, systemSettings.First(i => i.Name == SystemSettingType.PasswordRequiredMaxLength.ToString()).Value);
            _globalSettings.Add(SystemSettingType.MassLoadingUnitName, systemSettings.First(i => i.Name == SystemSettingType.MassLoadingUnitName.ToString()).Value);
            _globalSettings.Add(SystemSettingType.FileAvailableToAttachMaxAgeMonths, systemSettings.First(i => i.Name == SystemSettingType.FileAvailableToAttachMaxAgeMonths.ToString()).Value);
            _globalSettings.Add(SystemSettingType.CTSDatabaseMinVersion, systemSettings.First(i => i.Name == SystemSettingType.CTSDatabaseMinVersion.ToString()).Value);
            _globalSettings.Add(SystemSettingType.CTSDatabaseMinPatch, systemSettings.First(i => i.Name == SystemSettingType.CTSDatabaseMinPatch.ToString()).Value);

            //
            //Read config and pre-load "Settings Cache Duration Hours" dictionary so we know 
            //which Org Reg Program settings to cache (and for how many hours).
            //
            //      Example: "TimeZone:24|EmailContactInfoName:48|EmailContactInfoPhone:48|EmailContactInfoEmailAddress:48"
            //

            string settingsCacheDurationHoursString = ConfigurationManager.AppSettings[name: "SettingsCacheDurationHours"];
            
            string[] settingsCacheDurationHoursArray = settingsCacheDurationHoursString.Split('|');
            foreach (string settingHours in settingsCacheDurationHoursArray)
            {
                string[] settingHoursArray = settingHours.Split(':');
                string settingString = settingHoursArray[0];
                int hours = Convert.ToInt32(settingHoursArray[1]);
                SettingType settingType;
                if (Enum.TryParse(settingString, out settingType))
                {
                    _settingsCacheDurationHours.Add(settingType, hours);
                }
            }
        }

        public string GetSetting(SystemSettingType settingType)
        {
            return _globalSettings[settingType];
        }

        public IDictionary<SystemSettingType, string> GetGlobalSettings()
        {
            return _globalSettings;
        }

        public bool IsCacheRequired(SettingType cacheKey, out int durationHours)
        {
            if (_settingsCacheDurationHours.ContainsKey(cacheKey))
            {
                durationHours = _settingsCacheDurationHours[cacheKey];
                return true;
            }
            else
            {
                durationHours = 0;
                return false;
            }
        }
    }
}
