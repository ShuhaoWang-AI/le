using System;
using System.Configuration;

namespace Linko.LinkoExchange.Services.Config
{
    public class ConfigSettingService : IConfigSettingService
    {
        #region interface implementations

        public string GetConfigValue(string key)
        {
            if (ConfigurationManager.AppSettings[name:key] == null)
            {
                throw new Exception(message:$"ERROR: cannot find the \"{key}\" key in the web.config file");
            }
            else
            {
                return ConfigurationManager.AppSettings[name:key];
            }
        }

        #endregion
    }
}