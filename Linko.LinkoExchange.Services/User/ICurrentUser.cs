﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.User
{
    public interface ICurrentUser
    {
        int? GetCurrentOrgRegProgramUserId();
    }
}