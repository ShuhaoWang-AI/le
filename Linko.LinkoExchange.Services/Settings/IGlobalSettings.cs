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
        /// <summary>
        /// Returns the global settings dictionary that is instantiated and loaded with values as a singleton.
        /// Values are taken from the tSystemSetting table.
        /// </summary>
        /// <returns></returns>
        IDictionary<SystemSettingType, string> GetGlobalSettings();
    }
}
