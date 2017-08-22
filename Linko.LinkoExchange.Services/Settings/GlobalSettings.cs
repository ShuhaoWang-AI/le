using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;

namespace Linko.LinkoExchange.Services.Settings
{
    public class GlobalSettings : IGlobalSettings
    {
        #region static fields and constants

        private static Dictionary<SystemSettingType, string> _globalSettings = new Dictionary<SystemSettingType, string>();

        #endregion

        #region constructors and destructor

        /// <summary>
        ///     Constructor code is executed once only as this class is instantiated once per application lifetime by the Unity container.
        /// </summary>
        /// <param name="dbContext"> </param>
        public GlobalSettings(LinkoExchangeContext dbContext)
        {
            _globalSettings = new Dictionary<SystemSettingType, string>();

            var systemSettings = dbContext.SystemSettings.ToList();

            _globalSettings.Add(key:SystemSettingType.EmailServer, value:systemSettings.First(i => i.Name == SystemSettingType.EmailServer.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.PasswordExpiredDays, value:systemSettings.First(i => i.Name == SystemSettingType.PasswordExpiredDays.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.SupportPhoneNumber, value:systemSettings.First(i => i.Name == SystemSettingType.SupportPhoneNumber.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.SupportEmailAddress, value:systemSettings.First(i => i.Name == SystemSettingType.SupportEmailAddress.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.SystemEmailEmailAddress,
                                value:systemSettings.First(i => i.Name == SystemSettingType.SystemEmailEmailAddress.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.SystemEmailFirstName, value:systemSettings.First(i => i.Name == SystemSettingType.SystemEmailFirstName.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.SystemEmailLastName, value:systemSettings.First(i => i.Name == SystemSettingType.SystemEmailLastName.ToString()).Value);

            _globalSettings.Add(key:SystemSettingType.PasswordRequireDigit, value:systemSettings.First(i => i.Name == SystemSettingType.PasswordRequireDigit.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.PasswordRequiredLength, value:systemSettings.First(i => i.Name == SystemSettingType.PasswordRequiredLength.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.PasswordRequiredMaxLength,
                                value:systemSettings.First(i => i.Name == SystemSettingType.PasswordRequiredMaxLength.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.MassLoadingUnitName, value:systemSettings.First(i => i.Name == SystemSettingType.MassLoadingUnitName.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.FileAvailableToAttachMaxAgeMonths,
                                value:systemSettings.First(i => i.Name == SystemSettingType.FileAvailableToAttachMaxAgeMonths.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.CTSDatabaseMinVersion, value:systemSettings.First(i => i.Name == SystemSettingType.CTSDatabaseMinVersion.ToString()).Value);
            _globalSettings.Add(key:SystemSettingType.CTSDatabaseMinPatch, value:systemSettings.First(i => i.Name == SystemSettingType.CTSDatabaseMinPatch.ToString()).Value);
        }

        #endregion

        #region interface implementations

        public IDictionary<SystemSettingType, string> GetGlobalSettings()
        {
            return _globalSettings;
        }

        #endregion
    }
}