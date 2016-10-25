using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.User
{
    public interface ICurrentUser
    {
        void SetCurrentOrgRegProgUserId(int orgRegProgUserId);
        int? GetCurrentOrgRegProgUserId();
        //void SetCurrentOrgRegProgramId(int orgRegProgramId);
        //int? GetCurrentOrgRegProgramId();
        //void SetCurrentOrganizationId(int organizationId);
        //int? GetCurrentOrganizationId();


    }
}
