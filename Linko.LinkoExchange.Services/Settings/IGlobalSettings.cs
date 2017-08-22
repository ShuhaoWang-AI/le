using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Settings
{
    public interface IGlobalSettings
    {
        /// <summary>
        ///     Returns the global settings dictionary that is instantiated and loaded with values as a singleton.
        ///     Values are taken from the tSystemSetting table.
        /// </summary>
        /// <returns> </returns>
        IDictionary<SystemSettingType, string> GetGlobalSettings();
    }
}