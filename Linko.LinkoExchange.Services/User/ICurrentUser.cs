using Linko.LinkoExchange.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.User
{
    public interface ICurrentUser
    {
        string GetClaimsValue(string claimType);
        void SetClaimsValue(string claimType, string value);
        void SetClaimsForOrgRegProgramSelection(int orgRegProgId);
    }
}
