using Linko.LinkoExchange.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Settings
{
    public interface IGlobalSettings
    {
        string GetSetting(SystemSettingType settingType);
        IDictionary<SystemSettingType, string> GetGlobalSettings();
        bool IsCacheRequired(SettingType cacheKey, out int durationHours);
    }
}
