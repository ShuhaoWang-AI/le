using System.Runtime.CompilerServices;

namespace Linko.LinkoExchange.Services
{
    abstract public class BaseService
    {
        abstract public bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id);
    }
}
