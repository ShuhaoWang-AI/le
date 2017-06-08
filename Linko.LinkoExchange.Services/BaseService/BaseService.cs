using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services
{
    abstract public class BaseService
    {
        abstract public bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id);
    }
}
