using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Enum
{
    public enum SystemSettingType
    {
        SystemEmailFirstName = 1,
        SystemEmailLastName = 2,
        SystemEmailEmailAddress = 3,
        PasswordExpiredDays = 6,
        SupportPhoneNumber = 7,
        SupportEmailAddress = 8,
        EmailServer = 9,
        PasswordRequiredLength,
        PasswordRequireDigit
    }
}
