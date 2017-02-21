using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Program;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Linko.LinkoExchange.Services.Permission
{
    public interface IPermissionService
    {
        IEnumerable<UserDto> GetApprovalPeople(int organizationId);

        IEnumerable<PermissionGroupDto> GetRoles(int orgRegProgramId);
    }

}
