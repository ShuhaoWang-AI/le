using System.Runtime.CompilerServices;

namespace Linko.LinkoExchange.Services
{
    /// <summary>
    /// All service classes must inherit from this base class.
    /// Currently only used to implement function-level authorization. 
    /// </summary>
    abstract public class BaseService
    {
        /// <summary>
        /// Returns a boolean value indicating authorization to execute a method.
        /// This should be the first thing called upon entering a method.
        /// Returning false should typically trigger an UnauthorizedAccess exception
        /// </summary>
        /// <param name="apiName">Method name (automatically detected without explicit parameter)</param>
        /// <param name="id">Method specific "target" id of the object which the user is acting upon</param>
        /// <returns></returns>
        abstract public bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id);
    }
}
