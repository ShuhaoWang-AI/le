using System;
using System.Configuration;

namespace Linko.LinkoExchange.Services.Config
{
    public class ConfigSettingService : IConfigSettingService
    {
        public string GetConfigValue(string key)
        {
            if (ConfigurationManager.AppSettings[key] == null)
            {
                throw new Exception($"ERROR: cannot find the \"{ key }\" key in the web.config file");
            }
            else {
                return ConfigurationManager.AppSettings[key];
            }
        }
    }
}
